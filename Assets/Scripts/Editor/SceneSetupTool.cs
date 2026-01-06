using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor tool to reliably setup the scene if objects go missing.
/// Adds "Tools/GirliePunk/Fix Scene" menu item.
/// </summary>
public class SceneSetupTool : EditorWindow
{
    [MenuItem("Tools/GirliePunk/Fix Scene")]
    public static void FixScene()
    {
        Debug.Log("[SceneSetupTool] Starting scene verification...");

        // 1. Setup Player
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (player == null)
        {
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            Debug.Log("[SceneSetupTool] Created Player object");
        }

        // Force position
        player.transform.position = new Vector3(0, 503, 0);
        player.layer = LayerMask.NameToLayer("Player"); // Ensure layer 6 if named Player
        if (player.layer == -1) player.layer = 0; // Fallback

        // Add Components
        EnsureComponent<Rigidbody>(player, rb => {
            rb.useGravity = false; 
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        });
        
        // Note: Using string names for custom components to avoid compile errors if scripts are missing references
        // But here we know they exist.
        EnsureComponent<GirliePlayerController>(player);
        EnsureComponent<PlayerHealth>(player);
        EnsureComponent<PlayerDash>(player);
        EnsureComponent<ScreenShake>(player);
        EnsureComponent<HealthRegeneration>(player);
        EnsureComponent<PlayerSpawnFix>(player);

        Debug.Log("[SceneSetupTool] Player configured.");

        // 2. Setup GameSystems
        GameObject gameSystems = GameObject.Find("GameSystems");
        if (gameSystems == null)
        {
            gameSystems = new GameObject("GameSystems");
            Debug.Log("[SceneSetupTool] Created GameSystems object");
        }
        EnsureComponent<GameBootstrap>(gameSystems);
        EnsureComponent<AudioPlaceholders>(gameSystems);
        Debug.Log("[SceneSetupTool] GameSystems configured.");

        // 3. Setup PostProcessing
        GameObject postC = GameObject.Find("PostProcessing");
        if (postC == null)
        {
            postC = new GameObject("PostProcessing");
            Debug.Log("[SceneSetupTool] Created PostProcessing object");
        }
        EnsureComponent<PostProcessingController>(postC);
        Debug.Log("[SceneSetupTool] PostProcessing configured.");

        // 4. Setup Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            EnsureComponent<SphereFollowCamera>(mainCam.gameObject, cam => {
                cam.SetTarget(player.transform);
            });
            Debug.Log("[SceneSetupTool] Camera configured.");
        }

        // 5. Save Scene
        EditorSceneManager.MarkSceneDirty(player.scene);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[SceneSetupTool] SCENE SAVED SUCCESSFULLY!");
        
        EditorUtility.DisplayDialog("Scene Fixed", "Player and Systems restored!\nPosition: (0, 503, 0)\nScene Saved.", "OK");
    }

    private static void EnsureComponent<T>(GameObject go, System.Action<T> initializer = null) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null)
        {
            comp = go.AddComponent<T>();
        }
        initializer?.Invoke(comp);
    }
}
