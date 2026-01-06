using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Boss enemy with multiple phases and attack patterns.
/// Spawns after every N waves.
/// 
/// Reference: KB Section V - Boss Mechanics
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BossController : MonoBehaviour, IDamageable
{
    public enum BossPhase { Phase1, Phase2, Phase3, Enraged, Dead }
    public enum AttackType { Charge, Slam, Spawn, Laser, Spiral }

    [Header("Stats")]
    [SerializeField] private float maxHealth = 500f;
    [SerializeField] private float currentHealth = 500f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private int scoreValue = 5000;

    [Header("Phase Thresholds")]
    [SerializeField] private float phase2Threshold = 0.7f;
    [SerializeField] private float phase3Threshold = 0.4f;
    [SerializeField] private float enragedThreshold = 0.15f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float chargeDamage = 30f;
    [SerializeField] private float slamDamage = 40f;
    [SerializeField] private float slamRadius = 20f;
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int minionsPerSpawn = 3;

    [Header("Current State")]
    [SerializeField] private BossPhase currentPhase = BossPhase.Phase1;
    [SerializeField] private AttackType nextAttack;

    // IDamageable
    public bool IsAlive => currentHealth > 0;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    // Internal
    private Rigidbody _rb;
    private Transform _player;
    private Vector3 _currentUp;
    private float _lastAttackTime;
    private bool _isAttacking;
    private Renderer _renderer;
    private Color _originalColor;
    private List<AttackType> _availableAttacks = new List<AttackType>();

    // Events
    public System.Action<BossPhase> OnPhaseChanged;
    public System.Action<float, float> OnHealthChanged;

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
            _originalColor = Color.magenta;
            _renderer.material.color = _originalColor;
        }

        UpdateAvailableAttacks();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        // Announce boss
        Debug.Log("[BOSS] BOSS SPAWNED!");
        if (ScreenShake.Instance != null)
            ScreenShake.Instance.ShakeLarge();
    }

    private void FixedUpdate()
    {
        if (!IsAlive || _player == null) return;

        ApplyGravity();
        
        if (!_isAttacking)
        {
            UpdateMovement();
            UpdateRotation();
        }
    }

    private void Update()
    {
        if (!IsAlive || _player == null) return;

        // Attack logic
        if (!_isAttacking && Time.time > _lastAttackTime + attackCooldown)
        {
            StartAttack();
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
        Vector3 direction = (_player.position - transform.position).normalized;
        direction = Vector3.ProjectOnPlane(direction, _currentUp).normalized;

        float speed = currentPhase == BossPhase.Enraged ? moveSpeed * 1.5f : moveSpeed;
        Vector3 targetVelocity = direction * speed;
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
        }
    }

    private void StartAttack()
    {
        if (_availableAttacks.Count == 0) return;

        nextAttack = _availableAttacks[Random.Range(0, _availableAttacks.Count)];
        _isAttacking = true;
        _lastAttackTime = Time.time;

        StartCoroutine(ExecuteAttack(nextAttack));
    }

    private System.Collections.IEnumerator ExecuteAttack(AttackType attack)
    {
        Debug.Log($"[BOSS] Executing attack: {attack}");

        switch (attack)
        {
            case AttackType.Charge:
                yield return ChargeAttack();
                break;
            case AttackType.Slam:
                yield return SlamAttack();
                break;
            case AttackType.Spawn:
                yield return SpawnMinions();
                break;
            case AttackType.Spiral:
                yield return SpiralAttack();
                break;
        }

        _isAttacking = false;
    }

    private System.Collections.IEnumerator ChargeAttack()
    {
        // Wind up
        if (_renderer != null) _renderer.material.color = Color.yellow;
        yield return new WaitForSeconds(0.5f);

        // Charge toward player
        Vector3 chargeDir = Vector3.ProjectOnPlane(_player.position - transform.position, _currentUp).normalized;
        float chargeSpeed = 25f;
        float chargeDuration = 1f;
        float elapsed = 0f;

        if (_renderer != null) _renderer.material.color = Color.red;

        while (elapsed < chargeDuration)
        {
            elapsed += Time.deltaTime;
            Vector3 vertical = Vector3.Project(_rb.linearVelocity, _currentUp);
            _rb.linearVelocity = chargeDir * chargeSpeed + vertical;
            yield return null;
        }

        if (_renderer != null) _renderer.material.color = _originalColor;
    }

    private System.Collections.IEnumerator SlamAttack()
    {
        // Jump up
        _rb.AddForce(_currentUp * 30f, ForceMode.Impulse);
        yield return new WaitForSeconds(0.8f);

        // Slam down
        _rb.AddForce(-_currentUp * 50f, ForceMode.Impulse);
        yield return new WaitForSeconds(0.3f);

        // AOE damage
        Collider[] hits = Physics.OverlapSphere(transform.position, slamRadius);
        foreach (var hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                float falloff = 1f - (dist / slamRadius);
                playerHealth.TakeDamage(slamDamage * Mathf.Max(0.3f, falloff));
            }
        }

        // VFX
        if (VFXManager.Instance != null)
            VFXManager.Instance.SpawnBombExplosion(transform.position, slamRadius / 10f);
        if (ScreenShake.Instance != null)
            ScreenShake.Instance.ShakeLarge();

        yield return new WaitForSeconds(0.5f);
    }

    private System.Collections.IEnumerator SpawnMinions()
    {
        if (minionPrefab == null) yield break;

        for (int i = 0; i < minionsPerSpawn; i++)
        {
            float angle = (360f / minionsPerSpawn) * i;
            Vector3 right = Vector3.Cross(_currentUp, transform.forward).normalized;
            Vector3 forward = Vector3.Cross(right, _currentUp).normalized;
            Vector3 offset = (right * Mathf.Cos(angle * Mathf.Deg2Rad) + forward * Mathf.Sin(angle * Mathf.Deg2Rad)) * 5f;

            Vector3 spawnPos = transform.position + offset;
            spawnPos = spawnPos.normalized * transform.position.magnitude;

            Instantiate(minionPrefab, spawnPos, Quaternion.LookRotation(offset, _currentUp));
            yield return new WaitForSeconds(0.2f);
        }
    }

    private System.Collections.IEnumerator SpiralAttack()
    {
        int projectiles = 12;
        int waves = 3;

        for (int w = 0; w < waves; w++)
        {
            for (int i = 0; i < projectiles; i++)
            {
                float angle = (360f / projectiles) * i + (w * 15f);
                Vector3 right = Vector3.Cross(_currentUp, transform.forward).normalized;
                Vector3 forward = Vector3.Cross(right, _currentUp).normalized;
                Vector3 dir = (right * Mathf.Cos(angle * Mathf.Deg2Rad) + forward * Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

                // Spawn projectile
                GameObject proj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                proj.transform.position = transform.position + dir * 3f;
                proj.transform.localScale = Vector3.one * 0.8f;
                proj.GetComponent<Renderer>().material.color = Color.magenta;
                proj.AddComponent<EnemyProjectile>();
                
                Rigidbody projRb = proj.AddComponent<Rigidbody>();
                projRb.useGravity = false;
                projRb.linearVelocity = dir * 15f;

                Destroy(proj, 5f);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void UpdateAvailableAttacks()
    {
        _availableAttacks.Clear();
        _availableAttacks.Add(AttackType.Charge);
        _availableAttacks.Add(AttackType.Slam);

        if (currentPhase >= BossPhase.Phase2)
        {
            _availableAttacks.Add(AttackType.Spawn);
        }

        if (currentPhase >= BossPhase.Phase3)
        {
            _availableAttacks.Add(AttackType.Spiral);
        }
    }

    private void UpdatePhase()
    {
        float healthPercent = currentHealth / maxHealth;
        BossPhase newPhase = currentPhase;

        if (healthPercent <= enragedThreshold)
            newPhase = BossPhase.Enraged;
        else if (healthPercent <= phase3Threshold)
            newPhase = BossPhase.Phase3;
        else if (healthPercent <= phase2Threshold)
            newPhase = BossPhase.Phase2;

        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            UpdateAvailableAttacks();
            OnPhaseChanged?.Invoke(currentPhase);
            Debug.Log($"[BOSS] Phase changed to: {currentPhase}");

            // Phase change effects
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
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHitEnemy();

        UpdatePhase();

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        currentPhase = BossPhase.Dead;
        Debug.Log("[BOSS] BOSS DEFEATED!");

        // Massive explosion
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.SpawnBombExplosion(transform.position, 3f);
            VFXManager.Instance.SpawnEnemyDeath(transform.position);
        }

        if (ScreenShake.Instance != null)
            ScreenShake.Instance.ShakeLarge();

        // Score
        GameUI ui = FindFirstObjectByType<GameUI>();
        if (ui != null)
            ui.AddScore(scoreValue);

        // Tetris crumble
        TetrisCrumbleEffect.ApplyCrumbleEffect(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && _isAttacking && nextAttack == AttackType.Charge)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(chargeDamage);
            }
        }
    }
}
