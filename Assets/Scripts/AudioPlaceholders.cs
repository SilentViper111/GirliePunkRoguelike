using UnityEngine;

/// <summary>
/// Generates runtime audio clips for placeholder sounds.
/// Used until real audio assets are imported.
/// 
/// Reference: KB Section IV - Audio
/// </summary>
public class AudioPlaceholders : MonoBehaviour
{
    public static AudioPlaceholders Instance { get; private set; }

    [Header("Generated Clips")]
    public AudioClip shootTrashClip;
    public AudioClip shootBombClip;
    public AudioClip hitEnemyClip;
    public AudioClip enemyDeathClip;
    public AudioClip playerHitClip;
    public AudioClip playerDeathClip;
    public AudioClip healthPickupClip;
    public AudioClip bombPickupClip;
    public AudioClip uiClickClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        GeneratePlaceholderClips();
    }

    private void Start()
    {
        // Assign to AudioManager
        AssignToAudioManager();
    }

    private void GeneratePlaceholderClips()
    {
        // Simple synthesized sounds
        shootTrashClip = GenerateTone(0.1f, 800f, 0.3f); // Short high pop
        shootBombClip = GenerateTone(0.2f, 200f, 0.5f);  // Low thump
        hitEnemyClip = GenerateTone(0.05f, 1000f, 0.2f); // Very short high
        enemyDeathClip = GenerateNoise(0.2f, 0.4f);      // Short noise burst
        playerHitClip = GenerateTone(0.15f, 400f, 0.5f); // Mid hurt sound
        playerDeathClip = GenerateTone(0.5f, 150f, 0.7f); // Long low death
        healthPickupClip = GenerateUpChirp(0.2f);        // Rising tone
        bombPickupClip = GenerateTone(0.1f, 300f, 0.4f); // Click
        uiClickClip = GenerateTone(0.05f, 1200f, 0.2f);  // UI click

        Debug.Log("[AudioPlaceholders] Generated 9 placeholder audio clips");
    }

    private AudioClip GenerateTone(float duration, float frequency, float volume)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (t / duration); // Fade out
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
        }

        AudioClip clip = AudioClip.Create("Tone", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateNoise(float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (t / duration);
            data[i] = Random.Range(-1f, 1f) * volume * envelope;
        }

        AudioClip clip = AudioClip.Create("Noise", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateUpChirp(float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = Mathf.Lerp(400f, 1200f, t / duration);
            float envelope = 1f - (t / duration);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f * envelope;
        }

        AudioClip clip = AudioClip.Create("Chirp", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private void AssignToAudioManager()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[AudioPlaceholders] AudioManager not found!");
            return;
        }

        // Use reflection or public setters to assign clips
        // For now, just log that they're ready
        Debug.Log("[AudioPlaceholders] Clips ready for AudioManager assignment");
    }
}
