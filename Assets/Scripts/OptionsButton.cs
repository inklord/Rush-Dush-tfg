using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script simple para botón que abre el menú de opciones
/// Úsalo en cualquier botón de cualquier escena
/// </summary>
public class OptionsButton : MonoBehaviour
{
    [Header("🎮 Referencias")]
    public OptionsMenu optionsMenu;
    
    [Header("🔊 Audio (Opcional)")]
    public AudioClip buttonClickSound;
    
    private Button button;
    
    void Start()
    {
        // Obtener componente Button
        button = GetComponent<Button>();
        
        if (button != null)
        {
            // Añadir listener al botón
            button.onClick.AddListener(OpenOptionsMenu);
        }
        else
        {
            Debug.LogWarning("⚠️ OptionsButton: No hay componente Button en este GameObject");
        }
        
        // Buscar OptionsMenu automáticamente si no está asignado
        if (optionsMenu == null)
        {
            optionsMenu = FindObjectOfType<OptionsMenu>();
            
            if (optionsMenu == null)
            {
                Debug.LogWarning("⚠️ OptionsButton: No se encontró OptionsMenu en la escena");
            }
        }
    }
    
    public void OpenOptionsMenu()
    {
        // Reproducir sonido de click si está asignado
        if (buttonClickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUISFX(buttonClickSound);
        }
        
        // Abrir menú de opciones
        if (optionsMenu != null)
        {
            optionsMenu.ToggleOptionsMenu();
            Debug.Log("🎮 Abriendo menú de opciones...");
        }
        else
        {
            Debug.LogError("❌ OptionsButton: No hay OptionsMenu asignado");
        }
    }
    
    // Método público para usar desde el Inspector
    public void OpenOptions()
    {
        OpenOptionsMenu();
    }
} 