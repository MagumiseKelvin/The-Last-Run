using UnityEngine;

/// <summary>
/// Phase 4/5 — Manages all game audio: background music and sound effects.
/// Uses two AudioSources — one for music (looping), one for SFX (one-shot).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // ── Audio Sources ─────────────────────────────────────────────────────────
    [Header("Audio Sources")]
    [Tooltip("AudioSource used for background music")]
    public AudioSource musicSource;
    [Tooltip("AudioSource used for sound effects")]
    public AudioSource sfxSource;

    // ── Music Clips ───────────────────────────────────────────────────────────
    [Header("Music")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;
    public AudioClip gameOverMusic;

    // ── SFX Clips ─────────────────────────────────────────────────────────────
    [Header("Sound Effects")]
    public AudioClip coinPickupSFX;
    public AudioClip relicPickupSFX;
    public AudioClip jumpSFX;
    public AudioClip slideSFX;
    public AudioClip collisionSFX;
    public AudioClip countdownSFX;
    public AudioClip newHighScoreSFX;

    // ── Volume ────────────────────────────────────────────────────────────────
    [Header("Volume")]
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private const string MUSIC_VOL_KEY = "TheLastRun_MusicVol";
    private const string SFX_VOL_KEY   = "TheLastRun_SFXVol";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved volumes
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0.6f);
        sfxVolume   = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);

        ApplyVolumes();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart += HandleGameStart;
            GameManager.Instance.OnGameOver  += HandleGameOver;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart -= HandleGameStart;
            GameManager.Instance.OnGameOver  -= HandleGameOver;
        }
    }

    // ── Music Control ─────────────────────────────────────────────────────────

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource?.Stop();
    }

    private void HandleGameStart()
    {
        PlayMusic(gameplayMusic);
    }

    private void HandleGameOver()
    {
        PlayMusic(gameOverMusic);
        PlaySFX(collisionSFX);
    }

    // ── SFX Control ───────────────────────────────────────────────────────────

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayCoinPickup()   => PlaySFX(coinPickupSFX);
    public void PlayRelicPickup()  => PlaySFX(relicPickupSFX);
    public void PlayJump()         => PlaySFX(jumpSFX);
    public void PlaySlide()        => PlaySFX(slideSFX);
    public void PlayCollision()    => PlaySFX(collisionSFX);
    public void PlayNewHighScore() => PlaySFX(newHighScoreSFX);

    // ── Volume Settings ───────────────────────────────────────────────────────

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, musicVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        PlayerPrefs.SetFloat(SFX_VOL_KEY, sfxVolume);
        PlayerPrefs.Save();
    }

    private void ApplyVolumes()
    {
        if (musicSource != null) musicSource.volume = musicVolume;
        if (sfxSource != null)   sfxSource.volume   = sfxVolume;
    }
}
