using UnityEngine;

/// <summary>
/// Passive health regeneration for player.
/// 
/// Reference: KB Section VI - Player Abilities
/// </summary>
public class HealthRegeneration : MonoBehaviour
{
    [Header("Regeneration Settings")]
    [SerializeField] private float regenRate = 2f; // HP per second
    [SerializeField] private float regenDelay = 5f; // Seconds after last damage
    [SerializeField] private bool regenEnabled = true;

    [Header("State")]
    [SerializeField] private float lastDamageTime;
    [SerializeField] private bool isRegenerating;

    private PlayerHealth _playerHealth;

    private void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        if (_playerHealth != null)
        {
            _playerHealth.onDamaged.AddListener(OnDamaged);
        }
    }

    private void Update()
    {
        if (!regenEnabled || _playerHealth == null) return;
        if (!_playerHealth.IsAlive) return;

        // Check delay
        if (Time.time < lastDamageTime + regenDelay)
        {
            isRegenerating = false;
            return;
        }

        // Check if at max health
        if (_playerHealth.CurrentHealth >= _playerHealth.MaxHealth)
        {
            isRegenerating = false;
            return;
        }

        isRegenerating = true;

        // Apply power-up bonus
        float rate = regenRate;
        if (PowerUpManager.Instance != null)
            rate *= (1f + PowerUpManager.Instance.GetPowerUpValue(PowerUpManager.PowerUpType.HealthRegen));

        // Upgrade bonus
        if (UpgradeShop.Instance != null)
            rate *= (1f + UpgradeShop.Instance.GetUpgradeBonus(UpgradeShop.UpgradeType.HealthRegen));

        // Heal
        _playerHealth.Heal(rate * Time.deltaTime);
    }

    private void OnDamaged(float damage)
    {
        lastDamageTime = Time.time;
        isRegenerating = false;
    }

    public void SetEnabled(bool enabled) => regenEnabled = enabled;
    public bool IsRegenerating => isRegenerating;
}
