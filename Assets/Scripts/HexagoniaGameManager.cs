using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class HexagoniaGameManager : MonoBehaviourPunCallbacks
{
    [Header("⏱️ Timer Configuration")]
    public float gameDuration = 180f;  // 3 minutos
    public Text timerText;
    
    [Header("🎮 Game State")]
    public bool enableDebugLogs = true;
    private float timeRemaining;
    private bool gameStarted = false;
    private bool gameEnded = false;
    private List<GameObject> activePlayers = new List<GameObject>();
    private List<GameObject> eliminatedPlayers = new List<GameObject>();
    
    [Header("📊 Player Counter")]
    public Text playersAliveText; // Texto para mostrar jugadores restantes
    
    // Singleton
    public static HexagoniaGameManager Instance;
    
    private void Awake()
    {
        Debug.Log("HexagoniaGameManager: Awake llamado");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("HexagoniaGameManager: Instancia creada como singleton");
        }
        else
        {
            Debug.Log("HexagoniaGameManager: Ya existe una instancia, destruyendo duplicado");
            Destroy(gameObject);
            return;
        }
        
        // Inicializar variables
        timeRemaining = gameDuration;
        gameStarted = false;
        gameEnded = false;
    }
    
    private void Start()
    {
        Debug.Log("HexagoniaGameManager: Start llamado");
        // Inicializar lista de jugadores
        InitializePlayerList();
        
        // Iniciar actualización periódica de la lista de jugadores
        InvokeRepeating("UpdatePlayerList", 1f, 1f);
    }

    private void InitializePlayerList()
    {
        Debug.Log("HexagoniaGameManager: Inicializando lista de jugadores");
        activePlayers.Clear();
        eliminatedPlayers.Clear();

        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        // Encontrar todos los jugadores en la escena
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] ais = GameObject.FindGameObjectsWithTag("IA");

        activePlayers.Clear(); // Limpiar lista actual

        foreach (GameObject player in players)
        {
            if (player != null && player.activeInHierarchy)
            {
                activePlayers.Add(player);
                Debug.Log($"👤 Jugador activo encontrado: {player.name}");
            }
        }

        foreach (GameObject ai in ais)
        {
            if (ai != null && ai.activeInHierarchy)
            {
                activePlayers.Add(ai);
                Debug.Log($"🤖 IA activa encontrada: {ai.name}");
            }
        }

        // Actualizar UI del contador de jugadores
        if (playersAliveText != null)
        {
            playersAliveText.text = $"Jugadores: {activePlayers.Count}";
        }

        Debug.Log($"🎮 Total jugadores activos actualizados: {activePlayers.Count}");
    }

    public void StartGame()
    {
        Debug.Log("🎮 Iniciando juego de Hexagonia");
        
        gameStarted = true;
        gameEnded = false;
        timeRemaining = gameDuration;
        
        // Actualizar lista de jugadores al inicio
        UpdatePlayerList();
    }

    public void OnCountdownFinished()
    {
        Debug.Log("⏱️ Cuenta regresiva terminada - Iniciando juego");
        StartGame();
    }

    private void Update()
    {
        if (!gameStarted || gameEnded) return;

        // Actualizar timer
        timeRemaining -= Time.deltaTime;
        
        // Actualizar UI del timer
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        // Verificar condiciones de fin
        if (timeRemaining <= 0)
        {
            OnTimeUp();
        }
        else if (activePlayers.Count <= 1)
        {
            OnLastPlayerStanding();
        }
    }

    public void OnPlayerDeath(GameObject player)
    {
        if (!gameStarted || gameEnded) return;

        Debug.Log($"💀 Jugador eliminado: {player.name}");

        // Remover de la lista de activos
        if (activePlayers.Contains(player))
        {
            activePlayers.Remove(player);
            eliminatedPlayers.Add(player);
            
            // Actualizar UI
            if (playersAliveText != null)
            {
                playersAliveText.text = $"Jugadores: {activePlayers.Count}";
            }

            Debug.Log($"🎮 Jugadores restantes: {activePlayers.Count}");
        }

        // Verificar si quedan jugadores
        if (activePlayers.Count <= 1)
        {
            OnLastPlayerStanding();
        }
    }

    private void OnTimeUp()
    {
        if (gameEnded) return;
        
        Debug.Log("⏱️ ¡Tiempo agotado!");

        gameEnded = true;
        
        // Los jugadores que sobrevivieron ganan
        foreach (GameObject player in activePlayers)
        {
            if (player.CompareTag("Player"))
            {
                PhotonView playerView = player.GetComponent<PhotonView>();
                if (playerView != null && playerView.IsMine)
                {
                    StartCoroutine(TransitionToEndingSuccess());
                    return;
                }
            }
        }
    }

    private void OnLastPlayerStanding()
    {
        if (gameEnded) return;
        
        Debug.Log("👑 ¡Último jugador en pie!");

        gameEnded = true;

        // Si el último jugador es el jugador local, victoria
        if (activePlayers.Count == 1)
        {
            GameObject lastPlayer = activePlayers[0];
            if (lastPlayer.CompareTag("Player"))
            {
                PhotonView playerView = lastPlayer.GetComponent<PhotonView>();
                if (playerView != null && playerView.IsMine)
                {
                    StartCoroutine(TransitionToEndingSuccess());
                    return;
                }
            }
        }
    }

    private IEnumerator TransitionToEndingSuccess()
    {
        yield return new WaitForSeconds(2f);
        
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LoadLevel("Ending");
        }
        else
        {
            SceneManager.LoadScene("Ending");
        }
    }

    // Métodos públicos para acceso externo
    public float GetTimeRemaining() => timeRemaining;
    public int GetPlayersAlive() => activePlayers.Count;
    public bool IsGameRunning() => gameStarted && !gameEnded;
    
    void OnGUI()
    {
        if (!enableDebugLogs) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Box("🎮 HEXAGONIA MANAGER");
        GUILayout.Label($"Juego iniciado: {gameStarted}");
        GUILayout.Label($"Juego terminado: {gameEnded}");
        GUILayout.Label($"Tiempo restante: {timeRemaining:F1}s");
        GUILayout.Label($"Jugadores activos: {activePlayers.Count}");
        GUILayout.Label($"Jugadores eliminados: {eliminatedPlayers.Count}");
        GUILayout.EndArea();
    }
} 