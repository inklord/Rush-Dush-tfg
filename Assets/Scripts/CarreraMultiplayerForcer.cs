using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;

/// <summary>
/// 🏁 CARRERA MULTIPLAYER FORCER - Fuerza configuración multijugador específica para escena Carrera
/// Soluciona el problema de que cada instancia solo ve su propio jugador
/// </summary>
[DefaultExecutionOrder(-50)] // Ejecutar temprano pero después de AutoSceneMultiplayerFixer
public class CarreraMultiplayerForcer : MonoBehaviourPunCallbacks
{
    [Header("🏁 Configuración Carrera")]
    public string playerPrefabName = "NetworkPlayer";
    public bool forceSpawnInMultiplayer = true;
    public bool showDebugLogs = false;
    
    private bool hasConfigured = false;
    private static CarreraMultiplayerForcer instance;

    void Awake()
    {
        // Solo ejecutar en la escena Carrera
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "Carrera")
        {
            Destroy(gameObject);
            return;
        }

        // Singleton para evitar duplicados
        if (instance == null)
        {
            instance = this;
            Debug.Log("🏁 CarreraMultiplayerForcer iniciado");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Verificar si estamos en multijugador
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Debug.Log("🏁 CARRERA MULTIJUGADOR DETECTADO - Forzando configuración");
            StartCoroutine(ForceMultiplayerSetup());
        }
        else
        {
            Debug.Log("🏁 Carrera en modo singleplayer - No se requiere configuración especial");
        }
    }

    /// <summary>
    /// 🚀 Forzar configuración multijugador completa
    /// </summary>
    IEnumerator ForceMultiplayerSetup()
    {
        if (hasConfigured) yield break;

        Debug.Log("🏁 Iniciando configuración forzada de Carrera...");

        // Esperar que la escena se estabilice
        yield return new WaitForSeconds(0.5f);

        // 1. Forzar creación de MasterSpawnController si no existe
        EnsureMasterSpawnController();

        // 2. Verificar que PhotonNetwork.AutomaticallySyncScene está activado
        if (!PhotonNetwork.AutomaticallySyncScene)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            Debug.Log("🏁 PhotonNetwork.AutomaticallySyncScene activado");
        }

        // 3. Esperar un poco más antes de verificar spawns
        yield return new WaitForSeconds(1f);

        // 4. Forzar spawn si no hay jugadores
        ForcePlayerSpawnIfNeeded();

        // 5. Configurar cámara
        yield return new WaitForSeconds(0.5f);
        SetupCamera();

        hasConfigured = true;
        Debug.Log("✅ Configuración forzada de Carrera completada");
    }

    /// <summary>
    /// 🎯 Asegurar que existe MasterSpawnController
    /// </summary>
    void EnsureMasterSpawnController()
    {
        MasterSpawnController existing = FindObjectOfType<MasterSpawnController>();
        
        if (existing == null)
        {
            GameObject masterObj = new GameObject("MasterSpawnController_Forced");
            MasterSpawnController master = masterObj.AddComponent<MasterSpawnController>();
            master.playerPrefabName = playerPrefabName;
            master.showDebugInfo = showDebugLogs;
            
            DontDestroyOnLoad(masterObj);
            Debug.Log("🎯 MasterSpawnController creado forzadamente para Carrera");
        }
        else
        {
            Debug.Log("🎯 MasterSpawnController ya existe en Carrera");
        }
    }

    /// <summary>
    /// 🎮 Forzar spawn de jugador si es necesario
    /// </summary>
    void ForcePlayerSpawnIfNeeded()
    {
        if (!forceSpawnInMultiplayer) return;

        // Verificar si ya tengo un jugador
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;

        foreach (GameObject player in allPlayers)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                myPlayer = player;
                break;
            }
        }

        if (myPlayer == null)
        {
            Debug.Log("🚀 No se encontró mi jugador - Forzando spawn en Carrera");
            StartCoroutine(ForceSpawn());
        }
        else
        {
            Debug.Log($"✅ Mi jugador ya existe en Carrera: {myPlayer.name}");
        }

        // Mostrar información de todos los jugadores
        Debug.Log($"🏁 Jugadores totales en Carrera: {allPlayers.Length}");
        foreach (GameObject player in allPlayers)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null)
            {
                Debug.Log($"   - {player.name}: ViewID={pv.ViewID}, IsMine={pv.IsMine}, Owner={pv.Owner?.NickName}");
            }
            else
            {
                Debug.Log($"   - {player.name}: Sin PhotonView (Singleplayer)");
            }
        }
    }

    /// <summary>
    /// 🚀 Forzar spawn del jugador
    /// </summary>
    IEnumerator ForceSpawn()
    {
        // Verificar que estamos conectados
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            Debug.LogWarning("⚠️ No conectado a Photon - No se puede hacer spawn");
            yield break;
        }

        // Verificar que el prefab existe
        if (string.IsNullOrEmpty(playerPrefabName))
        {
            Debug.LogError("❌ Nombre de prefab no configurado");
            yield break;
        }

        // Obtener posición de spawn
        Vector3 spawnPosition = GetSpawnPosition();

        try
        {
            Debug.Log($"🚀 Spawneando jugador forzado en Carrera en: {spawnPosition}");

            GameObject player = PhotonNetwork.Instantiate(playerPrefabName, spawnPosition, Quaternion.identity);

            if (player != null)
            {
                Debug.Log($"✅ Jugador spawneado forzadamente: {player.name}");
                
                // Verificar componentes
                PhotonView pv = player.GetComponent<PhotonView>();
                LHS_MainPlayer playerScript = player.GetComponent<LHS_MainPlayer>();
                
                Debug.Log($"   - PhotonView: {(pv != null ? "✓" : "✗")} (IsMine: {(pv != null ? pv.IsMine.ToString() : "N/A")})");
                Debug.Log($"   - LHS_MainPlayer: {(playerScript != null ? "✓" : "✗")}");
            }
            else
            {
                Debug.LogError("❌ PhotonNetwork.Instantiate devolvió null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al spawnear jugador forzado: {e.Message}");
        }
    }

    /// <summary>
    /// 📍 Obtener posición de spawn
    /// </summary>
    Vector3 GetSpawnPosition()
    {
        // Buscar puntos de spawn con tag Respawn
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        
        if (spawnPoints.Length > 0)
        {
            int playerIndex = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
            Vector3 basePos = spawnPoints[playerIndex].transform.position;
            
            // Añadir pequeño offset aleatorio
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            );
            
            return basePos + offset;
        }

        // Posición por defecto basada en ActorNumber
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        return new Vector3(actorNumber * 3f, 2f, 0f);
    }

    /// <summary>
    /// 📷 Configurar cámara para seguir al jugador
    /// </summary>
    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("⚠️ No se encontró Camera.main");
            return;
        }

        // Buscar mi jugador
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        GameObject myPlayer = null;

        foreach (GameObject player in allPlayers)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                myPlayer = player;
                break;
            }
        }

        if (myPlayer != null)
        {
            // Intentar configurar cámara con MovimientoCamaraSimple
            MovimientoCamaraSimple cameraScript = mainCamera.GetComponent<MovimientoCamaraSimple>();
            if (cameraScript != null)
            {
                cameraScript.SetPlayer(myPlayer.transform);
                Debug.Log("📷 Cámara configurada con MovimientoCamaraSimple");
            }
            else
            {
                // Agregar script de cámara si no existe
                cameraScript = mainCamera.gameObject.AddComponent<MovimientoCamaraSimple>();
                cameraScript.SetPlayer(myPlayer.transform);
                Debug.Log("📷 MovimientoCamaraSimple agregado y configurado");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No se pudo configurar cámara - jugador local no encontrado");
        }
    }

    /// <summary>
    /// 📊 Mostrar información de debug
    /// </summary>
    void OnGUI()
    {
        if (!showDebugLogs) return;

        GUILayout.BeginArea(new Rect(10, 10, 400, 150));
        GUILayout.Box("🏁 CARRERA MULTIPLAYER FORCER");
        
        GUILayout.Label($"Escena: {SceneManager.GetActiveScene().name}");
        GUILayout.Label($"Configurado: {(hasConfigured ? "✅" : "⏳")}");
        GUILayout.Label($"Conectado: {(PhotonNetwork.IsConnected ? "🟢" : "🔴")}");
        GUILayout.Label($"En Sala: {(PhotonNetwork.InRoom ? "🟢" : "🔴")}");
        
        if (PhotonNetwork.InRoom)
        {
            GUILayout.Label($"Jugadores en sala: {PhotonNetwork.CurrentRoom.PlayerCount}");
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GUILayout.Label($"Jugadores en escena: {players.Length}");

        if (GUILayout.Button("Force Reconfig"))
        {
            hasConfigured = false;
            StartCoroutine(ForceMultiplayerSetup());
        }
        
        GUILayout.EndArea();
    }

    #region Photon Callbacks

    public override void OnJoinedRoom()
    {
        Debug.Log("🏁 Entré a sala en Carrera - Verificando configuración");
        if (!hasConfigured)
        {
            StartCoroutine(ForceMultiplayerSetup());
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"🏁 Nuevo jugador entró a Carrera: {newPlayer.NickName}");
    }

    #endregion
} 