using UnityEngine;

/// <summary>
/// Kill streak tracking for bonus rewards.
/// 
/// Reference: KB Section V - Combat
/// </summary>
public class KillStreakSystem : MonoBehaviour
{
    public static KillStreakSystem Instance { get; private set; }

    [Header("Streak Settings")]
    [SerializeField] private float streakWindowTime = 3f;
    [SerializeField] private int[] streakThresholds = { 3, 5, 10, 15, 25 };
    [SerializeField] private string[] streakNames = { "TRIPLE KILL", "RAMPAGE", "DOMINATING", "GODLIKE", "UNSTOPPABLE" };

    [Header("Rewards")]
    [SerializeField] private int[] streakBonusScores = { 50, 100, 250, 500, 1000 };
    [SerializeField] private float bulletTimeChargePerStreak = 0.1f;

    // State
    private int _currentStreak;
    private float _lastKillTime;
    private int _lastAnnouncedStreak;

    // Events
    public System.Action<int, string> OnStreakAchieved;
    public System.Action OnStreakEnded;

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
        // Check streak timeout
        if (_currentStreak > 0 && Time.time > _lastKillTime + streakWindowTime)
        {
            EndStreak();
        }
    }

    /// <summary>
    /// Registers a kill for streak tracking.
    /// </summary>
    public void RegisterKill()
    {
        _currentStreak++;
        _lastKillTime = Time.time;

        // Check thresholds
        for (int i = streakThresholds.Length - 1; i >= 0; i--)
        {
            if (_currentStreak >= streakThresholds[i] && _lastAnnouncedStreak < streakThresholds[i])
            {
                AnnounceStreak(i);
                break;
            }
        }
    }

    private void AnnounceStreak(int streakIndex)
    {
        _lastAnnouncedStreak = streakThresholds[streakIndex];
        string streakName = streakNames[streakIndex];
        int bonusScore = streakBonusScores[streakIndex];

        Debug.Log($"[Streak] {streakName}! +{bonusScore} points");

        // Award bonus
        GameUI ui = FindFirstObjectByType<GameUI>();
        if (ui != null)
            ui.AddScore(bonusScore);

        // Add bullet time charge
        if (BulletTime.Instance != null)
            BulletTime.Instance.AddCharge(bulletTimeChargePerStreak);

        // Screen shake
        if (ScreenShake.Instance != null)
            ScreenShake.Instance.ShakeMedium();

        OnStreakAchieved?.Invoke(_currentStreak, streakName);
    }

    private void EndStreak()
    {
        if (_currentStreak > 0)
        {
            Debug.Log($"[Streak] Streak ended at {_currentStreak} kills");
            OnStreakEnded?.Invoke();
        }

        _currentStreak = 0;
        _lastAnnouncedStreak = 0;
    }

    public int GetCurrentStreak() => _currentStreak;
}
