using UnityEngine;

/// <summary>
/// Tank enemy variant - slow but high health and damage.
/// 
/// Reference: KB Section V - Enemy Types
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class TankEnemyController : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 4f;
    
    [Header("Combat")]
    [SerializeField] private float maxHealth = 150f;
    [SerializeField] private float currentHealth = 150f;
    [SerializeField] private float contactDamage = 35f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float damageReduction = 0.3f; // Takes 30% less damage
    [SerializeField] private int scoreValue = 300;
    
    [Header("Visual")]
    [SerializeField] private Color tankColor = new Color(0.5f, 0f, 0.5f);
    
    // IDamageable
    public bool IsAlive => currentHealth > 0;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    
    // Internal
    private Rigidbody _rb;
    private Transform _player;
    private Vector3 _currentUp;
    private float _lastDamageTime;
    private bool _isDead;
    private Renderer _renderer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        _rb.mass = 5f; // Heavy
        
        currentHealth = maxHealth;
        _currentUp = transform.position.normalized;
        
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
            _renderer.material.color = tankColor;
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
        _rb.AddForce(gravityDir * 25f, ForceMode.Acceleration);
        _currentUp = -gravityDir;
    }

    private void UpdateMovement()
    {
        Vector3 direction = (_player.position - transform.position).normalized;
        direction = Vector3.ProjectOnPlane(direction, _currentUp).normalized;
        
        Vector3 targetVelocity = direction * moveSpeed;
        Vector3 currentHorizontal = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        Vector3 vertical = Vector3.Project(_rb.linearVelocity, _currentUp);
        
        _rb.linearVelocity = Vector3.Lerp(currentHorizontal, targetVelocity, Time.fixedDeltaTime * 3f) + vertical;
    }

    private void UpdateRotation()
    {
        Vector3 toPlayer = Vector3.ProjectOnPlane(_player.position - transform.position, _currentUp).normalized;
        if (toPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(toPlayer, _currentUp);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!IsAlive) return;
        if (Time.time < _lastDamageTime + damageInterval) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                _lastDamageTime = Time.time;
                
                if (ScreenShake.Instance != null)
                    ScreenShake.Instance.ShakeMedium();
            }
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
        
        // Apply damage reduction
        amount *= (1f - damageReduction);
        currentHealth -= amount;
        
        // Brief color flash
        StartCoroutine(FlashDamage());
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHitEnemy();
        
        // Combo system
        if (ComboSystem.Instance != null)
            ComboSystem.Instance.RegisterHit();
        
        if (currentHealth <= 0)
            Die();
    }

    private System.Collections.IEnumerator FlashDamage()
    {
        if (_renderer != null)
        {
            _renderer.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            if (_renderer != null && !_isDead)
                _renderer.material.color = tankColor;
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
            
        // Combo
        if (ComboSystem.Instance != null)
            ComboSystem.Instance.RegisterKill();
            
        // Achievement
        if (AchievementSystem.Instance != null)
            AchievementSystem.Instance.ReportProgress(AchievementSystem.AchievementType.KillCount, 1);
        
        // Pickup drop (higher chance for tanks)
        if (PickupSpawner.Instance != null)
        {
            if (Random.value < 0.3f) // 30% drop chance
                PickupSpawner.Instance.TrySpawnPickupAtPosition(transform.position);
        }
        
        // Tetris crumble
        TetrisCrumbleEffect.ApplyCrumbleEffect(gameObject);
    }
}
