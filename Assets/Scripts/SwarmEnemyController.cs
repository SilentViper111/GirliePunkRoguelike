using UnityEngine;

/// <summary>
/// Swarm minion - weak but spawns in groups.
/// Used by boss and special biomes.
/// 
/// Reference: KB Section V - Enemy Types
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SwarmEnemyController : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float separationDistance = 3f;
    [SerializeField] private float separationStrength = 2f;
    
    [Header("Combat")]
    [SerializeField] private float maxHealth = 15f;
    [SerializeField] private float currentHealth = 15f;
    [SerializeField] private float contactDamage = 5f;
    [SerializeField] private int scoreValue = 25;
    
    [Header("Visual")]
    [SerializeField] private Color swarmColor = Color.green;
    
    // IDamageable
    public bool IsAlive => currentHealth > 0;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    
    // Internal
    private Rigidbody _rb;
    private Transform _player;
    private Vector3 _currentUp;
    private bool _isDead;
    private Renderer _renderer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        _rb.mass = 0.3f; // Light
        
        currentHealth = maxHealth;
        _currentUp = transform.position.normalized;
        
        // Smaller scale
        transform.localScale = Vector3.one * 0.7f;
        
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
            _renderer.material.color = swarmColor;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
    }

    private void FixedUpdate()
    {
        if (_isDead || _player == null) return;
        
        ApplyGravity();
        UpdateMovement();
        UpdateRotation();
    }

    private void ApplyGravity()
    {
        Vector3 gravityDir = -transform.position.normalized;
        _rb.AddForce(gravityDir * 20f, ForceMode.Acceleration);
        _currentUp = -gravityDir;
    }

    private void UpdateMovement()
    {
        // Base direction toward player
        Vector3 direction = (_player.position - transform.position).normalized;
        direction = Vector3.ProjectOnPlane(direction, _currentUp).normalized;
        
        // Add separation from other swarm members
        Collider[] nearby = Physics.OverlapSphere(transform.position, separationDistance);
        Vector3 separation = Vector3.zero;
        int count = 0;
        
        foreach (var col in nearby)
        {
            SwarmEnemyController other = col.GetComponent<SwarmEnemyController>();
            if (other != null && other != this && other.IsAlive)
            {
                Vector3 away = transform.position - other.transform.position;
                separation += away.normalized / away.magnitude;
                count++;
            }
        }
        
        if (count > 0)
        {
            separation /= count;
            separation = Vector3.ProjectOnPlane(separation, _currentUp);
            direction = (direction + separation * separationStrength).normalized;
        }
        
        Vector3 targetVelocity = direction * moveSpeed;
        Vector3 currentHorizontal = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        Vector3 vertical = Vector3.Project(_rb.linearVelocity, _currentUp);
        
        _rb.linearVelocity = Vector3.Lerp(currentHorizontal, targetVelocity, Time.fixedDeltaTime * 8f) + vertical;
    }

    private void UpdateRotation()
    {
        Vector3 lookDir = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir.normalized, _currentUp);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsAlive) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
            }
            
            // Self-destruct on contact
            currentHealth = 0;
            Die();
        }
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
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHitEnemy();
        
        // Combo
        if (ComboSystem.Instance != null)
            ComboSystem.Instance.RegisterHit();
        
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        _isDead = true;
        
        if (VFXManager.Instance != null)
            VFXManager.Instance.SpawnImpactSparks(transform.position, _currentUp);
        
        // Score
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
            spawner.OnEnemyKilled(gameObject, scoreValue);
            
        // Combo
        if (ComboSystem.Instance != null)
            ComboSystem.Instance.RegisterKill();
            
        // Achievement
        if (AchievementSystem.Instance != null)
            AchievementSystem.Instance.ReportProgress(AchievementSystem.AchievementType.KillCount, 1);
        
        Destroy(gameObject);
    }
}
