using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Upgrade shop for purchasing permanent and temporary upgrades.
/// Uses score as currency.
/// 
/// Reference: KB Section VII - Progression
/// </summary>
public class UpgradeShop : MonoBehaviour
{
    public static UpgradeShop Instance { get; private set; }

    [System.Serializable]
    public class Upgrade
    {
        public string id;
        public string displayName;
        public string description;
        public int cost;
        public int maxLevel;
        public int currentLevel;
        public float valuePerLevel;
        public UpgradeType type;
    }

    public enum UpgradeType
    {
        MaxHealth,
        MoveSpeed,
        DashCooldown,
        FireRate,
        Damage,
        BombCapacity,
        HealthRegen,
        CritChance
    }

    [Header("Upgrades")]
    [SerializeField] private List<Upgrade> upgrades = new List<Upgrade>();

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerDash playerDash;
    [SerializeField] private GameUI gameUI;

    // Currency
    private int _currency;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeUpgrades();
    }

    private void Start()
    {
        // Find references
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerDash == null) playerDash = FindFirstObjectByType<PlayerDash>();
        if (gameUI == null) gameUI = FindFirstObjectByType<GameUI>();
    }

    private void InitializeUpgrades()
    {
        upgrades.Clear();

        upgrades.Add(new Upgrade
        {
            id = "max_health",
            displayName = "Max Health",
            description = "+25 max health",
            cost = 500,
            maxLevel = 5,
            currentLevel = 0,
            valuePerLevel = 25f,
            type = UpgradeType.MaxHealth
        });

        upgrades.Add(new Upgrade
        {
            id = "move_speed",
            displayName = "Move Speed",
            description = "+10% movement speed",
            cost = 300,
            maxLevel = 5,
            currentLevel = 0,
            valuePerLevel = 0.1f,
            type = UpgradeType.MoveSpeed
        });

        upgrades.Add(new Upgrade
        {
            id = "fire_rate",
            displayName = "Fire Rate",
            description = "-10% trash fire cooldown",
            cost = 400,
            maxLevel = 5,
            currentLevel = 0,
            valuePerLevel = 0.1f,
            type = UpgradeType.FireRate
        });

        upgrades.Add(new Upgrade
        {
            id = "damage",
            displayName = "Damage",
            description = "+15% damage",
            cost = 600,
            maxLevel = 5,
            currentLevel = 0,
            valuePerLevel = 0.15f,
            type = UpgradeType.Damage
        });

        upgrades.Add(new Upgrade
        {
            id = "bomb_capacity",
            displayName = "Bomb Capacity",
            description = "+1 max bombs",
            cost = 800,
            maxLevel = 3,
            currentLevel = 0,
            valuePerLevel = 1f,
            type = UpgradeType.BombCapacity
        });

        upgrades.Add(new Upgrade
        {
            id = "dash_cooldown",
            displayName = "Dash Cooldown",
            description = "-15% dash cooldown",
            cost = 350,
            maxLevel = 4,
            currentLevel = 0,
            valuePerLevel = 0.15f,
            type = UpgradeType.DashCooldown
        });

        upgrades.Add(new Upgrade
        {
            id = "crit_chance",
            displayName = "Critical Hit",
            description = "+5% critical hit chance",
            cost = 700,
            maxLevel = 5,
            currentLevel = 0,
            valuePerLevel = 0.05f,
            type = UpgradeType.CritChance
        });
    }

    /// <summary>
    /// Sets the currency (typically score).
    /// </summary>
    public void SetCurrency(int amount)
    {
        _currency = amount;
    }

    /// <summary>
    /// Add currency.
    /// </summary>
    public void AddCurrency(int amount)
    {
        _currency += amount;
    }

    /// <summary>
    /// Attempts to purchase an upgrade.
    /// </summary>
    public bool TryPurchaseUpgrade(string upgradeId)
    {
        Upgrade upgrade = upgrades.Find(u => u.id == upgradeId);
        if (upgrade == null) return false;

        if (upgrade.currentLevel >= upgrade.maxLevel)
        {
            Debug.Log($"[Shop] {upgrade.displayName} already at max level!");
            return false;
        }

        int cost = GetUpgradeCost(upgrade);
        if (_currency < cost)
        {
            Debug.Log($"[Shop] Not enough currency! Need {cost}, have {_currency}");
            return false;
        }

        // Purchase
        _currency -= cost;
        upgrade.currentLevel++;
        ApplyUpgrade(upgrade);

        Debug.Log($"[Shop] Purchased {upgrade.displayName} level {upgrade.currentLevel}!");
        return true;
    }

    private int GetUpgradeCost(Upgrade upgrade)
    {
        // Cost increases with level
        return upgrade.cost + (upgrade.currentLevel * (upgrade.cost / 2));
    }

    private void ApplyUpgrade(Upgrade upgrade)
    {
        float totalValue = upgrade.valuePerLevel * upgrade.currentLevel;

        switch (upgrade.type)
        {
            case UpgradeType.MaxHealth:
                if (playerHealth != null)
                    playerHealth.SetMaxHealth(100 + totalValue, true);
                break;

            // Other upgrades require modifying PlayerController fields
            // For now, store the values and let scripts query them
        }
    }

    /// <summary>
    /// Gets the total bonus for an upgrade type.
    /// </summary>
    public float GetUpgradeBonus(UpgradeType type)
    {
        Upgrade upgrade = upgrades.Find(u => u.type == type);
        if (upgrade == null) return 0f;
        return upgrade.valuePerLevel * upgrade.currentLevel;
    }

    public List<Upgrade> GetAllUpgrades() => upgrades;
    public int GetCurrency() => _currency;
}
