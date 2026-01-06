using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Auto-assigns prefabs to spawners and controllers.
/// </summary>
public class PrefabAssigner : EditorWindow
{
    [MenuItem("GirliePunk/Assign Prefabs")]
    public static void AssignAll()
    {
        AssignEnemies();
        AssignPickups();
        AssignPlayerRefs();
        Debug.Log("<b>[PrefabAssigner]</b> Assignments complete!");
    }

    private static void AssignEnemies()
    {
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner == null) return;

        // Load all enemy prefabs
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Enemies" });
        List<EnemyController> enemyPrefabs = new List<EnemyController>();
        BossController bossPrefab = null;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            // Check for EnemyController (base or derived)
            var controller = prefab.GetComponent<EnemyController>();
            if (controller != null)
            {
                // Check if it's the boss
                if (prefab.GetComponent<BossController>() != null)
                    bossPrefab = prefab.GetComponent<BossController>();
                else
                    enemyPrefabs.Add(controller);
            }
        }

        // Use SerializedObject to access private fields if needed, 
        // or ensure fields are public/serialized
        SerializedObject so = new SerializedObject(spawner);
        
        // Assign enemies array
        SerializedProperty enemiesProp = so.FindProperty("enemyPrefabs");
        enemiesProp.ClearArray();
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            enemiesProp.InsertArrayElementAtIndex(i);
            enemiesProp.GetArrayElementAtIndex(i).objectReferenceValue = enemyPrefabs[i];
        }

        // Assign boss
        SerializedProperty bossProp = so.FindProperty("bossPrefab");
        if (bossPrefab != null)
            bossProp.objectReferenceValue = bossPrefab;
            
        so.ApplyModifiedProperties();
        Debug.Log($"[PrefabAssigner] Assigned {enemyPrefabs.Count} enemies and {(bossPrefab!=null?"1":"0")} boss to Spawner.");
    }

    private static void AssignPickups()
    {
        PickupSpawner spawner = FindFirstObjectByType<PickupSpawner>();
        if (spawner == null) return;

        SerializedObject so = new SerializedObject(spawner);

        // Find specific pickups by name
        AssignPickup(so, "healthPickupPrefab", "HealthPickupPrefab");
        AssignPickup(so, "bombPickupPrefab", "BombPickupPrefab");
        AssignPickup(so, "speedPickupPrefab", "SpeedPickupPrefab");
        AssignPickup(so, "damagePickupPrefab", "DamagePickupPrefab");

        so.ApplyModifiedProperties();
        Debug.Log("[PrefabAssigner] Pickups assigned.");
    }

    private static void AssignPickup(SerializedObject so, string propName, string searchName)
    {
        string[] guids = AssetDatabase.FindAssets($"{searchName} t:Prefab");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            so.FindProperty(propName).objectReferenceValue = prefab;
        }
    }

    private static void AssignPlayerRefs()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        SerializedObject so = new SerializedObject(player);

        // Assign projectiles
        AssignAny(so, "trashPrefab", "TrashPrefab");
        AssignAny(so, "bombPrefab", "BombPrefab");

        so.ApplyModifiedProperties();
        Debug.Log("[PrefabAssigner] Player refs assigned.");
    }
    
    private static void AssignAny(SerializedObject so, string propName, string searchName)
    {
        string[] guids = AssetDatabase.FindAssets($"{searchName} t:Prefab");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var prop = so.FindProperty(propName);
            if (prop != null)
                prop.objectReferenceValue = prefab.GetComponent<Rigidbody>(); // TrashPrefab has Rigidbody
            else 
            {
                 // Try finding GameObject field?
                 // Some scripts might expect GameObject, others Rigidbody/Projectile.
                 // Assuming checking specific type is hard generically, but PlayerController expects Rigidbody for trash/bomb usually?
                 // Let's check PlayerController script... 
                 // public Rigidbody trashPrefab; public Rigidbody bombPrefab; 
                 // YES.
            }
        }
    }
}
