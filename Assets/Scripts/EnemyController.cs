using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemy AI controller with wander/chase/attack behavior on spherical world.
/// Implements IDamageable for unified damage system.
/// 
/// Reference: KB Section V, Implementation Plan Phase 4
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float waypointReachDistance = 2f;
    [SerializeField] private float chaseRange = 30f;
    [SerializeField] private float attackRange = 5f;

    [Header("Combat")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float currentHealth = 50f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int scoreValue = 100;

    [Header("State")]
    [SerializeField] private EnemyState currentState = EnemyState.Wandering;

    public enum EnemyState { Idle, Wandering, Chasing, Attacking, Dead }

    // IDamageable implementation
    public bool IsAlive => currentHealth > 0;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    // Internal
    private Rigidbody _rb;
    private Vector3 _currentWaypoint;
    private Vector3 _currentUp;
    private float _lastAttackTime;
    private Transform _player;
    private Renderer _renderer;
    private Color _originalColor;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        _currentUp = transform.position.normalized;
        currentHealth = maxHealth;
        
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
            _originalColor = _renderer.material.color;
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
            case EnemyState.Attacking:
                UpdateAttack();
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
        
        if (distToPlayer < attackRange)
        {
            currentState = EnemyState.Attacking;
        }
        else if (distToPlayer < chaseRange)
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
        
        Vector3 targetVelocity = direction * moveSpeed * 1.5f;
        Vector3 currentHorizontal = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        Vector3 vertical = Vector3.Project(_rb.linearVelocity, _currentUp);
        
        _rb.linearVelocity = Vector3.Lerp(currentHorizontal, targetVelocity, Time.fixedDeltaTime * 5f) + vertical;
    }

    private void UpdateAttack()
    {
        if (_player == null) return;
        
        // Stop moving when attacking
        Vector3 vertical = Vector3.Project(_rb.linearVelocity, _currentUp);
        _rb.linearVelocity = vertical;
        
        // Attack on cooldown
        if (Time.time >= _lastAttackTime + attackCooldown)
        {
            PerformAttack();
            _lastAttackTime = Time.time;
        }
    }

    private void PerformAttack()
    {
        if (_player == null) return;
        
        // Check if player is still in range
        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist > attackRange * 1.5f) return;
        
        // Deal damage to player
        PlayerHealth playerHealth = _player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            
            // Play effects
            if (VFXManager.Instance != null)
                VFXManager.Instance.SpawnPlayerHit(_player.position);
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayHitPlayer();
                
            Debug.Log($"[Enemy] Attacked player for {damage} damage!");
        }
    }

    private void UpdateRotation()
    {
        Vector3 velocity = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        if (velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized, _currentUp);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
        else if (_player != null && currentState == EnemyState.Attacking)
        {
            // Face player when attacking
            Vector3 toPlayer = Vector3.ProjectOnPlane(_player.position - transform.position, _currentUp).normalized;
            if (toPlayer.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(toPlayer, _currentUp);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
            }
        }
        else
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, _currentUp) * transform.rotation;
        }
    }

    private void SetNewWaypoint()
    {
        Vector3 right = Vector3.Cross(_currentUp, Vector3.forward).normalized;
        if (right.sqrMagnitude < 0.01f)
            right = Vector3.Cross(_currentUp, Vector3.right).normalized;
        Vector3 forward = Vector3.Cross(right, _currentUp).normalized;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(wanderRadius * 0.5f, wanderRadius);
        
        Vector3 offset = (right * Mathf.Cos(angle) + forward * Mathf.Sin(angle)) * distance;
        _currentWaypoint = (transform.position + offset).normalized * transform.position.magnitude;
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        TakeDamage(amount);
        
        if (VFXManager.Instance != null)
            VFXManager.Instance.SpawnImpactSparks(hitPoint, hitNormal);
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;
        
        currentHealth -= amount;
        
        // Flash red
        StartCoroutine(FlashDamage());
        
        // Audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHitEnemy();
        
        Debug.Log($"[Enemy] Took {amount} damage, health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashDamage()
    {
        if (_renderer != null)
        {
            _renderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (_renderer != null)
                _renderer.material.color = _originalColor;
        }
    }

    private void Die()
    {
        currentState = EnemyState.Dead;
        
        // VFX
        if (VFXManager.Instance != null)
            VFXManager.Instance.SpawnEnemyDeath(transform.position);
            
        // Audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEnemyDeath();
            
        // Score
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
            spawner.OnEnemyKilled(gameObject, scoreValue);
            
        Debug.Log($"[Enemy] Died! Worth {scoreValue} points");
        
        // Disable physics and destroy
        _rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 0.5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Contact damage to player
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null && Time.time >= _lastAttackTime + attackCooldown)
            {
                playerHealth.TakeDamage(damage * 0.5f); // Contact does less damage
                _lastAttackTime = Time.time;
            }
        }
    }
}
