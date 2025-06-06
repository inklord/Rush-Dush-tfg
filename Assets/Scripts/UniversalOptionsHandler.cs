using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor invisible de opciones que solo usa ESC
/// Simula el comportamiento de Fall Guys (sin botón visible)
/// </summary>
public class UniversalOptionsHandler : MonoBehaviour
{
    [Header("🎮 Configuración")]
    [SerializeField] private bool enableInScene = true;
    [SerializeField] private string[] excludeScenes = { "Login", "Intro" };
    
    [Header("🔊 Audio (Opcional)")]
    public AudioClip menuOpenSound;
    public AudioClip menuCloseSound;
    
    private bool isActive = false;
    private string currentSceneName;
    
    void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        CheckIfShouldBeActive();
        
        if (isActive)
        {
            Debug.Log($"🎮 UniversalOptionsHandler activo en escena: {currentSceneName}");
        }
        else
        {
            Debug.Log($"🚫 UniversalOptionsHandler desactivado en escena: {currentSceneName}");
            gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        if (!isActive) return;
        
        // Solo ESC para abrir/cerrar opciones
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }
    
    void CheckIfShouldBeActive()
    {
        // Verificar si esta escena está excluida
        foreach (string excludeScene in excludeScenes)
        {
            if (currentSceneName.Contains(excludeScene))
            {
                isActive = false;
                return;
            }
        }
        
        isActive = enableInScene;
    }
    
    void HandleEscapeKey()
    {
        // Verificar si hay un menú de opciones ya abierto
        OptionsMenu currentOptionsMenu = FindObjectOfType<OptionsMenu>();
        
        if (currentOptionsMenu != null && currentOptionsMenu.optionsPanel != null)
        {
            bool isMenuOpen = currentOptionsMenu.optionsPanel.activeSelf;
            
            if (isMenuOpen)
            {
                // Menú abierto - cerrar
                PlaySound(menuCloseSound);
                currentOptionsMenu.BackToGame();
                Debug.Log("🔙 Cerrando opciones con ESC");
            }
            else
            {
                // Menú cerrado - abrir
                PlaySound(menuOpenSound);
                currentOptionsMenu.ToggleOptionsMenu();
                Debug.Log("⚙️ Abriendo opciones con ESC");
            }
        }
        else
        {
            // No hay menú - usar GlobalOptionsManager
            if (GlobalOptionsManager.Instance != null)
            {
                PlaySound(menuOpenSound);
                GlobalOptionsManager.Instance.OpenOptionsMenu();
                Debug.Log("🌐 Abriendo opciones via GlobalOptionsManager");
            }
            else
            {
                Debug.LogWarning("⚠️ No se pudo abrir opciones - No hay sistema disponible");
            }
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUISFX(clip);
        }
    }
    
    #region 🎮 Public API
    
    /// <summary>
    /// Forzar apertura de opciones (para usar desde otros scripts si es necesario)
    /// </summary>
    public void ForceOpenOptions()
    {
        if (!isActive) return;
        
        if (GlobalOptionsManager.Instance != null)
        {
            PlaySound(menuOpenSound);
            GlobalOptionsManager.Instance.OpenOptionsMenu();
        }
    }
    
    /// <summary>
    /// Verificar si el handler está activo en esta escena
    /// </summary>
    public bool IsActiveInScene()
    {
        return isActive;
    }
    
    /// <summary>
    /// Activar/desactivar temporalmente (útil para cutscenes, etc.)
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        isActive = enabled;
        Debug.Log($"🎮 UniversalOptionsHandler {(enabled ? "activado" : "desactivado")} temporalmente");
    }
    
    #endregion
    
    #region 🐛 Debug
    
    [ContextMenu("Test Open Options")]
    public void TestOpenOptions()
    {
        ForceOpenOptions();
    }
    
    [ContextMenu("Simulate ESC Key")]
    public void SimulateEscKey()
    {
        HandleEscapeKey();
    }
    
    void OnGUI()
    {
        if (!Application.isEditor || !isActive) return;
        
        // Debug info en esquina superior izquierda
        GUI.color = Color.yellow;
        GUI.Label(new Rect(10, 60, 300, 20), $"ESC Handler Active - Scene: {currentSceneName}");
        GUI.Label(new Rect(10, 80, 300, 20), "Press ESC to open/close options");
        GUI.color = Color.white;
    }
    
    #endregion
} 