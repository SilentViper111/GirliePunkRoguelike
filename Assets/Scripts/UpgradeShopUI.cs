using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Upgrade shop UI panel.
/// Shows available upgrades and allows purchase.
/// 
/// Reference: KB Section VII - Progression
/// </summary>
public class UpgradeShopUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform upgradeContainer;
    [SerializeField] private GameObject upgradeEntryPrefab;
    [SerializeField] private TextMeshProUGUI currencyText;

    [Header("Toggle")]
    [SerializeField] private bool isOpen;

    private List<UpgradeEntryUI> _entries = new List<UpgradeEntryUI>();

    private void Start()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void Update()
    {
        // Toggle with U key
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.uKey.wasPressedThisFrame)
        {
            Toggle();
        }

        // Update currency display
        if (isOpen && UpgradeShop.Instance != null && currencyText != null)
        {
            currencyText.text = $"Currency: {UpgradeShop.Instance.GetCurrency()}";
        }
    }

    /// <summary>
    /// Toggles shop visibility.
    /// </summary>
    public void Toggle()
    {
        isOpen = !isOpen;

        if (shopPanel != null)
            shopPanel.SetActive(isOpen);

        if (isOpen)
        {
            RefreshUI();
            Time.timeScale = 0f; // Pause while shopping
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private void RefreshUI()
    {
        if (UpgradeShop.Instance == null) return;

        // For basic implementation, just log upgrades
        var upgrades = UpgradeShop.Instance.GetAllUpgrades();
        
        Debug.Log("[UpgradeShopUI] Available upgrades:");
        foreach (var upgrade in upgrades)
        {
            string status = upgrade.currentLevel >= upgrade.maxLevel ? "MAX" 
                : $"Lv.{upgrade.currentLevel}/{upgrade.maxLevel}";
            Debug.Log($"  - {upgrade.displayName}: {status}");
        }
    }

    /// <summary>
    /// Attempts to purchase an upgrade.
    /// </summary>
    public void TryPurchase(string upgradeId)
    {
        if (UpgradeShop.Instance == null) return;

        bool success = UpgradeShop.Instance.TryPurchaseUpgrade(upgradeId);
        
        if (success)
        {
            RefreshUI();
            
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayUIClick();
        }
    }
}

/// <summary>
/// Individual upgrade entry in the shop UI.
/// </summary>
public class UpgradeEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button buyButton;

    private string _upgradeId;
    private UpgradeShopUI _shopUI;

    public void Setup(UpgradeShop.Upgrade upgrade, UpgradeShopUI shopUI)
    {
        _upgradeId = upgrade.id;
        _shopUI = shopUI;

        if (nameText != null)
            nameText.text = upgrade.displayName;
        
        if (descriptionText != null)
            descriptionText.text = upgrade.description;
        
        if (levelText != null)
            levelText.text = $"Lv.{upgrade.currentLevel}/{upgrade.maxLevel}";

        if (upgrade.currentLevel >= upgrade.maxLevel)
        {
            if (costText != null)
                costText.text = "MAX";
            if (buyButton != null)
                buyButton.interactable = false;
        }
        else
        {
            int cost = upgrade.cost + (upgrade.currentLevel * (upgrade.cost / 2));
            if (costText != null)
                costText.text = $"Cost: {cost}";
        }

        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);
    }

    private void OnBuyClicked()
    {
        if (_shopUI != null)
            _shopUI.TryPurchase(_upgradeId);
    }
}
