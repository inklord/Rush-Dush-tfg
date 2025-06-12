using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;

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
    public string[] gameScenes = { "InGame", "Carrera", "Hexagonia" }; // Escenas del juego (sin WaitingUser)
    public bool showDebugLogs = false;         // Mostrar logs de debug
    

    
    // Variables privadas
    private string defaultPlayerName = "Player";
    private string defaultRoomName = "FallGuysRoom";
    private bool isConnecting = false;
    private bool isInMultiplayerMode = false;
    private bool isLoadingScene = false; // Protección contra ejecuciones múltiples
    
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

    void Awake()
    {
        // Verificar si necesitamos agregar un PhotonView
        if (photonView == null)
        {
            if (showDebugLogs) Debug.LogWarning("⚠️ PhotonView no encontrado - Agregando uno nuevo");
            gameObject.AddComponent<PhotonView>();
        }
        
        // Asegurar que existe un MasterSpawnController
        if (FindObjectOfType<MasterSpawnController>() == null)
        {
            GameObject spawnerObj = new GameObject("MasterSpawnController");
            spawnerObj.AddComponent<MasterSpawnController>();
            if (showDebugLogs) Debug.Log("🎯 MasterSpawnController creado automáticamente");
        }
    }
    
    #region Unity Lifecycle
    
    void Start()
    {
        // Validar y limpiar array de escenas
        ValidateGameScenes();
        
        // Configuración inicial
        SetupUI();
        InitializePhoton();
        UpdateUI();
        
        if (showDebugLogs) Debug.Log("🌐 LobbyManager iniciado");
    }
    
    void ValidateGameScenes()
    {
        // Asegurar que el array no contenga WaitingUser
        if (gameScenes != null)
        {
            List<string> validScenes = new List<string>();
            foreach (string scene in gameScenes)
            {
                if (!string.IsNullOrEmpty(scene) && scene != "WaitingUser")
                {
                    validScenes.Add(scene);
                }
            }
            
            // Si se eliminó WaitingUser, actualizar el array
            if (validScenes.Count != gameScenes.Length)
            {
                gameScenes = validScenes.ToArray();
                Debug.Log($"🔧 Array de escenas limpiado: [{string.Join(", ", gameScenes)}]");
            }
        }
        
        // Si el array está vacío o es null, usar valores por defecto
        if (gameScenes == null || gameScenes.Length == 0)
        {
            gameScenes = new string[] { "InGame", "Carrera", "Hexagonia" };
            Debug.Log("🔧 Array de escenas inicializado con valores por defecto");
        }
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
        PhotonNetwork.EnableCloseConnection = true;
        PhotonNetwork.GameVersion = gameVersion;

        // Asegurar que estamos usando la configuración del PhotonServerSettings
        if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime))
        {
            Debug.LogError("❌ AppID no configurado en PhotonServerSettings");
            ShowStatus("❌ Error de configuración", Color.red);
            return;
        }

        // Usar la región configurada en PhotonServerSettings
        string configuredRegion = PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion;
        if (!string.IsNullOrEmpty(configuredRegion))
        {
            Debug.Log($"🌍 Usando región configurada: {configuredRegion}");
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = configuredRegion;
        }
        else
        {
            Debug.Log("🌍 Usando selección automática de región");
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = ""; // Asegurar que está vacío para auto-selección
        }

        // Configurar nombre del jugador
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = defaultPlayerName + Random.Range(1000, 9999);
        }

        Debug.Log($"🔧 Photon configurado - Versión: {gameVersion}, AutoSync: {PhotonNetwork.AutomaticallySyncScene}, AppID: {PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime}, Región: {configuredRegion}");

        // Si ya estamos conectados pero en el servidor equivocado, reconectar
        if (PhotonNetwork.IsConnected && PhotonNetwork.Server != ServerConnection.MasterServer)
        {
            Debug.Log("🔄 Reconectando al MasterServer...");
            PhotonNetwork.Disconnect();
        }
        else if (!PhotonNetwork.IsConnected)
        {
            ConnectToPhoton();
        }
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
        
        // Ir a la secuencia normal del juego (saltando WaitingUser)
        SceneChange sceneChanger = FindObjectOfType<SceneChange>();
        if (sceneChanger != null)
        {
            sceneChanger.LobbySceneChange(); // Esto ahora va a "Intro"
        }
        else
        {
            SceneManager.LoadScene("Intro"); // Directamente a Intro sin WaitingUser
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
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("❌ No conectado a Photon Network");
            ShowStatus("❌ No conectado a Photon Network", Color.red);
            StartCoroutine(ReconnectAndRetry(CreateRoom));
            return;
        }

        // Si estamos en el GameServer, necesitamos volver al MasterServer
        if (PhotonNetwork.Server == ServerConnection.GameServer)
        {
            Debug.Log("🔄 Volviendo al MasterServer para crear sala...");
            ShowStatus("🔄 Preparando sala...", Color.yellow);
            PhotonNetwork.LeaveRoom();  // Esto nos devolverá al MasterServer
            return;
        }

        // Si no estamos listos en el MasterServer, esperar
        if (!PhotonNetwork.InLobby)
        {
            Debug.Log("🔄 Esperando conexión al lobby...");
            ShowStatus("🔄 Conectando al lobby...", Color.yellow);
            PhotonNetwork.JoinLobby();
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
            IsOpen = true,
            PublishUserId = true,
            // 🔧 Asegurar que la sala inicie SIN propiedades de escena
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
            {
                { "GameState", "InLobby" }
            }
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
        
        // Asegurar que la sincronización automática está activada
        PhotonNetwork.AutomaticallySyncScene = true;
        
        // Establecer el estado del juego en las propiedades de la sala
        var props = new ExitGames.Client.Photon.Hashtable();
        props.Add("GameState", "Starting");
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        
        // Notificar SOLO a otros jugadores (NO al host para evitar duplicación)
        photonView.RPC("OnGameStarting", RpcTarget.Others);
        
        // El host ejecuta directamente su preparación
        Debug.Log("🎮 Host preparando inicio de juego...");
        if (multiplayerPanel != null)
            multiplayerPanel.SetActive(false);
        if (connectionPanel != null)
            connectionPanel.SetActive(false);
        
        // Esperar un breve momento antes de cargar la escena para asegurar que todos reciban el RPC
        StartCoroutine(LoadGameSceneWithDelay());
    }
    
    [PunRPC]
    void OnGameStarting()
    {
        Debug.Log("🎮 Recibiendo notificación de inicio de juego...");
        ShowStatus("🎮 Iniciando juego...", Color.green);
        
        // Asegurar que la sincronización está activada en todos los clientes
        PhotonNetwork.AutomaticallySyncScene = true;
        
        // Preparar la interfaz para la transición
        if (multiplayerPanel != null)
            multiplayerPanel.SetActive(false);
        if (connectionPanel != null)
            connectionPanel.SetActive(false);
    }
    
    /// <summary>
    /// Cargar la escena del juego con un pequeño delay para asegurar sincronización
    /// </summary>
    private System.Collections.IEnumerator LoadGameSceneWithDelay()
    {
        // Protección contra ejecuciones múltiples
        if (isLoadingScene)
        {
            Debug.LogWarning("⚠️ Ya se está cargando una escena, cancelando ejecución duplicada");
            yield break;
        }
        
        isLoadingScene = true;
        Debug.Log("🔒 Iniciando carga de escena (bloqueando ejecuciones duplicadas)");
        
        // Esperar un frame para asegurar que el RPC se procesó
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.5f);
        
        if (PhotonNetwork.IsMasterClient)
        {
            // 🎬 SIEMPRE empezar por Intro para la experiencia completa
            string sceneToLoad = "Intro";
            
            Debug.Log($"🎮 Iniciando juego multijugador desde Intro como debe ser");
            
            // Actualizar el estado antes de cargar
            var props = new ExitGames.Client.Photon.Hashtable();
            props.Add("GameState", "StartingGame");
            props.Add("TargetScene", sceneToLoad);
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            
            // Cargar la escena del juego - PUN2 sincroniza automáticamente
            PhotonNetwork.LoadLevel(sceneToLoad);
        }
        else
        {
            Debug.Log("🔄 Cliente esperando que el MasterClient cargue la escena");
        }
        
        // Resetear flag después de cargar (aunque se destruirá el objeto al cambiar escena)
        isLoadingScene = false;
    }
    

    
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        
        Debug.Log($"👑 Nuevo Master Client: {newMasterClient.NickName}");
        ShowStatus($"👑 Nuevo Master Client: {newMasterClient.NickName}", Color.cyan);

        UpdateUI();
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

        // Intentar conectar
        if (!PhotonNetwork.ConnectUsingSettings())
        {
            Debug.LogError("❌ Error al iniciar conexión");
            ShowStatus("❌ Error de conexión", Color.red);
            isConnecting = false;
        }
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
            bool canStartGame = PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 2;
            startGameButton.gameObject.SetActive(PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient);
            startGameButton.interactable = canStartGame;
            
            if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                Debug.Log($"🎮 Estado del botón Start Game - Activo: {startGameButton.gameObject.activeSelf}, Interactuable: {startGameButton.interactable}, Jugadores: {PhotonNetwork.CurrentRoom.PlayerCount}");
            }
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
        Debug.Log($"✅ Conectado al Master Server de Photon en región: {PhotonNetwork.CloudRegion}");
        
        isConnecting = false;
        currentState = LobbyState.Connected;
        
        // Unirse al lobby automáticamente
        if (!PhotonNetwork.InLobby)
        {
            Debug.Log("🔄 Uniéndose al lobby...");
            PhotonNetwork.JoinLobby();
        }
        
        ShowStatus($"✅ Conectado ({PhotonNetwork.CloudRegion})", Color.green);
        
        // Actualizar nombre del jugador si se cambió
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
        {
            PhotonNetwork.NickName = playerNameInput.text;
        }
        
        UpdateUI();
    }
    
    public override void OnJoinedLobby()
    {
        Debug.Log("✅ Unido al lobby");
        ShowStatus("✅ Listo para crear/unirse a salas", Color.green);
        UpdateUI();
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        
        Debug.LogWarning($"❌ Desconectado: {cause}");
        ShowStatus($"❌ Desconectado: {cause}", Color.red);
        
        isConnecting = false;
        currentState = LobbyState.Disconnected;
        UpdateUI();
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log($"🏠 Unido a sala: {PhotonNetwork.CurrentRoom.Name}");
        
        currentState = LobbyState.InRoom;
        ShowStatus($"🏠 En sala: {PhotonNetwork.CurrentRoom.Name}", Color.cyan);
        
        // 🔧 IMPORTANTE: Limpiar propiedades de escena para evitar auto-sync no deseado
        // Solo el MasterClient puede hacer esto
        if (PhotonNetwork.IsMasterClient)
        {
            // Limpiar cualquier propiedad de escena que pueda estar configurada
            var cleanProps = new ExitGames.Client.Photon.Hashtable();
            cleanProps.Add("curScn", null); // Propiedad interna de Photon para escena actual
            cleanProps.Add("GameState", "InLobby"); // Estado del juego
            PhotonNetwork.CurrentRoom.SetCustomProperties(cleanProps);
            Debug.Log("🧹 Propiedades de escena limpiadas - Los jugadores permanecerán en lobby");
        }
        
        UpdateUI();
    }
    
    public override void OnLeftRoom()
    {
        Debug.Log("🚪 Salió de la sala");
        currentState = LobbyState.Connected;
        
        // Si estábamos intentando crear una sala, intentar de nuevo
        if (isInMultiplayerMode)
        {
            StartCoroutine(RetryCreateRoomAfterLeaving());
        }
        
        ShowStatus("✅ Conectado - Selecciona una opción", Color.green);
        UpdateUI();
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"❌ Error al crear sala: {message} (código: {returnCode})");
        
        // Si la sala ya existe, intentar con otro nombre
        if (message.Contains("already exist"))
        {
            string newRoomName = defaultRoomName + Random.Range(1000, 9999);
            Debug.Log($"🔄 Reintentando con nuevo nombre: {newRoomName}");
            
            if (roomNameInput != null)
                roomNameInput.text = newRoomName;
            
            CreateRoom();
        }
        else
        {
            ShowStatus($"❌ Error al crear sala: {message}", Color.red);
            // Si fallamos por otra razón, intentar volver al MasterServer
            if (PhotonNetwork.Server != ServerConnection.MasterServer)
            {
                Debug.Log("🔄 Reconectando al MasterServer...");
                PhotonNetwork.Disconnect();
                StartCoroutine(ReconnectAndRetry(CreateRoom));
            }
        }
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
        
        // 🔧 Asegurar que el estado permanezca en lobby cuando se une alguien
        if (PhotonNetwork.IsMasterClient)
        {
            var lobbyProps = new ExitGames.Client.Photon.Hashtable();
            lobbyProps.Add("GameState", "InLobby");
            PhotonNetwork.CurrentRoom.SetCustomProperties(lobbyProps);
            Debug.Log("🧹 Estado forzado a InLobby para evitar auto-sync");
        }
        
        UpdateUI();
    }
    
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"👋 Jugador salió: {otherPlayer.NickName}");
        ShowStatus($"👋 {otherPlayer.NickName} salió", Color.yellow);
        
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
    
    private IEnumerator ReconnectAndRetry(System.Action actionToRetry)
    {
        Debug.Log("🔄 Intentando reconectar...");
        ConnectToPhoton();
        
        float timeout = 0;
        while (!PhotonNetwork.IsConnected && timeout < 5f)
        {
            timeout += Time.deltaTime;
            yield return null;
        }
        
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("✅ Reconexión exitosa - Reintentando acción");
            actionToRetry?.Invoke();
        }
        else
        {
            Debug.LogError("❌ No se pudo reconectar");
            ShowStatus("❌ Error de conexión", Color.red);
        }
    }

    private IEnumerator RetryCreateRoomAfterLeaving()
    {
        // Esperar a que volvamos al MasterServer y al lobby
        yield return new WaitUntil(() => PhotonNetwork.InLobby);
        yield return new WaitForSeconds(0.5f); // Pequeño delay adicional por seguridad
        
        Debug.Log("🔄 Reintentando crear sala después de volver al MasterServer");
        CreateRoom();
    }


} 