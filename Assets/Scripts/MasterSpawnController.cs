using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 🎯 MASTER SPAWN CONTROLLER - Previene duplicación de spawns
/// Coordina todos los spawners del juego para garantizar UN solo jugador por cliente
/// </summary>
public class MasterSpawnController : MonoBehaviourPunCallbacks
{
    [Header("🎮 Master Spawn Settings")]
    public string playerPrefabName = "NetworkPlayer";
    public bool preventDuplicateSpawns = true;
    public bool showDebugInfo = true;
    
    // Estado global del spawn
    private static bool globalHasSpawned = false;
    private static GameObject globalPlayerInstance = null;
    private static MasterSpawnController instance;
    
    // Estado local
    private bool localHasSpawned = false;
    private GameObject localPlayerInstance = null;
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("🎯 MasterSpawnController: Singleton creado");
            
            // Suscribirse al evento de cambio de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Debug.Log("🎯 MasterSpawnController: Destruyendo duplicado");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        Debug.Log("🎯 === MASTER SPAWN CONTROLLER INICIADO ===");
        Debug.Log($"Estado global: HasSpawned={globalHasSpawned}, Player={globalPlayerInstance?.name ?? "null"}");
        
        // Iniciar limpieza automática periódica
        StartCoroutine(PeriodicCleanup());
        
        // Verificar estado inicial
        CheckAndCleanupExistingPlayers();
        
