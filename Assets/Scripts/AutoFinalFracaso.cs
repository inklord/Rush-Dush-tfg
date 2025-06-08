using UnityEngine;

/// <summary>
/// Script auto-configurador para la escena FinalFracaso
/// Se coloca en cualquier GameObject de la escena para auto-configurar todo
/// </summary>
public class AutoFinalFracaso : MonoBehaviour
{
    [Header("🚀 Auto-Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupFinalFracasoScene();
        }
    }
    
    void SetupFinalFracasoScene()
    {
        Debug.Log("🚀 AutoFinalFracaso: Configurando escena automáticamente...");
        
        // Verificar si ya existe FinalFracasoManager
        FinalFracasoManager existingManager = FindObjectOfType<FinalFracasoManager>();
        if (existingManager != null)
        {
            Debug.Log("✅ FinalFracasoManager ya existe");
            return;
        }
        
        // Crear FinalFracasoManager
        GameObject managerGO = new GameObject("FinalFracasoManager");
        FinalFracasoManager manager = managerGO.AddComponent<FinalFracasoManager>();
        
        Debug.Log("✅ FinalFracasoManager creado automáticamente");
        
        // Verificar si existe UniversalOptionsHandler
        UniversalOptionsHandler existingOptions = FindObjectOfType<UniversalOptionsHandler>();
        if (existingOptions == null)
        {
            // Crear OptionsHandler
            GameObject optionsGO = new GameObject("OptionsHandler");
            UniversalOptionsHandler optionsHandler = optionsGO.AddComponent<UniversalOptionsHandler>();
            
            Debug.Log("✅ UniversalOptionsHandler creado automáticamente");
        }
        
        // Destruir este script después de la configuración (opcional)
        Destroy(this.gameObject);
        
        Debug.Log("🎉 Escena FinalFracaso configurada completamente");
    }
    
    [ContextMenu("🚀 Setup Manual")]
    public void ManualSetup()
    {
        SetupFinalFracasoScene();
    }
} 