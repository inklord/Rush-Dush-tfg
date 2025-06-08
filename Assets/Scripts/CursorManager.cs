using UnityEngine;

/// <summary>
/// 🖱️ CURSOR MANAGER - Manejo global del cursor
/// Controla cuándo bloquear/liberar el cursor
/// </summary>
public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;
    
    [Header("🎮 Estado del Cursor")]
    public bool cursorLocked = true;
    public bool showDebug = true;
    
    // Estados del juego
    private bool inMenu = false;
    private bool inOptions = false;
    private bool inGame = true;
    
    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Inicializar cursor para juego
        SetGameCursor();
    }
    
    void Update()
    {
        // ESC para toggle cursor en juego
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inGame && !inMenu && !inOptions)
            {
                ToggleCursor();
            }
        }
        
        // Debug info
        if (showDebug && Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"🖱️ Cursor - Locked: {Cursor.lockState} | Visible: {Cursor.visible} | InGame: {inGame} | InMenu: {inMenu} | InOptions: {inOptions}");
        }
    }
    
    /// <summary>
    /// 🎮 Configurar cursor para juego (bloqueado e invisible)
    /// </summary>
    public void SetGameCursor()
    {
        inGame = true;
        inMenu = false;
        inOptions = false;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorLocked = true;
        
        if (showDebug) Debug.Log("🎮 Cursor configurado para JUEGO");
    }
    
    /// <summary>
    /// 📋 Configurar cursor para menú/opciones (libre y visible)
    /// </summary>
    public void SetMenuCursor()
    {
        inGame = false;
        inMenu = true;
        inOptions = false;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorLocked = false;
        
        if (showDebug) Debug.Log("📋 Cursor configurado para MENÚ");
    }
    
    /// <summary>
    /// ⚙️ Configurar cursor para opciones (libre y visible)
    /// </summary>
    public void SetOptionsCursor()
    {
        inGame = false;
        inMenu = false;
        inOptions = true;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorLocked = false;
        
        if (showDebug) Debug.Log("⚙️ Cursor configurado para OPCIONES");
    }
    
    /// <summary>
    /// 🔄 Toggle cursor (para ESC en juego)
    /// </summary>
    public void ToggleCursor()
    {
        if (cursorLocked)
        {
            SetMenuCursor();
        }
        else
        {
            SetGameCursor();
        }
    }
    
    /// <summary>
    /// 🎯 Verificar si cursor está libre para UI
    /// </summary>
    public bool IsCursorFree()
    {
        return !cursorLocked && Cursor.visible;
    }
    
    /// <summary>
    /// 🔒 Forzar bloqueo de cursor (para reinicio de nivel)
    /// </summary>
    public void ForceLockCursor()
    {
        SetGameCursor();
    }
} 