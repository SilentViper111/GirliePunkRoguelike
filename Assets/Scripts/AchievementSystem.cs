using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Achievement system for tracking player accomplishments.
/// 
/// Reference: KB Section VII - Progression
/// </summary>
public class AchievementSystem : MonoBehaviour
{
    public static AchievementSystem Instance { get; private set; }

    [System.Serializable]
    public class Achievement
    {
        public string id;
        public string title;
        public string description;
        public bool isUnlocked;
        public AchievementType type;
        public int targetValue;
        public int currentValue;
        public Sprite icon;
    }

    public enum AchievementType
    {
        KillCount,
        ComboCount,
        WaveReached,
        BossKilled,
        PickupsCollected,
        DashCount,
        NoHitWave,
        BulletTimeKills
    }

    [Header("Achievements")]
    [SerializeField] private List<Achievement> achievements = new List<Achievement>();

    [Header("Notification UI")]
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform notificationParent;
    [SerializeField] private float notificationDuration = 3f;

    private Queue<Achievement> _notificationQueue = new Queue<Achievement>();
    private bool _isShowingNotification;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeAchievements();
        LoadProgress();
    }

    private void InitializeAchievements()
    {
        achievements.Clear();

        // Kill achievements
        achievements.Add(new Achievement
        {
            id = "first_blood",
            title = "First Blood",
            description = "Kill your first enemy",
            type = AchievementType.KillCount,
            targetValue = 1
        });

        achievements.Add(new Achievement
        {
            id = "rampage",
            title = "Rampage",
            description = "Kill 100 enemies",
            type = AchievementType.KillCount,
            targetValue = 100
        });

        achievements.Add(new Achievement
        {
            id = "massacre",
            title = "Massacre",
            description = "Kill 500 enemies",
            type = AchievementType.KillCount,
            targetValue = 500
        });

        // Combo achievements
        achievements.Add(new Achievement
        {
            id = "combo_starter",
            title = "Combo Starter",
            description = "Reach a 10 hit combo",
            type = AchievementType.ComboCount,
            targetValue = 10
        });

        achievements.Add(new Achievement
        {
            id = "combo_master",
            title = "Combo Master",
            description = "Reach a 50 hit combo",
            type = AchievementType.ComboCount,
            targetValue = 50
        });

        achievements.Add(new Achievement
        {
            id = "sss_rank",
            title = "SSS Rank",
            description = "Achieve SSS style rank",
            type = AchievementType.ComboCount,
            targetValue = 50
        });

        // Wave achievements
        achievements.Add(new Achievement
        {
            id = "survivor",
            title = "Survivor",
            description = "Reach wave 5",
            type = AchievementType.WaveReached,
            targetValue = 5
        });

        achievements.Add(new Achievement
        {
            id = "veteran",
            title = "Veteran",
            description = "Reach wave 10",
            type = AchievementType.WaveReached,
            targetValue = 10
        });

        // Boss achievements
        achievements.Add(new Achievement
        {
            id = "boss_slayer",
            title = "Boss Slayer",
            description = "Defeat your first boss",
            type = AchievementType.BossKilled,
            targetValue = 1
        });

        // Special achievements
        achievements.Add(new Achievement
        {
            id = "untouchable",
            title = "Untouchable",
            description = "Complete a wave without taking damage",
            type = AchievementType.NoHitWave,
            targetValue = 1
        });
    }

    /// <summary>
    /// Reports progress toward an achievement.
    /// </summary>
    public void ReportProgress(AchievementType type, int value = 1)
    {
        foreach (var achievement in achievements)
        {
            if (achievement.isUnlocked) continue;
            if (achievement.type != type) continue;

            achievement.currentValue += value;

            if (achievement.currentValue >= achievement.targetValue)
            {
                UnlockAchievement(achievement);
            }
        }

        SaveProgress();
    }

    /// <summary>
    /// Sets a value (for max tracking like combo).
    /// </summary>
    public void SetMaxValue(AchievementType type, int value)
    {
        foreach (var achievement in achievements)
        {
            if (achievement.isUnlocked) continue;
            if (achievement.type != type) continue;

            if (value > achievement.currentValue)
            {
                achievement.currentValue = value;

                if (achievement.currentValue >= achievement.targetValue)
                {
                    UnlockAchievement(achievement);
                }
            }
        }

        SaveProgress();
    }

    private void UnlockAchievement(Achievement achievement)
    {
        achievement.isUnlocked = true;
        Debug.Log($"[Achievement] UNLOCKED: {achievement.title}!");

        // Queue notification
        _notificationQueue.Enqueue(achievement);

        if (!_isShowingNotification)
        {
            StartCoroutine(ShowNotifications());
        }

        // Audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUIClick(); // Placeholder
    }

    private System.Collections.IEnumerator ShowNotifications()
    {
        _isShowingNotification = true;

        while (_notificationQueue.Count > 0)
        {
            Achievement achievement = _notificationQueue.Dequeue();
            
            // Show notification (if UI exists)
            Debug.Log($"[Achievement] Showing notification: {achievement.title}");
            
            yield return new WaitForSeconds(notificationDuration);
        }

        _isShowingNotification = false;
    }

    private void SaveProgress()
    {
        foreach (var achievement in achievements)
        {
            PlayerPrefs.SetInt($"achievement_{achievement.id}_unlocked", achievement.isUnlocked ? 1 : 0);
            PlayerPrefs.SetInt($"achievement_{achievement.id}_progress", achievement.currentValue);
        }
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        foreach (var achievement in achievements)
        {
            achievement.isUnlocked = PlayerPrefs.GetInt($"achievement_{achievement.id}_unlocked", 0) == 1;
            achievement.currentValue = PlayerPrefs.GetInt($"achievement_{achievement.id}_progress", 0);
        }
    }

    public List<Achievement> GetAllAchievements() => achievements;
    public int GetUnlockedCount() => achievements.FindAll(a => a.isUnlocked).Count;
}
