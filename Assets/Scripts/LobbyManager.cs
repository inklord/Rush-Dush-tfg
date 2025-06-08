using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

/// <summary>
/// 🌐 Gestor principal del Lobby para modo multijugador
/// Maneja conexión a Photon, creación/unión a salas, y transición a modo multijugador
/// </summary>
public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("🎮 UI Elements")]
    public Button singlePlayerButton;          // Botón modo un jugador (existente)
    public Button multiplayerButton;           // Botón modo multijugador (NUEVO)
    public GameObject multiplayerPanel;        // Panel de opciones multijugador
    public GameObject connectionPanel;         // Panel de estado de conexión
    
    [Header("🌐 Multiplayer UI")]
    public Button createRoomButton;            // Crear sala
    public Button joinRandomButton;            // Unirse a sala aleatoria
    public Button backToMenuButton;            // Volver al menú principal
    public TMP_InputField roomNameInput;       // Nombre de sala personalizada (TMP)
    public TMP_InputField playerNameInput;     // Nombre del jugador (TMP)
    
    [Header("📊 Status Display")]
    public TextMeshProUGUI statusText;         // Estado de conexión
    public TextMeshProUGUI roomInfoText;       // Información de la sala
    public TextMeshProUGUI playerListText;     // Lista de jugadores
    public Button startGameButton;             // Iniciar juego (solo MasterClient)
    
    [Header("⚙️ Game Settings")]
    public int maxPlayersPerRoom = 20;         // Máximo jugadores por sala
    public string gameVersion = "1.0";         // Versión del juego
    public string[] gameScenes = { "WaitingUser", "InGame", "Carrera", "Hexagonia" }; // Escenas del juego
    
    // Variables privadas
    private string defaultPlayerName = "Player";
    private string defaultRoomName = "FallGuysRoom";
    private bool isConnecting = false;
    private bool isInMultiplayerMode = false;
    
    // Estados de conexión
    private enum LobbyState
    {
        MainMenu,
        Connecting,
        Connected,
        InRoom,
        Disconnected
    }
    
    private LobbyState currentState = LobbyState.MainMenu;
    
    #region Unity Lifecycle
    
    void Start()
    {
        // Configuración inicial
        SetupUI();
        InitializePhoton();
        UpdateUI();
        
        Debug.Log("🌐 LobbyManager iniciado");
    }
    
    void Update()
    {
        // Actualizar UI si está en sala
        if (currentState == LobbyState.InRoom)
        {
            UpdateRoomInfo();
        }
    }
    
    #endregion
    
    #region Initialization
    
    void SetupUI()
    {
        // Buscar input fields automáticamente si no están asignados
        if (playerNameInput == null || roomNameInput == null)
        {
            FindInputFields();
        }
        
        // Configurar eventos de botones
        if (singlePlayerButton != null)
            singlePlayerButton.onClick.AddListener(StartSinglePlayer);
        
        if (multiplayerButton != null)
            multiplayerButton.onClick.AddListener(OpenMultiplayerMenu);
        
        if (createRoomButton != null)
            createRoomButton.onClick.AddListener(CreateRoom);
        
        if (joinRandomButton != null)
            joinRandomButton.onClick.AddListener(JoinRandomRoom);
        
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(BackToMainMenu);
        
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartMultiplayerGame);
        
        // Configurar paneles iniciales
        if (multiplayerPanel != null)
            multiplayerPanel.SetActive(false);
        
        if (connectionPanel != null)
            connectionPanel.SetActive(false);
        
        // Configurar valores por defecto
        if (playerNameInput != null)
            playerNameInput.text = defaultPlayerName + Random.Range(1000, 9999);
        
        if (roomNameInput != null)
            roomNameInput.text = defaultRoomName + Random.Range(100, 999);
    }
    
    void FindInputFields()
    {
        TMP_InputField[] inputFields = FindObjectsOfType<TMP_InputField>();
        
        foreach (var input in inputFields)
        {
            string name = input.name.ToLower();
            
            if ((name.Contains("name") && !name.Contains("room")) || name.Contains("player"))
            {
                playerNameInput = input;
                Debug.Log($"🔗 PlayerNameInput encontrado: {input.name}");
            }
            else if (name.Contains("sala") || name.Contains("room") || name.Contains("id"))
            {
                roomNameInput = input;
                Debug.Log($"🔗 RoomNameInput encontrado: {input.name}");
            }
        }
        
        if (playerNameInput == null)
            Debug.LogWarning("⚠️ No se encontró playerNameInput");
        if (roomNameInput == null)
            Debug.LogWarning("⚠️ No se encontró roomNameInput");
    }
    
    void InitializePhoton()
    {
        // Configurar Photon
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;
        
        // Configurar nombre del jugador
        if (PhotonNetwork.NickName == string.Empty)
        {
            PhotonNetwork.NickName = defaultPlayerName + Random.Range(1000, 9999);
        }
        
        Debug.Log($"🔧 Photon configurado - Versión: {gameVersion}");
    }
    
    #endregion
    
    #region Button Events
    
    /// <summary>
    /// 🎮 Iniciar modo un jugador (funcionalidad existente)
    /// </summary>
    public void StartSinglePlayer()
    {
        Debug.Log("🎮 Iniciando modo un jugador...");
        
        // Desconectar de Photon si está conectado
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        
        // Ir a la secuencia normal del juego
        SceneChange sceneChanger = FindObjectOfType<SceneChange>();
        if (sceneChanger != null)
        {
            sceneChanger.LobbySceneChange();
        }
        else
        {
            SceneManager.LoadScene("WaitingUser");
        }
    }
    
    /// <summary>
    /// 🌐 Abrir menú de multijugador
    /// </summary>
    public void OpenMultiplayerMenu()
    {
        Debug.Log("🌐 Abriendo menú multijugador...");
        
        isInMultiplayerMode = true;
        
        if (multiplayerPanel != null)
            multiplayerPanel.SetActive(true);
        
        if (connectionPanel != null)
            connectionPanel.SetActive(true);
        
        // Conectar a Photon si no está conectado
        if (!PhotonNetwork.IsConnected && !isConnecting)
        {
            ConnectToPhoton();
        }
        
        UpdateUI();
    }
    
    /// <summary>
    /// 🏠 Volver al menú principal
    /// </summary>
    public void BackToMainMenu()
    {
        Debug.Log("🏠 Volviendo al menú principal...");
        
        isInMultiplayerMode = false;
        
        if (multiplayerPanel != null)
            multiplayerPanel.SetActive(false);
        
        if (connectionPanel != null)
            connectionPanel.SetActive(false);
        
        // Salir de la sala si está en una
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        
        currentState = LobbyState.MainMenu;
        UpdateUI();
    }
    
    /// <summary>
    /// 🏗️ Crear nueva sala
    /// </summary>
    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            ShowStatus("❌ No conectado a Photon", Color.red);
            return;
        }
        
        // Actualizar nickname del jugador ANTES de crear la sala
        string playerName = playerNameInput?.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.NickName = playerName;
            Debug.Log($"👤 Nickname actualizado: {PhotonNetwork.NickName}");
        }
        
        string roomName = roomNameInput?.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = defaultRoomName + Random.Range(100, 999);
        }
        
        // Configurar opciones de sala
        RoomOptions roomOptions = new RoomOptions()
        {
            MaxPlayers = maxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true
        };
        
        Debug.Log($"🏗️ Creando sala: {roomName}");
        ShowStatus($"🏗️ Creando sala: {roomName}...", Color.yellow);
        
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    
    /// <summary>
    /// 🎲 Unirse a sala aleatoria
    /// </summary>
    public void JoinRandomRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            ShowStatus("❌ No conectado a Photon", Color.red);
            return;
        }
        
        // Actualizar nickname del jugador ANTES de unirse
        string playerName = playerNameInput?.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.NickName = playerName;
            Debug.Log($"👤 Nickname actualizado: {PhotonNetwork.NickName}");
        }
        
        Debug.Log("🎲 Buscando sala aleatoria...");
        ShowStatus("🎲 Buscando sala...", Color.yellow);
        
        PhotonNetwork.JoinRandomRoom();
    }
    
    /// <summary>
    /// 🚀 Iniciar juego multijugador (solo MasterClient)
    /// </summary>
    public void StartMultiplayerGame()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            ShowStatus("❌ Solo el host puede iniciar el juego", Color.red);
            return;
        }
        
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            ShowStatus("❌ Se necesitan al menos 2 jugadores", Color.red);
            return;
        }
        
        Debug.Log("🚀 Iniciando juego multijugador...");
        ShowStatus("🚀 Iniciando juego...", Color.green);
        
        // Cargar la primera escena del juego
        PhotonNetwork.LoadLevel(gameScenes[0]);
    }
    
    #endregion
    
    #region Photon Connection
    
    void ConnectToPhoton()
    {
        if (isConnecting) return;
        
        isConnecting = true;
        currentState = LobbyState.Connecting;
        
        Debug.Log("🌐 Conectando a Photon...");
        ShowStatus("🌐 Conectando...", Color.yellow);
        
        PhotonNetwork.ConnectUsingSettings();
    }
    
    #endregion
    
    #region UI Updates
    
    void UpdateUI()
    {
        // Actualizar estado de botones
        if (createRoomButton != null)
            createRoomButton.interactable = PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InRoom;
        
        if (joinRandomButton != null)
            joinRandomButton.interactable = PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InRoom;
        
        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(PhotonNetwork.InRoom);
            startGameButton.interactable = PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom?.PlayerCount >= 2;
        }
        
        // Actualizar texto de estado
        UpdateStatusText();
    }
    
    void UpdateStatusText()
    {
        if (statusText == null) return;
        
        string status = "";
        Color color = Color.white;
        
        switch (currentState)
        {
            case LobbyState.MainMenu:
                status = "🎮 Modo multijugador";
                color = Color.white;
                break;
            case LobbyState.Connecting:
                status = "🌐 Conectando...";
                color = Color.yellow;
                break;
            case LobbyState.Connected:
                status = "✅ Conectado - Selecciona una opción";
                color = Color.green;
                break;
            case LobbyState.InRoom:
                status = $"🏠 En Sala: {PhotonNetwork.CurrentRoom?.Name}";
                color = Color.cyan;
                break;
            case LobbyState.Disconnected:
                status = "❌ Desconectado";
                color = Color.red;
                break;
        }
        
        statusText.text = CleanEmojiText(status);
        statusText.color = color;
    }
    
    void UpdateRoomInfo()
    {
        if (roomInfoText != null && PhotonNetwork.InRoom)
        {
            var room = PhotonNetwork.CurrentRoom;
            string roomInfo = $"🏠 {room.Name}\n👥 {room.PlayerCount}/{room.MaxPlayers} jugadores";
            roomInfoText.text = CleanEmojiText(roomInfo);
        }
        
        if (playerListText != null && PhotonNetwork.InRoom)
        {
            string playerList = "👥 Jugadores:\n";
            foreach (var player in PhotonNetwork.PlayerList)
            {
                string prefix = player.IsMasterClient ? "👑 " : "👤 ";
                playerList += $"{prefix}{player.NickName}\n";
            }
            playerListText.text = CleanEmojiText(playerList);
        }
    }
    
    void ShowStatus(string message, Color color)
    {
        Debug.Log(message);
        
        if (statusText != null)
        {
            statusText.text = CleanEmojiText(message);
            statusText.color = color;
        }
    }
    
    #endregion
    
    /// <summary>
    /// 🔧 Limpiar emojis problemáticos en texto
    /// </summary>
    string CleanEmojiText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        // Reemplazar emojis problemáticos con alternativas compatibles
        string cleanText = text;
        cleanText = cleanText.Replace("🌐", "[NET]");
        cleanText = cleanText.Replace("🎮", "[GAME]");
        cleanText = cleanText.Replace("🏠", "[ROOM]");
        cleanText = cleanText.Replace("👥", "[PLAYERS]");
        cleanText = cleanText.Replace("👑", "[HOST]");
        cleanText = cleanText.Replace("👤", "[USER]");
        cleanText = cleanText.Replace("👋", "[LEAVE]");
        cleanText = cleanText.Replace("🚪", "[EXIT]");
        cleanText = cleanText.Replace("✅", "[OK]");
        cleanText = cleanText.Replace("❌", "[ERROR]");
        cleanText = cleanText.Replace("🎲", "[RANDOM]");
        cleanText = cleanText.Replace("🏗️", "[CREATE]");
        
        // Reemplazar caracteres Unicode específicos
        cleanText = cleanText.Replace("\u2705", "[OK]");        // ✅
        cleanText = cleanText.Replace("\U0001F3E0", "[ROOM]");  // 🏠
        cleanText = cleanText.Replace("\U0001F465", "[USERS]"); // 👥
        cleanText = cleanText.Replace("\U0001F451", "[HOST]");  // 👑
        
        return cleanText;
    }
    
    #region Photon Callbacks
    
    public override void OnConnectedToMaster()
    {
        Debug.Log("✅ Conectado al Master Server de Photon");
        
        isConnecting = false;
        currentState = LobbyState.Connected;
        
        ShowStatus($"✅ Conectado ({PhotonNetwork.CloudRegion})", Color.green);
        
        // Actualizar nombre del jugador si se cambió
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
        {
            PhotonNetwork.NickName = playerNameInput.text;
        }
        
        UpdateUI();
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"❌ Desconectado de Photon: {cause}");
        
        isConnecting = false;
        currentState = LobbyState.Disconnected;
        
        ShowStatus($"❌ Desconectado: {cause}", Color.red);
        UpdateUI();
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log($"🏠 Unido a sala: {PhotonNetwork.CurrentRoom.Name}");
        
        currentState = LobbyState.InRoom;
        ShowStatus($"🏠 En sala: {PhotonNetwork.CurrentRoom.Name}", Color.cyan);
        
        UpdateUI();
    }
    
    public override void OnLeftRoom()
    {
        Debug.Log("🚪 Salió de la sala");
        
        currentState = LobbyState.Connected;
        ShowStatus("✅ Conectado - Selecciona una opción", Color.green);
        
        UpdateUI();
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"❌ Error al crear sala: {message}");
        ShowStatus($"❌ Error al crear sala: {message}", Color.red);
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"❌ Error al unirse a sala: {message}");
        ShowStatus($"❌ Error al unirse: {message}", Color.red);
    }
    
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("🎲 No hay salas disponibles, creando una nueva...");
        ShowStatus("🏗️ Creando nueva sala...", Color.yellow);
        
        // Crear sala automáticamente si no hay disponibles
        CreateRoom();
    }
    
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"👤 Jugador se unió: {newPlayer.NickName}");
        ShowStatus($"👤 {newPlayer.NickName} se unió", Color.green);
        
        UpdateUI();
    }
    
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"👋 Jugador salió: {otherPlayer.NickName}");
        ShowStatus($"👋 {otherPlayer.NickName} salió", Color.yellow);
        
        UpdateUI();
    }
    
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log($"👑 Nuevo host: {newMasterClient.NickName}");
        ShowStatus($"👑 Nuevo host: {newMasterClient.NickName}", Color.cyan);
        
        UpdateUI();
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// 🎯 Verificar si está en modo multijugador
    /// </summary>
    public bool IsInMultiplayerMode()
    {
        return isInMultiplayerMode && PhotonNetwork.IsConnected;
    }
    
    /// <summary>
    /// 📊 Obtener información de la sala actual
    /// </summary>
    public string GetRoomInfo()
    {
        if (!PhotonNetwork.InRoom) return "No en sala";
        
        var room = PhotonNetwork.CurrentRoom;
        return $"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})";
    }
    
    /// <summary>
    /// 🔄 Reiniciar conexión
    /// </summary>
    public void RestartConnection()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        else
        {
            ConnectToPhoton();
        }
    }
    
    #endregion
    
    #region Debug
    
    void OnGUI()
    {
        if (!isInMultiplayerMode) return;
        
        string title = CleanEmojiText("🌐 Lobby Debug");
        GUI.Box(new Rect(10, 10, 300, 150), title);
        GUI.Label(new Rect(15, 30, 290, 20), $"Estado: {currentState}");
        GUI.Label(new Rect(15, 50, 290, 20), $"Conectado: {PhotonNetwork.IsConnected}");
        GUI.Label(new Rect(15, 70, 290, 20), $"En Sala: {PhotonNetwork.InRoom}");
        GUI.Label(new Rect(15, 90, 290, 20), $"Jugadores: {PhotonNetwork.CountOfPlayers}");
        GUI.Label(new Rect(15, 110, 290, 20), $"Nombre: {PhotonNetwork.NickName}");
        GUI.Label(new Rect(15, 130, 290, 20), $"Región: {PhotonNetwork.CloudRegion}");
    }
    
    #endregion
} 