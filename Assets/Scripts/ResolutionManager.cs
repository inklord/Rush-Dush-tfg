using UnityEngine;

[DefaultExecutionOrder(-500)]
public class ResolutionManager : MonoBehaviour
{
    [Header("🖥️ Configuración de Resolución")]
    public bool setResolutionOnStart = true;        // Configurar resolución al inicio
    public bool allowFullscreen = true;             // Permitir pantalla completa
    public bool enableDebugLogs = true;             // Logs de debug
    public bool persistSettings = true;             // Guardar configuración
    public bool forceOnEveryScene = true;           // Forzar en cada escena
    
    [Header("📱 Resoluciones Preferidas")]
    public Vector2Int[] preferredResolutions = new Vector2Int[]
    {
        new Vector2Int(1920, 1080), // Full HD
        new Vector2Int(1600, 900),  // HD+
        new Vector2Int(1366, 768),  // HD estándar
        new Vector2Int(1280, 720),  // 720p
        new Vector2Int(1024, 768)   // 4:3 clásico
    };
    
    [Header("🔧 Configuración de Fallback")]
    public Vector2Int fallbackResolution = new Vector2Int(1280, 720); // Resolución por defecto
    
    // Singleton estático
    private static ResolutionManager instance;
    public static ResolutionManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Buscar en escena
                instance = FindObjectOfType<ResolutionManager>();
                
                // Si no existe, crear uno
                if (instance == null)
                {
                    GameObject go = new GameObject("ResolutionManager");
                    instance = go.AddComponent<ResolutionManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Variables de estado
    private Vector2Int lastAppliedResolution;
    private bool lastAppliedFullscreen;
    
    void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (enableDebugLogs)
            {
                Debug.Log("🖥️ ResolutionManager creado como singleton");
            }
        }
        else if (instance != this)
        {
            if (enableDebugLogs)
            {
                Debug.Log("🔧 ResolutionManager duplicado encontrado - destruyendo...");
            }
            Destroy(gameObject);
            return;
        }
        
