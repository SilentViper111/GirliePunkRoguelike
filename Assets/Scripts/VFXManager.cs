using UnityEngine;

/// <summary>
/// VFX Manager for creating particle effects on demand.
/// Pools and spawns impact, muzzle flash, and death effects.
/// 
/// Reference: KB Section IV - Visual Stack
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }
    
    [Header("Prefabs")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject impactSparksPrefab;
    [SerializeField] private GameObject enemyDeathPrefab;
    [SerializeField] private GameObject playerHitPrefab;
    [SerializeField] private GameObject bombExplosionPrefab;
    
    [Header("Settings")]
    [SerializeField] private float effectLifetime = 2f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Spawns a muzzle flash effect at the fire point.
    /// </summary>
    public void SpawnMuzzleFlash(Vector3 position, Quaternion rotation)
    {
        SpawnEffect(muzzleFlashPrefab, position, rotation);
    }

    /// <summary>
    /// Spawns impact sparks when a projectile hits something.
    /// </summary>
    public void SpawnImpactSparks(Vector3 position, Vector3 normal)
    {
        Quaternion rotation = Quaternion.LookRotation(normal);
        SpawnEffect(impactSparksPrefab, position, rotation);
    }

    /// <summary>
    /// Spawns enemy death explosion.
    /// </summary>
    public void SpawnEnemyDeath(Vector3 position)
    {
        SpawnEffect(enemyDeathPrefab, position, Quaternion.identity);
    }

    /// <summary>
    /// Spawns player hit effect.
    /// </summary>
    public void SpawnPlayerHit(Vector3 position)
    {
        SpawnEffect(playerHitPrefab, position, Quaternion.identity);
    }

    /// <summary>
    /// Spawns bomb explosion effect.
    /// </summary>
    public void SpawnBombExplosion(Vector3 position, float scale = 1f)
    {
        GameObject effect = SpawnEffect(bombExplosionPrefab, position, Quaternion.identity);
        if (effect != null)
            effect.transform.localScale = Vector3.one * scale;
    }

    private GameObject SpawnEffect(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            // Create default particle system if no prefab
            return CreateDefaultEffect(position, rotation);
        }

        GameObject effect = Instantiate(prefab, position, rotation);
        Destroy(effect, effectLifetime);
        return effect;
    }

    /// <summary>
    /// Creates a default particle effect when no prefab is assigned.
    /// </summary>
    private GameObject CreateDefaultEffect(Vector3 position, Quaternion rotation)
    {
        GameObject effect = new GameObject("DefaultVFX");
        effect.transform.position = position;
        effect.transform.rotation = rotation;

        ParticleSystem ps = effect.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 5f;
        main.startSize = 0.3f;
        main.startColor = new Color(1f, 0f, 1f, 1f); // Magenta
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 20));

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.magenta, 0f), new GradientColorKey(Color.cyan, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;

        Destroy(effect, effectLifetime);
        return effect;
    }
}
