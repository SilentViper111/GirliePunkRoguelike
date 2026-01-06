using UnityEngine;

/// <summary>
/// Emergency spawn fix - moves player to correct position on sphere surface.
/// Attach to Player. Uses Awake() to run BEFORE any Start() methods.
/// </summary>
public class PlayerSpawnFix : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float worldRadius = 500f;
    [SerializeField] private float spawnHeight = 3f;
    
    private void Awake()
    {
        Debug.Log($"[PlayerSpawnFix] Awake() called. Current position: {transform.position}");
        
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
        
        Debug.Log($"[PlayerSpawnFix] Moved player to {targetPos} (radius: {targetPos.magnitude})");
    }
    
    private void Start()
    {
        // Double-check position in Start as well
        Debug.Log($"[PlayerSpawnFix] Start() - position is now: {transform.position}");
    }
}
