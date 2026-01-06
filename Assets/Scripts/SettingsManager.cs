using UnityEngine;

/// <summary>
/// Settings manager for game options.
/// Persists settings using PlayerPrefs.
/// 
/// Reference: KB Section VIII - UI/UX
/// </summary>
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("Graphics")]
    [SerializeField] private bool screenShakeEnabled = true;
    [SerializeField] private bool postProcessingEnabled = true;
    [SerializeField] private int qualityLevel = 2;

    [Header("Gameplay")]
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private bool tutorialsEnabled = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    private void Start()
    {
        ApplySettings();
    }

    /// <summary>
    /// Applies all current settings.
    /// </summary>
    public void ApplySettings()
    {
        // Audio
        AudioListener.volume = masterVolume;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(musicVolume);
            AudioManager.Instance.SetSFXVolume(sfxVolume);
        }

        // Graphics
        if (ScreenShake.Instance != null)
            ScreenShake.Instance.enabled = screenShakeEnabled;

        if (PostProcessingController.Instance != null)
            PostProcessingController.Instance.enabled = postProcessingEnabled;

        QualitySettings.SetQualityLevel(qualityLevel);

        // Gameplay
        if (TutorialSystem.Instance != null)
            TutorialSystem.Instance.SetEnabled(tutorialsEnabled);

        Debug.Log("[Settings] Applied settings");
    }

    // === Setters ===

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        AudioListener.volume = masterVolume;
        SaveSettings();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(musicVolume);
        SaveSettings();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(sfxVolume);
        SaveSettings();
    }

    public void SetScreenShakeEnabled(bool enabled)
    {
        screenShakeEnabled = enabled;
        if (ScreenShake.Instance != null)
            ScreenShake.Instance.enabled = enabled;
        SaveSettings();
    }

    public void SetPostProcessingEnabled(bool enabled)
    {
        postProcessingEnabled = enabled;
        if (PostProcessingController.Instance != null)
            PostProcessingController.Instance.enabled = enabled;
        SaveSettings();
    }

    public void SetMouseSensitivity(float value)
    {
        mouseSensitivity = value;
        SaveSettings();
    }

    public void SetTutorialsEnabled(bool enabled)
    {
        tutorialsEnabled = enabled;
        if (TutorialSystem.Instance != null)
            TutorialSystem.Instance.SetEnabled(enabled);
        SaveSettings();
    }

    // === Getters ===

    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    public bool GetScreenShakeEnabled() => screenShakeEnabled;
    public bool GetPostProcessingEnabled() => postProcessingEnabled;
    public float GetMouseSensitivity() => mouseSensitivity;
    public bool GetTutorialsEnabled() => tutorialsEnabled;

    // === Persistence ===

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("settings_master_volume", masterVolume);
        PlayerPrefs.SetFloat("settings_music_volume", musicVolume);
        PlayerPrefs.SetFloat("settings_sfx_volume", sfxVolume);
        PlayerPrefs.SetInt("settings_screen_shake", screenShakeEnabled ? 1 : 0);
        PlayerPrefs.SetInt("settings_post_processing", postProcessingEnabled ? 1 : 0);
        PlayerPrefs.SetInt("settings_quality", qualityLevel);
        PlayerPrefs.SetFloat("settings_mouse_sensitivity", mouseSensitivity);
        PlayerPrefs.SetInt("settings_tutorials", tutorialsEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("settings_master_volume", 1f);
        musicVolume = PlayerPrefs.GetFloat("settings_music_volume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("settings_sfx_volume", 1f);
        screenShakeEnabled = PlayerPrefs.GetInt("settings_screen_shake", 1) == 1;
        postProcessingEnabled = PlayerPrefs.GetInt("settings_post_processing", 1) == 1;
        qualityLevel = PlayerPrefs.GetInt("settings_quality", 2);
        mouseSensitivity = PlayerPrefs.GetFloat("settings_mouse_sensitivity", 1f);
        tutorialsEnabled = PlayerPrefs.GetInt("settings_tutorials", 1) == 1;
    }

    /// <summary>
    /// Resets all settings to defaults.
    /// </summary>
    public void ResetToDefaults()
    {
        masterVolume = 1f;
        musicVolume = 0.7f;
        sfxVolume = 1f;
        screenShakeEnabled = true;
        postProcessingEnabled = true;
        qualityLevel = 2;
        mouseSensitivity = 1f;
        tutorialsEnabled = true;

        SaveSettings();
        ApplySettings();
    }
}
