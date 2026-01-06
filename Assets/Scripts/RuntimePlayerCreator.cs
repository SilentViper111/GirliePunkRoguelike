using UnityEngine;

/// <summary>
/// FAIL-SAFE: Creates the Player at runtime if it doesn't exist in the scene.
/// Add this to a persistent object like GameSystems or WorldGenerator.
/// This ensures the game always has a player even if scene setup is broken.
/// </summary>
public class RuntimePlayerCreator : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float worldRadius = 500f;
    [SerializeField] private float spawnHeight = 5f;
    [SerializeField] private Color playerColor = new Color(1f, 0.4f, 0.8f); // Pink

    private void Awake()
    {
        // Check if player exists
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        if (player == null)
        {
            Debug.Log("[RuntimePlayerCreator] Player not found! Creating one...");
            CreatePlayer();
        }
        else
        {
            Debug.Log($"[RuntimePlayerCreator] Player already exists at {player.transform.position}");
            // Ensure correct position
            EnsurePlayerPosition(player);
        }
    }

    private void CreatePlayer()
    {
        // Create capsule primitive
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.layer = 6; // Player layer

        // Position on sphere surface
        Vector3 spawnPos = Vector3.up * (worldRadius + spawnHeight);
        player.transform.position = spawnPos;
        player.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

        // Add bright material so player is VISIBLE
        Renderer renderer = player.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = playerColor;
            mat.SetFloat("_Smoothness", 0.8f);
            // Make it emissive for neon look
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", playerColor * 2f);
            renderer.material = mat;
        }

        // Add Rigidbody
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.useGravity = false; // We handle gravity manually on sphere
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Add all player components
        player.AddComponent<GirliePlayerController>();
        player.AddComponent<PlayerHealth>();
        player.AddComponent<PlayerDash>();
        player.AddComponent<HealthRegeneration>();

        Debug.Log($"[RuntimePlayerCreator] Created Player at {spawnPos} with all components!");

        // Hook up camera
        SphereFollowCamera cam = FindFirstObjectByType<SphereFollowCamera>();
        if (cam != null)
        {
            cam.SetTarget(player.transform);
            cam.SnapToTarget();
            Debug.Log("[RuntimePlayerCreator] Camera now following player");
        }
    }

    private void EnsurePlayerPosition(GameObject player)
    {
        // Check if player is in a bad position (inside sphere)
        float distFromCenter = player.transform.position.magnitude;
        if (distFromCenter < worldRadius * 0.9f)
        {
            Debug.LogWarning($"[RuntimePlayerCreator] Player at bad position (dist={distFromCenter}). Relocating to surface.");
            Vector3 correctPos = Vector3.up * (worldRadius + spawnHeight);
            player.transform.position = correctPos;
            
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // Ensure camera is following
        SphereFollowCamera cam = FindFirstObjectByType<SphereFollowCamera>();
        if (cam != null)
        {
            cam.SetTarget(player.transform);
        }
    }
}
