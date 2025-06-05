using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class LavaGameManager : MonoBehaviourPunCallbacks
{
    [Header("Configuración del Juego de Lava")]
    public float gameEndDelay = 2f;             // Tiempo antes de mostrar pantalla final
    public bool endGameOnFirstLavaDeath = true; // Si terminar el juego con la primera muerte por lava
    
    [Header("UI del Juego")]
    public Text gameStatusText;                 // Texto de estado del juego
    public GameObject gameOverPanel;            // Panel de Game Over
    public Text gameOverText;                   // Texto del Game Over
    public Button restartButton;                // Botón de reinicio
    public Button exitButton;                   // Botón de salir
    
    [Header("Efectos Visuales")]
    public GameObject lavaDeathEffect;          // Efecto visual de muerte por lava
    public AudioSource gameOverSound;          // Sonido de Game Over
    public AudioSource backgroundMusic;        // Música de fondo
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    // Variables privadas
    private List<GameObject> allPlayers = new List<GameObject>();
    private List<GameObject> lavaVictims = new List<GameObject>();
    private bool gameEnded = false;
    private bool lavaDeathOccurred = false;
    private int initialPlayerCount;
    
    // Singleton para fácil acceso
    public static LavaGameManager Instance;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        InitializeGame();
        SetupUI();
    }
    
    private void InitializeGame()
    {
        // Buscar solo los jugadores reales (Players)
        GameObject[] foundPlayers = GameObject.FindGameObjectsWithTag("Player");
        
        allPlayers.Clear();
        lavaVictims.Clear();
        
        // Solo añadir Players reales (no IAs)
        foreach (GameObject player in foundPlayers)
        {
            if (player != null && player.activeInHierarchy)
            {
                allPlayers.Add(player);
            }
        }
        
        initialPlayerCount = allPlayers.Count;
        gameEnded = false;
        lavaDeathOccurred = false;
        
        if (enableDebugLogs)
            Debug.Log($"LavaGameManager: Juego iniciado. Solo monitoreando {initialPlayerCount} Players reales (ignorando IAs)");
        
        UpdateGameStatus();
    }
    
    private void SetupUI()
    {
        // Configurar botones
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
        
        // Ocultar panel de Game Over al inicio
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    // Método principal para manejar muerte por lava (SOLO PLAYERS)
    public void HandleLavaDeath(GameObject victim)
    {
        if (gameEnded || victim == null)
            return;
        
        // ✅ SOLO PROCESAR SI ES UN PLAYER REAL
        if (!victim.CompareTag("Player"))
        {
            if (enableDebugLogs)
                Debug.Log($"LavaGameManager: {victim.name} (tag: {victim.tag}) tocó lava, pero no es Player. Ignorando.");
            return;
        }
        
        if (enableDebugLogs)
            Debug.Log($"LavaGameManager: ¡PLAYER {victim.name} murió por lava! Terminando juego...");
        
        // Añadir a lista de víctimas
        if (!lavaVictims.Contains(victim))
        {
            lavaVictims.Add(victim);
            lavaDeathOccurred = true;
        }
        
        // Crear efecto visual en la posición de la víctima
        CreateLavaDeathEffect(victim.transform.position);
        
        // Reproducir sonido de muerte
        PlayDeathSound();
        
        // ✅ SIEMPRE TERMINAR EL JUEGO cuando un Player toca lava
        StartCoroutine(EndGameAfterDelay());
    }
    
    private void CreateLavaDeathEffect(Vector3 position)
    {
        if (lavaDeathEffect != null)
        {
            GameObject effect = Instantiate(lavaDeathEffect, position, Quaternion.identity);
            
            // Destruir el efecto después de unos segundos
            Destroy(effect, 3f);
        }
    }
    
    private void PlayDeathSound()
    {
        if (gameOverSound != null && !gameOverSound.isPlaying)
        {
            gameOverSound.Play();
        }
    }
    
    private IEnumerator EndGameAfterDelay()
    {
        if (gameEnded)
            yield break;
        
        gameEnded = true;
        
        if (enableDebugLogs)
            Debug.Log($"LavaGameManager: Terminando juego en {gameEndDelay} segundos...");
        
        // Parar música de fondo
        if (backgroundMusic != null && backgroundMusic.isPlaying)
        {
            StartCoroutine(FadeOutMusic());
        }
        
        // Esperar antes de mostrar pantalla final
        yield return new WaitForSeconds(gameEndDelay);
        
        // Mostrar pantalla de Game Over
        ShowGameOverScreen();
    }
    
    private IEnumerator FadeOutMusic()
    {
        if (backgroundMusic == null)
            yield break;
        
        float startVolume = backgroundMusic.volume;
        float fadeTime = 1f;
        float elapsedTime = 0;
        
        while (elapsedTime < fadeTime)
        {
            backgroundMusic.volume = Mathf.Lerp(startVolume, 0, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        backgroundMusic.Stop();
        backgroundMusic.volume = startVolume; // Restaurar volumen original
    }
    
    private void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Determinar mensaje de Game Over
        string gameOverMessage = GetGameOverMessage();
        
        if (gameOverText != null)
        {
            gameOverText.text = gameOverMessage;
        }
        
        if (enableDebugLogs)
            Debug.Log($"LavaGameManager: {gameOverMessage}");
        
        // Opcional: Pausar el juego
        // Time.timeScale = 0f;
    }
    
    private string GetGameOverMessage()
    {
        if (lavaVictims.Count == 0)
        {
            return "¡Juego Terminado!\n¡Ningún Player tocó la lava!";
        }
        else if (lavaVictims.Count == 1)
        {
            return $"¡Juego Terminado!\nEl Player {lavaVictims[0].name} tocó la lava";
        }
        else
        {
            return $"¡Juego Terminado!\n{lavaVictims.Count} Players tocaron la lava";
        }
    }
    
    private void UpdateGameStatus()
    {
        if (gameStatusText != null && !gameEnded)
        {
            if (lavaDeathOccurred)
            {
                gameStatusText.text = $"¡GAME OVER! Player tocó la lava";
                gameStatusText.color = Color.red;
            }
            else
            {
                gameStatusText.text = $"Players activos: {allPlayers.Count} - ¡Evita la lava!";
                gameStatusText.color = Color.white;
            }
        }
    }
    
    // Métodos públicos para uso externo
    public void RestartGame()
    {
        if (enableDebugLogs)
            Debug.Log("LavaGameManager: Reiniciando juego...");
        
        // Restaurar time scale
        Time.timeScale = 1f;
        
        // Recargar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ExitGame()
    {
        if (enableDebugLogs)
            Debug.Log("LavaGameManager: Saliendo del juego...");
        
        // Restaurar time scale
        Time.timeScale = 1f;
        
        // En el editor, detener play mode
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // En build, cerrar aplicación
        Application.Quit();
        #endif
    }
    
    public bool IsGameEnded()
    {
        return gameEnded;
    }
    
    public int GetLavaVictimCount()
    {
        return lavaVictims.Count;
    }
    
    public List<GameObject> GetLavaVictims()
    {
        return new List<GameObject>(lavaVictims);
    }
    
    public int GetTotalPlayerCount()
    {
        return allPlayers.Count;
    }
    
    // Método para forzar el fin del juego (para testing)
    public void ForceEndGame()
    {
        if (!gameEnded)
        {
            StartCoroutine(EndGameAfterDelay());
        }
    }
    
    // Métodos de testing
    private void Update()
    {
        // TEST: Forzar fin de juego con tecla G
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (enableDebugLogs)
                Debug.Log("LavaGameManager: TEST - Forzando fin de juego");
            
            ForceEndGame();
        }
        
        // TEST: Simular muerte por lava de PLAYER con tecla V
        if (Input.GetKeyDown(KeyCode.V))
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length > 0)
            {
                GameObject randomPlayer = players[Random.Range(0, players.Length)];
                if (enableDebugLogs)
                    Debug.Log($"LavaGameManager: TEST - Simulando muerte por lava de PLAYER {randomPlayer.name}");
                
                HandleLavaDeath(randomPlayer);
            }
            else
            {
                Debug.Log("LavaGameManager: No se encontraron Players para simular muerte");
            }
        }
        
        // TEST: Debug completo con tecla I
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("=== DEBUG LAVA GAME MANAGER ===");
            Debug.Log($"Players totales: {allPlayers.Count}");
            Debug.Log($"Players víctimas de lava: {lavaVictims.Count}");
            Debug.Log($"Juego terminado: {gameEnded}");
            Debug.Log($"Muerte por lava ocurrió: {lavaDeathOccurred}");
            Debug.Log("NOTA: Solo Players terminan el juego, IAs son ignoradas");
        }
    }
    
    // UI de debug en pantalla
    private void OnGUI()
    {
        if (enableDebugLogs && !gameEnded)
        {
            GUI.Box(new Rect(10, 100, 250, 120), "");
            GUI.Label(new Rect(15, 105, 240, 20), $"Lava Game Manager");
            GUI.Label(new Rect(15, 125, 240, 20), $"Players: {allPlayers.Count}");
            GUI.Label(new Rect(15, 145, 240, 20), $"Víctimas Lava: {lavaVictims.Count}");
            GUI.Label(new Rect(15, 165, 240, 20), $"Juego Activo: {!gameEnded}");
            GUI.Label(new Rect(15, 185, 240, 20), $"Solo Players terminan juego");
            GUI.Label(new Rect(15, 205, 240, 20), $"Teclas: G(End) V(Kill) I(Debug)");
        }
    }
} 