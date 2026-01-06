using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Main menu controller with start game, settings, and quit functions.
/// 
/// Reference: KB Section VIII - UI/UX
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    
    [Header("Main Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button settingsBackButton;
    
    [Header("Credits")]
    [SerializeField] private Button creditsBackButton;
    
    [Header("Game Scene")]
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private int gameSceneIndex = 1;

    private void Start()
    {
        // Show main panel by default
        ShowMainPanel();
        
        // Setup button listeners
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);
        if (creditsButton != null)
            creditsButton.onClick.AddListener(ShowCredits);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(ShowMainPanel);
        if (creditsBackButton != null)
            creditsBackButton.onClick.AddListener(ShowMainPanel);
            
        // Setup volume sliders
        SetupVolumeSliders();
        
        // Ensure time is normal
        Time.timeScale = 1f;
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    private void ShowSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    private void ShowCredits()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    private void PlayGame()
    {
        // Try to load by name first, then by index
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameSceneIndex);
        }
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void SetupVolumeSliders()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = master;
            masterVolumeSlider.onValueChanged.AddListener(v => {
                PlayerPrefs.SetFloat("MasterVolume", v);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.SetMasterVolume(v);
            });
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = music;
            musicVolumeSlider.onValueChanged.AddListener(v => {
                PlayerPrefs.SetFloat("MusicVolume", v);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.SetMusicVolume(v);
            });
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfx;
            sfxVolumeSlider.onValueChanged.AddListener(v => {
                PlayerPrefs.SetFloat("SFXVolume", v);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.SetSFXVolume(v);
            });
        }
    }
}
