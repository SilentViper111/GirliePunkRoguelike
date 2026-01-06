using UnityEngine;

/// <summary>
/// Bombshell state machine controller.
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
    [SerializeField] private float damage = 25f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual References")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private ParticleSystem beaconParticles;
    [SerializeField] private Light beaconLight;

    [Header("Current State")]
    [SerializeField] private BombState currentState = BombState.Fired;

    // Internal
    private Rigidbody _rb;
    private SphereCollider _collider;
    private float _spawnTime;
    private PlayerController _player;

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
        gameObject.layer = LayerMask.NameToLayer("Projectile_Bomb");
        
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
        gameObject.layer = LayerMask.NameToLayer("Pickup_Bomb");
        
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

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
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
        
        // TODO: Play pickup sound and VFX
        
        Destroy(gameObject);
    }
    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        if (currentState != BombState.Fired) return;

        // Check for enemy collision
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            // TODO: Apply damage to enemy
            Debug.Log($"[Bombshell] Hit enemy: {collision.gameObject.name}");
        }
    }
}
