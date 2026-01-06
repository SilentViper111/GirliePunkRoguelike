using UnityEngine;

/// <summary>
/// Fast charger enemy that rushes at the player.
/// 
/// Reference: KB Section V - Enemy Types
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ChargerEnemyController : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    [SerializeField] private float wanderSpeed = 3f;
    [SerializeField] private float chargeSpeed = 15f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float chargeDistance = 40f;
    [SerializeField] private float chargeCooldown = 3f;
    [SerializeField] private float chargeWindupTime = 0.5f;
    
    [Header("Combat")]
    [SerializeField] private float maxHealth = 40f;
    [SerializeField] private float currentHealth = 40f;
    [SerializeField] private float chargeDamage = 25f;
    [SerializeField] private int scoreValue = 200;
    
    public enum ChargerState { Wandering, WindingUp, Charging, Stunned }
    
    [Header("State")]
    [SerializeField] private ChargerState currentState = ChargerState.Wandering;
    
    // IDamageable
    public bool IsAlive => currentHealth > 0;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    
    // Internal
    private Rigidbody _rb;
    private Transform _player;
    private Vector3 _currentUp;
    private Vector3 _chargeDirection;
    private float _lastChargeTime;
    private float _windupStartTime;
    private float _chargeStartTime;
    private bool _isDead;
    private Renderer _renderer;
    private Color _originalColor;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        currentHealth = maxHealth;
        _currentUp = transform.position.normalized;
        
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
            _renderer.material.color = Color.red; // Red tint for chargers
        }
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
        UpdateStateMachine();
        UpdateRotation();
    }

    private void ApplyGravity()
    {
        Vector3 gravityDir = -transform.position.normalized;
        _rb.AddForce(gravityDir * 20f, ForceMode.Acceleration);
        _currentUp = -gravityDir;
    }

    private void UpdateStateMachine()
    {
        float distToPlayer = Vector3.Distance(transform.position, _player.position);
        
        switch (currentState)
        {
            case ChargerState.Wandering:
                UpdateWander();
                
                // Check if should charge
                if (distToPlayer < chargeDistance && Time.time > _lastChargeTime + chargeCooldown)
                {
                    StartWindup();
                }
                break;
                
            case ChargerState.WindingUp:
                // Stop and face player
                _rb.linearVelocity = Vector3.Project(_rb.linearVelocity, _currentUp);
                
                if (Time.time > _windupStartTime + chargeWindupTime)
                {
                    StartCharge();
                }
                break;
                
            case ChargerState.Charging:
                UpdateCharge();
                break;
                
            case ChargerState.Stunned:
                // Gradually slow down
                Vector3 horizontal = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
                horizontal *= 0.95f;
                _rb.linearVelocity = horizontal + Vector3.Project(_rb.linearVelocity, _currentUp);
                
                if (horizontal.magnitude < 1f)
                {
                    currentState = ChargerState.Wandering;
                }
                break;
        }
    }

    private void UpdateWander()
    {
        // Simple wander toward player at slow speed
        Vector3 direction = (_player.position - transform.position).normalized;
        direction = Vector3.ProjectOnPlane(direction, _currentUp).normalized;
        
        Vector3 targetVelocity = direction * wanderSpeed;
        Vector3 currentHorizontal = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        Vector3 vertical = Vector3.Project(_rb.linearVelocity, _currentUp);
        
        _rb.linearVelocity = Vector3.Lerp(currentHorizontal, targetVelocity, Time.fixedDeltaTime * 5f) + vertical;
    }

    private void StartWindup()
    {
        currentState = ChargerState.WindingUp;
        _windupStartTime = Time.time;
        
        // Lock charge direction
        _chargeDirection = Vector3.ProjectOnPlane(_player.position - transform.position, _currentUp).normalized;
        
        // Visual feedback
        if (_renderer != null)
            _renderer.material.color = Color.yellow;
            
        Debug.Log("[Charger] Winding up!");
    }

    private void StartCharge()
    {
        currentState = ChargerState.Charging;
        _chargeStartTime = Time.time;
        _lastChargeTime = Time.time;
        
        // Visual feedback
        if (_renderer != null)
            _renderer.material.color = Color.red;
            
        // Apply charge velocity
        Vector3 vertical = Vector3.Project(_rb.linearVelocity, _currentUp);
        _rb.linearVelocity = _chargeDirection * chargeSpeed + vertical;
        
        Debug.Log("[Charger] CHARGING!");
    }

    private void UpdateCharge()
    {
        // Maintain charge speed
        Vector3 horizontal = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        if (horizontal.magnitude < chargeSpeed * 0.5f)
        {
            // Lost momentum, become stunned
            currentState = ChargerState.Stunned;
            
            if (_renderer != null)
                _renderer.material.color = Color.gray;
                
            Debug.Log("[Charger] Stunned!");
        }
        
        // Charge timeout
        if (Time.time > _chargeStartTime + 1.5f)
        {
            currentState = ChargerState.Stunned;
        }
    }

    private void UpdateRotation()
    {
        Vector3 lookDir = Vector3.zero;
        
        if (currentState == ChargerState.Charging)
        {
            lookDir = _chargeDirection;
        }
        else
        {
            lookDir = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        }
        
        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir.normalized, _currentUp);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Hit player while charging
        if (currentState == ChargerState.Charging && collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(chargeDamage);
                
                if (ScreenShake.Instance != null)
                    ScreenShake.Instance.ShakeLarge();
                    
                Debug.Log($"[Charger] Hit player for {chargeDamage} damage!");
            }
            
            currentState = ChargerState.Stunned;
        }
        // Hit wall while charging
        else if (currentState == ChargerState.Charging)
        {
            currentState = ChargerState.Stunned;
            
            if (ScreenShake.Instance != null)
                ScreenShake.Instance.ShakeMedium();
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
            Color prevColor = _renderer.material.color;
            _renderer.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            if (_renderer != null && !_isDead)
                _renderer.material.color = prevColor;
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
