using UnityEngine;

/// <summary>
/// Runs the game at startup - initializes all manager singletons.
/// 
/// Reference: KB Section - Architecture
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Manager Prefabs")]
    [SerializeField] private GameObject gameManagerPrefab;

    [Header("Singleton Check")]
    [SerializeField] private bool checkSingletons = true;

    private void Awake()
    {
        Debug.Log("[GameBootstrap] Initializing game systems...");

        // Create singletons if they don't exist
        EnsureManagerExists<GameManager>("GameManager");
        EnsureManagerExists<AudioManager>("AudioManager");
        EnsureManagerExists<VFXManager>("VFXManager");
        EnsureManagerExists<ComboSystem>("ComboSystem");
        EnsureManagerExists<AchievementSystem>("AchievementSystem");
        EnsureManagerExists<TutorialSystem>("TutorialSystem");
        EnsureManagerExists<PowerUpManager>("PowerUpManager");
        EnsureManagerExists<LeaderboardManager>("LeaderboardManager");
        EnsureManagerExists<ObjectiveSystem>("ObjectiveSystem");
        EnsureManagerExists<DifficultyScaler>("DifficultyScaler");
        EnsureManagerExists<WaveDirector>("WaveDirector");
        EnsureManagerExists<BulletTime>("BulletTime");
        EnsureManagerExists<NeonMaterialFactory>("NeonMaterialFactory");
        EnsureManagerExists<RoomManager>("RoomManager");
        EnsureManagerExists<UpgradeShop>("UpgradeShop");
        EnsureManagerExists<ScreenShake>("ScreenShake");

        Debug.Log("[GameBootstrap] All systems initialized!");
    }

    private void EnsureManagerExists<T>(string name) where T : MonoBehaviour
    {
        if (FindFirstObjectByType<T>() == null)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<T>();
            DontDestroyOnLoad(go);
            Debug.Log($"[GameBootstrap] Created {name}");
        }
    }

    private void Start()
    {
        // Connect systems
        ConnectSystems();

        // Show tutorial
        if (TutorialSystem.Instance != null)
            TutorialSystem.Instance.ShowTutorial("movement");
    }

    private void ConnectSystems()
    {
        // Connect combo to achievements
        if (ComboSystem.Instance != null && AchievementSystem.Instance != null)
        {
            ComboSystem.Instance.OnComboChanged += (combo, mult) =>
            {
                AchievementSystem.Instance.SetMaxValue(AchievementSystem.AchievementType.ComboCount, combo);
                
                if (ObjectiveSystem.Instance != null)
                    ObjectiveSystem.Instance.SetMaxValue(ObjectiveSystem.ObjectiveType.AchieveCombo, combo);
                
                if (DifficultyScaler.Instance != null)
                    DifficultyScaler.Instance.ReportCombo(combo);
            };
        }

        // Connect player health to difficulty
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null && DifficultyScaler.Instance != null)
        {
            playerHealth.onDamaged.AddListener(damage =>
            {
                DifficultyScaler.Instance.ReportPlayerHit(damage);
            });
        }

        Debug.Log("[GameBootstrap] Systems connected!");
    }
}
