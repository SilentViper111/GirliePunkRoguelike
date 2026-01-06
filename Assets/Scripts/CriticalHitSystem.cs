using UnityEngine;

/// <summary>
/// Critical hit system for additional damage.
/// 
/// Reference: KB Section V - Combat
/// </summary>
public class CriticalHitSystem : MonoBehaviour
{
    public static CriticalHitSystem Instance { get; private set; }

    [Header("Critical Settings")]
    [SerializeField, Range(0f, 1f)] private float baseCritChance = 0.05f;
    [SerializeField] private float critDamageMultiplier = 2f;

    [Header("Upgrades")]
    [SerializeField] private float bonusCritChance = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Rolls for a critical hit.
    /// </summary>
    public bool RollForCrit()
    {
        float totalChance = baseCritChance + bonusCritChance;
        
        // Add upgrade bonus
        if (UpgradeShop.Instance != null)
            totalChance += UpgradeShop.Instance.GetUpgradeBonus(UpgradeShop.UpgradeType.CritChance);

        return Random.value < totalChance;
    }

    /// <summary>
    /// Applies critical hit to damage.
    /// </summary>
    public float ApplyCrit(float damage, out bool wasCrit)
    {
        wasCrit = RollForCrit();
        
        if (wasCrit)
        {
            damage *= critDamageMultiplier;
            
            // Brief bullet time on crit
            if (BulletTime.Instance != null)
                BulletTime.Instance.TriggerBrief(0.1f);
                
            Debug.Log($"[Crit] CRITICAL HIT! Damage: {damage}");
        }
        
        return damage;
    }

    /// <summary>
    /// Adds bonus crit chance (from power-ups).
    /// </summary>
    public void AddBonusCritChance(float amount)
    {
        bonusCritChance += amount;
    }

    public float GetCritChance() => baseCritChance + bonusCritChance;
    public float GetCritMultiplier() => critDamageMultiplier;
}