        // Aplicar resolución inmediatamente en Awake
        if (setResolutionOnStart)
        {
            LoadAndApplySettings();
        }
    }
    
    void Start()
    {
        // Aplicar resolución también en Start por seguridad
        if (setResolutionOnStart)
        {
            LoadAndApplySettings();
        }
        
        // Suscribirse a cambios de escena
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDestroy()
    {
        // Desuscribirse de eventos
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (forceOnEveryScene && setResolutionOnStart)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"🔄 Aplicando resolución en nueva escena: {scene.name}");
            }
            LoadAndApplySettings();
        }
    }
    
    /// <summary>
    /// 💾 Cargar y aplicar configuración guardada
    /// </summary>
    void LoadAndApplySettings()
    {
        if (persistSettings)
        {
            // Cargar configuración guardada
            int savedWidth = PlayerPrefs.GetInt("ResolutionWidth", -1);
            int savedHeight = PlayerPrefs.GetInt("ResolutionHeight", -1);
            bool savedFullscreen = PlayerPrefs.GetInt("ResolutionFullscreen", 1) == 1;
            
            if (savedWidth > 0 && savedHeight > 0)
            {
                // Usar configuración guardada
                ApplyResolution(new Vector2Int(savedWidth, savedHeight), savedFullscreen);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"💾 Configuración cargada: {savedWidth}x{savedHeight} | Fullscreen: {savedFullscreen}");
                }
                return;
            }
        }
        
        // Si no hay configuración guardada, usar automática
        SetOptimalResolution();
    }
    
    /// <summary>
    /// 🖥️ Configurar resolución óptima basada en el monitor
    /// </summary>
    public void SetOptimalResolution()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"🖥️ Resolución actual del monitor: {Screen.currentResolution.width}x{Screen.currentResolution.height}");
            Debug.Log($"🖥️ Resolución actual del juego: {Screen.width}x{Screen.height}");
        }
        
        Resolution currentMonitorRes = Screen.currentResolution;
        Vector2Int bestResolution = FindBestResolution(currentMonitorRes);
        
        // Aplicar resolución
        bool fullscreen = allowFullscreen && ShouldUseFullscreen(bestResolution, currentMonitorRes);
        
        ApplyResolution(bestResolution, fullscreen);
    }
    
    /// <summary>
    /// ⚡ Aplicar resolución con validación y guardado
    /// </summary>
    void ApplyResolution(Vector2Int resolution, bool fullscreen)
    {
        // Evitar aplicar la misma resolución repetidamente
        if (resolution == lastAppliedResolution && fullscreen == lastAppliedFullscreen)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"⏭️ Resolución ya aplicada: {resolution.x}x{resolution.y}");
            }
            return;
        }
        
        Screen.SetResolution(resolution.x, resolution.y, fullscreen);
        
        lastAppliedResolution = resolution;
        lastAppliedFullscreen = fullscreen;
        
        // Guardar configuración
        if (persistSettings)
        {
            SaveSettings(resolution, fullscreen);
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"✅ Resolución configurada: {resolution.x}x{resolution.y} | Fullscreen: {fullscreen}");
        }
    }
    
    /// <summary>
    /// 💾 Guardar configuración en PlayerPrefs
    /// </summary>
    void SaveSettings(Vector2Int resolution, bool fullscreen)
    {
        PlayerPrefs.SetInt("ResolutionWidth", resolution.x);
        PlayerPrefs.SetInt("ResolutionHeight", resolution.y);
        PlayerPrefs.SetInt("ResolutionFullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();
        
        if (enableDebugLogs)
        {
            Debug.Log($"💾 Configuración guardada: {resolution.x}x{resolution.y}");
        }
    }
    
    /// <summary>
    /// 🔍 Encontrar la mejor resolución para el monitor actual
    /// </summary>
    Vector2Int FindBestResolution(Resolution monitorRes)
    {
        Vector2Int monitorSize = new Vector2Int(monitorRes.width, monitorRes.height);
        
        // Buscar resolución que quepa en el monitor
        foreach (Vector2Int resolution in preferredResolutions)
        {
            if (resolution.x <= monitorSize.x && resolution.y <= monitorSize.y)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"🎯 Resolución seleccionada: {resolution.x}x{resolution.y}");
                }
                return resolution;
            }
        }
        
        // Fallback si ninguna resolución encaja
        if (enableDebugLogs)
        {
            Debug.LogWarning($"⚠️ Usando resolución fallback: {fallbackResolution.x}x{fallbackResolution.y}");
        }
        return fallbackResolution;
    }
    
    /// <summary>
    /// 🖥️ Determinar si usar pantalla completa
    /// </summary>
    bool ShouldUseFullscreen(Vector2Int gameRes, Resolution monitorRes)
    {
        // Si la resolución del juego es igual a la del monitor, usar fullscreen
        return gameRes.x == monitorRes.width && gameRes.y == monitorRes.height;
    }
    
    /// <summary>
    /// 🔄 Forzar resolución específica (para testing)
    /// </summary>
    public void ForceResolution(int width, int height, bool fullscreen = false)
    {
        Vector2Int newResolution = new Vector2Int(width, height);
        ApplyResolution(newResolution, fullscreen);
    }
    
    /// <summary>
    /// 📱 Cambiar a resolución común
    /// </summary>
    public void SetCommonResolution(string resolution)
    {
        switch (resolution.ToLower())
        {
            case "fhd":
            case "1080p":
                ForceResolution(1920, 1080);
                break;
            case "hd":
            case "720p":
                ForceResolution(1280, 720);
                break;
            case "sd":
            case "480p":
                ForceResolution(854, 480);
                break;
            default:
                SetOptimalResolution();
                break;
        }
    }
    
    /// <summary>
    /// 🔧 Método público para integrar con menús de opciones
    /// </summary>
    public void SetResolutionFromMenu(int width, int height, bool fullscreen)
    {
        ForceResolution(width, height, fullscreen);
    }
} 