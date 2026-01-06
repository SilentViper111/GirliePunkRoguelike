using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player controller with sphere-aware movement.
/// Handles WASD movement relative to camera, mouse aiming, and firing.
/// 
/// Reference: KB Section VI.B - Player Implementation
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float gravityStrength = 20f;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject trashPrefab;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float trashFireRate = 0.15f;
    [SerializeField] private float bombCooldown = 0.5f;
    [SerializeField] private int maxBombs = 3;
    [SerializeField] private int currentBombs = 3;

    [Header("References")]
    [SerializeField] private Camera playerCamera;

    // Internal
    private Rigidbody _rb;
    private Vector2 _moveInput;
    private Vector2 _aimInput;
    private Vector3 _currentUp;
    private float _lastTrashTime;
    private float _lastBombTime;
    private bool _wantsToFireTrash;
    private bool _wantsToFireBomb;

    // Input Actions (generated from PlayerControls.inputactions)
    private PlayerControls _inputActions;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false; // We handle custom gravity
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        _currentUp = transform.up;
        
        // Initialize input
        _inputActions = new PlayerControls();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (firePoint == null)
            firePoint = transform;
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;
        _inputActions.Player.Aim.performed += OnAim;
        _inputActions.Player.Aim.canceled += OnAim;
        _inputActions.Player.FireTrash.performed += OnFireTrash;
        _inputActions.Player.FireTrash.canceled += ctx => _wantsToFireTrash = false;
        _inputActions.Player.FireBomb.performed += OnFireBomb;
    }

    private void OnDisable()
    {
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMove;
        _inputActions.Player.Aim.performed -= OnAim;
        _inputActions.Player.Aim.canceled -= OnAim;
        _inputActions.Player.FireTrash.performed -= OnFireTrash;
        _inputActions.Player.FireBomb.performed -= OnFireBomb;
        
        _inputActions.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx) => _moveInput = ctx.ReadValue<Vector2>();
    private void OnAim(InputAction.CallbackContext ctx) => _aimInput = ctx.ReadValue<Vector2>();
    private void OnFireTrash(InputAction.CallbackContext ctx) => _wantsToFireTrash = true;
    private void OnFireBomb(InputAction.CallbackContext ctx) => _wantsToFireBomb = true;

    private void FixedUpdate()
    {
        UpdateGravity();
        UpdateMovement();
        UpdateRotation();
    }

    private void Update()
    {
        UpdateFiring();
    }

    /// <summary>
    /// Applies custom gravity toward world center (for spherical world).
    /// </summary>
    private void UpdateGravity()
    {
        // Gravity points toward world center (0,0,0)
        Vector3 gravityDir = -transform.position.normalized;
        _rb.AddForce(gravityDir * gravityStrength, ForceMode.Acceleration);

        // Smoothly align player's up vector to match surface normal
        _currentUp = Vector3.Lerp(_currentUp, -gravityDir, Time.fixedDeltaTime * 5f);
    }

    /// <summary>
    /// Moves player based on WASD input relative to camera.
    /// </summary>
    private void UpdateMovement()
    {
        if (_moveInput.sqrMagnitude < 0.01f) return;

        // Get camera-relative directions projected onto player's tangent plane
        Vector3 camForward = playerCamera.transform.forward;
        Vector3 camRight = playerCamera.transform.right;

        // Project onto tangent plane (perpendicular to current up)
        camForward = Vector3.ProjectOnPlane(camForward, _currentUp).normalized;
        camRight = Vector3.ProjectOnPlane(camRight, _currentUp).normalized;

        // Calculate move direction
        Vector3 moveDir = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;

        // Apply movement
        Vector3 targetVelocity = moveDir * moveSpeed;
        Vector3 currentHorizontalVel = Vector3.ProjectOnPlane(_rb.linearVelocity, _currentUp);
        Vector3 verticalVel = Vector3.Project(_rb.linearVelocity, _currentUp);

        _rb.linearVelocity = Vector3.Lerp(currentHorizontalVel, targetVelocity, Time.fixedDeltaTime * 10f) + verticalVel;
    }

    /// <summary>
    /// Rotates player to face movement direction or aim direction.
    /// </summary>
    private void UpdateRotation()
    {
        Vector3 lookDir = Vector3.zero;

        if (_moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 camForward = Vector3.ProjectOnPlane(playerCamera.transform.forward, _currentUp).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(playerCamera.transform.right, _currentUp).normalized;
            lookDir = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;
        }

        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir, _currentUp);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
        else
        {
            // Just maintain up vector alignment
            transform.rotation = Quaternion.FromToRotation(transform.up, _currentUp) * transform.rotation;
        }
    }

    /// <summary>
    /// Handles trash and bomb firing.
    /// </summary>
    private void UpdateFiring()
    {
        // Trash (Left Click - rapid fire)
        if (_wantsToFireTrash && Time.time > _lastTrashTime + trashFireRate)
        {
            FireTrash();
            _lastTrashTime = Time.time;
        }

        // Bomb (Right Click - cooldown)
        if (_wantsToFireBomb && currentBombs > 0 && Time.time > _lastBombTime + bombCooldown)
        {
            FireBomb();
            _lastBombTime = Time.time;
            _wantsToFireBomb = false;
        }
        else if (_wantsToFireBomb)
        {
            _wantsToFireBomb = false;
        }
    }

    private void FireTrash()
    {
        if (trashPrefab == null) return;
        
        GameObject trash = Instantiate(trashPrefab, firePoint.position, transform.rotation);
        
        // VFX
        if (VFXManager.Instance != null)
            VFXManager.Instance.SpawnMuzzleFlash(firePoint.position, transform.rotation);
            
        // Audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayShootTrash();
            
        Debug.Log("[Player] Fired Trash");
    }

    private void FireBomb()
    {
        if (bombPrefab == null) return;

        currentBombs--;
        GameObject bomb = Instantiate(bombPrefab, firePoint.position, transform.rotation);
        
        // VFX
        if (VFXManager.Instance != null)
            VFXManager.Instance.SpawnMuzzleFlash(firePoint.position, transform.rotation);
            
        // Audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayShootBomb();
            
        Debug.Log($"[Player] Fired Bomb ({currentBombs}/{maxBombs} remaining)");
    }

    /// <summary>
    /// Called by Bombshell when retrieved.
    /// </summary>
    public void RetrieveBomb()
    {
        currentBombs = Mathf.Min(currentBombs + 1, maxBombs);
        Debug.Log($"[Player] Retrieved Bomb ({currentBombs}/{maxBombs})");
    }

    /// <summary>
    /// Gets current bomb count.
    /// </summary>
    public int GetBombCount() => currentBombs;
}
