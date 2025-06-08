using UnityEngine;
using Photon.Pun;

/// <summary>
/// 🎯 FALL GUYS MULTIPLAYER - SISTEMA UNIVERSAL
/// Funciona en TODAS las escenas: Carreras, Hexagonia, InGame, etc.
/// Se adapta automáticamente al tipo de escena
/// </summary>
public class SimpleFallGuysMultiplayer : MonoBehaviourPunCallbacks
{
    [Header("🎮 Configuración")]
    public string playerPrefabName = "NetworkPlayer";
    public Transform[] spawnPoints;
    public bool persistBetweenScenes = true;
    
    [Header("🎯 Auto-detección de Escena")]
    public bool autoDetectSceneType = true;
    
    private bool hasSpawned = false;
    private GameObject myPlayer;
    private string currentSceneType = "";
    private static SimpleFallGuysMultiplayer instance;
    
    void Start()
    {
        // Sistema de persistencia entre escenas
        if (persistBetweenScenes)
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("🎯 SimpleFallGuysMultiplayer - Modo persistente activado");
            }
            else if (instance != this)
            {
                Debug.Log("🎯 SimpleFallGuysMultiplayer ya existe - Eliminando duplicado");
                Destroy(gameObject);
                return;
            }
        }
        
        // Detectar tipo de escena automáticamente
        DetectSceneType();
        
        Debug.Log($"🎯 SimpleFallGuysMultiplayer iniciado en escena: {currentSceneType}");
        
        // Optimizar IAs para multiplayer
        OptimizeAIsForMultiplayer();
        
        // Si ya estoy en una sala, spawnear jugador
        if (PhotonNetwork.InRoom)
        {
            Invoke("SpawnPlayer", 1f);
        }
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log("✅ Me uní a la sala - Spawneando jugador");
        Invoke("SpawnPlayer", 1f);
    }
    
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"✅ Nuevo jugador entró a la sala: {newPlayer.NickName}");
        // El jugador que entra ya manejará su propio spawn
        // Pero yo (master client) necesito verificar que veo a todos
        Invoke("RefreshPlayerVisibility", 2f);
    }
    
    void SpawnPlayer()
    {
        // No spawnear en escenas de menú
        if (currentSceneType == "Lobby" || currentSceneType == "Login" || currentSceneType == "WaitingUser")
        {
            Debug.Log("📋 No spawneando jugador en escena de menú");
            return;
        }
        
        // Solo spawnear si no tengo jugador ya
        if (hasSpawned || myPlayer != null) return;
        
        // Verificar que no tenga ya un jugador
        GameObject[] existingPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in existingPlayers)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                Debug.Log("✅ Ya tengo un jugador");
                myPlayer = player;
                hasSpawned = true;
                SetupCamera();
                return;
            }
        }
        
        // Spawnear mi jugador
        Vector3 spawnPos = GetSpawnPosition();
        myPlayer = PhotonNetwork.Instantiate(playerPrefabName, spawnPos, Quaternion.identity);
        
        if (myPlayer != null)
        {
            hasSpawned = true;
            Debug.Log($"✅ Mi jugador spawneado: {myPlayer.name}");
            SetupCamera();
        }
    }
    
    Vector3 GetSpawnPosition()
    {
        if (spawnPoints.Length > 0)
        {
            int index = PhotonNetwork.LocalPlayer.ActorNumber % spawnPoints.Length;
            return spawnPoints[index].position;
        }
        
        // Posición por ActorNumber
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        return new Vector3(actorNumber * 3f, 1f, 0f);
    }
    
    void SetupCamera()
    {
        if (myPlayer == null) return;
        
        // Buscar cámara simple
        MovimientoCamaraSimple camera = FindObjectOfType<MovimientoCamaraSimple>();
        if (camera != null)
        {
            camera.player = myPlayer.transform;
            Debug.Log("✅ Cámara configurada");
        }
    }
    
    void RefreshPlayerVisibility()
    {
        Debug.Log("🔄 Actualizando visibilidad de jugadores...");
        
        // Contar todos los jugadores en la escena
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        int myPlayers = 0;
        int remotePlayers = 0;
        
        foreach (GameObject player in allPlayers)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null)
            {
                if (pv.IsMine)
                {
                    myPlayers++;
                    Debug.Log($"✅ Mi jugador: {player.name}");
                }
                else
                {
                    remotePlayers++;
                    Debug.Log($"🌐 Jugador remoto visible: {player.name} (Actor: {pv.OwnerActorNr})");
                }
            }
        }
        
        Debug.Log($"📊 Total visible: {myPlayers} míos + {remotePlayers} remotos = {allPlayers.Length}");
        
        // Si no veo jugadores remotos pero debería (hay más de 1 en la sala)
        if (remotePlayers == 0 && PhotonNetwork.PlayerList.Length > 1)
        {
            Debug.LogWarning("⚠️ No veo jugadores remotos - Puede ser problema de sincronización");
        }
    }
    
    void OptimizeAIsForMultiplayer()
    {
        Debug.Log("🤖 Optimizando IAs para velocidad de single player...");
        
        // Buscar todas las IAs en el mapa
        IAPlayerSimple[] ias = FindObjectsOfType<IAPlayerSimple>();
        
        foreach (IAPlayerSimple ia in ias)
        {
            if (ia != null)
            {
                // Asegurar que no tengan PhotonView (no deben sincronizarse por red)
                PhotonView pv = ia.GetComponent<PhotonView>();
                if (pv != null)
                {
                    Debug.Log($"🤖 ⚠️ Eliminando PhotonView de IA: {ia.name}");
                    DestroyImmediate(pv);
                }
                
                // Optimizar Rigidbody si es más lento que lo configurado
                Rigidbody rb = ia.GetComponent<Rigidbody>();
                if (rb != null && rb.drag > 1.5f)
                {
                    rb.drag = 1f; // Reducir drag para más velocidad
                    Debug.Log($"🤖 ✅ Optimizado drag de IA: {ia.name}");
                }
            }
        }
        
        Debug.Log($"🤖 ✅ {ias.Length} IAs optimizadas para velocidad normal");
    }
    
    void DetectSceneType()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();
        
        if (sceneName.Contains("carrera"))
        {
            currentSceneType = "Carrera";
        }
        else if (sceneName.Contains("hexagon"))
        {
            currentSceneType = "Hexagonia";
        }
        else if (sceneName.Contains("ingame") || sceneName.Contains("game"))
        {
            currentSceneType = "InGame";
        }
        else if (sceneName.Contains("lobby"))
        {
            currentSceneType = "Lobby";
        }
        else if (sceneName.Contains("ending") || sceneName.Contains("final"))
        {
            currentSceneType = "Ending";
        }
        else if (sceneName.Contains("waiting"))
        {
            currentSceneType = "WaitingUser";
        }
        else if (sceneName.Contains("login"))
        {
            currentSceneType = "Login";
        }
        else
        {
            currentSceneType = "Genérica";
        }
        
        // Configuración específica por tipo de escena
        ConfigureForSceneType();
    }
    
    void ConfigureForSceneType()
    {
        Debug.Log($"🎯 Configurando para escena tipo: {currentSceneType}");
        
        switch (currentSceneType)
        {
            case "Carrera":
                // Configuración específica para carreras
                Debug.Log("🏁 Modo Carrera - Spawn en línea de salida");
                break;
                
            case "Hexagonia":
                // Configuración específica para Hexagonia
                Debug.Log("🔷 Modo Hexagonia - Spawn distribuido");
                break;
                
            case "InGame":
                // Configuración genérica de juego
                Debug.Log("🎮 Modo InGame - Configuración estándar");
                break;
                
            case "Lobby":
            case "Login":
            case "WaitingUser":
                // No spawnear jugadores en escenas de menú
                Debug.Log("📋 Escena de menú - No spawn automático");
                break;
                
            case "Ending":
                Debug.Log("🏆 Escena de final - Configuración de victoria");
                break;
                
            default:
                Debug.Log("🎯 Escena genérica - Configuración por defecto");
                break;
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 170));
        GUILayout.Box("🎯 UNIVERSAL MULTIPLAYER");
        
        GUILayout.Label($"🎮 Escena: {currentSceneType}");
        GUILayout.Label($"🌐 Conectado: {PhotonNetwork.IsConnected}");
        GUILayout.Label($"🏠 En sala: {PhotonNetwork.InRoom}");
        
        if (PhotonNetwork.InRoom)
        {
            GUILayout.Label($"👥 Jugadores en sala: {PhotonNetwork.PlayerList.Length}");
            GUILayout.Label($"👤 Mi jugador spawneado: {myPlayer != null}");
            
            // Contar jugadores visibles
            GameObject[] visiblePlayers = GameObject.FindGameObjectsWithTag("Player");
            int remoteVisible = 0;
            foreach (GameObject player in visiblePlayers)
            {
                PhotonView pv = player.GetComponent<PhotonView>();
                if (pv != null && !pv.IsMine) remoteVisible++;
            }
            GUILayout.Label($"👁️ Jugadores remotos visibles: {remoteVisible}");
        }
        
        if (PhotonNetwork.InRoom && !hasSpawned)
        {
            if (GUILayout.Button("🎮 SPAWN JUGADOR"))
            {
                SpawnPlayer();
            }
        }
        
        if (PhotonNetwork.InRoom && hasSpawned)
        {
            if (GUILayout.Button("🔄 REFRESH VISIBILIDAD"))
            {
                RefreshPlayerVisibility();
            }
        }
        
        GUILayout.EndArea();
    }
} 