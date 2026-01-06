using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Basic enemy AI that patrols within its assigned room.
/// Uses simple wander behavior on the sphere surface.
/// 
/// Reference: KB Section V, Implementation Plan Phase 4
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float waypointReachDistance = 2f;

    [Header("Combat")]
    [SerializeField] private float health = 50f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("State")]
    [SerializeField] private EnemyState currentState = EnemyState.Wandering;

    public enum EnemyState { Idle, Wandering, Chasing, Attacking, Dead }

    // Internal
    private Rigidbody _rb;
    private Vector3 _currentWaypoint;
    private Vector3 _currentUp;
    private float _lastAttackTime;
    private Transform _player;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        _currentUp = transform.position.normalized;
    }

    private void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }

        // Set initial waypoint
        SetNewWaypoint();
    }

    private void FixedUpdate()
    {
        if (currentState == EnemyState.Dead) return;

        ApplyGravity();
        UpdateState();
        
        switch (currentState)
        {
            case EnemyState.Wandering:
                UpdateWander();
                break;
            case EnemyState.Chasing:
                UpdateChase();
                break;
        }

        UpdateRotation();
    }

    private void ApplyGravity()
    {
        Vector3 gravityDir = -transform.position.normalized;
        _rb.AddForce(gravityDir * 20f, ForceMode.Acceleration);
        _currentUp = -gravityDir;
    }

    private void UpdateState()
    {
        if (_player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, _player.position);
        
        if (distToPlayer < 30f)
        {
            currentState = EnemyState.Chasing;
        }
        else
        {
            currentState = EnemyState.Wandering;
        }
    }

    private void UpdateWander()
    {
        // Check if reached waypoint
        float distToWaypoint = Vector3.Distance(transform.position, _currentWaypoint);
        if (distToWaypoint < waypointReachDistance)
        {
            SetNewWaypoint();
        }

        // Move toward waypoint
        Vector3 direction = (_currentWaypoint - transform.position).normalized;
        direction = Vector3.ProjectOnPlane(direction, _currentUp).normalized;
        
        Vector3 targetVelocity = direction * moveSpeed;
        Vector3 currentHorizontal = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        Vector3 vertical = Vector3.Project(_rb.linearVelocity, _currentUp);
        
        _rb.linearVelocity = Vector3.Lerp(currentHorizontal, targetVelocity, Time.fixedDeltaTime * 5f) + vertical;
    }

    private void UpdateChase()
    {
        if (_player == null) return;

        Vector3 direction = (_player.position - transform.position).normalized;
        direction = Vector3.ProjectOnPlane(direction, _currentUp).normalized;
        
        Vector3 targetVelocity = direction * moveSpeed * 1.5f; // Faster when chasing
        Vector3 currentHorizontal = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        Vector3 vertical = Vector3.Project(_rb.linearVelocity, _currentUp);
        
        _rb.linearVelocity = Vector3.Lerp(currentHorizontal, targetVelocity, Time.fixedDeltaTime * 5f) + vertical;
    }

    private void UpdateRotation()
    {
        Vector3 velocity = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        if (velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized, _currentUp);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
        else
        {
            // Just maintain up vector
            transform.rotation = Quaternion.FromToRotation(transform.up, _currentUp) * transform.rotation;
        }
    }

    private void SetNewWaypoint()
    {
        // Random direction on tangent plane
        Vector3 right = Vector3.Cross(_currentUp, Vector3.forward).normalized;
        if (right.sqrMagnitude < 0.01f)
            right = Vector3.Cross(_currentUp, Vector3.right).normalized;
        Vector3 forward = Vector3.Cross(right, _currentUp).normalized;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(wanderRadius * 0.5f, wanderRadius);
        
        Vector3 offset = (right * Mathf.Cos(angle) + forward * Mathf.Sin(angle)) * distance;
        _currentWaypoint = (transform.position + offset).normalized * transform.position.magnitude;
    }

    /// <summary>
    /// Takes damage from projectiles.
    /// </summary>
    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log($"[Enemy] Took {amount} damage, health: {health}");
        
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        currentState = EnemyState.Dead;
        Debug.Log("[Enemy] Died!");
        
        // TODO: Death VFX, drop loot
        Destroy(gameObject, 0.5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Deal damage to player
            // TODO: Implement player damage system
            Debug.Log("[Enemy] Hit player!");
        }
    }
}
