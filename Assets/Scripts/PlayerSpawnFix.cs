using UnityEngine;

/// <summary>
/// Emergency spawn fix - moves player to correct position on sphere surface.
/// Attach to Player. This runs in Start() to ensure player spawns correctly.
/// </summary>
public class PlayerSpawnFix : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float worldRadius = 500f;
    [SerializeField] private float spawnHeight = 3f;
    
    private void Start()
    {
        // Force player position to top of sphere
        Vector3 spawnDirection = Vector3.up;
        Vector3 targetPos = spawnDirection * (worldRadius + spawnHeight);
        
        transform.position = targetPos;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, spawnDirection);
        
        // Reset rigidbody velocity
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        Debug.Log($"[PlayerSpawnFix] Moved player to {targetPos} on sphere surface");
    }
}
