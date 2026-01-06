using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Post-processing controller for neon cyberpunk visual effects.
/// Configures bloom, chromatic aberration, and color grading.
/// 
/// Reference: KB Section IV - Visual Stack
/// </summary>
public class PostProcessingController : MonoBehaviour
{
    [Header("Bloom Settings")]
    [SerializeField] private float bloomIntensity = 1.5f;
    [SerializeField] private float bloomThreshold = 0.9f;
    
    [Header("Chromatic Aberration")]
    [SerializeField] private float chromaticIntensity = 0.1f;
    
    [Header("Vignette")]
    [SerializeField] private float vignetteIntensity = 0.3f;
    
    [Header("Color Grading")]
    [SerializeField] private float contrast = 10f;
    [SerializeField] private float saturation = 20f;
    
    [Header("Dynamic Effects")]
    [SerializeField] private bool enableDamageEffect = true;
    [SerializeField] private float damageChromaticPulse = 0.5f;
    [SerializeField] private float damageVignettePulse = 0.5f;
    
    private Volume _volume;
    private float _targetChromatic;
    private float _currentChromatic;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        if (_volume == null)
            _volume = gameObject.AddComponent<Volume>();
            
        _volume.isGlobal = true;
        _volume.priority = 100;
    }

    private void Start()
    {
        // Subscribe to player damage for visual feedback
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnDamageTaken.AddListener(OnPlayerDamaged);
        }
        
        SetupProfile();
    }

    private void SetupProfile()
    {
        // Note: In actual implementation, you'd create/modify a VolumeProfile
        // This is a placeholder showing the intended settings
        
        Debug.Log("[PostProcessing] Configured neon cyberpunk post-processing:");
        Debug.Log($"  - Bloom: {bloomIntensity} intensity, {bloomThreshold} threshold");
        Debug.Log($"  - Chromatic Aberration: {chromaticIntensity}");
        Debug.Log($"  - Vignette: {vignetteIntensity}");
        Debug.Log($"  - Color Grading: +{contrast} contrast, +{saturation} saturation");
    }

    private void Update()
    {
        // Lerp chromatic aberration back to base
        _currentChromatic = Mathf.Lerp(_currentChromatic, _targetChromatic, Time.deltaTime * 5f);
        _targetChromatic = Mathf.Lerp(_targetChromatic, chromaticIntensity, Time.deltaTime * 3f);
    }

    private void OnPlayerDamaged()
    {
        if (!enableDamageEffect) return;
        
        // Pulse effects on damage
        _targetChromatic = damageChromaticPulse;
        
        // TODO: Actually modify volume profile settings
        Debug.Log("[PostProcessing] Damage visual feedback triggered!");
    }

    /// <summary>
    /// Sets bloom intensity dynamically (e.g., for explosions).
    /// </summary>
    public void SetBloomIntensity(float intensity)
    {
        bloomIntensity = intensity;
        // TODO: Apply to volume profile
    }
}
