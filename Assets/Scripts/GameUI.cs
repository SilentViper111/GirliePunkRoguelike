using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main game UI controller.
/// Displays health bar, ammo counter, score, and manages game over screen.
/// 
/// Reference: KB Section VIII - UI/UX
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFill;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    
    [Header("Ammo UI")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image[] bombIcons;
    
    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerController playerController;
    
    // Score tracking
    private int _currentScore;
    private int _highScore;
    
    private void Awake()
    {
        // Find player references if not assigned
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();
            
        // Load high score
        _highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        // Hide game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Start()
    {
        // Subscribe to player events
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealthUI);
            playerHealth.OnDeath.AddListener(ShowGameOver);
        }
        
        // Setup buttons
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
            
        // Initial UI update
        UpdateScoreUI();
        UpdateAmmoUI(3, 3); // Default bomb count
    }

    private void Update()
    {
        // Update ammo if player exists
        if (playerController != null)
        {
            UpdateAmmoUI(playerController.GetBombCount(), 3);
        }
    }

    /// <summary>
    /// Updates the health bar display.
    /// </summary>
    public void UpdateHealthUI(float current, float max)
    {
        float percent = current / max;
        
        if (healthSlider != null)
        {
            healthSlider.value = percent;
        }
        
        if (healthFill != null)
        {
            healthFill.color = percent <= lowHealthThreshold 
                ? lowHealthColor 
                : Color.Lerp(lowHealthColor, fullHealthColor, percent);
        }
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }

    /// <summary>
    /// Updates the ammo/bomb counter display.
    /// </summary>
    public void UpdateAmmoUI(int currentBombs, int maxBombs)
    {
        if (ammoText != null)
        {
            ammoText.text = $"BOMBS: {currentBombs}";
        }
        
        // Update bomb icons
        if (bombIcons != null)
        {
            for (int i = 0; i < bombIcons.Length; i++)
            {
                if (bombIcons[i] != null)
                    bombIcons[i].enabled = i < currentBombs;
            }
        }
    }

    /// <summary>
    /// Adds points to the score.
    /// </summary>
    public void AddScore(int points)
    {
        _currentScore += points;
        
        if (_currentScore > _highScore)
        {
            _highScore = _currentScore;
            PlayerPrefs.SetInt("HighScore", _highScore);
        }
        
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {_currentScore:N0}";
        if (highScoreText != null)
            highScoreText.text = $"HIGH: {_highScore:N0}";
    }

    /// <summary>
    /// Shows the game over screen.
    /// </summary>
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (finalScoreText != null)
                finalScoreText.text = $"FINAL SCORE: {_currentScore:N0}";
        }
        
        // Pause game
        Time.timeScale = 0f;
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Gets the current score.
    /// </summary>
    public int GetScore() => _currentScore;
}
