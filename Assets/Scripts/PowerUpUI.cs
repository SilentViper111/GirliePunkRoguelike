using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Power-up status icons display.
/// Shows active power-ups with remaining duration.
/// 
/// Reference: KB Section VIII - UI/UX
/// </summary>
public class PowerUpUI : MonoBehaviour
{
    [Header("Container")]
    [SerializeField] private Transform iconContainer;
    [SerializeField] private GameObject iconPrefab;

    [Header("Colors")]
    [SerializeField] private Color speedColor = Color.cyan;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color shieldColor = Color.yellow;
    [SerializeField] private Color rapidFireColor = Color.magenta;

    private Dictionary<PowerUpManager.PowerUpType, PowerUpIcon> _activeIcons = 
        new Dictionary<PowerUpManager.PowerUpType, PowerUpIcon>();

    private void Start()
    {
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.OnPowerUpActivated += OnPowerUpActivated;
            PowerUpManager.Instance.OnPowerUpExpired += OnPowerUpExpired;
        }
    }

    private void Update()
    {
        // Update durations
        if (PowerUpManager.Instance == null) return;

        foreach (var powerUp in PowerUpManager.Instance.GetActivePowerUps())
        {
            if (_activeIcons.ContainsKey(powerUp.type))
            {
                _activeIcons[powerUp.type].UpdateDuration(powerUp.timeRemaining, powerUp.duration);
            }
        }
    }

    private void OnPowerUpActivated(PowerUpManager.PowerUpType type, float duration)
    {
        // Create or update icon
        if (!_activeIcons.ContainsKey(type))
        {
            CreateIcon(type);
        }
    }

    private void OnPowerUpExpired(PowerUpManager.PowerUpType type)
    {
        // Remove icon
        if (_activeIcons.ContainsKey(type))
        {
            if (_activeIcons[type] != null)
                Destroy(_activeIcons[type].gameObject);
            _activeIcons.Remove(type);
        }
    }

    private void CreateIcon(PowerUpManager.PowerUpType type)
    {
        if (iconContainer == null) return;

        GameObject go = new GameObject($"PowerUp_{type}");
        go.transform.SetParent(iconContainer);
        
        PowerUpIcon icon = go.AddComponent<PowerUpIcon>();
        icon.Setup(type, GetColorForType(type));
        
        _activeIcons[type] = icon;
    }

    private Color GetColorForType(PowerUpManager.PowerUpType type)
    {
        return type switch
        {
            PowerUpManager.PowerUpType.SpeedBoost => speedColor,
            PowerUpManager.PowerUpType.DamageBoost => damageColor,
            PowerUpManager.PowerUpType.Shield => shieldColor,
            PowerUpManager.PowerUpType.RapidFire => rapidFireColor,
            _ => Color.white
        };
    }

    private void OnDestroy()
    {
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.OnPowerUpActivated -= OnPowerUpActivated;
            PowerUpManager.Instance.OnPowerUpExpired -= OnPowerUpExpired;
        }
    }
}

/// <summary>
/// Individual power-up icon.
/// </summary>
public class PowerUpIcon : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI timerText;

    private PowerUpManager.PowerUpType _type;
    private Color _color;

    public void Setup(PowerUpManager.PowerUpType type, Color color)
    {
        _type = type;
        _color = color;

        // Create basic UI elements
        RectTransform rect = gameObject.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(50, 50);

        // Background
        Image bg = gameObject.AddComponent<Image>();
        bg.color = new Color(_color.r, _color.g, _color.b, 0.5f);

        // Timer text (optional)
        GameObject textGo = new GameObject("Timer");
        textGo.transform.SetParent(transform);
        timerText = textGo.AddComponent<TextMeshProUGUI>();
        timerText.fontSize = 12;
        timerText.alignment = TextAlignmentOptions.Center;
        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    public void UpdateDuration(float remaining, float total)
    {
        if (timerText != null)
        {
            timerText.text = $"{remaining:F1}s";
        }
    }
}
