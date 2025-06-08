using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script helper para configurar automáticamente la UI de FinalFracaso
/// Ejecutar una vez en la escena para crear todos los elementos necesarios
/// </summary>
public class FinalFracasoSetupHelper : MonoBehaviour
{
    [Header("🔧 Setup Automático")]
    [SerializeField] private bool setupUI = false;
    
    [Header("📍 Referencias Encontradas")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private FinalFracasoManager fracasoManager;
    
    void Start()
    {
        if (setupUI)
        {
            SetupFinalFracasoUI();
        }
    }
    
    [ContextMenu("🚀 Setup FinalFracaso UI")]
    public void SetupFinalFracasoUI()
    {
        Debug.Log("🚀 Iniciando setup automático de FinalFracaso...");
        
        // 1. Buscar Canvas principal
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("❌ No se encontró Canvas en la escena");
            return;
        }
        
        // 2. Crear FinalFracasoManager
        CreateFinalFracasoManager();
        
        // 3. Crear Panel de Fracaso
        GameObject fracasoPanel = CreateFracasoPanel();
        
        // 4. Crear elementos UI
        CreateRetryButton(fracasoPanel);
        CreateExitButton(fracasoPanel);
        CreateFracasoText(fracasoPanel);
        CreateSubtitleText(fracasoPanel);
        
        // 5. Configurar referencias
        ConfigureFracasoManager(fracasoPanel);
        
        // 6. Crear OptionsHandler
        CreateOptionsHandler();
        
        Debug.Log("✅ Setup de FinalFracaso completado!");
    }
    
    void CreateFinalFracasoManager()
    {
        // Verificar si ya existe
        fracasoManager = FindObjectOfType<FinalFracasoManager>();
        if (fracasoManager != null)
        {
            Debug.Log("🎬 FinalFracasoManager ya existe");
            return;
        }
        
        GameObject managerGO = new GameObject("FinalFracasoManager");
        fracasoManager = managerGO.AddComponent<FinalFracasoManager>();
        managerGO.AddComponent<AudioSource>(); // Para el audio de fracaso
        
        Debug.Log("🎬 FinalFracasoManager creado");
    }
    
    GameObject CreateFracasoPanel()
    {
        GameObject panelGO = new GameObject("FracasoPanel");
        panelGO.transform.SetParent(mainCanvas.transform, false);
        
        // RectTransform para pantalla completa
        RectTransform rectTransform = panelGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Fondo semi-transparente
        Image backgroundImage = panelGO.AddComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.8f);
        
        // Animator para la animación de fracaso
        Animator animator = panelGO.AddComponent<Animator>();
        
        // Inicialmente desactivado
        panelGO.SetActive(false);
        
