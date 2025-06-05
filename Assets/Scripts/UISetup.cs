using UnityEngine;
using UnityEngine.UI;

public class UISetup : MonoBehaviour
{
    [Header("UI Configuration")]
    public Font uiFont;
    public int fontSize = 24;
    public Color textColor = Color.white;
    
    private void Start()
    {
        SetupPlayerCounterUI();
    }
    
    private void SetupPlayerCounterUI()
    {
        // Buscar si ya existe un Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        
        if (canvas == null)
        {
            // Crear Canvas si no existe
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Verificar si ya existe el texto del contador
        Text existingText = canvas.GetComponentInChildren<Text>();
        if (existingText != null && existingText.name == "PlayerCountText")
        {
            // Ya existe, no crear otro
            return;
        }
        
        // Crear el texto del contador de jugadores
        GameObject textObj = new GameObject("PlayerCountText");
        textObj.transform.SetParent(canvas.transform, false);
        
        Text playerCountText = textObj.AddComponent<Text>();
        
        // Configurar el texto
        if (uiFont != null)
        {
            playerCountText.font = uiFont;
        }
        else
        {
            // Usar font por defecto
            playerCountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        
        playerCountText.text = "Jugadores: 0";
        playerCountText.fontSize = fontSize;
        playerCountText.color = textColor;
        playerCountText.alignment = TextAnchor.UpperLeft;
        
        // Posicionar en la esquina superior izquierda
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(20, -20);
        rectTransform.sizeDelta = new Vector2(200, 50);
        
        // Asignar al GameManager si existe
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.playersCountText = playerCountText;
        }
        
        Debug.Log("UISetup: Contador de jugadores creado automáticamente");
    }
} 