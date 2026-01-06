using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Tutorial system with contextual pop-ups.
/// Shows hints for first-time actions.
/// 
/// Reference: KB Section VIII - UI/UX
/// </summary>
public class TutorialSystem : MonoBehaviour
{
    public static TutorialSystem Instance { get; private set; }

    [System.Serializable]
    public class TutorialStep
    {
        public string id;
        public string message;
        public bool shown;
        public KeyCode highlightKey;
    }

    [Header("Tutorial Steps")]
    [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();

    [Header("UI")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private CanvasGroup popupGroup;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float displayDuration = 4f;

    [Header("Settings")]
    [SerializeField] private bool tutorialEnabled = true;

    private Coroutine _currentPopup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeSteps();
        LoadProgress();
    }

    private void Start()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        // Show movement tutorial on start
        StartCoroutine(DelayedTutorial("movement", 2f));
    }

    private void InitializeSteps()
    {
        steps.Clear();

        steps.Add(new TutorialStep
        {
            id = "movement",
            message = "Use <color=#00FFFF>WASD</color> to move around the sphere",
            highlightKey = KeyCode.W
        });

        steps.Add(new TutorialStep
        {
            id = "aim",
            message = "Move your <color=#FF00FF>MOUSE</color> to aim",
            highlightKey = KeyCode.None
        });

        steps.Add(new TutorialStep
        {
            id = "shoot_trash",
            message = "Hold <color=#00FF00>LEFT CLICK</color> to fire trash projectiles",
            highlightKey = KeyCode.Mouse0
        });

        steps.Add(new TutorialStep
        {
            id = "shoot_bomb",
            message = "Press <color=#FFFF00>RIGHT CLICK</color> to launch a bombshell",
            highlightKey = KeyCode.Mouse1
        });

        steps.Add(new TutorialStep
        {
            id = "dash",
            message = "Press <color=#00FFFF>SPACE</color> to dash and avoid attacks",
            highlightKey = KeyCode.Space
        });

        steps.Add(new TutorialStep
        {
            id = "bullet_time",
            message = "Press <color=#FF00FF>Q</color> to activate bullet time",
            highlightKey = KeyCode.Q
        });

        steps.Add(new TutorialStep
        {
            id = "pause",
            message = "Press <color=#FFFFFF>ESC</color> to pause the game",
            highlightKey = KeyCode.Escape
        });

        steps.Add(new TutorialStep
        {
            id = "pickup_bomb",
            message = "Walk over grounded <color=#FFFF00>BOMBSHELLS</color> to retrieve them",
            highlightKey = KeyCode.None
        });

        steps.Add(new TutorialStep
        {
            id = "health_pickup",
            message = "Collect <color=#00FF00>GREEN ORBS</color> to restore health",
            highlightKey = KeyCode.None
        });

        steps.Add(new TutorialStep
        {
            id = "combo",
            message = "Chain kills to build <color=#FF00FF>COMBO</color> and multiply your score!",
            highlightKey = KeyCode.None
        });
    }

    private System.Collections.IEnumerator DelayedTutorial(string id, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowTutorial(id);
    }

    /// <summary>
    /// Shows a tutorial popup if not already shown.
    /// </summary>
    public void ShowTutorial(string id)
    {
        if (!tutorialEnabled) return;

        TutorialStep step = steps.Find(s => s.id == id);
        if (step == null || step.shown) return;

        step.shown = true;
        SaveProgress();

        if (_currentPopup != null)
            StopCoroutine(_currentPopup);

        _currentPopup = StartCoroutine(ShowPopup(step.message));
    }

    private System.Collections.IEnumerator ShowPopup(string message)
    {
        if (popupPanel == null || popupText == null) yield break;

        popupText.text = message;
        popupPanel.SetActive(true);

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (popupGroup != null)
                popupGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        // Hold
        yield return new WaitForSecondsRealtime(displayDuration);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (popupGroup != null)
                popupGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        popupPanel.SetActive(false);
    }

    /// <summary>
    /// Triggers tutorial based on player actions.
    /// </summary>
    public void OnPlayerAction(string action)
    {
        switch (action)
        {
            case "first_move":
                ShowTutorial("aim");
                break;
            case "first_aim":
                ShowTutorial("shoot_trash");
                break;
            case "first_trash":
                ShowTutorial("shoot_bomb");
                break;
            case "first_bomb":
                ShowTutorial("dash");
                break;
            case "first_dash":
                ShowTutorial("bullet_time");
                break;
            case "first_hit":
                ShowTutorial("combo");
                break;
            case "first_pickup":
                ShowTutorial("health_pickup");
                break;
        }
    }

    /// <summary>
    /// Resets all tutorials.
    /// </summary>
    public void ResetTutorials()
    {
        foreach (var step in steps)
            step.shown = false;
        SaveProgress();
    }

    private void SaveProgress()
    {
        foreach (var step in steps)
            PlayerPrefs.SetInt($"tutorial_{step.id}", step.shown ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        foreach (var step in steps)
            step.shown = PlayerPrefs.GetInt($"tutorial_{step.id}", 0) == 1;
    }

    public void SetEnabled(bool enabled) => tutorialEnabled = enabled;
}
