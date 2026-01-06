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
        // Get world radius from generator
        float worldRadius = 500f;
        if (_worldGenerator != null)
        {
            // Use reflection or a public getter if available
            worldRadius = GetWorldRadius();
        }
        
        // Try to find valid spawn position
        for (int attempts = 0; attempts < 10; attempts++)
        {
            // Player's position on sphere
            Vector3 playerPos = _player.position;
            Vector3 playerDir = playerPos.normalized;
            
            // Random point on the sphere surface
            Vector3 randomDir = Random.onUnitSphere;
            
            // Calculate arc distance and ensure minimum distance
            float angle = Random.Range(minDistanceFromPlayer / worldRadius, maxDistanceFromPlayer / worldRadius);
            angle = Mathf.Clamp(angle, 0.1f, Mathf.PI * 0.5f); // Max 90 degrees around sphere
            
            // Rotate player direction by random angle
            Vector3 perpendicular = Vector3.Cross(playerDir, Random.onUnitSphere).normalized;
            if (perpendicular.sqrMagnitude < 0.01f)
                perpendicular = Vector3.Cross(playerDir, Vector3.right).normalized;
                
            Vector3 spawnDir = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, perpendicular) * playerDir;
            Vector3 spawnPos = spawnDir.normalized * worldRadius;
            
            // Add small offset above surface so enemy doesn't spawn inside
            spawnPos += spawnDir.normalized * 2f;
            
            // Check distance is valid
            float actualDist = Vector3.Distance(_player.position, spawnPos);
            if (actualDist >= minDistanceFromPlayer * 0.5f)
            {
                return spawnPos;
            }
        }
        
        // Fallback: spawn directly opposite player
        Vector3 opposite = -_player.position.normalized * worldRadius;
        return opposite + opposite.normalized * 2f;
    }
    
    private float GetWorldRadius()
    {
        // Use public WorldRadius property from WorldGenerator
        if (_worldGenerator != null)
        {
            return _worldGenerator.WorldRadius;
        }
        return 500f; // Default fallback
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
