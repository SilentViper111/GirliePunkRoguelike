using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized audio manager for all game sounds.
/// Handles music, SFX, and spatial audio.
/// 
/// Reference: KB Section VII - Audio
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Volume Settings")]
    [SerializeField] [Range(0, 1)] private float masterVolume = 1f;
    [SerializeField] [Range(0, 1)] private float musicVolume = 0.5f;
    [SerializeField] [Range(0, 1)] private float sfxVolume = 0.8f;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip shootTrash;
    [SerializeField] private AudioClip shootBomb;
    [SerializeField] private AudioClip hitEnemy;
    [SerializeField] private AudioClip hitPlayer;
    [SerializeField] private AudioClip enemyDeath;
    [SerializeField] private AudioClip playerDeath;
    [SerializeField] private AudioClip bombPickup;
    [SerializeField] private AudioClip healthPickup;
    [SerializeField] private AudioClip uiClick;
    
    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    
    // Pool of audio sources for overlapping sounds
    private List<AudioSource> _sfxPool = new List<AudioSource>();
    private int _poolSize = 10;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Create audio sources if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        
        // Create SFX pool
        for (int i = 0; i < _poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _sfxPool.Add(source);
        }
        
        UpdateVolumes();
    }

    private void Start()
    {
        if (backgroundMusic != null)
            PlayMusic(backgroundMusic);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        
        musicSource.clip = clip;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableSource();
        source.clip = clip;
        source.volume = sfxVolume * masterVolume * volumeScale;
        source.pitch = Random.Range(0.95f, 1.05f); // Slight variation
        source.Play();
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null) return;
        
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume * volumeScale);
    }

    // Convenience methods
    public void PlayShootTrash() => PlaySFX(shootTrash);
    public void PlayShootBomb() => PlaySFX(shootBomb);
    public void PlayHitEnemy() => PlaySFX(hitEnemy);
    public void PlayHitPlayer() => PlaySFX(hitPlayer);
    public void PlayEnemyDeath() => PlaySFX(enemyDeath);
    public void PlayPlayerDeath() => PlaySFX(playerDeath);
    public void PlayBombPickup() => PlaySFX(bombPickup);
    public void PlayHealthPickup() => PlaySFX(healthPickup);
    public void PlayUIClick() => PlaySFX(uiClick);

    private AudioSource GetAvailableSource()
    {
        foreach (var source in _sfxPool)
        {
            if (!source.isPlaying)
                return source;
        }
        return sfxSource; // Fallback
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    private void UpdateVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
    }
}
