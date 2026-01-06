using UnityEngine;

/// <summary>
/// Spawns pickups in biomes based on room clearing and random chance.
/// 
/// Reference: KB Section V - Pickups
/// </summary>
public class PickupSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private GameObject bombPickupPrefab;
    [SerializeField] private GameObject speedPickupPrefab;
    [SerializeField] private GameObject damagePickupPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnChanceOnKill = 0.15f;
    [SerializeField] private float healthSpawnWeight = 0.4f;
    [SerializeField] private float bombSpawnWeight = 0.3f;
    [SerializeField] private float powerupSpawnWeight = 0.3f;
    
    [Header("Spawn Height")]
    [SerializeField] private float spawnHeightOffset = 2f;
    
    public static PickupSpawner Instance { get; private set; }

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
    /// Called when an enemy dies - may spawn a pickup.
    /// </summary>
    public void TrySpawnPickupAtPosition(Vector3 position)
    {
        if (Random.value > spawnChanceOnKill) return;
        
        float roll = Random.value;
        GameObject prefab = null;
        
        if (roll < healthSpawnWeight)
        {
            prefab = healthPickupPrefab;
        }
        else if (roll < healthSpawnWeight + bombSpawnWeight)
        {
            prefab = bombPickupPrefab;
        }
        else
        {
            // Random power-up
            if (Random.value > 0.5f && speedPickupPrefab != null)
                prefab = speedPickupPrefab;
            else if (damagePickupPrefab != null)
                prefab = damagePickupPrefab;
        }
        
        if (prefab != null)
        {
            SpawnPickup(prefab, position);
        }
    }

    /// <summary>
    /// Spawns a specific pickup at a position.
    /// </summary>
    public void SpawnPickup(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;
        
        // Offset above surface
        Vector3 normal = position.normalized;
        Vector3 spawnPos = position + normal * spawnHeightOffset;
        
        GameObject pickup = Instantiate(prefab, spawnPos, Quaternion.identity);
        pickup.transform.up = normal;
        
        Debug.Log($"[PickupSpawner] Spawned {prefab.name} at {position}");
    }

    /// <summary>
    /// Creates a default pickup if no prefab exists.
    /// </summary>
    public void SpawnDefaultPickup(Vector3 position, Pickup.PickupType type)
    {
        Vector3 normal = position.normalized;
        Vector3 spawnPos = position + normal * spawnHeightOffset;
        
        // Create primitive
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.transform.position = spawnPos;
        pickup.transform.localScale = Vector3.one * 1.5f;
        pickup.transform.up = normal;
        
        // Add pickup component
        Pickup pickupComp = pickup.AddComponent<Pickup>();
        
        // Make it a trigger
        pickup.GetComponent<Collider>().isTrigger = true;
        
        // Add rigidbody
        Rigidbody rb = pickup.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        
        pickup.name = $"Pickup_{type}";
    }
}
