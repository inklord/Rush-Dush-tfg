using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("🎵 Audio Settings")]
    public AudioMixer audioMixer;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;
    
    [Header("🖥️ Display Settings")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    
    [Header("🎮 Menu Buttons")]
    public Button backToGameButton;
    public Button quitGameButton;
    public Button applyButton;
    
    [Header("📱 UI Panels")]
    public GameObject optionsPanel;
    public Canvas gameUI; // Para desactivar la UI del juego cuando está abierto el menú
    
    // Variables privadas
    private Resolution[] resolutions;
    private bool isGamePaused = false;
    private float originalTimeScale;
    private PersistentSettingsManager.GameSettings cachedSettings;
    
    void Start()
    {
        SetupResolutions();
        LoadSettingsFromPersistentManager();
        SetupButtons();
        
        // El menú empieza cerrado
        optionsPanel.SetActive(false);
    }
    
    void Update()
    {
        // ESC key handling moved to UniversalOptionsHandler for consistency
    }
    
    #region 🎵 Volume Control
    
    public void SetVolume(float volume)
    {
        // Usar PersistentSettingsManager para mantener consistencia
        if (PersistentSettingsManager.Instance != null)
        {
            PersistentSettingsManager.Instance.SetMasterVolume(volume);
        }
        else
        {
            // Fallback al método anterior
            SetVolumeFallback(volume);
        }
        
        // Actualizar texto del volumen
        if (volumeText != null)
        {
            volumeText.text = $"Volumen: {Mathf.RoundToInt(volume * 100)}%";
        }
        
        DebugLog($"🔊 Volumen configurado: {Mathf.RoundToInt(volume * 100)}%");
    }
    
    void SetVolumeFallback(float volume)
    {
        // Convertir de 0-1 a dB (-80 a 0)
        float dB = Mathf.Log10(volume) * 20;
        if (volume == 0)
            dB = -80f;
            
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MasterVolume", dB);
        }
        
        // Guardar configuración
        PlayerPrefs.SetFloat("Volume", volume);
        
        // Sincronizar con GlobalOptionsManager si existe
        if (GlobalOptionsManager.Instance != null)
        {
            GlobalOptionsManager.Instance.SaveGlobalSettings(
                volume, 
                PlayerPrefs.GetInt("ResolutionIndex", -1), 
                Screen.fullScreen
            );
        }
    }
    
    #endregion
    
    #region 🖥️ Resolution Control
    
    void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            
            List<string> options = new List<string>();
            int currentResolutionIndex = 0;
            
            // Filtrar resoluciones duplicadas y mantener solo las más altas refresh rates
            List<Resolution> uniqueResolutions = new List<Resolution>();
            for (int i = 0; i < resolutions.Length; i++)
            {
                bool found = false;
                for (int j = 0; j < uniqueResolutions.Count; j++)
                {
                    if (resolutions[i].width == uniqueResolutions[j].width && 
                        resolutions[i].height == uniqueResolutions[j].height)
                    {
                        // Si encontramos la misma resolución, mantener la de mayor refresh rate
                        if (resolutions[i].refreshRateRatio.value > uniqueResolutions[j].refreshRateRatio.value)
                        {
                            uniqueResolutions[j] = resolutions[i];
                        }
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    uniqueResolutions.Add(resolutions[i]);
                }
            }
            
            resolutions = uniqueResolutions.ToArray();
            
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio.value + "Hz";
                options.Add(option);
                
                // Encontrar la resolución actual
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }
    
    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            
            // Usar PersistentSettingsManager para mantener consistencia
            if (PersistentSettingsManager.Instance != null)
            {
                PersistentSettingsManager.Instance.SetResolution(
                    resolution.width, 
                    resolution.height, 
                    Screen.fullScreen
                );
            }
            else
            {
                // Fallback al método anterior
                Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
                PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
                
                // Sincronizar con GlobalOptionsManager si existe
                if (GlobalOptionsManager.Instance != null)
                {
                    GlobalOptionsManager.Instance.SaveGlobalSettings(
                        PlayerPrefs.GetFloat("Volume", 0.75f), 
                        resolutionIndex, 
                        Screen.fullScreen
                    );
                }
            }
            
            DebugLog($"🖥️ Resolución cambiada a: {resolution.width}x{resolution.height}@{resolution.refreshRateRatio.value}Hz");
        }
    }
    
    public void SetFullscreen(bool isFullscreen)
    {
        // Usar PersistentSettingsManager para mantener consistencia
        if (PersistentSettingsManager.Instance != null)
        {
            var settings = PersistentSettingsManager.Instance.GetAllSettings();
            PersistentSettingsManager.Instance.SetResolution(
                settings.resolutionWidth,
                settings.resolutionHeight,
                isFullscreen
            );
        }
        else
        {
            // Fallback al método anterior
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
            
            // Sincronizar con GlobalOptionsManager si existe
            if (GlobalOptionsManager.Instance != null)
            {
                GlobalOptionsManager.Instance.SaveGlobalSettings(
                    PlayerPrefs.GetFloat("Volume", 0.75f), 
                    PlayerPrefs.GetInt("ResolutionIndex", -1), 
                    isFullscreen
                );
            }
        }
        
        DebugLog($"🖥️ Pantalla completa: {isFullscreen}");
    }
    
    #endregion
    
    #region 🎮 Menu Navigation
    
    void SetupButtons()
    {
        // Configurar botones
        if (backToGameButton != null)
            backToGameButton.onClick.AddListener(BackToGame);
        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(QuitGame);
        if (applyButton != null)
            applyButton.onClick.AddListener(ApplySettings);
        
        // Configurar sliders y dropdowns
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(SetVolume);
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }
    
    public void ToggleOptionsMenu()
    {
        bool isActive = optionsPanel.activeSelf;
        optionsPanel.SetActive(!isActive);
        
        if (!isActive)
        {
            // Abrir menú - pausar juego y cargar configuraciones actuales
            LoadSettingsFromPersistentManager();
            PauseGame();
        }
        else
        {
            // Cerrar menú - continuar juego
            ResumeGame();
        }
    }
    
    public void BackToGame()
    {
        optionsPanel.SetActive(false);
        ResumeGame();
        DebugLog("🎮 Volviendo al juego...");
    }
    
    public void QuitGame()
    {
        DebugLog("🚪 Cerrando juego...");
        
        // Guardar configuraciones antes de salir
        if (PersistentSettingsManager.Instance != null)
        {
            PersistentSettingsManager.Instance.SaveAllSettings();
        }
        PlayerPrefs.Save();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void ApplySettings()
    {
        // Forzar guardado inmediato
        if (PersistentSettingsManager.Instance != null)
        {
            PersistentSettingsManager.Instance.SaveAllSettings();
            PersistentSettingsManager.Instance.ApplyAllSettings();
        }
        else
        {
            PlayerPrefs.Save();
        }
        
        DebugLog("💾 Configuraciones aplicadas y guardadas");
        
        // Mostrar feedback visual
        StartCoroutine(ShowAppliedFeedback());
    }
    
    #endregion
    
    #region ⏸️ Game Pause System
    
    void PauseGame()
    {
        originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isGamePaused = true;
        
        // Desactivar UI del juego si existe
        if (gameUI != null)
            gameUI.enabled = false;
            
        DebugLog("⏸️ Juego pausado");
    }
    
    void ResumeGame()
    {
        Time.timeScale = originalTimeScale;
        isGamePaused = false;
        
        // Reactivar UI del juego
        if (gameUI != null)
            gameUI.enabled = true;
            
        DebugLog("▶️ Juego reanudado");
    }
    
    #endregion
    
    #region 💾 Settings Persistence Integration
    
    void LoadSettingsFromPersistentManager()
    {
        if (PersistentSettingsManager.Instance != null)
        {
            cachedSettings = PersistentSettingsManager.Instance.GetAllSettings();
            UpdateUIFromSettings();
        }
        else
        {
            LoadSettingsFallback();
        }
    }
    
    void UpdateUIFromSettings()
    {
        if (cachedSettings == null) return;
        
        // Actualizar volumen
        if (volumeSlider != null)
        {
            volumeSlider.value = cachedSettings.masterVolume;
        }
        if (volumeText != null)
        {
            volumeText.text = $"Volumen: {Mathf.RoundToInt(cachedSettings.masterVolume * 100)}%";
        }
        
        // Actualizar pantalla completa
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = cachedSettings.fullscreen;
        }
        
        // Actualizar resolución
        if (resolutionDropdown != null)
        {
            int resIndex = FindResolutionIndex(cachedSettings.resolutionWidth, cachedSettings.resolutionHeight);
            if (resIndex >= 0)
            {
                resolutionDropdown.value = resIndex;
            }
        }
        
        DebugLog("🔄 UI actualizada con configuraciones persistentes");
    }
    
    int FindResolutionIndex(int width, int height)
    {
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == width && resolutions[i].height == height)
            {
                return i;
            }
        }
        return -1;
    }
    
    void LoadSettingsFallback()
    {
        // Cargar volumen
        float savedVolume = PlayerPrefs.GetFloat("Volume", 0.75f);
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            SetVolume(savedVolume);
        }
        
        // Cargar resolución
        int savedResolution = PlayerPrefs.GetInt("ResolutionIndex", resolutions.Length - 1);
        if (savedResolution < resolutions.Length && resolutionDropdown != null)
        {
            resolutionDropdown.value = savedResolution;
            SetResolution(savedResolution);
        }
        
        // Cargar pantalla completa
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = savedFullscreen;
            SetFullscreen(savedFullscreen);
        }
        
        DebugLog("💾 Configuraciones cargadas (fallback)");
    }
    
    #endregion
    
    #region 🎨 UI Feedback
    
    System.Collections.IEnumerator ShowAppliedFeedback()
    {
        if (applyButton != null)
        {
            TextMeshProUGUI buttonText = applyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                string originalText = buttonText.text;
                buttonText.text = "✅ ¡Aplicado!";
                applyButton.interactable = false;
                
                yield return new WaitForSecondsRealtime(1.5f);
                
                buttonText.text = originalText;
                applyButton.interactable = true;
            }
        }
    }
    
    #endregion
    
    #region 🔄 Integration Methods
    
    /// <summary>
    /// 🔗 Método para sincronizar con sistemas externos
    /// </summary>
    public void RefreshFromPersistentSettings()
    {
        LoadSettingsFromPersistentManager();
    }
    
    /// <summary>
    /// 📊 Obtener estado actual del menú
    /// </summary>
    public bool IsMenuOpen()
    {
        return optionsPanel != null && optionsPanel.activeSelf;
    }
    
    /// <summary>
    /// 🎮 Configurar referencia de AudioMixer automáticamente
    /// </summary>
    public void AutoSetupAudioMixer()
    {
        if (audioMixer == null && PersistentSettingsManager.Instance != null)
        {
            // Intentar obtener AudioMixer del PersistentSettingsManager
            var psm = PersistentSettingsManager.Instance;
            if (psm.masterAudioMixer != null)
            {
                audioMixer = psm.masterAudioMixer;
                DebugLog("🎵 AudioMixer configurado automáticamente");
            }
        }
    }
    
    #endregion
    
    #region 🐛 Debug Methods
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void DebugLog(string message)
    {
        Debug.Log($"[OptionsMenu] {message}");
    }
    
    // Método para testing con teclas
    void OnGUI()
    {
        if (!Application.isEditor) return;
        
        GUI.Label(new Rect(10, 10, 200, 20), $"Paused: {isGamePaused}");
        GUI.Label(new Rect(10, 30, 200, 20), $"TimeScale: {Time.timeScale}");
        if (volumeSlider != null)
        {
            GUI.Label(new Rect(10, 50, 200, 20), $"Volume: {volumeSlider.value:F2}");
        }
        if (cachedSettings != null)
        {
            GUI.Label(new Rect(10, 70, 200, 20), $"Persistent Volume: {cachedSettings.masterVolume:F2}");
        }
    }
    
    #endregion
} 