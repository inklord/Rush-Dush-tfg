using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 💾 Gestor de Configuraciones Persistentes
/// Mantiene TODAS las configuraciones del juego automáticamente
/// </summary>
[DefaultExecutionOrder(-100)]
public class PersistentSettingsManager : MonoBehaviour
{
    [Header("🎵 Audio Configuration")]
    public AudioMixer masterAudioMixer;
    
    [Header("📊 Debug Settings")]
    public bool enableDebugLogs = false;
    public bool showDebugUI = false;
    
    // Singleton
    private static PersistentSettingsManager instance;
    public static PersistentSettingsManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Buscar existente
                instance = FindObjectOfType<PersistentSettingsManager>();
                
                // Crear si no existe
                if (instance == null)
                {
                    GameObject go = new GameObject("PersistentSettingsManager");
                    instance = go.AddComponent<PersistentSettingsManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // 📊 Configuraciones del Juego
    [System.Serializable]
    public class GameSettings
    {
        [Header("🔊 Audio Settings")]
        public float masterVolume = 0.75f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.8f;
        public float uiVolume = 0.9f;
        public bool muteAudio = false;
        
        [Header("🖥️ Display Settings")]
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public bool fullscreen = false;
        public int refreshRate = 60;
        public int qualityLevel = 3;
        public bool vsync = true;
        
        [Header("🎮 Gameplay Settings")]
        public float mouseSensitivity = 1.0f;
        public bool invertMouse = false;
        public float fieldOfView = 60f;
        
        [Header("⌨️ Controls Settings")]
        public KeyCode pauseKey = KeyCode.Escape;
        public KeyCode optionsKey = KeyCode.O;
        public KeyCode screenshotKey = KeyCode.F12;
        
        [Header("📱 UI Settings")]
        public bool showFPS = false;
        public bool showDebugInfo = false;
        public float uiScale = 1.0f;
        
        [Header("🌐 Game Preferences")]
        public string playerName = "Player";
        public bool skipIntros = false;
        public bool autoSaveEnabled = true;
        public int autoSaveInterval = 300; // seconds
    }
    
    [SerializeField] private GameSettings currentSettings = new GameSettings();
    
    // Variables de estado
    #pragma warning disable 0414
    private bool isInitialized = false;
    #pragma warning restore 0414
    private bool hasUnsavedChanges = false;
    private List<Resolution> availableResolutions;
    
    #region 🚀 Initialization
    
    void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Suscribirse a eventos
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Aplicar configuraciones iniciales
        ApplyAllSettings();
        
        // Setup auto-save
        if (currentSettings.autoSaveEnabled)
        {
            InvokeRepeating(nameof(AutoSave), currentSettings.autoSaveInterval, currentSettings.autoSaveInterval);
        }
    }
    
