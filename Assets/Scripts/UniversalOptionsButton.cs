using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Botón universal de opciones que funciona en cualquier escena
/// Se conecta automáticamente al GlobalOptionsManager
/// </summary>
public class UniversalOptionsButton : MonoBehaviour
{
    [Header("🎮 Configuración")]
    [SerializeField] private bool showOnlyInGame = false;
    [SerializeField] private string[] excludeScenes = { "Login", "Intro" };
    
    [Header("🔊 Audio (Opcional)")]
    public AudioClip buttonClickSound;
    
    [Header("🎨 Estilo Botón")]
    public string buttonText = "⚙️ Opciones";
    public Vector2 buttonPosition = new Vector2(-120, -50);
    public Vector2 buttonSize = new Vector2(100, 40);
    
    private Button button;
    private bool isSetup = false;
    
    void Start()
    {
        SetupUniversalButton();
    }
    
    void SetupUniversalButton()
    {
        // Verificar si debemos mostrar el botón en esta escena
        if (!ShouldShowButton()) 
        {
            gameObject.SetActive(false);
            return;
        }
        
        // Obtener o crear botón
        button = GetComponent<Button>();
        if (button == null)
        {
            CreateButton();
        }
        
        // Configurar listener
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenOptionsMenu);
        
        // Configurar posición si es nuevo
        ConfigureButtonPosition();
        
        isSetup = true;
        Debug.Log("🎮 UniversalOptionsButton configurado en escena: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    bool ShouldShowButton()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Verificar escenas excluidas
        foreach (string excludeScene in excludeScenes)
        {
            if (currentScene.Contains(excludeScene))
            {
                return false;
            }
        }
        
        // Si solo mostrar en juego, verificar escenas de juego
        if (showOnlyInGame)
        {
            string[] gameScenes = { "InGame", "Hexagonia", "Carrera", "WaitingUser" };
            bool isGameScene = false;
            
            foreach (string gameScene in gameScenes)
            {
                if (currentScene.Contains(gameScene))
                {
                    isGameScene = true;
                    break;
                }
            }
            
            return isGameScene;
        }
        
        return true;
    }
    
    void CreateButton()
    {
        // Encontrar o crear Canvas
        Canvas canvas = FindCanvas();
        if (canvas == null) return;
        
        // Crear GameObject del botón
        GameObject buttonGO = new GameObject("UniversalOptionsButton");
        buttonGO.transform.SetParent(canvas.transform, false);
        
        // Configurar RectTransform
        RectTransform rectTransform = buttonGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 1); // Esquina superior derecha
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.anchoredPosition = buttonPosition;
        rectTransform.sizeDelta = buttonSize;
        
        // Añadir componentes UI
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        button = buttonGO.AddComponent<Button>();
        
        // Crear texto del botón
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Text buttonTextComponent = textGO.AddComponent<Text>();
        buttonTextComponent.text = buttonText;
        buttonTextComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonTextComponent.fontSize = 12;
        buttonTextComponent.color = Color.white;
        buttonTextComponent.alignment = TextAnchor.MiddleCenter;
        
        // Configurar colores del botón
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        button.colors = colors;
        
        Debug.Log("🔧 Botón de opciones universal creado automáticamente");
    }
    
    Canvas FindCanvas()
    {
        // Buscar canvas existente
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        
        // Priorizar canvas de UI principal
        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay && canvas.sortingOrder >= 0)
            {
                return canvas;
            }
        }
        
        // Si no encuentra, crear uno
        return CreateCanvas();
    }
    
    Canvas CreateCanvas()
    {
        GameObject canvasGO = new GameObject("UniversalOptionsCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Muy encima
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        return canvas;
    }
    
    void ConfigureButtonPosition()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.anchoredPosition = buttonPosition;
            rectTransform.sizeDelta = buttonSize;
        }
    }
    
    public void OpenOptionsMenu()
    {
        // Reproducir sonido si está disponible
        if (buttonClickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUISFX(buttonClickSound);
        }
        
        // Abrir mediante GlobalOptionsManager
        if (GlobalOptionsManager.Instance != null)
        {
            GlobalOptionsManager.Instance.OpenOptionsMenu();
            Debug.Log("🎮 Abriendo opciones via GlobalOptionsManager");
        }
        else
        {
            // Fallback: buscar OptionsMenu local
            OptionsMenu localMenu = FindObjectOfType<OptionsMenu>();
            if (localMenu != null)
            {
                localMenu.ToggleOptionsMenu();
                Debug.Log("🎮 Abriendo opciones via OptionsMenu local");
            }
            else
            {
                Debug.LogWarning("⚠️ No se pudo abrir el menú de opciones - ni GlobalOptionsManager ni OptionsMenu encontrados");
            }
        }
    }
    
    #region 🎮 Public API para Inspector
    
    [ContextMenu("Test Open Options")]
    public void TestOpenOptions()
    {
        OpenOptionsMenu();
    }
    
    [ContextMenu("Force Setup")]
    public void ForceSetup()
    {
        SetupUniversalButton();
    }
    
    #endregion
    
    #region 🐛 Debug
    
    void OnValidate()
    {
        // Actualizar configuración en el editor
        if (Application.isPlaying && isSetup)
        {
            ConfigureButtonPosition();
            
            Text textComponent = GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.text = buttonText;
            }
        }
    }
    
    #endregion
} 