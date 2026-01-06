using UnityEngine;

/// <summary>
/// Defines the different biome types for procedural room generation.
/// Each biome has distinct visual and gameplay characteristics.
/// 
/// Reference: Implementation Plan Phase 4
/// </summary>
public enum BiomeType
{
    /// <summary>Urban environment with barricades and pillars.</summary>
    NeonCity,
    
    /// <summary>Frozen/water environment with ice crystals.</summary>
    CrystalLake,
    
    /// <summary>Bio-tech environment with wire trees and neon plants.</summary>
    TechForest,
    
    /// <summary>Volcanic environment with fire pits and lava cracks.</summary>
    LavaCore,
    
    /// <summary>Abstract void environment with floating debris.</summary>
    VoidZone
}

/// <summary>
/// Biome configuration data - colors and spawn settings.
/// </summary>
[System.Serializable]
public class BiomeData
{
    public BiomeType biomeType;
    public Color floorColor = Color.magenta;
    public Color emissionColor = Color.magenta;
    public float emissionIntensity = 1.5f;
    
    [Header("Spawning")]
    [Range(0, 10)] public int minObstacles = 2;
    [Range(0, 10)] public int maxObstacles = 5;
    [Range(0, 5)] public int minEnemies = 0;
    [Range(0, 5)] public int maxEnemies = 2;

    /// <summary>
    /// Creates default biome data for a given type.
    /// </summary>
    public static BiomeData CreateDefault(BiomeType type)
    {
        BiomeData data = new BiomeData { biomeType = type };
        
        switch (type)
        {
            case BiomeType.NeonCity:
                data.floorColor = new Color(0.1f, 0.02f, 0.15f);
                data.emissionColor = new Color(1f, 0f, 1f);
                data.minObstacles = 3; data.maxObstacles = 6;
                data.minEnemies = 1; data.maxEnemies = 3;
                break;
                
            case BiomeType.CrystalLake:
                data.floorColor = new Color(0.02f, 0.05f, 0.2f);
                data.emissionColor = new Color(0f, 0.5f, 1f);
                data.minObstacles = 4; data.maxObstacles = 8;
                data.minEnemies = 0; data.maxEnemies = 2;
                break;
                
            case BiomeType.TechForest:
                data.floorColor = new Color(0.02f, 0.1f, 0.08f);
                data.emissionColor = new Color(0f, 1f, 0.5f);
                data.minObstacles = 5; data.maxObstacles = 10;
                data.minEnemies = 1; data.maxEnemies = 2;
                break;
                
            case BiomeType.LavaCore:
                data.floorColor = new Color(0.15f, 0.02f, 0f);
                data.emissionColor = new Color(1f, 0.3f, 0f);
                data.minObstacles = 2; data.maxObstacles = 5;
                data.minEnemies = 2; data.maxEnemies = 4;
                break;
                
            case BiomeType.VoidZone:
                data.floorColor = new Color(0.02f, 0f, 0.05f);
                data.emissionColor = new Color(0.8f, 0f, 1f);
                data.minObstacles = 1; data.maxObstacles = 3;
                data.minEnemies = 1; data.maxEnemies = 2;
                break;
        }
        
        return data;
    }
}
