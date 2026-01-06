using UnityEngine;

/// <summary>
/// Game statistics tracker for endgame summary.
/// 
/// Reference: KB Section VII - Progression
/// </summary>
public class GameStats : MonoBehaviour
{
    public static GameStats Instance { get; private set; }

    [Header("Run Statistics")]
    [SerializeField] private int totalKills;
    [SerializeField] private int maxCombo;
    [SerializeField] private int wavesCompleted;
    [SerializeField] private int bossesDefeated;
    [SerializeField] private float totalDamageDealt;
    [SerializeField] private float totalDamageTaken;
    [SerializeField] private int pickupsCollected;
    [SerializeField] private int dashesUsed;
    [SerializeField] private int bombsUsed;
    [SerializeField] private float playTime;

    [Header("Best Run All-Time")]
    [SerializeField] private int bestKills;
    [SerializeField] private int bestCombo;
    [SerializeField] private int bestWave;
    [SerializeField] private int bestScore;

    private float _runStartTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadBestStats();
    }

    private void Start()
    {
        ResetRunStats();
        _runStartTime = Time.time;
    }

    private void Update()
    {
        playTime = Time.time - _runStartTime;
    }

    // === Tracking Methods ===

    public void RecordKill() => totalKills++;

    public void RecordCombo(int combo)
    {
        if (combo > maxCombo)
            maxCombo = combo;
    }

    public void RecordWaveComplete()
    {
        wavesCompleted++;
        
        if (AchievementSystem.Instance != null)
            AchievementSystem.Instance.ReportProgress(AchievementSystem.AchievementType.WaveReached, 1);
    }

    public void RecordBossKill()
    {
        bossesDefeated++;
        
        if (AchievementSystem.Instance != null)
            AchievementSystem.Instance.ReportProgress(AchievementSystem.AchievementType.BossKilled, 1);
    }

    public void RecordDamageDealt(float damage) => totalDamageDealt += damage;

    public void RecordDamageTaken(float damage) => totalDamageTaken += damage;

    public void RecordPickup() => pickupsCollected++;

    public void RecordDash() => dashesUsed++;

    public void RecordBombUsed() => bombsUsed++;

    // === End of Run ===

    /// <summary>
    /// Called when game ends. Saves bests and returns summary.
    /// </summary>
    public RunSummary EndRun(int finalScore)
    {
        // Update bests
        if (totalKills > bestKills) bestKills = totalKills;
        if (maxCombo > bestCombo) bestCombo = maxCombo;
        if (wavesCompleted > bestWave) bestWave = wavesCompleted;
        if (finalScore > bestScore) bestScore = finalScore;

        SaveBestStats();

        return new RunSummary
        {
            kills = totalKills,
            maxCombo = maxCombo,
            waves = wavesCompleted,
            bosses = bossesDefeated,
            damageDealt = totalDamageDealt,
            damageTaken = totalDamageTaken,
            pickups = pickupsCollected,
            dashes = dashesUsed,
            bombs = bombsUsed,
            playTime = playTime,
            finalScore = finalScore,
            isNewBestScore = finalScore > bestScore
        };
    }

    /// <summary>
    /// Resets stats for new run.
    /// </summary>
    public void ResetRunStats()
    {
        totalKills = 0;
        maxCombo = 0;
        wavesCompleted = 0;
        bossesDefeated = 0;
        totalDamageDealt = 0;
        totalDamageTaken = 0;
        pickupsCollected = 0;
        dashesUsed = 0;
        bombsUsed = 0;
        playTime = 0;
        _runStartTime = Time.time;
    }

    private void SaveBestStats()
    {
        PlayerPrefs.SetInt("stats_best_kills", bestKills);
        PlayerPrefs.SetInt("stats_best_combo", bestCombo);
        PlayerPrefs.SetInt("stats_best_wave", bestWave);
        PlayerPrefs.SetInt("stats_best_score", bestScore);
        PlayerPrefs.Save();
    }

    private void LoadBestStats()
    {
        bestKills = PlayerPrefs.GetInt("stats_best_kills", 0);
        bestCombo = PlayerPrefs.GetInt("stats_best_combo", 0);
        bestWave = PlayerPrefs.GetInt("stats_best_wave", 0);
        bestScore = PlayerPrefs.GetInt("stats_best_score", 0);
    }

    // === Data Classes ===

    [System.Serializable]
    public class RunSummary
    {
        public int kills;
        public int maxCombo;
        public int waves;
        public int bosses;
        public float damageDealt;
        public float damageTaken;
        public int pickups;
        public int dashes;
        public int bombs;
        public float playTime;
        public int finalScore;
        public bool isNewBestScore;
    }

    // === Getters ===

    public int GetTotalKills() => totalKills;
    public int GetMaxCombo() => maxCombo;
    public int GetWavesCompleted() => wavesCompleted;
    public float GetPlayTime() => playTime;
    public int GetBestScore() => bestScore;
}
