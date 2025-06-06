using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Gestor global de opciones que persiste entre todas las escenas
/// Se crea automáticamente y gestiona el menú de opciones universalmente
/// </summary>
public class GlobalOptionsManager : MonoBehaviour
{
    [Header("🎵 Audio Settings")]
    public AudioMixer audioMixer;
    
    [Header("🎨 UI Prefab")]
    public GameObject optionsMenuPrefab;
    
    // Singleton
    private static GlobalOptionsManager instance;
    public static GlobalOptionsManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Buscar en la escena
                instance = FindObjectOfType<GlobalOptionsManager>();
                
                // Si no existe, crear uno
                if (instance == null)
                {
                    GameObject go = new GameObject("GlobalOptionsManager");
                    instance = go.AddComponent<GlobalOptionsManager>();
                }
            }
            return instance;
        }
    }
    
    // Referencias actuales
    private OptionsMenu currentOptionsMenu;
    private Canvas currentUICanvas;
    private bool isInitialized = false;
    
    // Settings cache
    private float masterVolume = 0.75f;
    private int resolutionIndex = -1;
    private bool isFullscreen = true;
    
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
        SceneManager.sceneLoaded += OnSceneLoaded;
        SetupCurrentScene();
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void Initialize()
    {
        LoadGlobalSettings();
        isInitialized = true;
        Debug.Log("🌐 GlobalOptionsManager inicializado");
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"🔄 Escena cargada: {scene.name}");
        SetupCurrentScene();
    }
    
    void SetupCurrentScene()
    {
        // Esperar un frame para que la escena se configure completamente
        StartCoroutine(SetupSceneCoroutine());
    }
    
    System.Collections.IEnumerator SetupSceneCoroutine()
    {
        yield return new WaitForEndOfFrame();
        
        // Buscar OptionsMenu existente en la escena
        currentOptionsMenu = FindObjectOfType<OptionsMenu>();
        
        if (currentOptionsMenu == null)
        {
            // No hay OptionsMenu, crear uno automáticamente
            CreateOptionsMenuForScene();
        }
        else
        {
            // Hay OptionsMenu, configurarlo
            ConfigureExistingOptionsMenu();
        }
        
        // Buscar canvas principal
        currentUICanvas = FindMainCanvas();
        
        // Aplicar configuraciones guardadas
        ApplyGlobalSettings();
        
        Debug.Log($"✅ Opciones configuradas para escena: {SceneManager.GetActiveScene().name}");
    }
    
    void CreateOptionsMenuForScene()
    {
        // Buscar o crear canvas
        Canvas canvas = FindMainCanvas();
        if (canvas == null)
        {
            canvas = CreateMainCanvas();
        }
        
        // Crear el menú de opciones
        if (optionsMenuPrefab != null)
        {
            GameObject menuGO = Instantiate(optionsMenuPrefab, canvas.transform);
            currentOptionsMenu = menuGO.GetComponent<OptionsMenu>();
        }
        else
        {
            // Crear menú básico programáticamente
            CreateBasicOptionsMenu(canvas);
        }
        
        // Configurar referencias
        if (currentOptionsMenu != null)
        {
            ConfigureOptionsMenu();
        }
    }
    
    void ConfigureExistingOptionsMenu()
    {
        if (currentOptionsMenu != null)
        {
            ConfigureOptionsMenu();
        }
    }
    
    void ConfigureOptionsMenu()
    {
        // Asignar AudioMixer si no está asignado
        if (currentOptionsMenu.audioMixer == null && audioMixer != null)
        {
            currentOptionsMenu.audioMixer = audioMixer;
        }
        
        // Configurar referencias de UI
        if (currentUICanvas != null && currentOptionsMenu.gameUI == null)
        {
            currentOptionsMenu.gameUI = currentUICanvas;
        }
    }
    
    Canvas FindMainCanvas()
    {
        // Buscar canvas principal (diferentes nombres posibles)
        string[] possibleNames = { "Canvas", "UI", "MainCanvas", "GameUI", "HUD" };
        
        foreach (string name in possibleNames)
        {
            GameObject canvasGO = GameObject.Find(name);
            if (canvasGO != null)
            {
                Canvas canvas = canvasGO.GetComponent<Canvas>();
                if (canvas != null) return canvas;
            }
        }
        
        // Si no encuentra, buscar cualquier canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return canvas;
        }
        
        return null;
    }
    
    Canvas CreateMainCanvas()
    {
        GameObject canvasGO = new GameObject("OptionsCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Encima de otros UI
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        return canvas;
    }
    
    void CreateBasicOptionsMenu(Canvas canvas)
    {
        // Crear menú básico si no hay prefab
        GameObject menuGO = new GameObject("BasicOptionsMenu");
        menuGO.transform.SetParent(canvas.transform, false);
        
        currentOptionsMenu = menuGO.AddComponent<OptionsMenu>();
        
        // Aquí podrías crear UI básica programáticamente si es necesario
        Debug.Log("📋 Menú de opciones básico creado");
    }
    
    #region 💾 Global Settings Management
    
    void LoadGlobalSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("GlobalMasterVolume", 0.75f);
        resolutionIndex = PlayerPrefs.GetInt("GlobalResolutionIndex", -1);
        isFullscreen = PlayerPrefs.GetInt("GlobalFullscreen", 1) == 1;
        
        Debug.Log($"📂 Configuraciones globales cargadas - Vol: {masterVolume:F2}, Res: {resolutionIndex}, FS: {isFullscreen}");
    }
    
    void ApplyGlobalSettings()
    {
        // Aplicar volumen
        if (audioMixer != null)
        {
            float dB = masterVolume > 0 ? Mathf.Log10(masterVolume) * 20 : -80f;
            audioMixer.SetFloat("MasterVolume", dB);
        }
        
        // Aplicar resolución
        if (resolutionIndex >= 0 && resolutionIndex < Screen.resolutions.Length)
        {
            Resolution res = Screen.resolutions[resolutionIndex];
            Screen.SetResolution(res.width, res.height, isFullscreen);
        }
        
        // Aplicar pantalla completa
        Screen.fullScreen = isFullscreen;
        
        // Actualizar UI si existe
        if (currentOptionsMenu != null)
        {
            UpdateOptionsMenuUI();
        }
    }
    
    void UpdateOptionsMenuUI()
    {
        if (currentOptionsMenu.volumeSlider != null)
        {
            currentOptionsMenu.volumeSlider.value = masterVolume;
        }
        
        if (currentOptionsMenu.fullscreenToggle != null)
        {
            currentOptionsMenu.fullscreenToggle.isOn = isFullscreen;
        }
        
        if (currentOptionsMenu.resolutionDropdown != null && resolutionIndex >= 0)
        {
            currentOptionsMenu.resolutionDropdown.value = resolutionIndex;
        }
    }
    
    public void SaveGlobalSettings(float volume, int resolution, bool fullscreen)
    {
        masterVolume = volume;
        resolutionIndex = resolution;
        isFullscreen = fullscreen;
        
        PlayerPrefs.SetFloat("GlobalMasterVolume", volume);
        PlayerPrefs.SetInt("GlobalResolutionIndex", resolution);
        PlayerPrefs.SetInt("GlobalFullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log($"💾 Configuraciones globales guardadas - Vol: {volume:F2}, Res: {resolution}, FS: {fullscreen}");
    }
    
    #endregion
    
    #region 🎮 Public API
    
    public void OpenOptionsMenu()
    {
        if (currentOptionsMenu != null)
        {
            currentOptionsMenu.ToggleOptionsMenu();
        }
        else
        {
            Debug.LogWarning("⚠️ No hay OptionsMenu disponible en esta escena");
        }
    }
    
    public bool HasOptionsMenu()
    {
        return currentOptionsMenu != null;
    }
    
    public OptionsMenu GetCurrentOptionsMenu()
    {
        return currentOptionsMenu;
    }
    
    #endregion
    
    #region 🐛 Debug
    
    // ESC key handling is now managed by UniversalOptionsHandler
    // to avoid conflicts and provide consistent behavior across all scenes
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    void OnGUI()
    {
        if (!Application.isEditor) return;
        
        GUI.Box(new Rect(10, 100, 200, 80), "Global Options Debug");
        GUI.Label(new Rect(15, 120, 190, 20), $"Current Scene: {SceneManager.GetActiveScene().name}");
        GUI.Label(new Rect(15, 140, 190, 20), $"Has OptionsMenu: {currentOptionsMenu != null}");
        GUI.Label(new Rect(15, 160, 190, 20), $"Volume: {masterVolume:F2}");
    }
    
    #endregion
} 