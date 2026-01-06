using UnityEngine;

/// <summary>
/// Trash projectile - rapid fire, low damage, no retrieval.
/// Now with proper damage dealing to IDamageable targets.
/// 
/// Reference: KB Section III - Trash vs Bomb differentiation
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class TrashProjectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float launchSpeed = 25f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 3f;

    [Header("Visual")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Color trailColor = Color.magenta;

    private Rigidbody _rb;
    private float _spawnTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;

        // Set to Projectile_Trash layer
        int layer = LayerMask.NameToLayer("Projectile_Trash");
        if (layer >= 0)
            gameObject.layer = layer;
            
        // Setup trail if exists
        if (trailRenderer != null)
        {
            trailRenderer.startColor = trailColor;
            trailRenderer.endColor = Color.clear;
        }
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
        // Get contact info for VFX
        Vector3 hitPoint = collision.GetContact(0).point;
        Vector3 hitNormal = collision.GetContact(0).normal;
        
        // Try to damage target
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage, hitPoint, hitNormal);
            Debug.Log($"[Trash] Hit {collision.gameObject.name} for {damage} damage!");
        }
        else
        {
            // Hit environment - spawn sparks
            if (VFXManager.Instance != null)
                VFXManager.Instance.SpawnImpactSparks(hitPoint, hitNormal);
        }

        // Destroy on any collision
        Destroy(gameObject);
    }
}
