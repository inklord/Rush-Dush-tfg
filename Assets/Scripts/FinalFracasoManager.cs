using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor para la escena FinalFracaso - muestra animación de derrota en lugar de victoria
/// SE CONFIGURA AUTOMÁTICAMENTE - No necesita setup manual
/// </summary>
public class FinalFracasoManager : MonoBehaviour
{
    [Header("🎬 Elementos de Fracaso (Auto-configurados)")]
    public GameObject fracasoPanel;           // Panel principal de fracaso
    public Animator fracasoAnimator;          // Animator para la animación de fracaso
    public AudioSource fracasoAudio;         // Audio de fracaso
    public AudioClip fracasoSound;           // Clip de sonido de fracaso
    
    [Header("🎨 UI Elements (Auto-configurados)")]
    public Text fracasoText;                 // Texto principal de fracaso
    public Text subtitleText;                // Texto secundario
    public Button retryButton;               // Botón reintentar
    public Button exitButton;                // Botón salir al lobby
    
    [Header("⚙️ Configuración")]
    public float showDelay = 2f;             // Delay antes de mostrar la pantalla
    public float autoExitDelay = 10f;        // Tiempo antes de salir automáticamente
    
    private Canvas mainCanvas;
    
    #region 🚀 Auto-Configuración
    
    void Awake()
    {
        Debug.Log("🚀 FinalFracasoManager iniciando auto-configuración...");
        AutoSetup();
    }
    
    void Start()
    {
        StartCoroutine(ShowFracasoScreenWithDelay());
    }
    
    /// <summary>
    /// Configuración automática completa del sistema de fracaso
    /// </summary>
    void AutoSetup()
    {
        // 1. Buscar o crear AudioSource
        SetupAudioSource();
        
        // 2. Buscar Canvas principal
        FindMainCanvas();
        
        // 3. Buscar o crear panel de fracaso
        SetupFracasoPanel();
        
        // 4. Buscar o crear elementos UI
        SetupUIElements();
        
        // 5. Configurar eventos de botones
        SetupButtonEvents();
        
        Debug.Log("✅ FinalFracasoManager auto-configuración completada");
    }
    
    void SetupAudioSource()
    {
        fracasoAudio = GetComponent<AudioSource>();
        if (fracasoAudio == null)
        {
            fracasoAudio = gameObject.AddComponent<AudioSource>();
            fracasoAudio.playOnAwake = false;
            fracasoAudio.volume = 0.7f;
            Debug.Log("🔊 AudioSource creado automáticamente");
        }
    }
    