        // Intentar spawn si es necesario
        if (ShouldAttemptSpawn())
        {
            StartCoroutine(DelayedSpawn());
        }
    }
    
    #region Public API
    
    /// <summary>
    /// 🔍 Verificar si ya tenemos un jugador spawneado
    /// </summary>
    public static bool HasSpawnedPlayer()
    {
        return globalHasSpawned && globalPlayerInstance != null;
    }
    
    /// <summary>
    /// 🎮 Obtener la instancia del jugador spawneado
    /// </summary>
    public static GameObject GetSpawnedPlayer()
    {
        return globalPlayerInstance;
    }
    
    /// <summary>
    /// 🚫 Solicitar spawn (solo si no hay jugador spawneado)
    /// </summary>
    public static bool RequestSpawn(string requesterName = "Unknown")
    {
        // Limpiar duplicados antes de evaluar
        CleanupDuplicatedPlayers();
        
        // VERIFICACIÓN TRIPLE: Buscar jugadores en tiempo real
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in allPlayers)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                Debug.LogWarning($"🚫 {requesterName} DENEGADO - Ya existe jugador con PhotonView.IsMine: {player.name}");
                
                // Actualizar estado global si no estaba registrado
                if (!globalHasSpawned || globalPlayerInstance == null)
                {
                    globalHasSpawned = true;
                    globalPlayerInstance = player;
                }
                
                return false;
            }
        }
        
        if (HasSpawnedPlayer())
        {
            Debug.LogWarning($"🚫 {requesterName} solicitó spawn, pero ya existe jugador: {globalPlayerInstance.name}");
            return false;
        }
        
        Debug.Log($"✅ {requesterName} puede proceder con el spawn");
        return true;
    }
    
    /// <summary>
    /// 📝 Registrar que se spawneó un jugador
    /// </summary>
    public static void RegisterSpawnedPlayer(GameObject player, string spawnerName = "Unknown")
    {
        if (globalHasSpawned && globalPlayerInstance != null && globalPlayerInstance != player)
        {
            Debug.LogWarning($"⚠️ {spawnerName} intentó registrar {player.name}, pero ya existe {globalPlayerInstance.name}. Destruyendo duplicado.");
            
            // Destruir el duplicado
            if (player.GetComponent<PhotonView>() != null)
            {
                PhotonNetwork.Destroy(player);
            }
            else
            {
                Destroy(player);
            }
            return;
        }
        
        globalHasSpawned = true;
        globalPlayerInstance = player;
        
        Debug.Log($"✅ {spawnerName} registró jugador exitosamente: {player.name}");
        
        // Configurar cámara inmediatamente
        if (instance != null)
        {
            instance.SetupCameraForPlayer(player);
        }
        
        // Notificar a otros spawners para que se desactiven
        BroadcastSpawnCompleted();
    }
    
    /// <summary>
    /// 🧹 Limpiar estado cuando el jugador es destruido
    /// </summary>
    public static void UnregisterPlayer()
    {
        Debug.Log("🧹 Limpiando estado de spawn");
        globalHasSpawned = false;
        globalPlayerInstance = null;
    }
    
    #endregion
    
    #region Scene Management
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        string[] allowedScenes = { "InGame", "Carrera", "Hexagonia" };
        
        bool isAllowedScene = false;
        foreach (string allowedScene in allowedScenes)
        {
            if (sceneName == allowedScene)
            {
                isAllowedScene = true;
                break;
            }
        }
        
        if (!isAllowedScene)
        {
            Debug.Log($"🧹 Cambiando a escena '{sceneName}' - Limpiando estado de spawn");
            
            // Limpiar jugador existente si cambiamos a escena no permitida
            if (localPlayerInstance != null)
            {
                if (PhotonNetwork.IsConnected)
                {
                    PhotonNetwork.Destroy(localPlayerInstance);
                }
                else
                {
                    Destroy(localPlayerInstance);
                }
            }
            
            // Resetear estado
            localHasSpawned = false;
            localPlayerInstance = null;
            UnregisterPlayer();
        }
        else
        {
            Debug.Log($"✅ Cambiando a escena permitida '{sceneName}' - Intentando spawn si es necesario");
            
            // Limpiar duplicados al cambiar a escena de juego
            CleanupDuplicatedPlayers();
            
            // Esperar un momento y verificar si necesitamos spawn
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                StartCoroutine(DelayedSpawnAfterSceneChange());
            }
        }
    }
    
    IEnumerator DelayedSpawnAfterSceneChange()
    {
        yield return new WaitForSeconds(2f); // Esperar más tiempo para que la escena se estabilice
        
        if (ShouldAttemptSpawn())
        {
            StartCoroutine(DelayedSpawn());
        }
    }
    
    void OnDestroy()
    {
        // Desuscribirse del evento al destruir
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    #endregion
    
    #region Private Methods
    
    bool ShouldAttemptSpawn()
    {
        // Verificar que estamos en una escena permitida
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string[] allowedScenes = { "InGame", "Carrera", "Hexagonia" };
        
        bool isAllowedScene = false;
        foreach (string scene in allowedScenes)
        {
            if (currentScene == scene)
            {
                isAllowedScene = true;
                break;
            }
        }
        
        if (!isAllowedScene)
        {
            Debug.Log($"🚫 Escena '{currentScene}' no permite spawn - Solo permitido en: InGame, Carrera, Hexagonia");
            return false;
        }
        
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            Debug.Log("🚫 No conectado o no en sala - No spawning");
            return false;
        }
        
        if (HasSpawnedPlayer())
        {
            Debug.Log("🚫 Ya existe jugador spawneado - No spawning");
            return false;
        }
        
        Debug.Log($"✅ Escena '{currentScene}' permite spawn");
        return true;
    }
    
    void CheckAndCleanupExistingPlayers()
    {
        GameObject[] existingPlayers = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;
        int myPlayerCount = 0;
        
        foreach (GameObject player in existingPlayers)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                myPlayerCount++;
                if (myPlayer == null)
                {
                    myPlayer = player;
                }
                else
                {
                    // Encontrado duplicado - destruir
                    Debug.LogWarning($"⚠️ Destruyendo jugador duplicado: {player.name}");
                    PhotonNetwork.Destroy(player);
                }
            }
        }
        
        if (myPlayer != null)
        {
            RegisterSpawnedPlayer(myPlayer, "MasterSpawnController (Cleanup)");
        }
        
        Debug.Log($"🔍 Cleanup completado - Jugadores míos encontrados: {myPlayerCount}, Activo: {myPlayer?.name ?? "ninguno"}");
    }
    
    public static void CleanupDuplicatedPlayers()
    {
        if (instance == null) return;
        
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        GameObject myBestPlayer = null;
        List<GameObject> myDuplicates = new List<GameObject>();
        
        foreach (GameObject player in allPlayers)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                if (myBestPlayer == null)
                {
                    myBestPlayer = player;
                }
                else
                {
                    myDuplicates.Add(player);
                }
            }
        }
        
        // Destruir todos los duplicados
        foreach (GameObject duplicate in myDuplicates)
        {
            Debug.LogWarning($"🧹 Destruyendo duplicado detectado: {duplicate.name}");
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Destroy(duplicate);
            }
            else
            {
                Destroy(duplicate);
            }
        }
        
        if (myDuplicates.Count > 0)
        {
            Debug.Log($"🧹 Limpieza de duplicados completada - Destruidos: {myDuplicates.Count}, Conservado: {myBestPlayer?.name ?? "ninguno"}");
            
            // Reconfigurar cámara para el jugador conservado
            if (myBestPlayer != null && instance != null)
            {
                instance.StartCoroutine(instance.DelayedCameraSetup(myBestPlayer));
            }
        }
    }
    
    IEnumerator PeriodicCleanup()
    {
        yield return new WaitForSeconds(5f); // Esperar inicio
        
        while (true)
        {
            yield return new WaitForSeconds(10f); // Cada 10 segundos (más espaciado)
            
            // Solo limpiar si estamos conectados y en una escena de juego
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                string currentScene = SceneManager.GetActiveScene().name;
                string[] allowedScenes = { "InGame", "Carrera", "Hexagonia" };
                
                bool isAllowedScene = false;
                foreach (string scene in allowedScenes)
                {
                    if (currentScene == scene)
                    {
                        isAllowedScene = true;
                        break;
                    }
                }
                
                if (isAllowedScene)
                {
                    // Solo limpiar si detectamos múltiples jugadores
                    GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
                    int myPlayerCount = 0;
                    
                    foreach (GameObject player in allPlayers)
                    {
                        PhotonView pv = player.GetComponent<PhotonView>();
                        if (pv != null && pv.IsMine)
                        {
                            myPlayerCount++;
                        }
                    }
                    
                    if (myPlayerCount > 1)
                    {
                        Debug.LogWarning($"🧹 Limpieza periódica detectó {myPlayerCount} jugadores duplicados");
                        CleanupDuplicatedPlayers();
                    }
                }
            }
        }
    }
    
    IEnumerator DelayedCameraSetup(GameObject player)
    {
        yield return new WaitForSeconds(0.5f); // Esperar que se complete la destrucción
        
        Debug.Log($"📷 Reconfigurando cámara después de limpieza para: {player?.name ?? "null"}");
        
        if (player != null)
        {
            SetupCameraForPlayer(player);
        }
    }
    
    IEnumerator DelayedSpawn()
    {
        Debug.Log("⏳ Iniciando spawn con delay...");
        yield return new WaitForSeconds(1f);
        
        // Verificar nuevamente antes de spawnear
        if (!ShouldAttemptSpawn())
        {
            Debug.Log("🚫 Condiciones cambiaron - Cancelando spawn");
            yield break;
        }
        
        // Solicitar permiso para spawn
        if (!RequestSpawn("MasterSpawnController"))
        {
            yield break;
        }
        
        DoSpawn();
    }
    
    void DoSpawn()
    {
        try
        {
            Vector3 spawnPosition = GetSpawnPosition();
            Debug.Log($"🎮 MasterSpawnController spawneando en: {spawnPosition}");
            
            GameObject player = PhotonNetwork.Instantiate(playerPrefabName, spawnPosition, Quaternion.identity);
            
            if (player != null)
            {
                localHasSpawned = true;
                localPlayerInstance = player;
                
                RegisterSpawnedPlayer(player, "MasterSpawnController");
                
                // Configurar cámara
                SetupCameraForPlayer(player);
            }
            else
            {
                Debug.LogError("❌ PhotonNetwork.Instantiate retornó null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error en spawn: {e.Message}");
        }
    }
    
    Vector3 GetSpawnPosition()
    {
        // Posición basada en ActorNumber
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        return new Vector3((actorNumber - 1) * 3f, 1f, 0f);
    }
    
    void SetupCameraForPlayer(GameObject player)
    {
        if (player == null) return;
        
        Debug.Log($"📷 Configurando cámara para jugador: {player.name}");
        
        // Método 1: MovimientoCamaraSimple
        MovimientoCamaraSimple cameraScript = FindObjectOfType<MovimientoCamaraSimple>();
        if (cameraScript != null)
        {
            cameraScript.SetPlayer(player.transform);
            Debug.Log("📷 Cámara configurada via MovimientoCamaraSimple");
        }
        
        // Método 2: LHS_Camera en Camera.main
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            LHS_Camera lhsCamera = mainCamera.GetComponent<LHS_Camera>();
            if (lhsCamera == null)
            {
                lhsCamera = mainCamera.gameObject.AddComponent<LHS_Camera>();
            }
            lhsCamera.player = player;
            Debug.Log("📷 Cámara configurada via LHS_Camera");
        }
        
        // Método 3: Forzar setup en SimplePlayerMovement si existe
        SimplePlayerMovement playerMovement = player.GetComponent<SimplePlayerMovement>();
        if (playerMovement != null)
        {
            // Invocar el setup de cámara del jugador
            playerMovement.Invoke("SetupCamera", 0.1f);
            Debug.Log("📷 Solicitado setup de cámara via SimplePlayerMovement");
        }
        
        // Método 4: LHS_MainPlayer setup
        LHS_MainPlayer mainPlayer = player.GetComponent<LHS_MainPlayer>();
        if (mainPlayer != null)
        {
            // Forzar setup de cámara
            mainPlayer.Invoke("SetupCamera", 0.1f);
            Debug.Log("📷 Solicitado setup de cámara via LHS_MainPlayer");
        }
    }
    
    static void BroadcastSpawnCompleted()
    {
        Debug.Log("🛑 KILL SWITCH ACTIVADO - Desactivando TODOS los spawners");
        
        // Encontrar y DESTRUIR otros spawners
        SimplePlayerSpawner[] simpleSpawners = FindObjectsOfType<SimplePlayerSpawner>();
        foreach (var spawner in simpleSpawners)
        {
            if (spawner.gameObject != instance?.gameObject)
            {
                Debug.LogWarning($"🛑 DESTRUYENDO SimplePlayerSpawner: {spawner.name}");
                spawner.enabled = false;
                spawner.gameObject.SetActive(false);
            }
        }
        
        WaitingUserSpawner[] waitingSpawners = FindObjectsOfType<WaitingUserSpawner>();
        foreach (var spawner in waitingSpawners)
        {
            Debug.LogWarning($"🛑 DESTRUYENDO WaitingUserSpawner: {spawner.name}");
            spawner.enabled = false;
            spawner.gameObject.SetActive(false);
        }
        
        PhotonSpawnController[] photonSpawners = FindObjectsOfType<PhotonSpawnController>();
        foreach (var spawner in photonSpawners)
        {
            if (spawner.gameObject != instance?.gameObject)
            {
                Debug.LogWarning($"🛑 DESTRUYENDO PhotonSpawnController: {spawner.name}");
                spawner.enabled = false;
                spawner.gameObject.SetActive(false);
            }
        }
        
        GuaranteedPlayerSpawner[] guaranteedSpawners = FindObjectsOfType<GuaranteedPlayerSpawner>();
        foreach (var spawner in guaranteedSpawners)
        {
            Debug.LogWarning($"🛑 DESTRUYENDO GuaranteedPlayerSpawner: {spawner.name}");
            spawner.enabled = false;
            spawner.gameObject.SetActive(false);
        }
        
        PhotonLauncher[] photonLaunchers = FindObjectsOfType<PhotonLauncher>();
        foreach (var launcher in photonLaunchers)
        {
            Debug.LogWarning($"🛑 DESTRUYENDO PhotonLauncher: {launcher.name}");
            launcher.enabled = false;
            launcher.gameObject.SetActive(false);
        }
        
        SimpleFallGuysMultiplayer[] multiplayers = FindObjectsOfType<SimpleFallGuysMultiplayer>();
        foreach (var mp in multiplayers)
        {
            Debug.LogWarning($"🛑 DESTRUYENDO SimpleFallGuysMultiplayer: {mp.name}");
            mp.enabled = false;
            mp.gameObject.SetActive(false);
        }
        
        Debug.Log("🛑 KILL SWITCH COMPLETADO - Todos los spawners neutralizados");
    }
    
    #endregion
    
    #region Photon Callbacks
    
    public override void OnJoinedRoom()
    {
        Debug.Log("🎮 MasterSpawnController - OnJoinedRoom");
        
        // Limpiar duplicados al unirse a la sala
        CleanupDuplicatedPlayers();
        
        if (ShouldAttemptSpawn())
        {
            StartCoroutine(DelayedSpawn());
        }
    }
    
    public override void OnLeftRoom()
    {
        Debug.Log("👋 MasterSpawnController - OnLeftRoom");
        
        if (localPlayerInstance != null)
        {
            PhotonNetwork.Destroy(localPlayerInstance);
        }
        
        localHasSpawned = false;
        localPlayerInstance = null;
        UnregisterPlayer();
    }
    
    #endregion
    
    #region Debug GUI
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 150, 350, 280));
        GUILayout.Box("🎯 MASTER SPAWN CONTROLLER");
        
        string currentScene = SceneManager.GetActiveScene().name;
        string[] allowedScenes = { "InGame", "Carrera", "Hexagonia" };
        bool isAllowed = System.Array.Exists(allowedScenes, scene => scene == currentScene);
        
        GUILayout.Label($"Escena Actual: {currentScene} {(isAllowed ? "✅" : "🚫")}");
        GUILayout.Label($"Escenas Permitidas: InGame, Carrera, Hexagonia");
        GUILayout.Label($"🛑 KILL SWITCH: ACTIVO (cada 10s)");
        GUILayout.Label($"Global HasSpawned: {globalHasSpawned}");
        GUILayout.Label($"Global Player: {(globalPlayerInstance != null ? globalPlayerInstance.name : "null")}");
        GUILayout.Label($"Local HasSpawned: {localHasSpawned}");
        GUILayout.Label($"Local Player: {(localPlayerInstance != null ? localPlayerInstance.name : "null")}");
        
        if (GUILayout.Button("Force Cleanup"))
        {
            CheckAndCleanupExistingPlayers();
        }
        
        if (GUILayout.Button("Clean Duplicates"))
        {
            CleanupDuplicatedPlayers();
        }
        
        if (GUILayout.Button("Fix Camera"))
        {
            GameObject myPlayer = GetSpawnedPlayer();
            if (myPlayer != null)
            {
                SetupCameraForPlayer(myPlayer);
            }
        }
        
        if (GUILayout.Button("Reset State"))
        {
            UnregisterPlayer();
            localHasSpawned = false;
            localPlayerInstance = null;
        }
        
        GUILayout.EndArea();
    }
    
    #endregion
} 