using UnityEngine;

/// <summary>
/// COMPLETE FAIL-SAFE: Creates Player with full functionality at runtime.
/// Creates projectile prefabs at runtime, ensures camera follows, and enables input.
/// Add this to WorldGenerator or any persistent object.
/// 
/// Reference: KB Section VI.B - Player Implementation
/// </summary>
public class RuntimePlayerCreator : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float worldRadius = 500f;
    [SerializeField] private float spawnHeight = 5f;
    [SerializeField] private Color playerColor = new Color(1f, 0.4f, 0.8f); // Pink

    [Header("Projectile Settings")]
    [SerializeField] private Color trashColor = Color.cyan;
    [SerializeField] private Color bombColor = new Color(1f, 0.5f, 0f); // Orange

    private static GameObject _trashPrefab;
    private static GameObject _bombPrefab;
    private static bool _isInitialized = false;

    private void Awake()
    {
        if (_isInitialized)
        {
            Debug.Log("[RuntimePlayerCreator] Already initialized this session.");
            EnsureCameraFollowsPlayer();
            return;
        }
        _isInitialized = true;

        Debug.Log("[RuntimePlayerCreator] ========== GAME INITIALIZATION ==========");

        // Create projectile prefabs
        CreateProjectilePrefabs();

        // Check if player exists
        GameObject player = FindPlayer();

        if (player == null)
        {
            Debug.Log("[RuntimePlayerCreator] Player not found! Creating one...");
            player = CreatePlayer();
        }
        else
        {
            Debug.Log($"[RuntimePlayerCreator] Player exists at {player.transform.position}");
            EnsurePlayerPosition(player);
            EnsurePlayerComponents(player);
        }

        // Configure projectiles on player
        ConfigurePlayerProjectiles(player);

        // Ensure camera follows
        EnsureCameraFollowsPlayer();

        Debug.Log("[RuntimePlayerCreator] ========== INITIALIZATION COMPLETE ==========");
    }

    private GameObject FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        return player;
    }

    private void CreateProjectilePrefabs()
    {
        // Create Trash Prefab (sphere projectile)
        if (_trashPrefab == null)
        {
            _trashPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _trashPrefab.name = "TrashProjectile";
            _trashPrefab.transform.localScale = Vector3.one * 0.3f;
            
            // Add components
            Rigidbody rb = _trashPrefab.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            _trashPrefab.AddComponent<TrashProjectile>();
            
            // Material
            ApplyNeonMaterial(_trashPrefab, trashColor);
            
            // Disable and keep as template
            _trashPrefab.SetActive(false);
            DontDestroyOnLoad(_trashPrefab);
            
            Debug.Log("[RuntimePlayerCreator] Created TrashProjectile prefab");
        }

        // Create Bomb Prefab (larger sphere)
        if (_bombPrefab == null)
        {
            _bombPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _bombPrefab.name = "BombProjectile";
            _bombPrefab.transform.localScale = Vector3.one * 0.6f;
            
            // Add components
            Rigidbody rb = _bombPrefab.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            _bombPrefab.AddComponent<BombshellController>();
            
            // Material
            ApplyNeonMaterial(_bombPrefab, bombColor);
            
            // Disable and keep as template
            _bombPrefab.SetActive(false);
            DontDestroyOnLoad(_bombPrefab);
            
            Debug.Log("[RuntimePlayerCreator] Created BombProjectile prefab");
        }
    }

    private GameObject CreatePlayer()
    {
        // Create capsule primitive
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");
        if (player.layer == -1) player.layer = 6;

        // Position on sphere surface
        Vector3 spawnPos = Vector3.up * (worldRadius + spawnHeight);
        player.transform.position = spawnPos;
        player.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

        // Add bright neon material
        ApplyNeonMaterial(player, playerColor);

        // Add Rigidbody
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Add all player components
        EnsurePlayerComponents(player);

        Debug.Log($"[RuntimePlayerCreator] Created Player at {spawnPos}");
        return player;
    }

    private void EnsurePlayerComponents(GameObject player)
    {
        if (player.GetComponent<GirliePlayerController>() == null)
            player.AddComponent<GirliePlayerController>();
        if (player.GetComponent<PlayerHealth>() == null)
            player.AddComponent<PlayerHealth>();
        if (player.GetComponent<PlayerDash>() == null)
            player.AddComponent<PlayerDash>();
        if (player.GetComponent<HealthRegeneration>() == null)
            player.AddComponent<HealthRegeneration>();
    }

    private void EnsurePlayerPosition(GameObject player)
    {
        float distFromCenter = player.transform.position.magnitude;
        if (distFromCenter < worldRadius * 0.9f)
        {
            Debug.LogWarning($"[RuntimePlayerCreator] Player inside sphere (dist={distFromCenter:F1}). Relocating.");
            Vector3 correctPos = Vector3.up * (worldRadius + spawnHeight);
            player.transform.position = correctPos;
            
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void ConfigurePlayerProjectiles(GameObject player)
    {
        GirliePlayerController controller = player.GetComponent<GirliePlayerController>();
        if (controller == null) return;

        // Use reflection to set the private/serialized prefab fields
        var trashField = typeof(GirliePlayerController).GetField("trashPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var bombField = typeof(GirliePlayerController).GetField("bombPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (trashField != null && _trashPrefab != null)
        {
            trashField.SetValue(controller, _trashPrefab);
            Debug.Log("[RuntimePlayerCreator] Assigned trashPrefab to player");
        }

        if (bombField != null && _bombPrefab != null)
        {
            bombField.SetValue(controller, _bombPrefab);
            Debug.Log("[RuntimePlayerCreator] Assigned bombPrefab to player");
        }
    }

    private void EnsureCameraFollowsPlayer()
    {
        SphereFollowCamera cam = FindFirstObjectByType<SphereFollowCamera>();
        Camera mainCam = Camera.main;

        // If no SphereFollowCamera, add one to main camera
        if (cam == null && mainCam != null)
        {
            cam = mainCam.gameObject.AddComponent<SphereFollowCamera>();
            Debug.Log("[RuntimePlayerCreator] Added SphereFollowCamera to main camera");
        }

        // Set target
        if (cam != null)
        {
            GameObject player = FindPlayer();
            if (player != null)
            {
                cam.SetTarget(player.transform);
                cam.SnapToTarget();
                Debug.Log("[RuntimePlayerCreator] Camera now following player");
            }
        }
    }

    private void ApplyNeonMaterial(GameObject go, Color color)
    {
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
        }
        
        mat.color = color;
        mat.SetFloat("_Smoothness", 0.8f);
        
        // Enable emission for neon glow
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * 2f);
        
        renderer.material = mat;
    }

    // Reset static state when exiting play mode
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _trashPrefab = null;
        _bombPrefab = null;
        _isInitialized = false;
    }
}
