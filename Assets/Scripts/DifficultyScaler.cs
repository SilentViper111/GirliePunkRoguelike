using UnityEngine;

/// <summary>
/// Dynamic difficulty adjustment based on player performance.
/// 
/// Reference: KB Section V - Enemy Waves
/// </summary>
public class DifficultyScaler : MonoBehaviour
{
    public static DifficultyScaler Instance { get; private set; }

    [Header("Current Difficulty")]
    [SerializeField, Range(0f, 2f)] private float difficultyMultiplier = 1f;

    [Header("Scaling Factors")]
    [SerializeField] private float baseScalePerWave = 0.1f;
    [SerializeField] private float performanceWeight = 0.3f;

    [Header("Performance Tracking")]
    [SerializeField] private float averageCombo;
    [SerializeField] private float damageReceived;
    [SerializeField] private float timeSinceLastHit;
    [SerializeField] private int currentWave;

    [Header("Limits")]
    [SerializeField] private float minDifficulty = 0.5f;
    [SerializeField] private float maxDifficulty = 2.5f;

    // Internal tracking
    private int _totalHits;
    private int _totalKills;
    private float _sessionStartTime;
    private float _lastHitTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _sessionStartTime = Time.time;
        _lastHitTime = Time.time;
        difficultyMultiplier = 1f;
    }

    private void Update()
    {
        timeSinceLastHit = Time.time - _lastHitTime;

        // Slight difficulty increase over time (player is doing well)
        if (timeSinceLastHit > 30f)
        {
            difficultyMultiplier = Mathf.Min(difficultyMultiplier + Time.deltaTime * 0.01f, maxDifficulty);
        }
    }

    /// <summary>
    /// Reports when player takes damage.
    /// </summary>
    public void ReportPlayerHit(float damage)
    {
        _totalHits++;
        damageReceived += damage;
        _lastHitTime = Time.time;

        // Reduce difficulty slightly when player struggles
        difficultyMultiplier = Mathf.Max(difficultyMultiplier - 0.05f, minDifficulty);
    }

    /// <summary>
    /// Reports when player kills enemy.
    /// </summary>
    public void ReportKill()
    {
        _totalKills++;
    }

    /// <summary>
    /// Reports wave completion.
    /// </summary>
    public void ReportWaveComplete(int wave)
    {
        currentWave = wave;

        // Base scaling per wave
        float waveScale = wave * baseScalePerWave;

        // Performance adjustment
        float performance = CalculatePerformanceScore();
        float performanceAdjust = (performance - 0.5f) * 2f * performanceWeight;

        difficultyMultiplier = Mathf.Clamp(1f + waveScale + performanceAdjust, minDifficulty, maxDifficulty);

        Debug.Log($"[Difficulty] Wave {wave} complete. New difficulty: {difficultyMultiplier:F2}x");
    }

    /// <summary>
    /// Reports combo for performance tracking.
    /// </summary>
    public void ReportCombo(int combo)
    {
        // Running average
        averageCombo = (averageCombo * 0.8f) + (combo * 0.2f);
    }

    private float CalculatePerformanceScore()
    {
        float score = 0.5f; // Base

        // Combo bonus (high combos = playing well)
        score += Mathf.Clamp(averageCombo / 20f, 0f, 0.25f);

        // Damage penalty (lots of damage = struggling)
        float damagePerMinute = damageReceived / ((Time.time - _sessionStartTime) / 60f + 0.01f);
        score -= Mathf.Clamp(damagePerMinute / 100f, 0f, 0.25f);

        return Mathf.Clamp01(score);
    }

    // === Getters for enemy/spawner use ===

    /// <summary>
    /// Gets scaled enemy health.
    /// </summary>
    public float ScaleEnemyHealth(float baseHealth)
    {
        return baseHealth * difficultyMultiplier;
    }

    /// <summary>
    /// Gets scaled enemy damage.
    /// </summary>
    public float ScaleEnemyDamage(float baseDamage)
    {
        return baseDamage * Mathf.Lerp(1f, difficultyMultiplier, 0.5f);
    }

    /// <summary>
    /// Gets scaled enemy speed.
    /// </summary>
    public float ScaleEnemySpeed(float baseSpeed)
    {
        return baseSpeed * Mathf.Lerp(1f, difficultyMultiplier, 0.3f);
    }

    /// <summary>
    /// Gets scaled spawn count.
    /// </summary>
    public int ScaleSpawnCount(int baseCount)
    {
        return Mathf.RoundToInt(baseCount * difficultyMultiplier);
    }

    /// <summary>
    /// Gets current difficulty multiplier.
    /// </summary>
    public float GetDifficulty() => difficultyMultiplier;
}
