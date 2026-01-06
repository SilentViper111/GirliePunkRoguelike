using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimap that shows player position and nearby enemies.
/// Uses a secondary camera or UI overlay.
/// 
/// Reference: KB Section VIII - UI/UX
/// </summary>
public class Minimap : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float zoomLevel = 100f;
    [SerializeField] private bool rotateWithPlayer = true;
    
    [Header("Icons")]
    [SerializeField] private RectTransform playerIcon;
    [SerializeField] private GameObject enemyIconPrefab;
    [SerializeField] private RectTransform minimapRect;
    
    [Header("References")]
    [SerializeField] private Transform player;
    
    private System.Collections.Generic.List<RectTransform> _enemyIcons = new System.Collections.Generic.List<RectTransform>();
    private float _updateInterval = 0.1f;
    private float _lastUpdateTime;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null || minimapRect == null) return;
        
        // Throttle updates
        if (Time.time - _lastUpdateTime < _updateInterval) return;
        _lastUpdateTime = Time.time;
        
        UpdatePlayerIcon();
        UpdateEnemyIcons();
        
        // Rotate minimap with player
        if (rotateWithPlayer)
        {
            float yRotation = -player.eulerAngles.y;
            minimapRect.localRotation = Quaternion.Euler(0, 0, yRotation);
        }
    }

    private void UpdatePlayerIcon()
    {
        if (playerIcon == null) return;
        
        // Player is always at center
        playerIcon.anchoredPosition = Vector2.zero;
        
        // Rotate player icon
        float zRotation = -player.eulerAngles.y;
        playerIcon.localRotation = Quaternion.Euler(0, 0, zRotation);
    }

    private void UpdateEnemyIcons()
    {
        if (enemyIconPrefab == null || minimapRect == null) return;
        
        // Find all enemies
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        
        // Add/remove icons as needed
        while (_enemyIcons.Count < enemies.Length)
        {
            GameObject icon = Instantiate(enemyIconPrefab, minimapRect);
            _enemyIcons.Add(icon.GetComponent<RectTransform>());
        }
        
        while (_enemyIcons.Count > enemies.Length)
        {
            RectTransform icon = _enemyIcons[_enemyIcons.Count - 1];
            _enemyIcons.RemoveAt(_enemyIcons.Count - 1);
            if (icon != null)
                Destroy(icon.gameObject);
        }
        
        // Position icons
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null || !enemies[i].IsAlive)
            {
                _enemyIcons[i].gameObject.SetActive(false);
                continue;
            }
            
            _enemyIcons[i].gameObject.SetActive(true);
            
            Vector3 offset = enemies[i].transform.position - player.position;
            Vector2 minimapPos = new Vector2(offset.x, offset.z) / zoomLevel * 50f;
            
            // Clamp to minimap bounds
            float maxDist = minimapRect.rect.width * 0.45f;
            if (minimapPos.magnitude > maxDist)
            {
                minimapPos = minimapPos.normalized * maxDist;
            }
            
            _enemyIcons[i].anchoredPosition = minimapPos;
        }
    }
}