    void OnDestroy()
    {
        // Guardar antes de destruir
        SaveAllSettings();
        
        // Desuscribirse
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void Initialize()
    {
        SetupResolutions();
        LoadAllSettings();
        isInitialized = true;
        
        LogDebug("💾 PersistentSettingsManager inicializado");
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LogDebug($"🔄 Aplicando configuraciones en escena: {scene.name}");
        
        // Aplicar configuraciones en la nueva escena
        ApplyAllSettings();
        
        // Buscar y configurar componentes de la escena
        ConfigureSceneComponents();
    }
    
    #endregion
    
    #region 💾 Save/Load System
    
    /// <summary>
    /// 📁 Cargar TODAS las configuraciones
    /// </summary>
    public void LoadAllSettings()
    {
        // Audio Settings
        currentSettings.masterVolume = PlayerPrefs.GetFloat("Settings_MasterVolume", 0.75f);
        currentSettings.musicVolume = PlayerPrefs.GetFloat("Settings_MusicVolume", 0.7f);
        currentSettings.sfxVolume = PlayerPrefs.GetFloat("Settings_SFXVolume", 0.8f);
        currentSettings.uiVolume = PlayerPrefs.GetFloat("Settings_UIVolume", 0.9f);
        currentSettings.muteAudio = PlayerPrefs.GetInt("Settings_MuteAudio", 0) == 1;
        
        // Display Settings
        currentSettings.resolutionWidth = PlayerPrefs.GetInt("Settings_ResolutionWidth", 1920);
        currentSettings.resolutionHeight = PlayerPrefs.GetInt("Settings_ResolutionHeight", 1080);
        currentSettings.fullscreen = PlayerPrefs.GetInt("Settings_Fullscreen", 0) == 1;
        currentSettings.refreshRate = PlayerPrefs.GetInt("Settings_RefreshRate", 60);
        currentSettings.qualityLevel = PlayerPrefs.GetInt("Settings_QualityLevel", 3);
        currentSettings.vsync = PlayerPrefs.GetInt("Settings_VSync", 1) == 1;
        
        // Gameplay Settings
        currentSettings.mouseSensitivity = PlayerPrefs.GetFloat("Settings_MouseSensitivity", 1.0f);
        currentSettings.invertMouse = PlayerPrefs.GetInt("Settings_InvertMouse", 0) == 1;
        currentSettings.fieldOfView = PlayerPrefs.GetFloat("Settings_FieldOfView", 60f);
        
        // Controls (usando int para KeyCode)
        currentSettings.pauseKey = (KeyCode)PlayerPrefs.GetInt("Settings_PauseKey", (int)KeyCode.Escape);
        currentSettings.optionsKey = (KeyCode)PlayerPrefs.GetInt("Settings_OptionsKey", (int)KeyCode.O);
        currentSettings.screenshotKey = (KeyCode)PlayerPrefs.GetInt("Settings_ScreenshotKey", (int)KeyCode.F12);
        
        // UI Settings
        currentSettings.showFPS = PlayerPrefs.GetInt("Settings_ShowFPS", 0) == 1;
        currentSettings.showDebugInfo = PlayerPrefs.GetInt("Settings_ShowDebugInfo", 0) == 1;
        currentSettings.uiScale = PlayerPrefs.GetFloat("Settings_UIScale", 1.0f);
        
        // Game Preferences
        currentSettings.playerName = PlayerPrefs.GetString("Settings_PlayerName", "Player");
        currentSettings.skipIntros = PlayerPrefs.GetInt("Settings_SkipIntros", 0) == 1;
        currentSettings.autoSaveEnabled = PlayerPrefs.GetInt("Settings_AutoSave", 1) == 1;
        currentSettings.autoSaveInterval = PlayerPrefs.GetInt("Settings_AutoSaveInterval", 300);
        
        LogDebug("📂 Todas las configuraciones cargadas");
    }
    
    /// <summary>
    /// 💾 Guardar TODAS las configuraciones
    /// </summary>
    public void SaveAllSettings()
    {
        // Audio Settings
        PlayerPrefs.SetFloat("Settings_MasterVolume", currentSettings.masterVolume);
        PlayerPrefs.SetFloat("Settings_MusicVolume", currentSettings.musicVolume);
        PlayerPrefs.SetFloat("Settings_SFXVolume", currentSettings.sfxVolume);
        PlayerPrefs.SetFloat("Settings_UIVolume", currentSettings.uiVolume);
        PlayerPrefs.SetInt("Settings_MuteAudio", currentSettings.muteAudio ? 1 : 0);
        
        // Display Settings
        PlayerPrefs.SetInt("Settings_ResolutionWidth", currentSettings.resolutionWidth);
        PlayerPrefs.SetInt("Settings_ResolutionHeight", currentSettings.resolutionHeight);
        PlayerPrefs.SetInt("Settings_Fullscreen", currentSettings.fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("Settings_RefreshRate", currentSettings.refreshRate);
        PlayerPrefs.SetInt("Settings_QualityLevel", currentSettings.qualityLevel);
        PlayerPrefs.SetInt("Settings_VSync", currentSettings.vsync ? 1 : 0);
        
        // Gameplay Settings
        PlayerPrefs.SetFloat("Settings_MouseSensitivity", currentSettings.mouseSensitivity);
        PlayerPrefs.SetInt("Settings_InvertMouse", currentSettings.invertMouse ? 1 : 0);
        PlayerPrefs.SetFloat("Settings_FieldOfView", currentSettings.fieldOfView);
        
        // Controls
        PlayerPrefs.SetInt("Settings_PauseKey", (int)currentSettings.pauseKey);
        PlayerPrefs.SetInt("Settings_OptionsKey", (int)currentSettings.optionsKey);
        PlayerPrefs.SetInt("Settings_ScreenshotKey", (int)currentSettings.screenshotKey);
        
        // UI Settings
        PlayerPrefs.SetInt("Settings_ShowFPS", currentSettings.showFPS ? 1 : 0);
        PlayerPrefs.SetInt("Settings_ShowDebugInfo", currentSettings.showDebugInfo ? 1 : 0);
        PlayerPrefs.SetFloat("Settings_UIScale", currentSettings.uiScale);
        
        // Game Preferences
        PlayerPrefs.SetString("Settings_PlayerName", currentSettings.playerName);
        PlayerPrefs.SetInt("Settings_SkipIntros", currentSettings.skipIntros ? 1 : 0);
        PlayerPrefs.SetInt("Settings_AutoSave", currentSettings.autoSaveEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Settings_AutoSaveInterval", currentSettings.autoSaveInterval);
        
        PlayerPrefs.Save();
        hasUnsavedChanges = false;
        
        LogDebug("💾 Todas las configuraciones guardadas");
    }
    
    void AutoSave()
    {
        if (hasUnsavedChanges)
        {
            SaveAllSettings();
            LogDebug("🔄 Auto-guardado ejecutado");
        }
    }
    
    #endregion
    
    #region ⚡ Apply Settings
    
    /// <summary>
    /// 🎯 Aplicar TODAS las configuraciones
    /// </summary>
    public void ApplyAllSettings()
    {
        ApplyAudioSettings();
        ApplyDisplaySettings();
        ApplyGameplaySettings();
        ApplyUISettings();
        
        LogDebug("✅ Todas las configuraciones aplicadas");
    }
    
    void ApplyAudioSettings()
    {
        if (masterAudioMixer != null)
        {
            // Master Volume
            float masterDB = currentSettings.muteAudio ? -80f : 
                           (currentSettings.masterVolume > 0 ? Mathf.Log10(currentSettings.masterVolume) * 20 : -80f);
            masterAudioMixer.SetFloat("MasterVolume", masterDB);
            
            // Music Volume
            float musicDB = currentSettings.musicVolume > 0 ? Mathf.Log10(currentSettings.musicVolume) * 20 : -80f;
            masterAudioMixer.SetFloat("MusicVolume", musicDB);
            
            // SFX Volume
            float sfxDB = currentSettings.sfxVolume > 0 ? Mathf.Log10(currentSettings.sfxVolume) * 20 : -80f;
            masterAudioMixer.SetFloat("SFXVolume", sfxDB);
            
            // UI Volume
            float uiDB = currentSettings.uiVolume > 0 ? Mathf.Log10(currentSettings.uiVolume) * 20 : -80f;
            masterAudioMixer.SetFloat("UIVolume", uiDB);
        }
        
        // Configurar AudioListener
        AudioListener.volume = currentSettings.muteAudio ? 0f : 1f;
    }
    
    void ApplyDisplaySettings()
    {
        // Resolución
        Screen.SetResolution(currentSettings.resolutionWidth, currentSettings.resolutionHeight, currentSettings.fullscreen);
        
        // Calidad gráfica
        QualitySettings.SetQualityLevel(currentSettings.qualityLevel, true);
        
        // VSync
        QualitySettings.vSyncCount = currentSettings.vsync ? 1 : 0;
        
        // Target frame rate (para builds)
        if (!currentSettings.vsync)
        {
            Application.targetFrameRate = currentSettings.refreshRate;
        }
    }
    
    void ApplyGameplaySettings()
    {
        // Mouse sensitivity se aplicará a los controles de jugador cuando sea necesario
        // Field of view se aplicará a las cámaras cuando sea necesario
    }
    
    void ApplyUISettings()
    {
        // UI Scale se aplicará a los Canvas cuando sea necesario
        ConfigureCanvasScalers();
    }
    
    #endregion
    
    #region 🔧 Component Configuration
    
    void ConfigureSceneComponents()
    {
        ConfigureCanvasScalers();
        ConfigureCameras();
        ConfigureAudioSources();
    }
    
    void ConfigureCanvasScalers()
    {
        CanvasScaler[] scalers = FindObjectsOfType<CanvasScaler>();
        foreach (CanvasScaler scaler in scalers)
        {
            if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                // Aplicar escala de UI
                scaler.scaleFactor = currentSettings.uiScale;
            }
        }
    }
    
    void ConfigureCameras()
    {
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)
        {
            if (cam.CompareTag("MainCamera") || cam.name.Contains("Main"))
            {
                cam.fieldOfView = currentSettings.fieldOfView;
            }
        }
    }
    
    void ConfigureAudioSources()
    {
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source.outputAudioMixerGroup == null && masterAudioMixer != null)
            {
                // Asignar grupo por defecto
                var groups = masterAudioMixer.FindMatchingGroups("Master");
                if (groups.Length > 0)
                {
                    source.outputAudioMixerGroup = groups[0];
                }
            }
        }
    }
    
    void SetupResolutions()
    {
        availableResolutions = new List<Resolution>();
        Resolution[] resolutions = Screen.resolutions;
        
        // Filtrar resoluciones duplicadas
        foreach (Resolution res in resolutions)
        {
            bool found = false;
            foreach (Resolution existing in availableResolutions)
            {
                if (existing.width == res.width && existing.height == res.height)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                availableResolutions.Add(res);
            }
        }
    }
    
    #endregion
    
    #region 🎮 Public API - Settings Getters/Setters
    
    public GameSettings GetAllSettings() => currentSettings;
    
    // Audio
    public void SetMasterVolume(float volume)
    {
        currentSettings.masterVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        MarkDirty();
    }
    
    public void SetMusicVolume(float volume)
    {
        currentSettings.musicVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        MarkDirty();
    }
    
    public void SetSFXVolume(float volume)
    {
        currentSettings.sfxVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        MarkDirty();
    }
    
    public void SetMuteAudio(bool mute)
    {
        currentSettings.muteAudio = mute;
        ApplyAudioSettings();
        MarkDirty();
    }
    
    // Display
    public void SetResolution(int width, int height, bool fullscreen)
    {
        currentSettings.resolutionWidth = width;
        currentSettings.resolutionHeight = height;
        currentSettings.fullscreen = fullscreen;
        ApplyDisplaySettings();
        MarkDirty();
    }
    
    public void SetQualityLevel(int level)
    {
        currentSettings.qualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
        ApplyDisplaySettings();
        MarkDirty();
    }
    
    public void SetVSync(bool enabled)
    {
        currentSettings.vsync = enabled;
        ApplyDisplaySettings();
        MarkDirty();
    }
    
    // Gameplay
    public void SetMouseSensitivity(float sensitivity)
    {
        currentSettings.mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 5f);
        MarkDirty();
    }
    
    public void SetFieldOfView(float fov)
    {
        currentSettings.fieldOfView = Mathf.Clamp(fov, 30f, 120f);
        ConfigureCameras();
        MarkDirty();
    }
    
    // UI
    public void SetShowFPS(bool show)
    {
        currentSettings.showFPS = show;
        MarkDirty();
    }
    
    public void SetPlayerName(string name)
    {
        currentSettings.playerName = name;
        MarkDirty();
    }
    
    void MarkDirty()
    {
        hasUnsavedChanges = true;
    }
    
    #endregion
    
    #region 🔄 Integration with Existing Systems
    
    /// <summary>
    /// 🔗 Sincronizar con GlobalOptionsManager existente
    /// </summary>
    public void SyncWithGlobalOptionsManager()
    {
        if (GlobalOptionsManager.Instance != null)
        {
            GlobalOptionsManager.Instance.SaveGlobalSettings(
                currentSettings.masterVolume,
                GetCurrentResolutionIndex(),
                currentSettings.fullscreen
            );
        }
    }
    
    /// <summary>
    /// 🔗 Sincronizar con ResolutionManager existente
    /// </summary>
    public void SyncWithResolutionManager()
    {
        if (ResolutionManager.Instance != null)
        {
            ResolutionManager.Instance.SetResolutionFromMenu(
                currentSettings.resolutionWidth,
                currentSettings.resolutionHeight,
                currentSettings.fullscreen
            );
        }
    }
    
    int GetCurrentResolutionIndex()
    {
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].width == currentSettings.resolutionWidth &&
                availableResolutions[i].height == currentSettings.resolutionHeight)
            {
                return i;
            }
        }
        return -1;
    }
    
    #endregion
    
    #region 🐛 Debug & Utilities
    
    void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[PersistentSettings] {message}");
        }
    }
    
    void OnGUI()
    {
        if (!showDebugUI || !Application.isEditor) return;
        
        GUILayout.BeginArea(new Rect(10, 200, 300, 400));
        GUILayout.Label("🔧 Persistent Settings Debug");
        GUILayout.Label($"Master Volume: {currentSettings.masterVolume:F2}");
        GUILayout.Label($"Resolution: {currentSettings.resolutionWidth}x{currentSettings.resolutionHeight}");
        GUILayout.Label($"Fullscreen: {currentSettings.fullscreen}");
        GUILayout.Label($"Quality: {QualitySettings.names[currentSettings.qualityLevel]}");
        GUILayout.Label($"Unsaved Changes: {hasUnsavedChanges}");
        
        if (GUILayout.Button("🔄 Reload Settings"))
        {
            LoadAllSettings();
            ApplyAllSettings();
        }
        
        if (GUILayout.Button("💾 Save Settings"))
        {
            SaveAllSettings();
        }
        
        if (GUILayout.Button("🎯 Apply All"))
        {
            ApplyAllSettings();
        }
        
        GUILayout.EndArea();
    }
    
    #endregion
} 