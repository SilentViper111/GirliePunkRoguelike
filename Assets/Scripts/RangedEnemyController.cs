using UnityEngine;

/// <summary>
/// Ranged enemy variant that shoots projectiles at the player.
/// 
/// Reference: KB Section V - Enemy Types
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RangedEnemyController : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float preferredDistance = 25f;
    [SerializeField] private float retreatDistance = 10f;
    
    [Header("Combat")]
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private float currentHealth = 30f;
    [SerializeField] private float projectileDamage = 8f;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private int scoreValue = 150;
    
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    
    // IDamageable
    public bool IsAlive => currentHealth > 0;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    
    // Internal
    private Rigidbody _rb;
    private Transform _player;
    private float _lastFireTime;
    private Vector3 _currentUp;
    private Renderer _renderer;
    private Color _originalColor;
    private bool _isDead;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        currentHealth = maxHealth;
        _currentUp = transform.position.normalized;
        
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
            _originalColor = _renderer.material.color;
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

    private void Update()
    {
        if (_isDead || _player == null) return;
        
        // Fire at player if in range
        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist < preferredDistance * 1.5f && Time.time > _lastFireTime + fireRate)
        {
            Fire();
            _lastFireTime = Time.time;
        }
    }

    private void ApplyGravity()
    {
        Vector3 gravityDir = -transform.position.normalized;
        _rb.AddForce(gravityDir * 20f, ForceMode.Acceleration);
        _currentUp = -gravityDir;
    }

    private void UpdateMovement()
    {
        float dist = Vector3.Distance(transform.position, _player.position);
        Vector3 direction = Vector3.zero;
        
        if (dist < retreatDistance)
        {
            // Too close, retreat
            direction = (transform.position - _player.position).normalized;
        }
        else if (dist > preferredDistance)
        {
            // Too far, approach
            direction = (_player.position - transform.position).normalized;
        }
        
        direction = Vector3.ProjectOnPlane(direction, _currentUp).normalized;
        
        Vector3 targetVelocity = direction * moveSpeed;
        Vector3 currentHorizontal = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        Vector3 vertical = Vector3.Project(_rb.linearVelocity, _currentUp);
        
        _rb.linearVelocity = Vector3.Lerp(currentHorizontal, targetVelocity, Time.fixedDeltaTime * 5f) + vertical;
    }

    private void UpdateRotation()
    {
        // Always face player
        Vector3 toPlayer = Vector3.ProjectOnPlane(_player.position - transform.position, _currentUp).normalized;
        if (toPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(toPlayer, _currentUp);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
    }

    private void Fire()
    {
        // Create projectile
        Vector3 firePos = transform.position + transform.forward * 1.5f;
        
        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, firePos, transform.rotation);
            Rigidbody projRb = proj.GetComponent<Rigidbody>();
            if (projRb != null)
            {
                projRb.linearVelocity = transform.forward * projectileSpeed;
            }
        }
        else
        {
            // Create default projectile
            GameObject proj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            proj.transform.position = firePos;
            proj.transform.localScale = Vector3.one * 0.5f;
            proj.GetComponent<Renderer>().material.color = Color.red;
            
            Rigidbody projRb = proj.AddComponent<Rigidbody>();
            projRb.useGravity = false;
            projRb.linearVelocity = transform.forward * projectileSpeed;
            
            EnemyProjectile ep = proj.AddComponent<EnemyProjectile>();
            
            Destroy(proj, 3f);
        }
        
        Debug.Log("[RangedEnemy] Fired projectile!");
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
        StartCoroutine(FlashDamage());
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHitEnemy();
        
        if (currentHealth <= 0)
            Die();
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
        _isDead = true;
        
        if (VFXManager.Instance != null)
            VFXManager.Instance.SpawnEnemyDeath(transform.position);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEnemyDeath();
        
        // Score
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
            spawner.OnEnemyKilled(gameObject, scoreValue);
            
        // Pickup
        if (PickupSpawner.Instance != null)
            PickupSpawner.Instance.TrySpawnPickupAtPosition(transform.position);
        
        _rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 0.5f);
    }
}
