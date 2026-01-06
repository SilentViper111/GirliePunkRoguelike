using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bombshell state machine controller with AOE explosion damage.
/// Implements the 4-state lifecycle: Fired → Decaying → Grounded → Retrieved
/// 
/// Reference: KB Section III.B - Bombshell State Machine
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class BombshellController : MonoBehaviour
{
    public enum BombState { Fired, Decaying, Grounded, Retrieved }

    [Header("Launch Settings")]
    [SerializeField] private float launchSpeed = 30f;
    [SerializeField] private float velocityDecayThreshold = 2f;
    [SerializeField] private float groundedVelocityThreshold = 0.1f;
    [SerializeField] private float maxFlightTime = 3f;

    [Header("Physics Settings")]
    [SerializeField] private float bounciness = 0.8f;
    [SerializeField] private float decayDrag = 5f;

    [Header("Damage Settings")]
    [SerializeField] private float impactDamage = 25f;
    [SerializeField] private float explosionDamage = 50f;
    [SerializeField] private float explosionRadius = 15f;
    [SerializeField] private bool explodeOnImpact = true;

    [Header("Visual References")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private ParticleSystem beaconParticles;
    [SerializeField] private Light beaconLight;
    [SerializeField] private Color trailStartColor = Color.magenta;
    [SerializeField] private Color trailEndColor = Color.cyan;

    [Header("Current State")]
    [SerializeField] private BombState currentState = BombState.Fired;

    // Internal
    private Rigidbody _rb;
    private SphereCollider _collider;
    private float _spawnTime;
    private PlayerController _player;
    private bool _hasExploded;
    private HashSet<Collider> _hitEnemies = new HashSet<Collider>();

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<SphereCollider>();
        _player = FindFirstObjectByType<PlayerController>();

        // Configure physics material for bouncing
        PhysicsMaterial bounceMat = new PhysicsMaterial("BombBounce");
        bounceMat.bounciness = bounciness;
        bounceMat.frictionCombine = PhysicsMaterialCombine.Minimum;
        bounceMat.bounceCombine = PhysicsMaterialCombine.Maximum;
        _collider.material = bounceMat;
        
        // Setup trail
        if (trailRenderer != null)
        {
            trailRenderer.startColor = trailStartColor;
            trailRenderer.endColor = trailEndColor;
        }
    }

    private void Start()
    {
        _spawnTime = Time.time;
        EnterFiredState();
    }

    private void FixedUpdate()
    {
        // Apply gravity toward world center
        Vector3 gravityDir = -transform.position.normalized;
        _rb.AddForce(gravityDir * 20f, ForceMode.Acceleration);

        // State machine updates
        switch (currentState)
        {
            case BombState.Fired:
                UpdateFiredState();
                break;
            case BombState.Decaying:
                UpdateDecayingState();
                break;
            case BombState.Grounded:
                // No physics updates needed
                break;
        }
    }

    #region State: Fired
    private void EnterFiredState()
    {
        currentState = BombState.Fired;
        
        _rb.isKinematic = false;
        _collider.isTrigger = false;
        _rb.linearDamping = 0f;
        
        // Set to Projectile_Bomb layer
        int bombLayer = LayerMask.NameToLayer("Projectile_Bomb");
        if (bombLayer >= 0)
            gameObject.layer = bombLayer;
        
        // Launch forward
        _rb.AddForce(transform.forward * launchSpeed, ForceMode.Impulse);
        
        // Visuals
        if (trailRenderer) trailRenderer.enabled = true;
        if (beaconParticles) beaconParticles.Stop();
        if (beaconLight) beaconLight.enabled = false;

        Debug.Log("[Bombshell] State: FIRED");
    }

    private void UpdateFiredState()
    {
        float elapsed = Time.time - _spawnTime;
        float velocity = _rb.linearVelocity.magnitude;

        // Transition conditions
        if (velocity < velocityDecayThreshold || elapsed > maxFlightTime)
        {
            EnterDecayingState();
        }
    }
    #endregion

    #region State: Decaying
    private void EnterDecayingState()
    {
        currentState = BombState.Decaying;
        _rb.linearDamping = decayDrag;
        
        Debug.Log("[Bombshell] State: DECAYING");
    }

    private void UpdateDecayingState()
    {
        if (_rb.linearVelocity.magnitude < groundedVelocityThreshold)
        {
            EnterGroundedState();
        }
    }
    #endregion

    #region State: Grounded
    private void EnterGroundedState()
    {
        currentState = BombState.Grounded;
        
        // Switch to pickup layer
        int pickupLayer = LayerMask.NameToLayer("Pickup_Bomb");
        if (pickupLayer >= 0)
            gameObject.layer = pickupLayer;
        
        // Lock in place
        _rb.isKinematic = true;
        _collider.isTrigger = true;
        
        // Visuals
        if (trailRenderer) trailRenderer.enabled = false;
        if (beaconParticles) beaconParticles.Play();
        if (beaconLight) beaconLight.enabled = true;

        Debug.Log("[Bombshell] State: GROUNDED (Ready for pickup)");
    }
    #endregion

    #region State: Retrieved
    private void OnTriggerEnter(Collider other)
    {
        if (currentState != BombState.Grounded) return;

        int playerLayer = LayerMask.NameToLayer("Player");
        if (other.gameObject.layer == playerLayer || other.CompareTag("Player"))
        {
            RetrieveBomb(other.GetComponent<PlayerController>());
        }
    }

    private void RetrieveBomb(PlayerController player)
    {
        currentState = BombState.Retrieved;
        
        if (player != null)
        {
            player.RetrieveBomb();
        }
        else if (_player != null)
        {
            _player.RetrieveBomb();
        }

        Debug.Log("[Bombshell] State: RETRIEVED");
        
        // Audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBombPickup();
        
        Destroy(gameObject);
    }
    #endregion

    #region Damage & Explosion
    private void OnCollisionEnter(Collision collision)
    {
        if (currentState != BombState.Fired) return;
        
        Vector3 hitPoint = collision.GetContact(0).point;
        Vector3 hitNormal = collision.GetContact(0).normal;

        // Check if we hit an enemy
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null && !_hitEnemies.Contains(collision.collider))
        {
            _hitEnemies.Add(collision.collider);
            damageable.TakeDamage(impactDamage, hitPoint, hitNormal);
            Debug.Log($"[Bombshell] Direct hit on {collision.gameObject.name} for {impactDamage} damage!");
            
            // Explode on enemy hit
            if (explodeOnImpact)
            {
                Explode();
                return;
            }
        }
        
        // Spawn impact VFX
        if (VFXManager.Instance != null)
            VFXManager.Instance.SpawnImpactSparks(hitPoint, hitNormal);
    }

    /// <summary>
    /// Triggers AOE explosion, damaging all enemies in radius.
    /// </summary>
    public void Explode()
    {
        if (_hasExploded) return;
        _hasExploded = true;
        
        // VFX
        if (VFXManager.Instance != null)
            VFXManager.Instance.SpawnBombExplosion(transform.position, explosionRadius / 10f);
            
        // Audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayShootBomb();
        
        // Find all enemies in radius
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (_hitEnemies.Contains(hit)) continue; // Already damaged
            
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null && hit.GetComponent<PlayerHealth>() == null) // Don't damage player
            {
                // Damage falloff by distance
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                float falloff = 1f - (dist / explosionRadius);
                float actualDamage = explosionDamage * Mathf.Max(0.2f, falloff);
                
                damageable.TakeDamage(actualDamage);
                Debug.Log($"[Bombshell] Explosion hit {hit.gameObject.name} for {actualDamage:F1} damage!");
            }
        }
        
        Debug.Log($"[Bombshell] EXPLODED! Radius: {explosionRadius}");
        Destroy(gameObject);
    }
    #endregion
}
