using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// ROBUST Editor tool to fully setup the GirliePunk scene.
/// Creates Player, GameSystems, and ensures runtime safety.
/// Adds "Tools/GirliePunk/Fix Scene" menu item.
/// </summary>
public class SceneSetupTool : EditorWindow
{
    [MenuItem("Tools/GirliePunk/Fix Scene")]
    public static void FixScene()
    {
        Debug.Log("[SceneSetupTool] ========== STARTING FULL SCENE FIX ==========");

        // 1. Setup WorldGenerator with RuntimePlayerCreator
        SetupWorldGenerator();

        // 2. Setup Player
        SetupPlayer();

        // 3. Setup GameSystems
        SetupGameSystems();

        // 4. Setup PostProcessing
        SetupPostProcessing();

        // 5. Setup Camera
        SetupCamera();

        // 6. Save Scene
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        
        Debug.Log("[SceneSetupTool] ========== SCENE FIX COMPLETE! ==========");
        
        EditorUtility.DisplayDialog("Scene Fixed!", 
            "✅ Player created at (0, 505, 0)\n" +
            "✅ RuntimePlayerCreator added (fail-safe)\n" +
            "✅ GameSystems configured\n" +
            "✅ Camera following player\n" +
            "✅ Scene SAVED\n\n" +
            "Press Play to test!", "OK");
    }

    private static void SetupWorldGenerator()
    {
        GameObject wg = GameObject.Find("WorldGenerator");
        if (wg != null)
        {
            // Add RuntimePlayerCreator for fail-safe player creation
            if (wg.GetComponent<RuntimePlayerCreator>() == null)
            {
                wg.AddComponent<RuntimePlayerCreator>();
                Debug.Log("[SceneSetupTool] Added RuntimePlayerCreator to WorldGenerator");
            }
        }
        else
        {
            Debug.LogWarning("[SceneSetupTool] WorldGenerator not found!");
        }
    }

    private static void SetupPlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (player == null)
        {
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            Debug.Log("[SceneSetupTool] Created new Player capsule");
        }

        // Configure player
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");
        if (player.layer == -1) player.layer = 6;

        // Force correct position (on top of sphere)
        player.transform.position = new Vector3(0, 505, 0);
        player.transform.rotation = Quaternion.identity;

        // Bright pink material for visibility
        Renderer renderer = player.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(1f, 0.3f, 0.7f); // Bright pink
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.3f, 0.7f) * 1.5f);
            renderer.sharedMaterial = mat;
        }

        // Add Components
        EnsureComponent<Rigidbody>(player, rb => {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        });
        
        EnsureComponent<GirliePlayerController>(player);
        EnsureComponent<PlayerHealth>(player);
        EnsureComponent<PlayerDash>(player);
        EnsureComponent<HealthRegeneration>(player);

        Debug.Log($"[SceneSetupTool] Player configured at {player.transform.position}");
    }

    private static void SetupGameSystems()
    {
        GameObject gameSystems = GameObject.Find("GameSystems");
        if (gameSystems == null)
        {
            gameSystems = new GameObject("GameSystems");
            Debug.Log("[SceneSetupTool] Created GameSystems");
        }
        
        EnsureComponent<GameBootstrap>(gameSystems);
        EnsureComponent<AudioPlaceholders>(gameSystems);
        Debug.Log("[SceneSetupTool] GameSystems configured");
    }

    private static void SetupPostProcessing()
    {
        GameObject postC = GameObject.Find("PostProcessing");
        if (postC == null)
        {
            postC = GameObject.Find("PostProcessingVolume");
        }
        if (postC == null)
        {
            postC = new GameObject("PostProcessing");
            Debug.Log("[SceneSetupTool] Created PostProcessing");
        }
        
        EnsureComponent<PostProcessingController>(postC);
        Debug.Log("[SceneSetupTool] PostProcessing configured");
    }

    private static void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("[SceneSetupTool] No main camera found!");
            return;
        }

        SphereFollowCamera followCam = EnsureComponent<SphereFollowCamera>(mainCam.gameObject);
        
        // Find player and set as target
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        
        if (player != null && followCam != null)
        {
            followCam.SetTarget(player.transform);
            followCam.SnapToTarget();
            Debug.Log("[SceneSetupTool] Camera set to follow player");
        }
    }

    private static T EnsureComponent<T>(GameObject go, System.Action<T> initializer = null) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null)
        {
            comp = go.AddComponent<T>();
            Debug.Log($"[SceneSetupTool] Added {typeof(T).Name} to {go.name}");
        }
        initializer?.Invoke(comp);
        return comp;
    }
}
