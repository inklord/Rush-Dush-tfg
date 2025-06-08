using UnityEngine;

/// <summary>
/// Script simple para FinalFracaso - Solo cambia la animación de celebración por fracaso
/// Mantiene la misma estructura que la escena Ending
/// </summary>
public class SimpleFinalFracaso : MonoBehaviour
{
    [Header("🎬 Configuración de Animación")]
    public Animator characterAnimator;  // Animator del personaje principal
    public GameObject uiPanel;          // Panel de UI (si existe)
    
    void Start()
    {
        // Buscar y configurar automáticamente
        SetupFailureAnimation();
        
        // Crear UniversalOptionsHandler si no existe
        CreateOptionsHandler();
    }
    
    void SetupFailureAnimation()
    {
        // Si no se asignó el animator, intentar encontrarlo
        if (characterAnimator == null)
        {
            characterAnimator = FindObjectOfType<Animator>();
        }
        
        if (characterAnimator != null)
        {
            // Buscar el RuntimeAnimatorController de fracaso
            RuntimeAnimatorController failureController = Resources.Load<RuntimeAnimatorController>("RoundFailure");
            
            if (failureController == null)
            {
                // Buscar en otras ubicaciones
                failureController = Resources.Load<RuntimeAnimatorController>("Animation/RoundFailure");
            }
            
            if (failureController == null)
            {
                failureController = Resources.Load<RuntimeAnimatorController>("Animator/RoundFailure");
            }
            
            // Asignar el controller de fracaso
            if (failureController != null)
            {
                characterAnimator.runtimeAnimatorController = failureController;
                Debug.Log("🎬 Animación de fracaso asignada correctamente");
                
                // Si el animator tiene un trigger para iniciar fracaso, usarlo
                if (HasParameter(characterAnimator, "PlayFailure"))
                {
                    characterAnimator.SetTrigger("PlayFailure");
                }
                else if (HasParameter(characterAnimator, "Failure"))
                {
                    characterAnimator.SetTrigger("Failure");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró RoundFailure.controller. La animación seguirá igual que en Ending.");
            }
        }
    }
    
    void CreateOptionsHandler()
    {
        // Verificar si ya existe UniversalOptionsHandler
        UniversalOptionsHandler existingOptions = FindObjectOfType<UniversalOptionsHandler>();
        if (existingOptions == null)
        {
            // Crear OptionsHandler para que funcione ESC
            GameObject optionsGO = new GameObject("OptionsHandler");
            optionsGO.AddComponent<UniversalOptionsHandler>();
            Debug.Log("⚙️ UniversalOptionsHandler creado para ESC");
        }
    }
    
    /// <summary>
    /// Verifica si el animator tiene un parámetro específico
    /// </summary>
    bool HasParameter(Animator animator, string parameterName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return false;
            
        foreach (var parameter in animator.parameters)
        {
            if (parameter.name == parameterName)
            {
                return true;
            }
        }
        
        return false;
    }
    
    void Update()
    {
        // Permitir saltar con ESC o Space
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            // Buscar SceneChange si existe
            SceneChange sceneChanger = FindObjectOfType<SceneChange>();
            if (sceneChanger != null)
            {
                sceneChanger.FinalFracasoSceneChange();
            }
            else
            {
                // Fallback directo
                UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
            }
        }
    }
} 