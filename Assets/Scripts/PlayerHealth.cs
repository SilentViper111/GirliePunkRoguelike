using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Player health and damage system.
/// Handles taking damage, healing, death, and invincibility frames.
/// 
/// Reference: KB Section VI.B - Player Implementation
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float invincibilityDuration = 1f;
    
    [Header("Visual Feedback")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private Renderer playerRenderer;
    
    [Header("Events")]
    public UnityEvent<float, float> OnHealthChanged; // current, max
    public UnityEvent OnDeath;
    public UnityEvent OnDamageTaken;
    public UnityEvent<float> onDamaged; // damage amount
    
    // Properties
    public bool IsAlive => currentHealth > 0;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercent => currentHealth / maxHealth;
    
    // Internal
    private bool _isInvincible;
    private float _invincibilityEndTime;
    private Material _originalMaterial;
    private Color _originalColor;
    
    private void Awake()
    {
        if (OnHealthChanged == null) OnHealthChanged = new UnityEvent<float, float>();
        if (OnDeath == null) OnDeath = new UnityEvent();
        if (OnDamageTaken == null) OnDamageTaken = new UnityEvent();
        if (onDamaged == null) onDamaged = new UnityEvent<float>();

        currentHealth = maxHealth;
        
        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<Renderer>();
            
        if (playerRenderer != null)
        {
            _originalMaterial = playerRenderer.material;
            _originalColor = _originalMaterial.color;
        }
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Update()
    {
        // Check invincibility expiry
        if (_isInvincible && Time.time > _invincibilityEndTime)
        {
            _isInvincible = false;
            ResetVisuals();
        }
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        TakeDamage(amount);
        // Could spawn directional hit effect here
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive || _isInvincible) return;
        
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"[PlayerHealth] Took {amount} damage, health: {currentHealth}/{maxHealth}");
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamageTaken?.Invoke();
        onDamaged?.Invoke(amount);
        
        // Visual feedback
        StartCoroutine(FlashDamage());
        
        // Start invincibility
        _isInvincible = true;
        _invincibilityEndTime = Time.time + invincibilityDuration;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"[PlayerHealth] Healed {amount}, health: {currentHealth}/{maxHealth}");
    }

    public void SetMaxHealth(float newMax, bool healToFull = false)
    {
        maxHealth = newMax;
        if (healToFull)
            currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log("[PlayerHealth] Player died!");
        OnDeath?.Invoke();
        
        // TODO: Death animation, game over screen
        // For now, just disable movement
        var controller = GetComponent<GirliePlayerController>();
        if (controller != null)
            controller.enabled = false;
    }

    private System.Collections.IEnumerator FlashDamage()
    {
        if (playerRenderer != null && _originalMaterial != null)
        {
            _originalMaterial.color = damageFlashColor;
            yield return new WaitForSeconds(flashDuration);
            _originalMaterial.color = _originalColor;
        }
    }

    private void ResetVisuals()
    {
        if (_originalMaterial != null)
            _originalMaterial.color = _originalColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Health pickup
        if (other.CompareTag("HealthPickup"))
        {
            Heal(25f);
            Destroy(other.gameObject);
        }
    }
}
