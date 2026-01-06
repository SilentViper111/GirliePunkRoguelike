using UnityEngine;

/// <summary>
/// Base pickup class for health, ammo, and power-ups.
/// 
/// Reference: KB Section V - Pickups
/// </summary>
public class Pickup : MonoBehaviour
{
    public enum PickupType { Health, Bomb, Speed, Damage, Shield }
    
    [Header("Pickup Settings")]
    [SerializeField] private PickupType pickupType = PickupType.Health;
    [SerializeField] private float value = 25f;
    [SerializeField] private float duration = 10f; // For temporary power-ups
    
    [Header("Visual")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 0.5f;
    [SerializeField] private Light glowLight;
    [SerializeField] private Color pickupColor = Color.green;
    
    private Vector3 _startPos;
    private float _bobOffset;

    private void Start()
    {
        _startPos = transform.position;
        _bobOffset = Random.Range(0f, Mathf.PI * 2f);
        
        // Set color based on type
        SetColorByType();
        
        if (glowLight != null)
            glowLight.color = pickupColor;
    }

    private void Update()
    {
        // Rotate
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        
        // Bob up and down
        Vector3 normal = _startPos.normalized;
        float bob = Mathf.Sin((Time.time + _bobOffset) * bobSpeed) * bobAmount;
        transform.position = _startPos + normal * bob;
    }

    private void SetColorByType()
    {
        switch (pickupType)
        {
            case PickupType.Health:
                pickupColor = Color.green;
                break;
            case PickupType.Bomb:
                pickupColor = Color.magenta;
                break;
            case PickupType.Speed:
                pickupColor = Color.cyan;
                break;
            case PickupType.Damage:
                pickupColor = Color.red;
                break;
            case PickupType.Shield:
                pickupColor = Color.blue;
                break;
        }
        
        // Apply to renderer
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = pickupColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        GirliePlayerController player = other.GetComponent<GirliePlayerController>();
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        
        bool collected = false;
        
        switch (pickupType)
        {
            case PickupType.Health:
                if (health != null && health.CurrentHealth < health.MaxHealth)
                {
                    health.Heal(value);
                    collected = true;
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlayHealthPickup();
                }
                break;
                
            case PickupType.Bomb:
                if (player != null)
                {
                    player.RetrieveBomb();
                    collected = true;
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlayBombPickup();
                }
                break;
                
            case PickupType.Speed:
            case PickupType.Damage:
            case PickupType.Shield:
                // TODO: Apply temporary power-up
                collected = true;
                Debug.Log($"[Pickup] {pickupType} power-up collected (duration: {duration}s)");
                break;
        }
        
        if (collected)
        {
            Debug.Log($"[Pickup] {pickupType} collected!");
            
            // VFX
            if (VFXManager.Instance != null)
                VFXManager.Instance.SpawnImpactSparks(transform.position, transform.position.normalized);
                
            Destroy(gameObject);
        }
    }
}
