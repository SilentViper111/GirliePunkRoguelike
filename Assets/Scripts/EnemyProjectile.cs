using UnityEngine;

/// <summary>
/// Enemy projectile that damages the player on contact.
/// 
/// Reference: KB Section V - Enemy Combat
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;
    
    private float _spawnTime;

    private void Start()
    {
        _spawnTime = Time.time;
        
        // Make sure it has a collider
        if (GetComponent<Collider>() == null)
        {
            SphereCollider col = gameObject.AddComponent<SphereCollider>();
            col.radius = 0.5f;
        }
        
        // Set layer to avoid hitting other enemies
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0)
        {
            // Ignore collision with enemies
            Physics.IgnoreLayerCollision(gameObject.layer, enemyLayer, true);
        }
    }

    private void Update()
    {
        // Apply gravity toward world center
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 gravityDir = -transform.position.normalized;
            rb.AddForce(gravityDir * 10f, ForceMode.Acceleration);
        }
        
        // Lifetime
        if (Time.time > _spawnTime + lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check for player
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"[EnemyProjectile] Hit player for {damage} damage!");
            }
            
            // VFX
            if (VFXManager.Instance != null)
            {
                Vector3 hitPoint = collision.GetContact(0).point;
                Vector3 hitNormal = collision.GetContact(0).normal;
                VFXManager.Instance.SpawnImpactSparks(hitPoint, hitNormal);
            }
        }
        
        // Destroy on any collision
        Destroy(gameObject);
    }
}
