using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Intelligent wave composition director.
/// Selects enemy types based on wave number and player performance.
/// 
/// Reference: KB Section V - Enemy Waves
/// </summary>
public class WaveDirector : MonoBehaviour
{
    public static WaveDirector Instance { get; private set; }

    [System.Serializable]
    public class EnemyWeight
    {
        public string enemyType;
        public GameObject prefab;
        public float baseWeight;
        public int minWave;
        public int maxPerWave;
    }

    [Header("Enemy Pool")]
    [SerializeField] private List<EnemyWeight> enemyPool = new List<EnemyWeight>();

    [Header("Wave Composition")]
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private int enemiesPerWaveIncrease = 2;
    [SerializeField] private int bossWaveInterval = 5;

    [Header("References")]
    [SerializeField] private GameObject bossPrefab;

    // Current wave state
    private int _currentWave;
    private List<GameObject> _currentWaveEnemies = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeDefaultPool();
    }

    private void InitializeDefaultPool()
    {
        if (enemyPool.Count > 0) return;

        // Will be auto-filled if prefabs exist
        enemyPool.Add(new EnemyWeight
        {
            enemyType = "Melee",
            baseWeight = 1f,
            minWave = 1,
            maxPerWave = 10
        });

        enemyPool.Add(new EnemyWeight
        {
            enemyType = "Ranged",
            baseWeight = 0.7f,
            minWave = 2,
            maxPerWave = 5
        });

        enemyPool.Add(new EnemyWeight
        {
            enemyType = "Charger",
            baseWeight = 0.5f,
            minWave = 3,
            maxPerWave = 3
        });

        enemyPool.Add(new EnemyWeight
        {
            enemyType = "Exploder",
            baseWeight = 0.4f,
            minWave = 4,
            maxPerWave = 2
        });

        enemyPool.Add(new EnemyWeight
        {
            enemyType = "Tank",
            baseWeight = 0.3f,
            minWave = 5,
            maxPerWave = 2
        });

        enemyPool.Add(new EnemyWeight
        {
            enemyType = "Swarm",
            baseWeight = 0.8f,
            minWave = 2,
            maxPerWave = 8
        });
    }

    /// <summary>
    /// Generates wave composition for a given wave number.
    /// </summary>
    public List<GameObject> GenerateWaveComposition(int wave)
    {
        _currentWave = wave;
        List<GameObject> composition = new List<GameObject>();

        // Check for boss wave
        bool isBossWave = wave > 0 && wave % bossWaveInterval == 0;
        if (isBossWave && bossPrefab != null)
        {
            composition.Add(bossPrefab);
            Debug.Log($"[WaveDirector] Wave {wave} is a BOSS WAVE!");
        }

        // Calculate enemy count
        int enemyCount = baseEnemiesPerWave + ((wave - 1) * enemiesPerWaveIncrease);
        if (DifficultyScaler.Instance != null)
            enemyCount = DifficultyScaler.Instance.ScaleSpawnCount(enemyCount);

        // Get available enemies for this wave
        List<EnemyWeight> available = enemyPool.FindAll(e => e.minWave <= wave && e.prefab != null);

        if (available.Count == 0)
        {
            Debug.LogWarning("[WaveDirector] No enemies available!");
            return composition;
        }

        // Calculate total weight
        float totalWeight = 0f;
        foreach (var e in available)
            totalWeight += e.baseWeight;

        // Build composition
        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (var e in available)
            counts[e.enemyType] = 0;

        for (int i = 0; i < enemyCount; i++)
        {
            // Weighted random selection
            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var e in available)
            {
                cumulative += e.baseWeight;
                if (roll <= cumulative)
                {
                    // Check max per wave
                    if (counts[e.enemyType] < e.maxPerWave)
                    {
                        composition.Add(e.prefab);
                        counts[e.enemyType]++;
                    }
                    else
                    {
                        // Try next type
                        foreach (var alt in available)
                        {
                            if (counts[alt.enemyType] < alt.maxPerWave)
                            {
                                composition.Add(alt.prefab);
                                counts[alt.enemyType]++;
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }

        Debug.Log($"[WaveDirector] Wave {wave} composition: {composition.Count} enemies");
        return composition;
    }

    /// <summary>
    /// Registers a prefab for an enemy type.
    /// </summary>
    public void RegisterEnemyPrefab(string enemyType, GameObject prefab)
    {
        var entry = enemyPool.Find(e => e.enemyType == enemyType);
        if (entry != null)
        {
            entry.prefab = prefab;
        }
    }

    /// <summary>
    /// Checks if current wave is a boss wave.
    /// </summary>
    public bool IsBossWave(int wave)
    {
        return wave > 0 && wave % bossWaveInterval == 0;
    }

    public int GetBossWaveInterval() => bossWaveInterval;
}
