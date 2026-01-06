using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent scene manager that handles transitions between scenes.
/// 
/// Reference: KB Section IX - Game Flow
/// </summary>
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }
    
    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameScene = "SampleScene";
    
    [Header("Transition")]
    [SerializeField] private float transitionDelay = 0.5f;
    
    [Header("Loading")]
    [SerializeField] private GameObject loadingScreen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadSceneAsync(mainMenuScene));
    }

    public void LoadGame()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadSceneAsync(gameScene));
    }

    public void RestartCurrentScene()
    {
        Time.timeScale = 1f;
        string currentScene = SceneManager.GetActiveScene().name;
        StartCoroutine(LoadSceneAsync(currentScene));
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);
            
        yield return new WaitForSeconds(transitionDelay);
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            // Could update a loading bar here
            yield return null;
        }
        
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }
}
