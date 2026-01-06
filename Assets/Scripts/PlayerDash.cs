using UnityEngine;

/// <summary>
/// Player dash ability component.
/// Add to Player for quick dash movement with cooldown.
/// 
/// Reference: KB Section VI - Player Abilities
/// </summary>
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private KeyCode dashKey = KeyCode.Space;
    
    [Header("Invincibility")]
    [SerializeField] private bool invincibleDuringDash = true;
    
    [Header("Effects")]
    [SerializeField] private TrailRenderer dashTrail;
    [SerializeField] private Color dashColor = Color.cyan;
    
    // State
    private bool _isDashing;
    private bool _canDash = true;
    private float _dashEndTime;
    private float _lastDashTime;
    private Vector3 _dashDirection;
    private Rigidbody _rb;
    private PlayerHealth _health;
    private GirliePlayerController _controller;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _health = GetComponent<PlayerHealth>();
        _controller = GetComponent<GirliePlayerController>();
    }

    private void Update()
    {
        // Check for dash input using new Input System
        bool dashPressed = UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame;
        
        if (dashPressed && _canDash && !_isDashing)
        {
            StartDash();
        }
        
        // Update dash state
        if (_isDashing && Time.time > _dashEndTime)
        {
            EndDash();
        }
        
        // Cooldown
        if (!_canDash && Time.time > _lastDashTime + dashCooldown)
        {
            _canDash = true;
        }
    }

    private void FixedUpdate()
    {
        if (_isDashing)
        {
            // Apply dash movement
            Vector3 currentUp = transform.position.normalized;
            Vector3 dashVelocity = _dashDirection * (dashDistance / dashDuration);
            Vector3 vertical = Vector3.Project(_rb.linearVelocity, currentUp);
            
            _rb.linearVelocity = dashVelocity + vertical;
        }
    }

    private void StartDash()
    {
        _isDashing = true;
        _canDash = false;
        _lastDashTime = Time.time;
        _dashEndTime = Time.time + dashDuration;
        
        // Get dash direction from movement input or facing direction
        Vector3 currentUp = transform.position.normalized;
        _dashDirection = Vector3.ProjectOnPlane(transform.forward, currentUp).normalized;
        
        // Check for WASD input using new Input System
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        float h = 0, v = 0;
        if (keyboard.aKey.isPressed) h -= 1;
        if (keyboard.dKey.isPressed) h += 1;
        if (keyboard.wKey.isPressed) v += 1;
        if (keyboard.sKey.isPressed) v -= 1;
        
        Vector2 input = new Vector2(h, v);
        if (input.sqrMagnitude > 0.1f)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 camForward = Vector3.ProjectOnPlane(cam.transform.forward, currentUp).normalized;
                Vector3 camRight = Vector3.ProjectOnPlane(cam.transform.right, currentUp).normalized;
                _dashDirection = (camForward * input.y + camRight * input.x).normalized;
            }
        }
        
        // Trail
        if (dashTrail != null)
        {
            dashTrail.enabled = true;
            dashTrail.startColor = dashColor;
        }
        
        // Screen shake
        if (ScreenShake.Instance != null)
            ScreenShake.Instance.ShakeSmall();
            
        Debug.Log("[Player] DASH!");
    }

    private void EndDash()
    {
        _isDashing = false;
        
        // Trail
        if (dashTrail != null)
            dashTrail.enabled = false;
    }

    public bool IsDashing => _isDashing;
    public bool CanDash => _canDash;
}