        Debug.Log("🎨 FracasoPanel creado");
        return panelGO;
    }
    
    void CreateRetryButton(GameObject parent)
    {
        GameObject buttonGO = new GameObject("RetryButton");
        buttonGO.transform.SetParent(parent.transform, false);
        
        RectTransform rectTransform = buttonGO.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(-200f, -300f);
        rectTransform.sizeDelta = new Vector2(300f, 80f);
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.8f, 0.2f, 0.9f); // Verde
        
        Button button = buttonGO.AddComponent<Button>();
        
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
        
        Debug.Log("🔄 RetryButton creado");
    }
    
    void CreateExitButton(GameObject parent)
    {
        GameObject buttonGO = new GameObject("ExitButton");
        buttonGO.transform.SetParent(parent.transform, false);
        
        RectTransform rectTransform = buttonGO.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(200f, -300f);
        rectTransform.sizeDelta = new Vector2(300f, 80f);
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 0.9f); // Rojo
        
        Button button = buttonGO.AddComponent<Button>();
        
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
        
        Debug.Log("🚪 ExitButton creado");
    }
    
    void CreateFracasoText(GameObject parent)
    {
        GameObject textGO = new GameObject("FracasoText");
        textGO.transform.SetParent(parent.transform, false);
        
        RectTransform rectTransform = textGO.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0f, 100f);
        rectTransform.sizeDelta = new Vector2(800f, 100f);
        
        Text fracasoText = textGO.AddComponent<Text>();
        fracasoText.text = "💀 ¡HAS SIDO ELIMINADO!";
        fracasoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        fracasoText.fontSize = 48;
        fracasoText.color = Color.red;
        fracasoText.alignment = TextAnchor.MiddleCenter;
        fracasoText.fontStyle = FontStyle.Bold;
        
        Debug.Log("💀 FracasoText creado");
    }
    
    void CreateSubtitleText(GameObject parent)
    {
        GameObject textGO = new GameObject("SubtitleText");
        textGO.transform.SetParent(parent.transform, false);
        
        RectTransform rectTransform = textGO.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0f, 0f);
        rectTransform.sizeDelta = new Vector2(600f, 60f);
        
        Text subtitleText = textGO.AddComponent<Text>();
        subtitleText.text = "No llegaste a la meta a tiempo";
        subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subtitleText.fontSize = 32;
        subtitleText.color = Color.white;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        
        Debug.Log("📝 SubtitleText creado");
    }
    
    void ConfigureFracasoManager(GameObject fracasoPanel)
    {
        if (fracasoManager == null) return;
        
        // Buscar componentes creados
        GameObject retryButton = fracasoPanel.transform.Find("RetryButton")?.gameObject;
        GameObject exitButton = fracasoPanel.transform.Find("ExitButton")?.gameObject;
        GameObject fracasoText = fracasoPanel.transform.Find("FracasoText")?.gameObject;
        GameObject subtitleText = fracasoPanel.transform.Find("SubtitleText")?.gameObject;
        
        // Usar reflexión para asignar las referencias privadas
        var fields = typeof(FinalFracasoManager).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        
        foreach (var field in fields)
        {
            switch (field.Name)
            {
                case "fracasoPanel":
                    field.SetValue(fracasoManager, fracasoPanel);
                    break;
                case "fracasoAnimator":
                    field.SetValue(fracasoManager, fracasoPanel.GetComponent<Animator>());
                    break;
                case "fracasoAudio":
                    field.SetValue(fracasoManager, fracasoManager.GetComponent<AudioSource>());
                    break;
                case "retryButton":
                    field.SetValue(fracasoManager, retryButton?.GetComponent<Button>());
                    break;
                case "exitButton":
                    field.SetValue(fracasoManager, exitButton?.GetComponent<Button>());
                    break;
                case "fracasoText":
                    field.SetValue(fracasoManager, fracasoText?.GetComponent<Text>());
                    break;
                case "subtitleText":
                    field.SetValue(fracasoManager, subtitleText?.GetComponent<Text>());
                    break;
            }
        }
        
        Debug.Log("🔗 Referencias configuradas en FinalFracasoManager");
    }
    
    void CreateOptionsHandler()
    {
        // Verificar si ya existe
        UniversalOptionsHandler existingHandler = FindObjectOfType<UniversalOptionsHandler>();
        if (existingHandler != null)
        {
            Debug.Log("⚙️ UniversalOptionsHandler ya existe");
            return;
        }
        
        GameObject optionsGO = new GameObject("OptionsHandler");
        UniversalOptionsHandler optionsHandler = optionsGO.AddComponent<UniversalOptionsHandler>();
        
        Debug.Log("⚙️ OptionsHandler creado");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FinalFracasoSetupHelper))]
public class FinalFracasoSetupHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        FinalFracasoSetupHelper helper = (FinalFracasoSetupHelper)target;
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("🚀 SETUP AUTOMÁTICO", GUILayout.Height(40)))
        {
            helper.SetupFinalFracasoUI();
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Este script configurará automáticamente:\n" +
            "• FinalFracasoManager\n" +
            "• Panel de Fracaso con Animator\n" +
            "• Botones de Reintentar y Salir\n" +
            "• Textos de fracaso\n" +
            "• OptionsHandler para ESC\n\n" +
            "Haz click en 'Setup Automático' para configurar todo de una vez.",
            MessageType.Info
        );
    }
}
#endif 