    void FindMainCanvas()
    {
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            // Crear Canvas si no existe
            GameObject canvasGO = new GameObject("Canvas");
            mainCanvas = canvasGO.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            Debug.Log("🎨 Canvas creado automáticamente");
        }
    }
    
    void SetupFracasoPanel()
    {
        // Buscar panel existente
        fracasoPanel = GameObject.Find("FracasoPanel");
        
        if (fracasoPanel == null)
        {
            // Crear panel de fracaso
            fracasoPanel = new GameObject("FracasoPanel");
            fracasoPanel.transform.SetParent(mainCanvas.transform, false);
            
            // RectTransform para pantalla completa
            RectTransform rectTransform = fracasoPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Fondo semi-transparente
            Image backgroundImage = fracasoPanel.AddComponent<Image>();
            backgroundImage.color = new Color(0f, 0f, 0f, 0.8f);
            
            Debug.Log("🎨 FracasoPanel creado automáticamente");
        }
        
        // Configurar Animator
        fracasoAnimator = fracasoPanel.GetComponent<Animator>();
        if (fracasoAnimator == null)
        {
            fracasoAnimator = fracasoPanel.AddComponent<Animator>();
            
            // Buscar AnimatorController de fracaso
            RuntimeAnimatorController fracasoController = Resources.Load<RuntimeAnimatorController>("RoundFailure");
            if (fracasoController == null)
            {
                fracasoController = Resources.Load<RuntimeAnimatorController>("Animation/RoundFailure");
            }
            
            if (fracasoController != null)
            {
                fracasoAnimator.runtimeAnimatorController = fracasoController;
                Debug.Log("🎬 AnimatorController de fracaso asignado automáticamente");
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró RoundFailure.controller");
            }
        }
        
        // Inicialmente desactivado
        fracasoPanel.SetActive(false);
    }
    
    void SetupUIElements()
    {
        // Crear texto principal de fracaso
        CreateFracasoText();
        
        // Crear texto secundario
        CreateSubtitleText();
        
        // Crear botones
        CreateRetryButton();
        CreateExitButton();
    }
    
    void CreateFracasoText()
    {
        Transform existing = fracasoPanel.transform.Find("FracasoText");
        if (existing != null)
        {
            fracasoText = existing.GetComponent<Text>();
            return;
        }
        
        GameObject textGO = new GameObject("FracasoText");
        textGO.transform.SetParent(fracasoPanel.transform, false);
        
        RectTransform rectTransform = textGO.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0f, 100f);
        rectTransform.sizeDelta = new Vector2(800f, 100f);
        
        fracasoText = textGO.AddComponent<Text>();
        fracasoText.text = "💀 ¡HAS SIDO ELIMINADO!";
        fracasoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        fracasoText.fontSize = 48;
        fracasoText.color = Color.red;
        fracasoText.alignment = TextAnchor.MiddleCenter;
        fracasoText.fontStyle = FontStyle.Bold;
        
        Debug.Log("💀 FracasoText creado automáticamente");
    }
    
    void CreateSubtitleText()
    {
        Transform existing = fracasoPanel.transform.Find("SubtitleText");
        if (existing != null)
        {
            subtitleText = existing.GetComponent<Text>();
            return;
        }
        
        GameObject textGO = new GameObject("SubtitleText");
        textGO.transform.SetParent(fracasoPanel.transform, false);
        
        RectTransform rectTransform = textGO.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0f, 0f);
        rectTransform.sizeDelta = new Vector2(600f, 60f);
        
        subtitleText = textGO.AddComponent<Text>();
        subtitleText.text = "No llegaste a la meta a tiempo";
        subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subtitleText.fontSize = 32;
        subtitleText.color = Color.white;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        
        Debug.Log("📝 SubtitleText creado automáticamente");
    }
    
    void CreateRetryButton()
    {
        Transform existing = fracasoPanel.transform.Find("RetryButton");
        if (existing != null)
        {
            retryButton = existing.GetComponent<Button>();
            return;
        }
        
        GameObject buttonGO = new GameObject("RetryButton");
        buttonGO.transform.SetParent(fracasoPanel.transform, false);
        
        RectTransform rectTransform = buttonGO.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(-200f, -300f);
        rectTransform.sizeDelta = new Vector2(300f, 80f);
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.8f, 0.2f, 0.9f); // Verde
        
        retryButton = buttonGO.AddComponent<Button>();
        
        // Texto del botón
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        Text buttonText = textGO.AddComponent<Text>();
        buttonText.text = "🔄 REINTENTAR";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Debug.Log("🔄 RetryButton creado automáticamente");
    }
    
    void CreateExitButton()
    {
        Transform existing = fracasoPanel.transform.Find("ExitButton");
        if (existing != null)
        {
            exitButton = existing.GetComponent<Button>();
            return;
        }
        
        GameObject buttonGO = new GameObject("ExitButton");
        buttonGO.transform.SetParent(fracasoPanel.transform, false);
        
        RectTransform rectTransform = buttonGO.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(200f, -300f);
        rectTransform.sizeDelta = new Vector2(300f, 80f);
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 0.9f); // Rojo
        
        exitButton = buttonGO.AddComponent<Button>();
        
        // Texto del botón
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        Text buttonText = textGO.AddComponent<Text>();
        buttonText.text = "🚪 SALIR AL LOBBY";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Debug.Log("🚪 ExitButton creado automáticamente");
    }
    
    void SetupButtonEvents()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(RetryGame);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ExitToLobby);
        }
        
        Debug.Log("🔗 Eventos de botones configurados automáticamente");
    }
    
    #endregion
    
    #region 🎨 Visual Effects
    
    /// <summary>
    /// Método para agregar efectos visuales adicionales si es necesario
    /// </summary>
    public void AddFailureEffect(Vector3 position)
    {
        // Aquí se pueden agregar partículas, shakes de cámara, etc.
        Debug.Log($"🎆 Efecto de fracaso en posición: {position}");
    }
    
    #endregion
    
    #region 🎬 Fracaso Sequence
    
    /// <summary>
    /// Corrutina para mostrar la pantalla de fracaso con delay
    /// </summary>
    IEnumerator ShowFracasoScreenWithDelay()
    {
        // Esperar delay inicial
        yield return new WaitForSeconds(showDelay);
        
        // Activar panel de fracaso
        if (fracasoPanel != null)
        {
            fracasoPanel.SetActive(true);
            Debug.Log("💀 Panel de fracaso activado");
        }
        
        // Reproducir sonido de fracaso
        if (fracasoAudio != null && fracasoSound != null)
        {
            fracasoAudio.PlayOneShot(fracasoSound);
            Debug.Log("🔊 Reproduciendo sonido de fracaso");
        }
        
        // Activar animación de fracaso
        if (fracasoAnimator != null)
        {
            fracasoAnimator.enabled = true;
            
            // Si el animator tiene un trigger "PlayFailure", usarlo
            if (HasTrigger(fracasoAnimator, "PlayFailure"))
            {
                fracasoAnimator.SetTrigger("PlayFailure");
            }
            
            Debug.Log("🎬 Iniciando animación de fracaso");
        }
        
        // Auto-exit después del delay configurado
        yield return new WaitForSeconds(autoExitDelay);
        
        Debug.Log("⏰ Tiempo agotado, volviendo al lobby automáticamente...");
        ExitToLobby();
    }
    
    /// <summary>
    /// Verifica si el animator tiene un trigger específico
    /// </summary>
    private bool HasTrigger(Animator animator, string triggerName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return false;
            
        foreach (var parameter in animator.parameters)
        {
            if (parameter.name == triggerName && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                return true;
            }
        }
        
        return false;
    }
    
    #endregion
    
    #region 🔧 Helper Methods
    
    #endregion
    
    #region 🔧 Unity Events
    
    void Update()
    {
        // Permitir saltar la secuencia con ESC o Space
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            ExitToLobby();
        }
    }
    
    void OnDestroy()
    {
        // Limpiar listeners
        if (retryButton != null)
            retryButton.onClick.RemoveAllListeners();
        
        if (exitButton != null)
            exitButton.onClick.RemoveAllListeners();
    }
    
    #endregion
    
    #region 🐛 Debug
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    void OnGUI()
    {
        if (!Application.isEditor) return;
        
        GUI.Box(new Rect(10, 10, 250, 100), "FinalFracaso Debug");
        GUI.Label(new Rect(15, 30, 240, 20), $"Panel Active: {fracasoPanel?.activeSelf}");
        GUI.Label(new Rect(15, 50, 240, 20), "ESC/SPACE para salir");
        GUI.Label(new Rect(15, 70, 240, 20), $"Scene: {SceneManager.GetActiveScene().name}");
    }
    
    #endregion
    
    #region 🎮 Button Actions
    
    public void RetryGame()
    {
        Debug.Log("🔄 Reintentando el juego...");
        
        // Volver al lobby para empezar de nuevo
        SceneManager.LoadScene("Lobby");
    }
    
    public void ExitToLobby()
    {
        Debug.Log("🚪 Volviendo al lobby...");
        
        // Buscar SceneChange si existe
        SceneChange sceneChanger = FindObjectOfType<SceneChange>();
        if (sceneChanger != null)
        {
            sceneChanger.FinalFracasoSceneChange();
        }
        else
        {
            // Fallback directo
            SceneManager.LoadScene("Lobby");
        }
    }
    
    #endregion
} 