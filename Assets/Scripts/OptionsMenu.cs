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
    
    void Start()
    {
        SetupResolutions();
        LoadSettings();
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
        // Convertir de 0-1 a dB (-80 a 0)
        float dB = Mathf.Log10(volume) * 20;
        if (volume == 0)
            dB = -80f;
            
        audioMixer.SetFloat("MasterVolume", dB);
        
        // Actualizar texto del volumen
        volumeText.text = $"Volumen: {Mathf.RoundToInt(volume * 100)}%";
        
        // Guardar configuración globalmente
        PlayerPrefs.SetFloat("Volume", volume);
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
                    if (resolutions[i].refreshRate > uniqueResolutions[j].refreshRate)
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
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + "Hz";
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
    
    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            
            // Guardar configuración globalmente
            PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
            if (GlobalOptionsManager.Instance != null)
            {
                GlobalOptionsManager.Instance.SaveGlobalSettings(
                    PlayerPrefs.GetFloat("Volume", 0.75f), 
                    resolutionIndex, 
                    Screen.fullScreen
                );
            }
            
            DebugLog($"🖥️ Resolución cambiada a: {resolution.width}x{resolution.height}@{resolution.refreshRate}Hz");
        }
    }
    
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        
        // Guardar configuración globalmente
        if (GlobalOptionsManager.Instance != null)
        {
            GlobalOptionsManager.Instance.SaveGlobalSettings(
                PlayerPrefs.GetFloat("Volume", 0.75f), 
                PlayerPrefs.GetInt("ResolutionIndex", -1), 
                isFullscreen
            );
        }
        
        DebugLog($"🖥️ Pantalla completa: {isFullscreen}");
    }
    
    #endregion
    
    #region 🎮 Menu Navigation
    
    void SetupButtons()
    {
        // Configurar botones
        backToGameButton.onClick.AddListener(BackToGame);
        quitGameButton.onClick.AddListener(QuitGame);
        applyButton.onClick.AddListener(ApplySettings);
        
        // Configurar sliders y dropdowns
        volumeSlider.onValueChanged.AddListener(SetVolume);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }
    
    public void ToggleOptionsMenu()
    {
        bool isActive = optionsPanel.activeSelf;
        optionsPanel.SetActive(!isActive);
        
        if (!isActive)
        {
            // Abrir menú - pausar juego
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
        PlayerPrefs.Save();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void ApplySettings()
    {
        // Las configuraciones se aplican automáticamente, pero guardamos todo
        PlayerPrefs.Save();
        DebugLog("💾 Configuraciones guardadas");
        
        // Mostrar feedback visual (opcional)
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
    
    #region 💾 Settings Persistence
    
    void LoadSettings()
    {
        // Cargar volumen
        float savedVolume = PlayerPrefs.GetFloat("Volume", 0.75f);
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);
        
        // Cargar resolución
        int savedResolution = PlayerPrefs.GetInt("ResolutionIndex", resolutions.Length - 1);
        if (savedResolution < resolutions.Length)
        {
            resolutionDropdown.value = savedResolution;
            SetResolution(savedResolution);
        }
        
        // Cargar pantalla completa
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        fullscreenToggle.isOn = savedFullscreen;
        SetFullscreen(savedFullscreen);
        
        DebugLog("💾 Configuraciones cargadas");
    }
    
    #endregion
    
    #region 🎨 UI Feedback
    
    System.Collections.IEnumerator ShowAppliedFeedback()
    {
        string originalText = applyButton.GetComponentInChildren<TextMeshProUGUI>().text;
        applyButton.GetComponentInChildren<TextMeshProUGUI>().text = "✅ ¡Aplicado!";
        applyButton.interactable = false;
        
        yield return new WaitForSecondsRealtime(1.5f);
        
        applyButton.GetComponentInChildren<TextMeshProUGUI>().text = originalText;
        applyButton.interactable = true;
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
        GUI.Label(new Rect(10, 50, 200, 20), $"Volume: {volumeSlider.value:F2}");
    }
    
    #endregion
} 