using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Configuración del Juego")]
    public float fallHeightLimit = -50f;        // Altura bajo la cual se considera "caído"
    public float checkInterval = 1f;            // Frecuencia de verificación de jugadores caídos
    
    [Header("🎯 Configuración Hexagonia")]
    public bool isHexagoniaLevel = false;       // Si este GameManager es para Hexagonia
    public float hexagoniaTimerDuration = 180f; // 3 minutos para Hexagonia
    public Text hexagoniaTimerText;             // UI Text para mostrar timer de Hexagonia
    
    [Header("UI del Contador")]
    public Text playersCountText;               // Texto UI para mostrar contador
    public GameObject winPanel;                 // Panel de victoria (opcional)
    public Text winnerText;                     // Texto del ganador (opcional)
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    // Variables privadas
    private List<GameObject> activePlayers = new List<GameObject>();
    private List<GameObject> eliminatedPlayers = new List<GameObject>();
    private int initialPlayerCount;
    private bool gameEnded = false;
    
    // Variables para Hexagonia
    private float hexagoniaTimeRemaining;
    private bool hexagoniaTimerStarted = false;
    
    // Singleton para fácil acceso
    public static GameManager Instance;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Auto-detectar si estamos en Hexagonia
        if (SceneManager.GetActiveScene().name == "Hexagonia")
        {
            isHexagoniaLevel = true;
            if (enableDebugLogs)
                Debug.Log("🔵 GameManager: Detectado nivel Hexagonia - Timer de 3 minutos activado");
        }
    }
    
    private void Start()
    {
        InitializePlayerTracking();
        StartCoroutine(CheckFallenPlayers());
        
        // Inicializar timer de Hexagonia si corresponde
        if (isHexagoniaLevel)
        {
            InitializeHexagoniaTimer();
        }
        
        UpdateUI();
    }
    
    private void InitializeHexagoniaTimer()
    {
        hexagoniaTimeRemaining = hexagoniaTimerDuration;
        hexagoniaTimerStarted = true;
        
        if (enableDebugLogs)
            Debug.Log($"🔵 GameManager: Timer de Hexagonia iniciado - {hexagoniaTimerDuration} segundos ({hexagoniaTimerDuration/60f:F1} minutos)");
    }
    
    private void Update()
    {
        // Solo para Hexagonia: Actualizar timer
        if (isHexagoniaLevel && hexagoniaTimerStarted && !gameEnded)
        {
            UpdateHexagoniaTimer();
        }
    }
    
    private void UpdateHexagoniaTimer()
    {
        hexagoniaTimeRemaining -= Time.deltaTime;
        
        // Actualizar UI del timer
        if (hexagoniaTimerText != null)
        {
            int minutes = (int)(hexagoniaTimeRemaining / 60);
            int seconds = (int)(hexagoniaTimeRemaining % 60);
            
            if (hexagoniaTimeRemaining > 30f)
            {
                hexagoniaTimerText.text = $"{minutes}:{seconds:D2}";
                hexagoniaTimerText.color = Color.white;
            }
            else if (hexagoniaTimeRemaining > 10f)
            {
                hexagoniaTimerText.text = $"<color=yellow>{minutes}:{seconds:D2}</color>";
            }
            else if (hexagoniaTimeRemaining > 0f)
            {
                hexagoniaTimerText.text = $"<color=red>{seconds}</color>";
            }
            else
            {
                hexagoniaTimerText.text = "<color=red>¡TIEMPO!</color>";
            }
        }
        
        // ⏰ TIEMPO AGOTADO - Los supervivientes ganan
        if (hexagoniaTimeRemaining <= 0f)
        {
            OnHexagoniaTimeUp();
        }
    }
    
    /// <summary>
    /// 🎯 Se acabó el tiempo en Hexagonia - Los supervivientes ganan
    /// </summary>
    private void OnHexagoniaTimeUp()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        hexagoniaTimerStarted = false;
        
        if (enableDebugLogs)
            Debug.Log($"🔵 GameManager: ¡TIEMPO AGOTADO! Supervivientes: {activePlayers.Count}");
        
        if (activePlayers.Count > 0)
        {
            // Los supervivientes ganan
            ShowHexagoniaTimeUpVictory();
        }
        else
        {
            // Nadie sobrevivió
            ShowDraw();
        }
    }
    
    private void ShowHexagoniaTimeUpVictory()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        
        if (winnerText != null)
        {
            if (activePlayers.Count == 1)
            {
                winnerText.text = $"¡{activePlayers[0].name} Sobrevivió 3 Minutos!";
            }
            else
            {
                winnerText.text = $"¡{activePlayers.Count} Jugadores Sobrevivieron!";
            }
        }
        
        if (enableDebugLogs)
            Debug.Log($"🔵 GameManager: Victoria por tiempo - {activePlayers.Count} supervivientes");
        
        // Transición a escena de éxito después de un delay
        StartCoroutine(TransitionToSuccessAfterDelay());
    }
    
    private IEnumerator TransitionToSuccessAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        
        // Buscar SceneChange component
        SceneChange sceneChanger = FindObjectOfType<SceneChange>();
        
        if (sceneChanger != null)
        {
            if (enableDebugLogs)
                Debug.Log("🔵 GameManager: Transicionando a escena de éxito...");
            
            sceneChanger.GoToEndingSuccess();
        }
        else
        {
            // Fallback
            SceneManager.LoadScene("Ending");
        }
    }
    
    private void InitializePlayerTracking()
    {
        // Buscar todos los jugadores con tag "IA" y "Player"
        GameObject[] foundIAs = GameObject.FindGameObjectsWithTag("IA");
        GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");
        
        activePlayers.Clear();
        
        // Añadir IAs
        foreach (GameObject player in foundIAs)
        {
            if (player != null && player.activeInHierarchy)
            {
                activePlayers.Add(player);
            }
        }
        
        // Añadir Players reales
        foreach (GameObject player in foundPlayers)
        {
            if (player != null && player.activeInHierarchy)
            {
                activePlayers.Add(player);
            }
        }
        
        initialPlayerCount = activePlayers.Count;
        
        if (enableDebugLogs)
            Debug.Log($"GameManager: {foundIAs.Length} IAs + {foundPlayers.Length} Players = {initialPlayerCount} jugadores totales");
    }
    
    private IEnumerator CheckFallenPlayers()
    {
        while (!gameEnded)
        {
            yield return new WaitForSeconds(checkInterval);
            
            // Crear lista temporal para evitar modificar la lista mientras iteramos
            List<GameObject> playersToRemove = new List<GameObject>();
            
            foreach (GameObject player in activePlayers)
            {
                if (player == null)
                {
                    // Jugador ya fue destruido
                    playersToRemove.Add(player);
                }
                else if (player.transform.position.y < fallHeightLimit)
                {
                    // Jugador cayó por debajo del límite
                    playersToRemove.Add(player);
                }
            }
            
            // Eliminar jugadores caídos
            foreach (GameObject player in playersToRemove)
            {
                EliminatePlayer(player);
            }
            
            // Actualizar UI
            UpdateUI();
            
            // Verificar condición de victoria
            CheckWinCondition();
        }
    }
    
    private void EliminatePlayer(GameObject player)
    {
        if (player != null && activePlayers.Contains(player))
        {
            activePlayers.Remove(player);
            eliminatedPlayers.Add(player);
            
            if (enableDebugLogs)
                Debug.Log($"GameManager: Jugador {player.name} eliminado. Restantes: {activePlayers.Count}");
            
            // Opcional: Añadir efectos de eliminación aquí
            StartCoroutine(DestroyPlayerWithEffect(player));
        }
    }
    
    private IEnumerator DestroyPlayerWithEffect(GameObject player)
    {
        // Opcional: Añadir efectos visuales de eliminación
        if (player != null)
        {
            // Cambiar color o añadir efecto antes de destruir
            Renderer playerRenderer = player.GetComponent<Renderer>();
            if (playerRenderer != null)
            {
                playerRenderer.material.color = Color.gray; // Color de eliminado
            }
            
            // Esperar un poco antes de destruir
            yield return new WaitForSeconds(1f);
            
            // Destruir el jugador
            Destroy(player);
        }
    }
    
    private void UpdateUI()
    {
        if (playersCountText != null)
        {
            // Solo mostrar información de jugadores (sin tiempo)
            // El tiempo ya se muestra en hexagoniaTimerText separadamente
            playersCountText.text = $"{eliminatedPlayers.Count}/{initialPlayerCount}";
        }
        
        // Mostrar en consola también para debug
        if (enableDebugLogs)
        {
            if (isHexagoniaLevel)
            {
                Debug.Log($"🔵 GameManager: Jugadores activos: {activePlayers.Count}/{initialPlayerCount} | Tiempo: {hexagoniaTimeRemaining:F1}s");
            }
            else
            {
                Debug.Log($"GameManager: Jugadores activos: {activePlayers.Count}/{initialPlayerCount}");
            }
        }
    }
    
    private void CheckWinCondition()
    {
        // ✅ Condición 1: Último jugador en pie (siempre)
        if (activePlayers.Count <= 1 && !gameEnded)
        {
            gameEnded = true;
            
            if (activePlayers.Count == 1)
            {
                // Hay un ganador por eliminación
                GameObject winner = activePlayers[0];
                if (enableDebugLogs)
                    Debug.Log($"🔵 GameManager: ¡{winner.name} es el último en pie!");
                
                ShowLastPlayerStandingWinner(winner);
            }
            else
            {
                // Empate (todos eliminados)
                if (enableDebugLogs)
                    Debug.Log($"GameManager: ¡Empate! Todos los jugadores fueron eliminados");
                
                ShowDraw();
            }
        }
        
        // ✅ Para niveles NO-Hexagonia: La condición de arriba es suficiente
        // ✅ Para Hexagonia: La condición de tiempo se maneja en UpdateHexagoniaTimer()
    }
    
    private void ShowLastPlayerStandingWinner(GameObject winner)
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        
        if (winnerText != null)
        {
            if (isHexagoniaLevel)
            {
                winnerText.text = $"¡{winner.name} es el Último en Pie!";
            }
            else
            {
                winnerText.text = $"¡{winner.name} Ganó!";
            }
        }
        
        // Detener timer de Hexagonia si está activo
        if (isHexagoniaLevel)
        {
            hexagoniaTimerStarted = false;
        }
        
        // Transición a escena de éxito
        StartCoroutine(TransitionToSuccessAfterDelay());
    }
    
    private void ShowDraw()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        
        if (winnerText != null)
        {
            winnerText.text = "¡Empate!";
        }
        
        // Detener timer de Hexagonia si está activo
        if (isHexagoniaLevel)
        {
            hexagoniaTimerStarted = false;
        }
    }
    
    // Métodos públicos para uso externo
    public void RestartGame()
    {
        gameEnded = false;
        activePlayers.Clear();
        eliminatedPlayers.Clear();
        
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        
        // Reiniciar timer de Hexagonia
        if (isHexagoniaLevel)
        {
            InitializeHexagoniaTimer();
        }
        
        // Recargar la escena o reinicializar
        InitializePlayerTracking();
        
        // Opcional: Restaurar time scale
        Time.timeScale = 1f;
    }
    
    public void ForceEliminatePlayer(GameObject player)
    {
        if (player != null && activePlayers.Contains(player))
        {
            EliminatePlayer(player);
        }
    }
    
    public int GetActivePlayerCount()
    {
        return activePlayers.Count;
    }
    
    public int GetEliminatedPlayerCount()
    {
        return eliminatedPlayers.Count;
    }
    
    public List<GameObject> GetActivePlayers()
    {
        return new List<GameObject>(activePlayers);
    }
    
    /// <summary>
    /// 🔵 Obtener tiempo restante en Hexagonia
    /// </summary>
    public float GetHexagoniaTimeRemaining()
    {
        return hexagoniaTimeRemaining;
    }
    
    /// <summary>
    /// 🔵 Verificar si el timer de Hexagonia está activo
    /// </summary>
    public bool IsHexagoniaTimerActive()
    {
        return isHexagoniaLevel && hexagoniaTimerStarted;
    }
    
    private void OnGUI()
    {
        if (enableDebugLogs && !gameEnded)
        {
            if (isHexagoniaLevel)
            {
                GUI.Box(new Rect(10, 10, 300, 120), "");
                GUI.Label(new Rect(15, 15, 290, 20), $"🔵 HEXAGONIA GameManager");
                GUI.Label(new Rect(15, 35, 290, 20), $"Jugadores activos: {activePlayers.Count}");
                GUI.Label(new Rect(15, 55, 290, 20), $"Tiempo restante: {hexagoniaTimeRemaining:F1}s");
                GUI.Label(new Rect(15, 75, 290, 20), $"Timer activo: {hexagoniaTimerStarted}");
                GUI.Label(new Rect(15, 95, 290, 20), $"Condición: Último en pie O 3 minutos");
            }
            else
            {
                GUI.Box(new Rect(10, 10, 250, 80), "");
                GUI.Label(new Rect(15, 15, 240, 20), $"GameManager");
                GUI.Label(new Rect(15, 35, 240, 20), $"Jugadores: {activePlayers.Count}");
                GUI.Label(new Rect(15, 55, 240, 20), $"Juego activo: {!gameEnded}");
            }
        }
    }
} 