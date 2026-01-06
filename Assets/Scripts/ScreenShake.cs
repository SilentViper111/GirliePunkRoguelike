using UnityEngine;

/// <summary>
/// Camera screen shake effect for impacts and explosions.
/// 
/// Reference: KB Section IV - Visual Stack
/// </summary>
public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }
    
    [Header("Shake Settings")]
    [SerializeField] private float maxShakeAmount = 0.5f;
    [SerializeField] private float maxRotationAmount = 3f;
    [SerializeField] private float decreaseFactor = 1.5f;
    
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private float _currentShakeAmount;
    private float _currentRotationAmount;
    private Transform _cameraTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _cameraTransform = Camera.main?.transform;
        if (_cameraTransform != null)
        {
            _originalPosition = _cameraTransform.localPosition;
            _originalRotation = _cameraTransform.localRotation;
        }
    }

    private void LateUpdate()
    {
        if (_cameraTransform == null) return;
        
        if (_currentShakeAmount > 0.01f)
        {
            // Apply random offset
            Vector3 offset = Random.insideUnitSphere * _currentShakeAmount;
            offset.z = 0; // Keep Z stable for 2D-like effect
            _cameraTransform.localPosition = _originalPosition + offset;
            
            // Apply random rotation
            Vector3 rotOffset = Random.insideUnitSphere * _currentRotationAmount;
            _cameraTransform.localRotation = _originalRotation * Quaternion.Euler(rotOffset);
            
            // Decay
            _currentShakeAmount *= (1f - decreaseFactor * Time.deltaTime);
            _currentRotationAmount *= (1f - decreaseFactor * Time.deltaTime);
        }
        else
        {
            // Reset
            _cameraTransform.localPosition = _originalPosition;
            _cameraTransform.localRotation = _originalRotation;
        }
    }

    /// <summary>
    /// Triggers a screen shake.
    /// </summary>
    /// <param name="intensity">Shake intensity (0-1)</param>
    public void Shake(float intensity = 0.5f)
    {
        intensity = Mathf.Clamp01(intensity);
        _currentShakeAmount = Mathf.Max(_currentShakeAmount, maxShakeAmount * intensity);
        _currentRotationAmount = Mathf.Max(_currentRotationAmount, maxRotationAmount * intensity);
    }

    /// <summary>
    /// Triggers a small impact shake.
    /// </summary>
    public void ShakeSmall() => Shake(0.2f);

    /// <summary>
    /// Triggers a medium impact shake.
    /// </summary>
    public void ShakeMedium() => Shake(0.5f);

    /// <summary>
    /// Triggers a large explosion shake.
    /// </summary>
    public void ShakeLarge() => Shake(1f);
}
