using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("🎵 Audio Setup")]
    public AudioMixer masterMixer;
    
    [Header("🔊 Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;
    
    [Header("🎼 Background Music")]
    public AudioClip[] backgroundTracks;
    public bool playMusicOnStart = true;
    public bool loopMusic = true;
    
    private static AudioManager instance;
    public static AudioManager Instance 
    { 
        get 
        {
            if (instance == null)
                instance = FindObjectOfType<AudioManager>();
            return instance;
        }
    }
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        SetupAudioSources();
    }
    
    void Start()
    {
        LoadVolumeSettings();
        
        if (playMusicOnStart && backgroundTracks.Length > 0)
        {
            PlayBackgroundMusic(0);
        }
    }
    
    void SetupAudioSources()
    {
        // Configurar fuentes de audio si no están asignadas
        if (musicSource == null)
        {
            GameObject musicGO = new GameObject("Music Source");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
            musicSource.loop = loopMusic;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFX Source");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        
        if (uiSource == null)
        {
            GameObject uiGO = new GameObject("UI Source");
            uiGO.transform.SetParent(transform);
            uiSource = uiGO.AddComponent<AudioSource>();
            uiSource.playOnAwake = false;
        }
        
        // Asignar al mixer si existe
        if (masterMixer != null)
        {
            musicSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups("Music")[0];
            sfxSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups("SFX")[0];
            uiSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups("UI")[0];
        }
    }
    
    #region 🎵 Volume Control
    
    public void SetMasterVolume(float volume)
    {
        if (masterMixer != null)
        {
            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            masterMixer.SetFloat("MasterVolume", dB);
        }
        
        PlayerPrefs.SetFloat("MasterVolume", volume);
        Debug.Log($"🔊 Volumen maestro: {Mathf.RoundToInt(volume * 100)}%");
    }
    
    public void SetMusicVolume(float volume)
    {
        if (masterMixer != null)
        {
            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            masterMixer.SetFloat("MusicVolume", dB);
        }
        
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        if (masterMixer != null)
        {
            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            masterMixer.SetFloat("SFXVolume", dB);
        }
        
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    
    void LoadVolumeSettings()
    {
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        
        SetMasterVolume(masterVol);
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
    }
    
    #endregion
    
    #region 🎼 Music Control
    
    public void PlayBackgroundMusic(int trackIndex)
    {
        if (trackIndex >= 0 && trackIndex < backgroundTracks.Length)
        {
            musicSource.clip = backgroundTracks[trackIndex];
            musicSource.Play();
            Debug.Log($"🎵 Reproduciendo: {backgroundTracks[trackIndex].name}");
        }
    }
    
    public void StopMusic()
    {
        musicSource.Stop();
    }
    
    public void PauseMusic()
    {
        musicSource.Pause();
    }
    
    public void ResumeMusic()
    {
        musicSource.UnPause();
    }
    
    #endregion
    
    #region 🔊 Sound Effects
    
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
    
    public void PlayUISFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            uiSource.PlayOneShot(clip, volume);
        }
    }
    
    // Método para los botones de UI
    public void PlayButtonClick()
    {
        // Puedes asignar un clip de click aquí o usar uno por defecto
        // PlayUISFX(buttonClickClip);
    }
    
    #endregion
    
    #region 🎮 Game Integration
    
    public void OnGamePaused()
    {
        PauseMusic();
        Debug.Log("🔇 Audio pausado");
    }
    
    public void OnGameResumed()
    {
        ResumeMusic();
        Debug.Log("🔊 Audio reanudado");
    }
    
    #endregion
} 