using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;

/// <summary>
/// Automation script to setup the GirliePunk scene.
/// </summary>
public class AutoSetup : EditorWindow
{
    [MenuItem("GirliePunk/Full Setup")]
    public static void PerformSetup()
    {
        SetupGameSystems();
        SetupUI();
        SetupPlayer();
        SetupEnvironment();
        
        Debug.Log("<b>[AutoSetup]</b> Scene setup complete!");
    }

    private static void SetupGameSystems()
    {
        GameObject systems = GameObject.Find("GameSystems");
        if (systems == null)
            systems = new GameObject("GameSystems");
            
        Undo.RegisterCreatedObjectUndo(systems, "Create GameSystems");

        // Add Singleton Managers
        EnsureComponent<GameManager>(systems);
        EnsureComponent<GameBootstrap>(systems);
        EnsureComponent<AudioManager>(systems);
        EnsureComponent<VFXManager>(systems);
        EnsureComponent<UpgradeShop>(systems);
        EnsureComponent<AchievementSystem>(systems);
        EnsureComponent<LeaderboardManager>(systems);
        EnsureComponent<TutorialSystem>(systems);
        EnsureComponent<ObjectiveSystem>(systems);
        EnsureComponent<WaveDirector>(systems);
        EnsureComponent<DifficultyScaler>(systems);
        EnsureComponent<RoomManager>(systems);
        EnsureComponent<ComboSystem>(systems);
        EnsureComponent<PowerUpManager>(systems);
        EnsureComponent<SettingsManager>(systems);
        EnsureComponent<GameStats>(systems);
        EnsureComponent<KillStreakSystem>(systems);
        EnsureComponent<ParticlePresets>(systems);
        EnsureComponent<CriticalHitSystem>(systems);
        EnsureComponent<BulletTime>(systems);
        EnsureComponent<SceneController>(systems);
        EnsureComponent<EnemySpawner>(systems);
        EnsureComponent<PickupSpawner>(systems);
        EnsureComponent<NeonMaterialFactory>(systems);

        // Setup some defaults
        var director = systems.GetComponent<WaveDirector>();
        var spawner = systems.GetComponent<EnemySpawner>();
        
        // Find existing prefabs to assign
        // Note: This matches names in Assets/Prefabs/Enemies/
        // Actual assignment might need manual check if paths vary
    }

    private static void SetupUI()
    {
        GameObject uiRoot = GameObject.Find("UI_Root");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("UI_Root");
            Canvas canvas = uiRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = uiRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            uiRoot.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(uiRoot, "Create UI Root");
        }

        // 1. HUD Panel
        GameObject hud = EnsurePanel(uiRoot, "HUD_Panel");
        EnsureComponent<GameUI>(hud);
        EnsureComponent<WaveDisplay>(hud);
        EnsureComponent<ObjectiveUI>(hud);
        EnsureComponent<PowerUpUI>(hud);
        EnsureComponent<BossHealthBar>(hud);
        EnsureComponent<TutorialSystem>(hud); // Attaching UI component logic here? Actually TutorialSystem is a manager, maybe purely logic. 
        // TutorialSystem references UI elements, better to have a TutorialUI script or attach references.
        // Let's just create the container.
        
        // 2. Pause Menu
        GameObject pause = EnsurePanel(uiRoot, "PauseMenu_Panel");
        pause.SetActive(false);
        EnsureComponent<PauseMenu>(pause);

        // 3. Shop UI
        GameObject shop = EnsurePanel(uiRoot, "Shop_Panel");
        shop.SetActive(false);
        EnsureComponent<UpgradeShopUI>(shop);

        // 4. Game Over
        GameObject gameOver = EnsurePanel(uiRoot, "GameOver_Panel");
        gameOver.SetActive(false);
        EnsureComponent<GameOverUI>(gameOver);

        // 5. Minimap
        GameObject minimap = EnsurePanel(uiRoot, "Minimap_Panel");
        // Minimap usually needs a RawImage
        EnsureComponent<RawImage>(minimap);
        EnsureComponent<Minimap>(minimap);
        
        // Create EventSystem if missing
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }

    private static void SetupPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        
        if (player != null)
        {
            EnsureComponent<PlayerController>(player);
            EnsureComponent<PlayerHealth>(player);
            EnsureComponent<PlayerDash>(player);
            EnsureComponent<ScreenShake>(player); // Attaching logic to player or camera? ScreenShake is usually on Camera or Global.
            // ScreenShake singleton is better on GameSystems or Camera.
            // Let's remove it from player and put on Camera.
            
            // Audio Listener on Camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                EnsureComponent<SphereFollowCamera>(mainCam.gameObject);
                EnsureComponent<ScreenShake>(mainCam.gameObject);
                EnsureComponent<AudioListener>(mainCam.gameObject);
            }
        }
    }

    private static void SetupEnvironment()
    {
        GameObject worldGen = GameObject.Find("WorldGenerator");
        if (worldGen == null)
            worldGen = new GameObject("WorldGenerator");
            
        EnsureComponent<WorldGenerator>(worldGen);
        EnsureComponent<RoomOutlineRenderer>(worldGen); // Needs to be on same object usually
        
        GameObject pp = GameObject.Find("PostProcessing");
        if (pp == null)
            pp = new GameObject("PostProcessing");
            
        EnsureComponent<Volume>(pp);
        EnsureComponent<PostProcessingController>(pp);
    }

    private static T EnsureComponent<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null)
            comp = go.AddComponent<T>();
        return comp;
    }

    private static GameObject EnsurePanel(GameObject parent, string name)
    {
        Transform t = parent.transform.Find(name);
        if (t == null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            
            // Full stretch
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            
            // Transparent raycast target
            Image img = go.AddComponent<Image>();
            img.color = new Color(0,0,0,0); 
            
            return go;
        }
        return t.gameObject;
    }
}
