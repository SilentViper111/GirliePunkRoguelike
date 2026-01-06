using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Wave information display showing current wave, enemies remaining, and wave announcements.
/// 
/// Reference: KB Section VIII - UI/UX
/// </summary>
public class WaveDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI enemyCountText;
    [SerializeField] private TextMeshProUGUI announcementText;
    [SerializeField] private CanvasGroup announcementGroup;
    
    [Header("Animation")]
    [SerializeField] private float announcementDuration = 2f;
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float fadeOutTime = 0.5f;
    
    private EnemySpawner _spawner;
    private int _lastWave = 0;
    private Coroutine _announcementCoroutine;

    private void Start()
    {
        _spawner = FindFirstObjectByType<EnemySpawner>();
        
        if (announcementGroup != null)
            announcementGroup.alpha = 0f;
    }

    private void Update()
    {
        if (_spawner == null) return;
        
        int currentWave = _spawner.GetCurrentWave();
        int enemiesAlive = _spawner.GetEnemiesAlive();
        
        // Update wave text
        if (waveText != null)
            waveText.text = $"WAVE {currentWave}";
            
        // Update enemy count
        if (enemyCountText != null)
            enemyCountText.text = $"ENEMIES: {enemiesAlive}";
            
        // Check for new wave
        if (currentWave > _lastWave)
        {
            _lastWave = currentWave;
            ShowWaveAnnouncement(currentWave);
        }
    }

    private void ShowWaveAnnouncement(int wave)
    {
        if (announcementText == null || announcementGroup == null) return;
        
        if (_announcementCoroutine != null)
            StopCoroutine(_announcementCoroutine);
            
        _announcementCoroutine = StartCoroutine(AnimateAnnouncement(wave));
    }

    private System.Collections.IEnumerator AnimateAnnouncement(int wave)
    {
        announcementText.text = $"WAVE {wave}";
        
        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            announcementGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
            yield return null;
        }
        announcementGroup.alpha = 1f;
        
        // Hold
        yield return new WaitForSeconds(announcementDuration);
        
        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            announcementGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
            yield return null;
        }
        announcementGroup.alpha = 0f;
    }
}
