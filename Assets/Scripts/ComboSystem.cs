using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Combo system that tracks hit chains and multiplies score.
/// 
/// Reference: KB Section V - Combat Mechanics
/// </summary>
public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance { get; private set; }

    [Header("Combo Settings")]
    [SerializeField] private float comboWindowTime = 2f;
    [SerializeField] private int maxComboMultiplier = 10;
    [SerializeField] private float comboDecayRate = 0.5f;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI comboCountText;
    [SerializeField] private TextMeshProUGUI comboMultiplierText;
    [SerializeField] private CanvasGroup comboGroup;
    [SerializeField] private Animator comboAnimator;

    [Header("Style Ranks")]
    [SerializeField] private string[] styleRanks = { "D", "C", "B", "A", "S", "SS", "SSS" };
    [SerializeField] private Color[] styleColors;
    [SerializeField] private TextMeshProUGUI styleRankText;

    // State
    private int _currentCombo;
    private float _lastHitTime;
    private float _comboTimer;
    private int _currentStyleIndex;

    // Events
    public System.Action<int, int> OnComboChanged; // combo, multiplier

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (styleColors == null || styleColors.Length == 0)
        {
            styleColors = new Color[]
            {
                Color.gray,    // D
                Color.white,   // C
                Color.green,   // B
                Color.cyan,    // A
                Color.yellow,  // S
                Color.magenta, // SS
                Color.red      // SSS
            };
        }
    }

    private void Start()
    {
        UpdateUI();
        if (comboGroup != null)
            comboGroup.alpha = 0f;
    }

    private void Update()
    {
        if (_currentCombo > 0)
        {
            _comboTimer -= Time.deltaTime;
            
            if (_comboTimer <= 0)
            {
                BreakCombo();
            }
            else
            {
                // Visual feedback for timer
                float timerPercent = _comboTimer / comboWindowTime;
                // Could animate combo UI here
            }
        }
    }

    /// <summary>
    /// Registers a hit - call this when damaging an enemy.
    /// </summary>
    public void RegisterHit()
    {
        _currentCombo++;
        _lastHitTime = Time.time;
        _comboTimer = comboWindowTime;

        // Update style rank
        UpdateStyleRank();

        // Fire event
        OnComboChanged?.Invoke(_currentCombo, GetMultiplier());

        // UI
        UpdateUI();

        // Animation
        if (comboAnimator != null)
            comboAnimator.SetTrigger("Hit");

        Debug.Log($"[Combo] Combo: {_currentCombo}x (Multiplier: {GetMultiplier()}x)");
    }

    /// <summary>
    /// Registers a kill - gives bonus combo points.
    /// </summary>
    public void RegisterKill()
    {
        RegisterHit();
        RegisterHit(); // Kills count as 2 hits
    }

    /// <summary>
    /// Breaks the current combo.
    /// </summary>
    public void BreakCombo()
    {
        if (_currentCombo > 0)
        {
            Debug.Log($"[Combo] Combo broken at {_currentCombo}!");
            _currentCombo = 0;
            _currentStyleIndex = 0;
            UpdateUI();
            OnComboChanged?.Invoke(0, 1);
        }
    }

    /// <summary>
    /// Gets the current score multiplier based on combo.
    /// </summary>
    public int GetMultiplier()
    {
        // Every 5 hits increases multiplier by 1
        int multiplier = 1 + (_currentCombo / 5);
        return Mathf.Min(multiplier, maxComboMultiplier);
    }

    /// <summary>
    /// Applies multiplier to a score value.
    /// </summary>
    public int ApplyMultiplier(int baseScore)
    {
        return baseScore * GetMultiplier();
    }

    private void UpdateStyleRank()
    {
        // Determine style rank based on combo
        if (_currentCombo >= 50)
            _currentStyleIndex = 6; // SSS
        else if (_currentCombo >= 40)
            _currentStyleIndex = 5; // SS
        else if (_currentCombo >= 30)
            _currentStyleIndex = 4; // S
        else if (_currentCombo >= 20)
            _currentStyleIndex = 3; // A
        else if (_currentCombo >= 10)
            _currentStyleIndex = 2; // B
        else if (_currentCombo >= 5)
            _currentStyleIndex = 1; // C
        else
            _currentStyleIndex = 0; // D
    }

    private void UpdateUI()
    {
        if (comboGroup != null)
        {
            comboGroup.alpha = _currentCombo > 0 ? 1f : 0f;
        }

        if (comboCountText != null)
        {
            comboCountText.text = $"{_currentCombo}";
        }

        if (comboMultiplierText != null)
        {
            comboMultiplierText.text = $"x{GetMultiplier()}";
        }

        if (styleRankText != null && _currentStyleIndex < styleRanks.Length)
        {
            styleRankText.text = styleRanks[_currentStyleIndex];
            if (_currentStyleIndex < styleColors.Length)
                styleRankText.color = styleColors[_currentStyleIndex];
        }
    }

    public int GetCurrentCombo() => _currentCombo;
    public string GetCurrentStyleRank() => styleRanks[Mathf.Clamp(_currentStyleIndex, 0, styleRanks.Length - 1)];
}
