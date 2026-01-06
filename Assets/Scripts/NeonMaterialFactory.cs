using UnityEngine;

/// <summary>
/// Creates and assigns neon materials at runtime.
/// 
/// Reference: KB Section IV - Visual Stack
/// </summary>
public class NeonMaterialFactory : MonoBehaviour
{
    public static NeonMaterialFactory Instance { get; private set; }

    [Header("Shader Reference")]
    [SerializeField] private Shader neonGlowShader;

    [Header("Biome Colors")]
    [SerializeField] private Color neonCityColor = new Color(1f, 0f, 1f);
    [SerializeField] private Color crystalLakeColor = new Color(0f, 1f, 1f);
    [SerializeField] private Color techForestColor = new Color(0f, 1f, 0.5f);
    [SerializeField] private Color lavaCoreColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color voidZoneColor = new Color(0.5f, 0f, 1f);

    private Material _baseMaterial;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Try to find shader
        if (neonGlowShader == null)
            neonGlowShader = Shader.Find("Custom/NeonGlow");

        // Fallback to standard shader
        if (neonGlowShader == null)
            neonGlowShader = Shader.Find("Standard");

        _baseMaterial = new Material(neonGlowShader);
    }

    /// <summary>
    /// Creates a neon material with specified color.
    /// </summary>
    public Material CreateNeonMaterial(Color mainColor, Color glowColor = default)
    {
        if (glowColor == default)
            glowColor = mainColor;

        Material mat = new Material(_baseMaterial);
        mat.SetColor("_Color", mainColor);
        mat.SetColor("_GlowColor", glowColor);
        mat.SetFloat("_GlowIntensity", 2f);
        mat.SetFloat("_PulseSpeed", 2f);

        // For standard shader
        mat.color = mainColor;
        mat.SetColor("_EmissionColor", glowColor * 2f);
        mat.EnableKeyword("_EMISSION");

        return mat;
    }

    /// <summary>
    /// Creates a neon material for a biome type.
    /// </summary>
    public Material CreateBiomeMaterial(BiomeType biome)
    {
        Color color = biome switch
        {
            BiomeType.NeonCity => neonCityColor,
            BiomeType.CrystalLake => crystalLakeColor,
            BiomeType.TechForest => techForestColor,
            BiomeType.LavaCore => lavaCoreColor,
            BiomeType.VoidZone => voidZoneColor,
            _ => Color.magenta
        };

        return CreateNeonMaterial(color);
    }

    /// <summary>
    /// Creates an enemy material based on type.
    /// </summary>
    public Material CreateEnemyMaterial(string enemyType)
    {
        Color color = enemyType.ToLower() switch
        {
            "melee" => new Color(0.8f, 0f, 0f),
            "ranged" => new Color(0f, 0.8f, 0.8f),
            "charger" => new Color(1f, 0f, 0f),
            "exploder" => new Color(1f, 0.5f, 0f),
            "tank" => new Color(0.5f, 0f, 0.5f),
            "swarm" => new Color(0f, 1f, 0f),
            "boss" => new Color(1f, 0f, 1f),
            _ => Color.red
        };

        return CreateNeonMaterial(color, Color.white);
    }

    /// <summary>
    /// Creates a player material.
    /// </summary>
    public Material CreatePlayerMaterial()
    {
        return CreateNeonMaterial(new Color(0f, 1f, 1f), Color.white);
    }

    /// <summary>
    /// Creates a projectile trail material.
    /// </summary>
    public Material CreateTrailMaterial(Color color)
    {
        Material mat = CreateNeonMaterial(color);
        mat.SetFloat("_GlowIntensity", 5f);
        return mat;
    }
}
