using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

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
    
    [Header("🔧 Debug")]
    public bool enableDebugLogs = false;
    
    // Variables privadas
    private List<GameObject> activePlayers = new List<GameObject>();
    private List<GameObject> eliminatedPlayers = new List<GameObject>();
    private int initialPlayerCount;
    private bool gameEnded = false;
    
    // Variables para Hexagonia
    private float hexagoniaTimeRemaining;
    private bool hexagoniaTimerStarted = false;
    private HexagoniaGameManager hexagoniaManager;
    
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
            return;
        }
        
        // Auto-detectar tipo de escena
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (currentScene == "Hexagonia")
        {
            isHexagoniaLevel = true;
            if (enableDebugLogs)
                Debug.Log("🔵 GameManager: Detectado nivel Hexagonia - Delegando control a HexagoniaGameManager");
            
            // Buscar o crear HexagoniaGameManager
            hexagoniaManager = FindObjectOfType<HexagoniaGameManager>();
            if (hexagoniaManager == null)
            {
                GameObject hmObj = new GameObject("HexagoniaGameManager");
                hexagoniaManager = hmObj.AddComponent<HexagoniaGameManager>();
                
                // Configurar referencias
                hexagoniaManager.timerText = hexagoniaTimerText;
                hexagoniaManager.gameDuration = hexagoniaTimerDuration;
                
                if (enableDebugLogs)
                    Debug.Log("🔵 GameManager: HexagoniaGameManager creado y configurado");
            }
        }
        else if (currentScene == "Carrera" || currentScene == "InGame")
        {
            isHexagoniaLevel = false;
            if (enableDebugLogs)
                Debug.Log($"🏁 GameManager: Detectado nivel de CARRERA - '{currentScene}' - Victoria por llegada a meta");
        }
        else
        {
            isHexagoniaLevel = false;
            if (enableDebugLogs)
                Debug.Log($"🎮 GameManager: Nivel estándar detectado - '{currentScene}'");
        }
    }
    
    private void Start()
    {
        if (!isHexagoniaLevel)
    {
        InitializePlayerTracking();
        StartCoroutine(CheckFallenPlayers());
            UpdateUI();
        }
    }
        
    private void Update()
    {
        if (!isHexagoniaLevel)
        {
            UpdateUI();
        }
    }
    
    private void InitializeHexagoniaTimer()
    {
        hexagoniaTimeRemaining = hexagoniaTimerDuration;
        hexagoniaTimerStarted = true;
        
        if (enableDebugLogs)
            Debug.Log($"🔵 GameManager: Timer de Hexagonia iniciado - {hexagoniaTimerDuration} segundos ({hexagoniaTimerDuration/60f:F1} minutos)");
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
        
        string currentScene = SceneManager.GetActiveScene().name;
        bool isRaceLevel = (currentScene == "Carrera" || currentScene == "InGame");
        
        if (enableDebugLogs)
        {
            if (isRaceLevel)
            {
                Debug.Log($"🏁 GameManager [CARRERA]: {foundIAs.Length} IAs + {foundPlayers.Length} Players = {initialPlayerCount} jugadores totales");
                Debug.Log($"🏁 En carreras, victoria se determina por llegada a meta, NO por eliminación");
            }
            else
            {
                Debug.Log($"GameManager: {foundIAs.Length} IAs + {foundPlayers.Length} Players = {initialPlayerCount} jugadores totales");
            }
        }
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
        string currentScene = SceneManager.GetActiveScene().name;
        bool isRaceLevel = (currentScene == "Carrera" || currentScene == "InGame");
        
        if (isRaceLevel)
        {
            // 🏁 LÓGICA DE CARRERAS: NO declarar victoria automáticamente
            // En carreras, la victoria se maneja por:
            // 1. Llegada a la meta (trigger externo)
            // 2. Fin del tiempo límite
            // 3. El GameManager solo trackea jugadores activos
            
            if (enableDebugLogs && activePlayers.Count <= 1)
            {
                Debug.Log($"🏁 GameManager [CARRERA]: {activePlayers.Count} jugadores activos - NO declarando victoria automática");
                Debug.Log("🏁 Victoria se determina por llegada a meta o tiempo límite");
            }
            
            // NO hacer nada - dejar que UIManager o CarreraManager manejen la victoria
            return;
        }
        else if (isHexagoniaLevel)
        {
            // 🔵 LÓGICA DE HEXAGONIA: Último en pie O tiempo agotado
            if (activePlayers.Count <= 1 && !gameEnded)
            {
                gameEnded = true;
                
                if (activePlayers.Count == 1)
                {
                    GameObject winner = activePlayers[0];
                    if (enableDebugLogs)
                        Debug.Log($"🔵 GameManager [HEXAGONIA]: ¡{winner.name} es el último en pie!");
                    
                    ShowLastPlayerStandingWinner(winner);
                }
                else
                {
                    if (enableDebugLogs)
                        Debug.Log($"🔵 GameManager [HEXAGONIA]: ¡Empate! Todos eliminados");
                    
                    ShowDraw();
                }
            }
        }
        else
        {
            // 🎮 LÓGICA ESTÁNDAR: Último en pie
            if (activePlayers.Count <= 1 && !gameEnded)
            {
                gameEnded = true;
                
                if (activePlayers.Count == 1)
                {
                    GameObject winner = activePlayers[0];
                    if (enableDebugLogs)
                        Debug.Log($"🎮 GameManager [ESTÁNDAR]: ¡{winner.name} es el último en pie!");
                    
                    ShowLastPlayerStandingWinner(winner);
                }
                else
                {
                    if (enableDebugLogs)
                        Debug.Log($"🎮 GameManager [ESTÁNDAR]: ¡Empate! Todos eliminados");
                    
                    ShowDraw();
                }
            }
        }
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
        if (isHexagoniaLevel && hexagoniaManager != null)
        {
            hexagoniaManager.OnPlayerDeath(player);
            return;
        }
        
        if (player != null && activePlayers.Contains(player))
        {
            EliminatePlayer(player);
        }
    }
    
    public int GetActivePlayerCount()
    {
        if (isHexagoniaLevel && hexagoniaManager != null)
        {
            return hexagoniaManager.GetPlayersAlive();
        }
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
        if (isHexagoniaLevel && hexagoniaManager != null)
        {
            return hexagoniaManager.GetTimeRemaining();
        }
        return hexagoniaTimeRemaining;
    }
    
    /// <summary>
    /// 🔵 Verificar si el timer de Hexagonia está activo
    /// </summary>
    public bool IsHexagoniaTimerActive()
    {
        if (isHexagoniaLevel && hexagoniaManager != null)
        {
            return hexagoniaManager.IsGameRunning();
        }
        return isHexagoniaLevel && hexagoniaTimerStarted;
    }
    
    /// <summary>
    /// 🏁 Forzar victoria en carrera (llamado externamente por llegada a meta)
    /// </summary>
    public void ForceRaceVictory(GameObject winner = null)
    {
        if (gameEnded) return;
        
        string currentScene = SceneManager.GetActiveScene().name;
        bool isRaceLevel = (currentScene == "Carrera" || currentScene == "InGame");
        
        if (!isRaceLevel)
        {
            Debug.LogWarning("⚠️ ForceRaceVictory llamado en escena que no es carrera");
            return;
        }
        
        gameEnded = true;
        
        if (winner != null)
        {
            if (enableDebugLogs)
                Debug.Log($"🏁 GameManager [CARRERA]: ¡{winner.name} llegó a la meta!");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log($"🏁 GameManager [CARRERA]: Victoria forzada (tiempo agotado)");
        }
        
        // NO deshabilitar el jugador - dejar que UIManager maneje la transición
    }
    
    /// <summary>
    /// 🏁 Verificar si estamos en un nivel de carrera
    /// </summary>
    public bool IsRaceLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        return (currentScene == "Carrera" || currentScene == "InGame");
    }
    
    /// <summary>
    /// 🎮 Eliminar un jugador del juego
    /// </summary>
    public void PlayerEliminated(GameObject player)
    {
        if (player == null || gameEnded) return;

        // Remover de jugadores activos y agregar a eliminados
        if (activePlayers.Contains(player))
        {
            activePlayers.Remove(player);
            eliminatedPlayers.Add(player);
            
            if (enableDebugLogs)
                Debug.Log($"👻 Jugador eliminado: {player.name} - Quedan: {activePlayers.Count}");

            // Actualizar UI
            UpdateUI();

            // Verificar condición de victoria
            CheckWinCondition();
        }
    }

    private void GameOver(GameObject winner)
    {
        gameEnded = true;

        // Mostrar panel de victoria si existe
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            
            if (winnerText != null)
            {
                if (winner != null)
                {
                    string winnerName = winner.name;
                    PhotonView pv = winner.GetComponent<PhotonView>();
                    if (pv != null)
                    {
                        winnerName = pv.Owner.NickName;
                    }
                    winnerText.text = $"¡{winnerName} ha ganado!";
                }
                else
                {
                    winnerText.text = "¡Todos han sido eliminados!";
                }
            }
        }

        // Logging
        if (enableDebugLogs)
        {
            if (winner != null)
                Debug.Log($"🏆 Juego terminado - Ganador: {winner.name}");
            else
                Debug.Log("💀 Juego terminado - Todos eliminados");
        }

        // Volver al lobby después de 5 segundos
        StartCoroutine(ReturnToLobbyAfterDelay(5f));
    }

    private IEnumerator ReturnToLobbyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Lobby");
    }
    
    private void OnGUI()
    {
        if (enableDebugLogs && !gameEnded)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            bool isRaceLevel = IsRaceLevel();
            
            if (isHexagoniaLevel)
            {
                GUI.Box(new Rect(10, 10, 300, 120), "");
                GUI.Label(new Rect(15, 15, 290, 20), $"🔵 HEXAGONIA GameManager");
                GUI.Label(new Rect(15, 35, 290, 20), $"Jugadores activos: {activePlayers.Count}");
                GUI.Label(new Rect(15, 55, 290, 20), $"Tiempo restante: {hexagoniaTimeRemaining:F1}s");
                GUI.Label(new Rect(15, 75, 290, 20), $"Timer activo: {hexagoniaTimerStarted}");
                GUI.Label(new Rect(15, 95, 290, 20), $"Condición: Último en pie O 3 minutos");
            }
            else if (isRaceLevel)
            {
                GUI.Box(new Rect(10, 10, 280, 100), "");
                GUI.Label(new Rect(15, 15, 270, 20), $"🏁 CARRERA GameManager");
                GUI.Label(new Rect(15, 35, 270, 20), $"Escena: {currentScene}");
                GUI.Label(new Rect(15, 55, 270, 20), $"Jugadores activos: {activePlayers.Count}");
                GUI.Label(new Rect(15, 75, 270, 20), $"Condición: Llegada a meta o tiempo");
            }
            else
            {
                GUI.Box(new Rect(10, 10, 250, 80), "");
                GUI.Label(new Rect(15, 15, 240, 20), $"🎮 ESTÁNDAR GameManager");
                GUI.Label(new Rect(15, 35, 240, 20), $"Jugadores: {activePlayers.Count}");
                GUI.Label(new Rect(15, 55, 240, 20), $"Juego activo: {!gameEnded}");
            }
        }
    }

    /// <summary>
    /// 🎯 Configurar los RealDestPos de la escena
    /// </summary>
    private void SetupRealDestPos()
    {
        // Buscar todos los RealDestPos en la escena
        GameObject[] realDestPosObjects = GameObject.FindGameObjectsWithTag("RealDestPos");
        
        if (realDestPosObjects.Length == 0)
        {
            if (enableDebugLogs)
                Debug.Log("🎯 GameManager: No se encontraron RealDestPos en la escena");
            return;
        }

        foreach (GameObject destPos in realDestPosObjects)
        {
            // Asegurarse de que tenga el script RealDestPosTrigger
            RealDestPosTrigger trigger = destPos.GetComponent<RealDestPosTrigger>();
            if (trigger == null)
            {
                trigger = destPos.AddComponent<RealDestPosTrigger>();
                if (enableDebugLogs)
                    Debug.Log($"🎯 GameManager: Añadido RealDestPosTrigger a {destPos.name}");
            }

            // Configurar el trigger según el tipo de nivel
            string currentScene = SceneManager.GetActiveScene().name;
            bool isRaceLevel = (currentScene == "Carrera" || currentScene == "InGame");

            trigger.isFinishLine = isRaceLevel; // Solo es línea de meta en niveles de carrera
            trigger.enableDebugLogs = this.enableDebugLogs;

            if (enableDebugLogs)
                Debug.Log($"🎯 GameManager: Configurado {destPos.name} como {(isRaceLevel ? "meta" : "checkpoint")}");
        }
    }
} 