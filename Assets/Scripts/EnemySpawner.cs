using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns enemies in waves with increasing difficulty.
/// Manages enemy population and wave progression.
/// 
/// Reference: KB Section V - Enemy System
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;
    
    [Header("Wave Settings")]
    [SerializeField] private float timeBetweenWaves = 30f;
    [SerializeField] private float timeBetweenSpawns = 2f;
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private int enemiesPerWaveIncrease = 2;
    [SerializeField] private int maxEnemiesAlive = 30;
    
    [Header("Spawn Settings")]
    [SerializeField] private float minDistanceFromPlayer = 50f;
    [SerializeField] private float maxDistanceFromPlayer = 150f;
    
    [Header("State")]
    [SerializeField] private int currentWave = 0;
    [SerializeField] private int enemiesAlive = 0;
    [SerializeField] private int enemiesRemainingInWave = 0;
    
    // References
    private WorldGenerator _worldGenerator;
    private Transform _player;
    private List<GameObject> _activeEnemies = new List<GameObject>();
    private float _nextSpawnTime;
    private float _waveStartTime;
    private bool _waveInProgress;

    private void Start()
    {
        _worldGenerator = FindFirstObjectByType<WorldGenerator>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
            
        StartNextWave();
    }

    private void Update()
    {
        // Clean up destroyed enemies
        _activeEnemies.RemoveAll(e => e == null);
        enemiesAlive = _activeEnemies.Count;
        
        // Spawn enemies during wave
        if (_waveInProgress && enemiesRemainingInWave > 0 && Time.time >= _nextSpawnTime)
        {
            if (enemiesAlive < maxEnemiesAlive)
            {
                SpawnEnemy();
                enemiesRemainingInWave--;
                _nextSpawnTime = Time.time + timeBetweenSpawns;
            }
        }
        
        // Check for wave completion
        if (_waveInProgress && enemiesRemainingInWave <= 0 && enemiesAlive <= 0)
        {
            _waveInProgress = false;
            Invoke(nameof(StartNextWave), timeBetweenWaves);
        }
    }

    private void StartNextWave()
    {
        currentWave++;
        enemiesRemainingInWave = baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease;
        _waveStartTime = Time.time;
        _waveInProgress = true;
        _nextSpawnTime = Time.time;
        
        Debug.Log($"[EnemySpawner] Wave {currentWave} started! {enemiesRemainingInWave} enemies to spawn.");
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] No enemy prefabs assigned!");
            return;
        }
        
        if (_player == null) return;
        
        // Get spawn position
        Vector3 spawnPos = GetSpawnPosition();
        if (spawnPos == Vector3.zero) return;
        
        // Pick random enemy type
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        
        // Spawn
        Vector3 normal = spawnPos.normalized;
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.LookRotation(Vector3.forward, normal));
        enemy.name = $"Enemy_Wave{currentWave}_{_activeEnemies.Count}";
        
        _activeEnemies.Add(enemy);
    }

    private Vector3 GetSpawnPosition()
    {
        // Try to find valid spawn position
        for (int attempts = 0; attempts < 10; attempts++)
        {
            // Random direction from player
            Vector3 playerUp = _player.position.normalized;
            Vector3 randomDir = Random.onUnitSphere;
            randomDir = Vector3.ProjectOnPlane(randomDir, playerUp).normalized;
            
            float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
            Vector3 spawnPos = _player.position + randomDir * distance;
            
            // Project onto sphere surface
            float worldRadius = _worldGenerator != null ? 500f : 500f;
            spawnPos = spawnPos.normalized * worldRadius;
            
            // Check distance is valid
            float actualDist = Vector3.Distance(_player.position, spawnPos);
            if (actualDist >= minDistanceFromPlayer)
            {
                return spawnPos;
            }
        }
        
        return Vector3.zero;
    }

    /// <summary>
    /// Called when an enemy is killed.
    /// </summary>
    public void OnEnemyKilled(GameObject enemy, int pointValue = 100)
    {
        _activeEnemies.Remove(enemy);
        
        // Add score
        GameUI ui = FindFirstObjectByType<GameUI>();
        if (ui != null)
            ui.AddScore(pointValue);
    }

    public int GetCurrentWave() => currentWave;
    public int GetEnemiesAlive() => enemiesAlive;
}
