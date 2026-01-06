using UnityEngine;

/// <summary>
/// Top-down follow camera for spherical world.
/// Positions camera above player while orienting to player's local "up" direction.
/// 
/// Reference: KB Section VI.A - Camera System
/// </summary>
public class SphereFollowCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private bool findPlayerOnStart = true;

    [Header("Camera Position")]
    [SerializeField] private float heightAbovePlayer = 30f;
    [SerializeField] private float distanceBehind = 10f;
    [SerializeField] private float lookAheadDistance = 5f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothTime = 0.15f;
    [SerializeField] private float rotationSmoothSpeed = 10f;

    [Header("Field of View")]
    [SerializeField] private float fieldOfView = 60f;

    // Internal
    private Vector3 _currentVelocity;
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera != null)
        {
            _camera.fieldOfView = fieldOfView;
        }
    }

    private void Start()
    {
        if (findPlayerOnStart && target == null)
        {
            // Find player by tag or name
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
            }
            
            if (player != null)
            {
                target = player.transform;
                Debug.Log($"[SphereFollowCamera] Found target: {target.name}");
            }
            else
            {
                Debug.LogWarning("[SphereFollowCamera] No player found! Assign target manually.");
            }
        }

        // Initial position
        if (target != null)
        {
            SnapToTarget();
        }
    }

    private void LateUpdate()
    {
        // Auto-find player if target is null
        if (target == null)
        {
            TryFindPlayer();
            if (target == null) return;
        }

        UpdateCameraPosition();
        UpdateCameraRotation();
    }

    private void TryFindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        
        if (player != null)
        {
            target = player.transform;
            SnapToTarget();
            Debug.Log($"[SphereFollowCamera] Auto-found player: {target.name}");
        }
    }

    /// <summary>
    /// Calculates and smoothly moves camera to ideal position.
    /// </summary>
    private void UpdateCameraPosition()
    {
        // Player's "up" on sphere = direction from world center to player
        Vector3 playerUp = target.position.normalized;
        
        // Get player's forward direction (or use a default if no velocity)
        Vector3 playerForward = Vector3.ProjectOnPlane(target.forward, playerUp).normalized;
        if (playerForward.sqrMagnitude < 0.01f)
        {
            // Fallback: use world forward projected onto tangent plane
            playerForward = Vector3.ProjectOnPlane(Vector3.forward, playerUp).normalized;
        }

        // Calculate ideal camera position:
        // - Above the player (along playerUp)
        // - Slightly behind (opposite of playerForward)
        Vector3 idealPosition = target.position 
            + playerUp * heightAbovePlayer 
            - playerForward * distanceBehind;

        // Smooth movement
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            idealPosition, 
            ref _currentVelocity, 
            positionSmoothTime
        );
    }

    /// <summary>
    /// Smoothly rotates camera to look at player.
    /// </summary>
    private void UpdateCameraRotation()
    {
        // Player's up direction
        Vector3 playerUp = target.position.normalized;
        
        // Look at point slightly ahead of player
        Vector3 playerForward = Vector3.ProjectOnPlane(target.forward, playerUp).normalized;
        Vector3 lookAtPoint = target.position + playerForward * lookAheadDistance;

        // Calculate look rotation
        Vector3 lookDirection = (lookAtPoint - transform.position).normalized;
        if (lookDirection.sqrMagnitude < 0.01f)
        {
            lookDirection = -playerUp; // Look straight down
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection, playerUp);
        
        // Smooth rotation
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            Time.deltaTime * rotationSmoothSpeed
        );
    }

    /// <summary>
    /// Instantly snaps camera to ideal position (no smoothing).
    /// </summary>
    [ContextMenu("Snap to Target")]
    public void SnapToTarget()
    {
        if (target == null) return;

        Vector3 playerUp = target.position.normalized;
        Vector3 playerForward = Vector3.ProjectOnPlane(target.forward, playerUp).normalized;
        if (playerForward.sqrMagnitude < 0.01f)
        {
            playerForward = Vector3.ProjectOnPlane(Vector3.forward, playerUp).normalized;
        }

        transform.position = target.position + playerUp * heightAbovePlayer - playerForward * distanceBehind;
        
        Vector3 lookAtPoint = target.position + playerForward * lookAheadDistance;
        transform.LookAt(lookAtPoint, playerUp);
    }

    /// <summary>
    /// Assigns a new target at runtime.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        SnapToTarget();
    }
}
