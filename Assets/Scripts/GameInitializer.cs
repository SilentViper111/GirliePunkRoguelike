using UnityEngine;

/// <summary>
/// AUTO-INITIALIZER: Runs automatically when the game starts.
/// Creates Player, projectiles, and sets up camera without requiring
/// any scene setup or component attachment.
/// 
/// Uses [RuntimeInitializeOnLoadMethod] to run before any scene objects.
/// </summary>
public static class GameInitializer
{
    private static GameObject _player;
    private static GameObject _trashPrefab;
    private static GameObject _bombPrefab;
    private static bool _initialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        Debug.Log("[GameInitializer] ========== AUTO-INITIALIZING GAME ==========");

        // Create projectile prefabs first
        CreateProjectilePrefabs();

        // Find or create player
        SetupPlayer();

        // Setup camera
        SetupCamera();

        Debug.Log("[GameInitializer] ========== INITIALIZATION COMPLETE ==========");
    }

    private static void CreateProjectilePrefabs()
    {
        // Trash Prefab
        if (_trashPrefab == null)
        {
            _trashPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _trashPrefab.name = "TrashPrefab_Runtime";
            _trashPrefab.transform.localScale = Vector3.one * 0.3f;
            
            Rigidbody rb = _trashPrefab.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            // Try to add TrashProjectile if it exists
            var trashType = System.Type.GetType("TrashProjectile");
            if (trashType != null)
            {
                _trashPrefab.AddComponent(trashType);
            }
            
            ApplyNeonMaterial(_trashPrefab, Color.cyan);
            _trashPrefab.SetActive(false);
            Object.DontDestroyOnLoad(_trashPrefab);
            
            Debug.Log("[GameInitializer] Created Trash prefab");
        }

        // Bomb Prefab
        if (_bombPrefab == null)
        {
            _bombPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _bombPrefab.name = "BombPrefab_Runtime";
            _bombPrefab.transform.localScale = Vector3.one * 0.6f;
            
            Rigidbody rb = _bombPrefab.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            // Try to add BombshellController if it exists
            var bombType = System.Type.GetType("BombshellController");
            if (bombType != null)
            {
                _bombPrefab.AddComponent(bombType);
            }
            
            ApplyNeonMaterial(_bombPrefab, new Color(1f, 0.5f, 0f));
            _bombPrefab.SetActive(false);
            Object.DontDestroyOnLoad(_bombPrefab);
            
            Debug.Log("[GameInitializer] Created Bomb prefab");
        }
    }

    private static void SetupPlayer()
    {
        // Find existing player
        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null)
        {
            _player = GameObject.Find("Player");
        }

        if (_player == null)
        {
            Debug.Log("[GameInitializer] No player found - creating one");
            _player = CreatePlayer();
        }
        else
        {
            Debug.Log($"[GameInitializer] Found existing player at {_player.transform.position}");
            EnsurePlayerPosition();
            EnsurePlayerComponents();
        }

        // Configure projectiles
        ConfigurePlayerProjectiles();
    }

    private static GameObject CreatePlayer()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        
        int playerLayer = LayerMask.NameToLayer("Player");
        player.layer = (playerLayer >= 0) ? playerLayer : 6;

        // Position on top of sphere (radius 500 + 5 offset)
        player.transform.position = new Vector3(0, 505, 0);
        player.transform.rotation = Quaternion.identity;

        // Bright pink neon material
        ApplyNeonMaterial(player, new Color(1f, 0.4f, 0.8f));

        // Rigidbody
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Player components
        player.AddComponent<GirliePlayerController>();
        player.AddComponent<PlayerHealth>();
        player.AddComponent<PlayerDash>();
        player.AddComponent<HealthRegeneration>();

        Debug.Log($"[GameInitializer] Created player at {player.transform.position}");
        return player;
    }

    private static void EnsurePlayerPosition()
    {
        if (_player == null) return;

        float dist = _player.transform.position.magnitude;
        if (dist < 450f) // Too close to center
        {
            Debug.Log($"[GameInitializer] Player at bad position ({dist:F0}), relocating");
            _player.transform.position = new Vector3(0, 505, 0);
            
            Rigidbody rb = _player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private static void EnsurePlayerComponents()
    {
        if (_player == null) return;

        if (_player.GetComponent<GirliePlayerController>() == null)
            _player.AddComponent<GirliePlayerController>();
        if (_player.GetComponent<PlayerHealth>() == null)
            _player.AddComponent<PlayerHealth>();
        if (_player.GetComponent<PlayerDash>() == null)
            _player.AddComponent<PlayerDash>();
        if (_player.GetComponent<HealthRegeneration>() == null)
            _player.AddComponent<HealthRegeneration>();
    }

    private static void ConfigurePlayerProjectiles()
    {
        if (_player == null) return;

        var controller = _player.GetComponent<GirliePlayerController>();
        if (controller == null) return;

        // Use reflection to set prefabs
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        
        var trashField = typeof(GirliePlayerController).GetField("trashPrefab", flags);
        if (trashField != null && _trashPrefab != null)
        {
            trashField.SetValue(controller, _trashPrefab);
            Debug.Log("[GameInitializer] Assigned trash prefab to player");
        }

        var bombField = typeof(GirliePlayerController).GetField("bombPrefab", flags);
        if (bombField != null && _bombPrefab != null)
        {
            bombField.SetValue(controller, _bombPrefab);
            Debug.Log("[GameInitializer] Assigned bomb prefab to player");
        }
    }

    private static void SetupCamera()
    {
        // Find or add SphereFollowCamera
        SphereFollowCamera followCam = Object.FindFirstObjectByType<SphereFollowCamera>();
        Camera mainCam = Camera.main;

        if (followCam == null && mainCam != null)
        {
            followCam = mainCam.gameObject.AddComponent<SphereFollowCamera>();
            Debug.Log("[GameInitializer] Added SphereFollowCamera to main camera");
        }

        if (followCam != null && _player != null)
        {
            followCam.SetTarget(_player.transform);
            followCam.SnapToTarget();
            Debug.Log("[GameInitializer] Camera now following player");
        }
    }

    private static void ApplyNeonMaterial(GameObject go, Color color)
    {
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.color = color;
        mat.SetFloat("_Smoothness", 0.8f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * 2f);
        
        renderer.material = mat;
    }

    // Reset when exiting play mode
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _player = null;
        _trashPrefab = null;
        _bombPrefab = null;
        _initialized = false;
    }
}
