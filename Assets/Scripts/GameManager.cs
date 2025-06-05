using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Configuración del Juego")]
    public float fallHeightLimit = -50f;        // Altura bajo la cual se considera "caído"
    public float checkInterval = 1f;            // Frecuencia de verificación de jugadores caídos
    
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
    }
    
    private void Start()
    {
        InitializePlayerTracking();
        StartCoroutine(CheckFallenPlayers());
        UpdateUI();
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
            playersCountText.text = $"Jugadores: {activePlayers.Count}";
        }
        
        // Mostrar en consola también para debug
        if (enableDebugLogs)
        {
            Debug.Log($"GameManager: Jugadores activos: {activePlayers.Count}/{initialPlayerCount}");
        }
    }
    
    private void CheckWinCondition()
    {
        if (activePlayers.Count <= 1 && !gameEnded)
        {
            gameEnded = true;
            
            if (activePlayers.Count == 1)
            {
                // Hay un ganador
                GameObject winner = activePlayers[0];
                if (enableDebugLogs)
                    Debug.Log($"GameManager: ¡{winner.name} ha ganado el juego!");
                
                ShowWinner(winner);
            }
            else
            {
                // Empate (todos eliminados)
                if (enableDebugLogs)
                    Debug.Log($"GameManager: ¡Empate! Todos los jugadores fueron eliminados");
                
                ShowDraw();
            }
        }
    }
    
    private void ShowWinner(GameObject winner)
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        
        if (winnerText != null)
        {
            winnerText.text = $"¡{winner.name} Ganó!";
        }
        
        // Opcional: Pausar el juego
        // Time.timeScale = 0f;
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
            UpdateUI();
            CheckWinCondition();
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
        return new List<GameObject>(activePlayers); // Retornar copia para evitar modificaciones externas
    }
    
    // Método para mostrar estadísticas en UI de debug
    private void OnGUI()
    {
        if (enableDebugLogs)
        {
            GUI.Box(new Rect(10, 10, 200, 80), "");
            GUI.Label(new Rect(15, 15, 190, 20), $"Jugadores Activos: {activePlayers.Count}");
            GUI.Label(new Rect(15, 35, 190, 20), $"Eliminados: {eliminatedPlayers.Count}");
            GUI.Label(new Rect(15, 55, 190, 20), $"Total Inicial: {initialPlayerCount}");
            GUI.Label(new Rect(15, 75, 190, 20), $"Juego Terminado: {gameEnded}");
        }
    }
} 