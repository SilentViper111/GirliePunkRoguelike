using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Game over screen showing run summary.
/// 
/// Reference: KB Section VIII - UI/UX
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Title")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("New Best")]
    [SerializeField] private GameObject newBestBadge;

    private void Awake()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (newBestBadge != null)
            newBestBadge.SetActive(false);
    }

    private void Start()
    {
        // Subscribe to game over
        if (GameManager.Instance != null)
        {
            // GameManager would call ShowGameOver when game ends
        }

        // Button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    /// <summary>
    /// Shows game over screen with run summary.
    /// </summary>
    public void ShowGameOver(GameStats.RunSummary summary = null)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;

        if (summary != null)
        {
            DisplayStats(summary);
        }
        else if (GameStats.Instance != null)
        {
            GameUI gameUI = FindFirstObjectByType<GameUI>();
            int score = gameUI != null ? 0 : 0; // Get final score
            DisplayStats(GameStats.Instance.EndRun(score));
        }

        // Animate in
        StartCoroutine(FadeIn());
    }

    private void DisplayStats(GameStats.RunSummary summary)
    {
        if (titleText != null)
            titleText.text = "GAME OVER";

        if (subtitleText != null)
            subtitleText.text = summary.isNewBestScore ? "NEW HIGH SCORE!" : "";

        if (killsText != null)
            killsText.text = $"Kills: {summary.kills}";

        if (waveText != null)
            waveText.text = $"Waves: {summary.waves}";

        if (comboText != null)
            comboText.text = $"Best Combo: {summary.maxCombo}x";

        if (scoreText != null)
            scoreText.text = $"Score: {summary.finalScore:N0}";

        if (timeText != null)
        {
            int mins = (int)(summary.playTime / 60);
            int secs = (int)(summary.playTime % 60);
            timeText.text = $"Time: {mins}:{secs:D2}";
        }

        if (newBestBadge != null)
            newBestBadge.SetActive(summary.isNewBestScore);

        // Submit to leaderboard
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.UpdateCurrentRun(summary.finalScore, summary.waves);
            LeaderboardManager.Instance.SubmitScore();
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.alpha = 0f;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = elapsed / duration;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        
        if (SceneController.Instance != null)
        {
            SceneController.Instance.RestartCurrentScene();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadMainMenu();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
