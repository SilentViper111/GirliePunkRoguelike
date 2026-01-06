using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Temporary power-up effect manager.
/// Handles duration, stacking, and visual feedback.
/// 
/// Reference: KB Section V - Pickups
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [System.Serializable]
    public class ActivePowerUp
    {
        public PowerUpType type;
        public float duration;
        public float timeRemaining;
        public float value;
    }

    public enum PowerUpType
    {
        SpeedBoost,
        DamageBoost,
        Shield,
        RapidFire,
        Magnet,
        HealthRegen,
        BulletTimeCharge
    }

    [Header("Active Power-Ups")]
    [SerializeField] private List<ActivePowerUp> activePowerUps = new List<ActivePowerUp>();

    [Header("Default Durations")]
    [SerializeField] private float speedDuration = 10f;
    [SerializeField] private float damageDuration = 15f;
    [SerializeField] private float shieldDuration = 8f;
    [SerializeField] private float rapidFireDuration = 12f;

    // Events
    public System.Action<PowerUpType, float> OnPowerUpActivated;
    public System.Action<PowerUpType> OnPowerUpExpired;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // Update active power-ups
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            activePowerUps[i].timeRemaining -= Time.deltaTime;

            if (activePowerUps[i].timeRemaining <= 0)
            {
                PowerUpType type = activePowerUps[i].type;
                activePowerUps.RemoveAt(i);
                OnPowerUpExpired?.Invoke(type);
                Debug.Log($"[PowerUp] {type} expired!");
            }
        }
    }

    /// <summary>
    /// Activates a power-up.
    /// </summary>
    public void ActivatePowerUp(PowerUpType type, float value = 1f, float duration = -1f)
    {
        // Get default duration if not specified
        if (duration < 0)
        {
            duration = type switch
            {
                PowerUpType.SpeedBoost => speedDuration,
                PowerUpType.DamageBoost => damageDuration,
                PowerUpType.Shield => shieldDuration,
                PowerUpType.RapidFire => rapidFireDuration,
                _ => 10f
            };
        }

        // Check if already active - refresh duration
        ActivePowerUp existing = activePowerUps.Find(p => p.type == type);
        if (existing != null)
        {
            existing.timeRemaining = Mathf.Max(existing.timeRemaining, duration);
            existing.value = Mathf.Max(existing.value, value);
        }
        else
        {
            activePowerUps.Add(new ActivePowerUp
            {
                type = type,
                duration = duration,
                timeRemaining = duration,
                value = value
            });
        }

        OnPowerUpActivated?.Invoke(type, duration);
        Debug.Log($"[PowerUp] {type} activated for {duration}s!");

        // Audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHealthPickup(); // Placeholder
    }

    /// <summary>
    /// Checks if a power-up is active.
    /// </summary>
    public bool IsPowerUpActive(PowerUpType type)
    {
        return activePowerUps.Exists(p => p.type == type);
    }

    /// <summary>
    /// Gets the value of an active power-up.
    /// </summary>
    public float GetPowerUpValue(PowerUpType type)
    {
        ActivePowerUp powerUp = activePowerUps.Find(p => p.type == type);
        return powerUp?.value ?? 0f;
    }

    /// <summary>
    /// Gets total speed multiplier.
    /// </summary>
    public float GetSpeedMultiplier()
    {
        if (IsPowerUpActive(PowerUpType.SpeedBoost))
            return 1f + GetPowerUpValue(PowerUpType.SpeedBoost);
        return 1f;
    }

    /// <summary>
    /// Gets total damage multiplier.
    /// </summary>
    public float GetDamageMultiplier()
    {
        if (IsPowerUpActive(PowerUpType.DamageBoost))
            return 1f + GetPowerUpValue(PowerUpType.DamageBoost);
        return 1f;
    }

    /// <summary>
    /// Gets fire rate multiplier.
    /// </summary>
    public float GetFireRateMultiplier()
    {
        if (IsPowerUpActive(PowerUpType.RapidFire))
            return 0.5f; // Double fire rate
        return 1f;
    }

    /// <summary>
    /// Checks if shield is active.
    /// </summary>
    public bool HasShield() => IsPowerUpActive(PowerUpType.Shield);

    public List<ActivePowerUp> GetActivePowerUps() => activePowerUps;
}
