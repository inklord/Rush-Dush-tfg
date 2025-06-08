using UnityEngine;

/// <summary>
/// 🔧 Script que fuerza resolución INMEDIATAMENTE al inicio del juego
/// Debe ejecutarse antes que cualquier otro script
/// </summary>
[DefaultExecutionOrder(-1000)]
public class EarlyResolutionFix : MonoBehaviour
{
    [Header("🖥️ Configuración Forzada")]
    public Vector2Int targetResolution = new Vector2Int(1920, 1080);
    public bool forceFullscreen = false;
    public bool enableDebugLogs = true;
    
    [Header("⚡ Configuración Agresiva")]
    public bool applyInAwake = true;
    public bool applyInStart = true;
    public bool applyInFirstUpdate = true;
    public bool applyEveryFrame = false;
    
    private bool hasAppliedOnce = false;
    
    void Awake()
    {
        if (applyInAwake)
        {
            ForceResolutionNow("Awake");
        }
    }
    
    void Start()
    {
        if (applyInStart)
        {
            ForceResolutionNow("Start");
        }
    }
    
    void Update()
    {
        if (applyInFirstUpdate && !hasAppliedOnce)
        {
            ForceResolutionNow("FirstUpdate");
            hasAppliedOnce = true;
        }
        
        if (applyEveryFrame)
        {
            ForceResolutionNow("EveryFrame");
        }
    }
    
    /// <summary>
    /// 🔧 Forzar resolución inmediatamente
    /// </summary>
    void ForceResolutionNow(string context)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"🔧 [{context}] Resolución ANTES: {Screen.width}x{Screen.height}");
        }
        
        // Aplicar resolución target
        Screen.SetResolution(targetResolution.x, targetResolution.y, forceFullscreen);
        
        if (enableDebugLogs)
        {
            Debug.Log($"✅ [{context}] Resolución FORZADA: {targetResolution.x}x{targetResolution.y} | Fullscreen: {forceFullscreen}");
        }
    }
    
    /// <summary>
    /// 🎯 Configurar resolución específica desde código
    /// </summary>
    public void SetTargetResolution(int width, int height, bool fullscreen = false)
    {
        targetResolution = new Vector2Int(width, height);
        forceFullscreen = fullscreen;
        ForceResolutionNow("SetTarget");
    }
    
    /// <summary>
    /// 📱 Configuraciones rápidas
    /// </summary>
    public void SetTo1080p() => SetTargetResolution(1920, 1080, false);
    public void SetTo720p() => SetTargetResolution(1280, 720, false);
    public void SetTo1080pFullscreen() => SetTargetResolution(1920, 1080, true);
} 