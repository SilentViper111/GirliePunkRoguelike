using UnityEngine;

/// <summary>
/// Trash projectile - rapid fire, low damage, no retrieval.
/// 
/// Reference: KB Section III - Trash vs Bomb differentiation
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class TrashProjectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float launchSpeed = 25f;
    [SerializeField] private float damage = 5f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual")]
    [SerializeField] private TrailRenderer trailRenderer;

    private Rigidbody _rb;
    private float _spawnTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;

        // Set to Projectile_Trash layer
        gameObject.layer = LayerMask.NameToLayer("Projectile_Trash");
    }

    private void Start()
    {
        _spawnTime = Time.time;
        _rb.AddForce(transform.forward * launchSpeed, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        // Apply gravity toward world center
        Vector3 gravityDir = -transform.position.normalized;
        _rb.AddForce(gravityDir * 15f, ForceMode.Acceleration);

        // Lifetime check
        if (Time.time > _spawnTime + lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check for enemy hit
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            // TODO: Apply damage
            Debug.Log($"[Trash] Hit enemy: {collision.gameObject.name}");
        }

        // Destroy on any collision
        Destroy(gameObject);
    }
}
