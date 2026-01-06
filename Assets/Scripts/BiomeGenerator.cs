using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates procedural content (obstacles, enemies) within rooms based on biome type.
/// 
/// Reference: Implementation Plan Phase 4
/// </summary>
public class BiomeGenerator : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject defaultObstaclePrefab;
    [SerializeField] private GameObject neonCityObstaclePrefab;
    [SerializeField] private GameObject crystalLakeObstaclePrefab;
    [SerializeField] private GameObject techForestObstaclePrefab;
    [SerializeField] private GameObject lavaCoreObstaclePrefab;
    [SerializeField] private GameObject voidZoneObstaclePrefab;

    [Header("Enemy Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Generation Settings")]
    [SerializeField] private int randomSeed = 42;
    
    private Dictionary<BiomeType, BiomeData> _biomeDataCache = new Dictionary<BiomeType, BiomeData>();

    private void Awake()
    {
        // Cache biome data
        foreach (BiomeType type in System.Enum.GetValues(typeof(BiomeType)))
        {
            _biomeDataCache[type] = BiomeData.CreateDefault(type);
        }
    }

    /// <summary>
    /// Generates all content for a room based on its biome.
    /// </summary>
    public void GenerateRoomContent(RoomData room, float worldRadius, Transform parent)
    {
        if (room.isVIP)
        {
            // VIP rooms get special treatment (no random obstacles)
            return;
        }

        BiomeData biomeData = _biomeDataCache[room.biome];
        
        // Set random seed for reproducibility
        Random.InitState(randomSeed + room.roomIndex);
        
        // Generate obstacles
        int obstacleCount = Random.Range(biomeData.minObstacles, biomeData.maxObstacles + 1);
        for (int i = 0; i < obstacleCount; i++)
        {
            Vector3 position = room.GetRandomPositionInRoom(worldRadius);
            room.obstaclePositions.Add(position);
            
            SpawnObstacle(room.biome, position, room.normal, parent);
        }

        // Generate enemy spawn points (enemies spawned separately)
        int enemyCount = Random.Range(biomeData.minEnemies, biomeData.maxEnemies + 1);
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 position = room.GetRandomPositionInRoom(worldRadius);
            room.enemySpawnPoints.Add(position);
        }

        Debug.Log($"[BiomeGenerator] Room {room.roomIndex} ({room.biome}): {obstacleCount} obstacles, {enemyCount} enemy spawns");
    }

    /// <summary>
    /// Spawns an obstacle at the given position.
    /// </summary>
    private void SpawnObstacle(BiomeType biome, Vector3 position, Vector3 normal, Transform parent)
    {
        GameObject prefab = GetObstaclePrefab(biome);
        if (prefab == null)
        {
            // Create default cube obstacle
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.transform.position = position;
            obstacle.transform.up = normal;
            obstacle.transform.localScale = Vector3.one * Random.Range(1f, 3f);
            obstacle.transform.SetParent(parent);
            obstacle.name = $"Obstacle_{biome}";
            
            // Color based on biome
            var renderer = obstacle.GetComponent<Renderer>();
            if (renderer != null)
            {
                BiomeData data = _biomeDataCache[biome];
                renderer.material.color = data.emissionColor * 0.5f;
            }
            
            // Add collider layer
            obstacle.layer = LayerMask.NameToLayer("Default");
            return;
        }

        GameObject obj = Instantiate(prefab, position, Quaternion.LookRotation(Vector3.forward, normal), parent);
        obj.name = $"Obstacle_{biome}_{position.GetHashCode()}";
    }

    /// <summary>
    /// Spawns enemies at all spawn points for a room.
    /// </summary>
    public void SpawnEnemies(RoomData room, Transform parent)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("[BiomeGenerator] No enemy prefab assigned!");
            return;
        }

        foreach (Vector3 spawnPoint in room.enemySpawnPoints)
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.LookRotation(Vector3.forward, room.normal), parent);
            enemy.name = $"Enemy_Room{room.roomIndex}";
        }
    }

    private GameObject GetObstaclePrefab(BiomeType biome)
    {
        switch (biome)
        {
            case BiomeType.NeonCity: return neonCityObstaclePrefab ?? defaultObstaclePrefab;
            case BiomeType.CrystalLake: return crystalLakeObstaclePrefab ?? defaultObstaclePrefab;
            case BiomeType.TechForest: return techForestObstaclePrefab ?? defaultObstaclePrefab;
            case BiomeType.LavaCore: return lavaCoreObstaclePrefab ?? defaultObstaclePrefab;
            case BiomeType.VoidZone: return voidZoneObstaclePrefab ?? defaultObstaclePrefab;
            default: return defaultObstaclePrefab;
        }
    }

    /// <summary>
    /// Gets the biome data for a specific type.
    /// </summary>
    public BiomeData GetBiomeData(BiomeType type)
    {
        return _biomeDataCache.TryGetValue(type, out BiomeData data) ? data : BiomeData.CreateDefault(type);
    }
}
