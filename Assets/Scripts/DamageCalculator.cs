using UnityEngine;

/// <summary>
/// Integrates all damage systems.
/// Central point for dealing damage with crits, power-ups, upgrades.
/// 
/// Reference: KB Section V - Combat
/// </summary>
public static class DamageCalculator
{
    /// <summary>
    /// Calculates final damage with all modifiers applied.
    /// </summary>
    public static float Calculate(float baseDamage, bool isPlayerDamage = true)
    {
        float damage = baseDamage;

        if (isPlayerDamage)
        {
            // Power-up bonus
            if (PowerUpManager.Instance != null)
                damage *= PowerUpManager.Instance.GetDamageMultiplier();

            // Upgrade bonus
            if (UpgradeShop.Instance != null)
                damage *= (1f + UpgradeShop.Instance.GetUpgradeBonus(UpgradeShop.UpgradeType.Damage));

            // Combo bonus (small scaling)
            if (ComboSystem.Instance != null)
            {
                int combo = ComboSystem.Instance.GetCurrentCombo();
                damage *= (1f + combo * 0.01f); // 1% per combo
            }
        }

        return damage;
    }

    /// <summary>
    /// Calculates damage with critical hit check.
    /// </summary>
    public static float CalculateWithCrit(float baseDamage, out bool wasCrit, bool isPlayerDamage = true)
    {
        float damage = Calculate(baseDamage, isPlayerDamage);
        wasCrit = false;

        if (isPlayerDamage && CriticalHitSystem.Instance != null)
        {
            damage = CriticalHitSystem.Instance.ApplyCrit(damage, out wasCrit);
        }

        return damage;
    }

    /// <summary>
    /// Deals damage to a target with full integration.
    /// Handles VFX, damage numbers, combos, achievements.
    /// </summary>
    public static void DealDamage(IDamageable target, float baseDamage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (target == null || !target.IsAlive) return;

        float damage = CalculateWithCrit(baseDamage, out bool wasCrit);

        // Apply damage
        target.TakeDamage(damage, hitPoint, hitNormal);

        // Floating damage number
        FloatingDamageNumber.Spawn(hitPoint, damage, wasCrit);

        // VFX
        if (VFXManager.Instance != null)
            VFXManager.Instance.SpawnImpactSparks(hitPoint, hitNormal);

        // Track stats
        if (GameStats.Instance != null)
            GameStats.Instance.RecordDamageDealt(damage);

        // Kill streak (if killed)
        if (!target.IsAlive && KillStreakSystem.Instance != null)
            KillStreakSystem.Instance.RegisterKill();
    }

    /// <summary>
    /// Deals AOE damage centered at a point.
    /// </summary>
    public static void DealAOEDamage(Vector3 center, float radius, float baseDamage, LayerMask targetLayers)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius, targetLayers);

        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                float dist = Vector3.Distance(center, hit.transform.position);
                float falloff = 1f - (dist / radius);
                float damage = CalculateWithCrit(baseDamage * falloff, out bool wasCrit);

                Vector3 hitDir = (hit.transform.position - center).normalized;
                damageable.TakeDamage(damage, hit.ClosestPoint(center), hitDir);

                // Floating number
                FloatingDamageNumber.Spawn(hit.transform.position + Vector3.up, damage, wasCrit);
            }
        }
    }
}
