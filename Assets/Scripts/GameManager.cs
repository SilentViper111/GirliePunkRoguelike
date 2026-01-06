using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager singleton.
/// Handles game state, initialization, and scene management.
/// 
/// Reference: KB Section IX - Game Flow
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public enum GameState { MainMenu, Playing, Paused, GameOver, Victory }
    
    [Header("State")]
    [SerializeField] private GameState currentState = GameState.Playing;
    
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameUI gameUI;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private WorldGenerator worldGenerator;
    
    [Header("Victory Conditions")]
    [SerializeField] private int wavesToWin = 10;
    
    // Events
    public System.Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Find references
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (gameUI == null)
            gameUI = FindFirstObjectByType<GameUI>();
        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (worldGenerator == null)
            worldGenerator = FindFirstObjectByType<WorldGenerator>();
            
        // Subscribe to player death
        if (playerHealth != null)
            playerHealth.OnDeath.AddListener(OnPlayerDeath);
            
        SetState(GameState.Playing);
    }

    private void Update()
    {
        // Check victory condition
        if (currentState == GameState.Playing && enemySpawner != null)
        {
            if (enemySpawner.GetCurrentWave() > wavesToWin)
            {
                Victory();
            }
        }
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(newState);
        
        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
            case GameState.Victory:
                Time.timeScale = 0.2f; // Slow-mo
                break;
        }
        
        Debug.Log($"[GameManager] State changed to: {newState}");
    }

    private void OnPlayerDeath()
    {
        SetState(GameState.GameOver);
    }

    private void Victory()
    {
        SetState(GameState.Victory);
        Debug.Log("[GameManager] VICTORY! Player completed all waves!");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Assumes main menu is scene 0
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public GameState GetCurrentState() => currentState;
    public bool IsPlaying => currentState == GameState.Playing;
}
