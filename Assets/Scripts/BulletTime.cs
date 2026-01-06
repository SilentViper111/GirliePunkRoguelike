using UnityEngine;

/// <summary>
/// Bullet time ability that slows down game time.
/// Activated on special conditions or player ability.
/// 
/// Reference: KB Section VI - Player Abilities
/// </summary>
public class BulletTime : MonoBehaviour
{
    public static BulletTime Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float slowMotionScale = 0.3f;
    [SerializeField] private float maxDuration = 5f;
    [SerializeField] private float rechargeRate = 0.5f;
    [SerializeField] private float minChargeToActivate = 0.3f;

    [Header("Visual Effects")]
    [SerializeField] private float chromaticIntensity = 0.3f;
    [SerializeField] private Color bulletTimeColor = new Color(0.5f, 0f, 1f, 0.3f);

    [Header("Input")]
    [SerializeField] private bool useQKey = true;

    // State
    private float _currentCharge = 1f;
    private bool _isActive;
    private float _originalTimeScale;
    private float _originalFixedDelta;

    // Events
    public System.Action<float> OnChargeChanged;
    public System.Action<bool> OnBulletTimeToggled;

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
        // Input check
        if (useQKey)
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.qKey.wasPressedThisFrame)
            {
                if (_isActive)
                    Deactivate();
                else if (_currentCharge >= minChargeToActivate)
                    Activate();
            }
        }

        // Update charge
        if (_isActive)
        {
            _currentCharge -= Time.unscaledDeltaTime / maxDuration;
            OnChargeChanged?.Invoke(_currentCharge);

            if (_currentCharge <= 0)
            {
                Deactivate();
            }
        }
        else
        {
            // Recharge
            if (_currentCharge < 1f)
            {
                _currentCharge += Time.deltaTime * rechargeRate;
                _currentCharge = Mathf.Min(_currentCharge, 1f);
                OnChargeChanged?.Invoke(_currentCharge);
            }
        }
    }

    /// <summary>
    /// Activates bullet time.
    /// </summary>
    public void Activate()
    {
        if (_isActive || _currentCharge < minChargeToActivate) return;

        _isActive = true;
        _originalTimeScale = Time.timeScale;
        _originalFixedDelta = Time.fixedDeltaTime;

        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * slowMotionScale;

        OnBulletTimeToggled?.Invoke(true);

        // Audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUIClick(); // Placeholder

        Debug.Log("[BulletTime] ACTIVATED");
    }

    /// <summary>
    /// Deactivates bullet time.
    /// </summary>
    public void Deactivate()
    {
        if (!_isActive) return;

        _isActive = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        OnBulletTimeToggled?.Invoke(false);

        Debug.Log("[BulletTime] DEACTIVATED");
    }

    /// <summary>
    /// Adds charge to bullet time meter.
    /// </summary>
    public void AddCharge(float amount)
    {
        _currentCharge = Mathf.Min(_currentCharge + amount, 1f);
        OnChargeChanged?.Invoke(_currentCharge);
    }

    /// <summary>
    /// Triggers brief bullet time on special events (kills, etc).
    /// </summary>
    public void TriggerBrief(float duration = 0.3f)
    {
        if (_isActive) return;

        StartCoroutine(BriefSlowdown(duration));
    }

    private System.Collections.IEnumerator BriefSlowdown(float duration)
    {
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * slowMotionScale;

        yield return new WaitForSecondsRealtime(duration);

        if (!_isActive)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }

    public bool IsActive => _isActive;
    public float CurrentCharge => _currentCharge;
}
