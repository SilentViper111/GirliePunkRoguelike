using UnityEngine;

/// <summary>
/// Explosive enemy that detonates on death, damaging nearby targets.
/// 
/// Reference: KB Section V - Enemy Types
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ExploderEnemyController : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float chaseRange = 40f;
    [SerializeField] private float detonationRange = 5f;
    
    [Header("Combat")]
    [SerializeField] private float maxHealth = 25f;
    [SerializeField] private float currentHealth = 25f;
    [SerializeField] private float explosionDamage = 40f;
    [SerializeField] private float explosionRadius = 12f;
    [SerializeField] private int scoreValue = 250;
    
    [Header("Detonation")]
    [SerializeField] private float fuseTime = 1f;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color explosionColor = Color.red;
    
    public enum ExploderState { Chasing, Fusing, Exploding, Dead }
    
    [Header("State")]
    [SerializeField] private ExploderState currentState = ExploderState.Chasing;
    
    // IDamageable
    public bool IsAlive => currentHealth > 0;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    
    // Internal
    private Rigidbody _rb;
    private Transform _player;
    private Vector3 _currentUp;
    private float _fuseStartTime;
    private bool _isDead;
    private Renderer _renderer;
    private Color _originalColor;
    private float _pulseSpeed = 5f;

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
            _originalColor = new Color(1f, 0.5f, 0f); // Orange tint
            _renderer.material.color = _originalColor;
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
            case ExploderState.Chasing:
                UpdateChase();
                
                if (distToPlayer < detonationRange)
                {
                    StartFusing();
                }
                break;
                
            case ExploderState.Fusing:
                // Stop moving
                Vector3 vertical = Vector3.Project(_rb.linearVelocity, _currentUp);
                _rb.linearVelocity = vertical;
                
                // Pulse color
                float pulse = Mathf.PingPong(Time.time * _pulseSpeed, 1f);
                if (_renderer != null)
                    _renderer.material.color = Color.Lerp(warningColor, explosionColor, pulse);
                
                // Check fuse time
                if (Time.time > _fuseStartTime + fuseTime)
                {
                    Explode();
                }
                break;
        }
    }

    private void UpdateChase()
    {
        if (_player == null) return;
        
        Vector3 direction = (_player.position - transform.position).normalized;
        direction = Vector3.ProjectOnPlane(direction, _currentUp).normalized;
        
        Vector3 targetVelocity = direction * moveSpeed;
        Vector3 currentHorizontal = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        Vector3 vertical = Vector3.Project(_rb.linearVelocity, _currentUp);
        
        _rb.linearVelocity = Vector3.Lerp(currentHorizontal, targetVelocity, Time.fixedDeltaTime * 5f) + vertical;
    }

    private void StartFusing()
    {
        currentState = ExploderState.Fusing;
        _fuseStartTime = Time.time;
        _pulseSpeed = 10f; // Fast pulsing during fuse
        
        Debug.Log("[Exploder] FUSING!");
    }

    private void Explode()
    {
        if (_isDead) return;
        _isDead = true;
        currentState = ExploderState.Exploding;
        
        // VFX
        if (VFXManager.Instance != null)
            VFXManager.Instance.SpawnBombExplosion(transform.position, explosionRadius / 10f);
            
        // Audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayShootBomb(); // Reuse bomb sound
            
        // Screen shake
        if (ScreenShake.Instance != null)
            ScreenShake.Instance.ShakeLarge();
        
        // AOE damage
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            // Damage player
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                float falloff = 1f - (dist / explosionRadius);
                float actualDamage = explosionDamage * Mathf.Max(0.2f, falloff);
                
                playerHealth.TakeDamage(actualDamage);
                Debug.Log($"[Exploder] Hit player for {actualDamage:F1} damage!");
            }
            
            // Damage other enemies
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null && hit.gameObject != gameObject)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                float falloff = 1f - (dist / explosionRadius);
                float actualDamage = explosionDamage * 0.5f * Mathf.Max(0.2f, falloff);
                
                damageable.TakeDamage(actualDamage);
            }
        }
        
        Debug.Log("[Exploder] EXPLODED!");
        
        // Tetris crumble
        TetrisCrumbleEffect.ApplyCrumbleEffect(gameObject);
    }

    private void UpdateRotation()
    {
        if (currentState == ExploderState.Fusing) return;
        
        Vector3 lookDir = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir.normalized, _currentUp);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
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
        
        // Getting hit while fusing reduces fuse time
        if (currentState == ExploderState.Fusing)
        {
            _fuseStartTime -= 0.2f;
        }
        
        if (currentHealth <= 0)
        {
            // Score
            EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
            if (spawner != null)
                spawner.OnEnemyKilled(gameObject, scoreValue);
                
            Explode();
        }
    }
}
