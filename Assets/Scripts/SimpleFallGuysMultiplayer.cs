using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;
using ExitGames.Client.Photon;

/// <summary>
/// 🎯 FALL GUYS MULTIPLAYER - SISTEMA UNIVERSAL
/// Funciona en TODAS las escenas: Carreras, Hexagonia, InGame, etc.
/// Se adapta automáticamente al tipo de escena
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class SimpleFallGuysMultiplayer : MonoBehaviourPunCallbacks
{
    [Header("🎮 Configuración")]
    public string playerPrefabName = "NetworkPlayer";
    public Transform[] spawnPoints;
    public bool persistBetweenScenes = true;
    
    [Header("🎯 Auto-detección de Escena")]
    public bool autoDetectSceneType = true;
    
    [Header("Player Settings")]
    public GameObject playerPrefab;
    public float respawnHeight = -10f;
    public bool enableDebugLogs = true;

    // Referencias privadas
    private Dictionary<Photon.Realtime.Player, GameObject> playerList = new Dictionary<Photon.Realtime.Player, GameObject>();
    private bool hasSpawned = false;
    private Vector3 lastSpawnPosition;

    void Awake()
    {
        // Verificar si ya hay un jugador spawneado
        if (MasterSpawnController.HasSpawnedPlayer())
        {
            Debug.Log("🚫 SimpleFallGuysMultiplayer: Ya existe jugador, desactivando spawner");
            enabled = false;
            return;
        }
        
        // Verificar si ya existe una instancia
        SimpleFallGuysMultiplayer[] managers = FindObjectsOfType<SimpleFallGuysMultiplayer>();
        if (managers.Length > 1)
        {
            Debug.LogWarning("⚠️ Múltiples instancias de SimpleFallGuysMultiplayer detectadas - destruyendo duplicado");
            Destroy(gameObject);
            return;
        }

        // Persistir entre escenas si está configurado
        if (persistBetweenScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        // Cargar prefab si no está asignado
        if (playerPrefab == null)
        {
            Debug.Log("🔍 Buscando prefab NetworkPlayer en Resources...");
            playerPrefab = Resources.Load<GameObject>("NetworkPlayer");
            if (playerPrefab == null)
            {
                Debug.LogError($"❌ No se encontró el prefab 'NetworkPlayer' en Resources");
                return;
            }
            else
            {
                // Verificar componentes del prefab
                PhotonView pv = playerPrefab.GetComponent<PhotonView>();
                LHS_MainPlayer player = playerPrefab.GetComponent<LHS_MainPlayer>();
                Animator anim = playerPrefab.GetComponentInChildren<Animator>();

                string status = "✅ Prefab NetworkPlayer encontrado con:\n";
                status += pv != null ? "- PhotonView ✓\n" : "- PhotonView ✗\n";
                status += player != null ? "- LHS_MainPlayer ✓\n" : "- LHS_MainPlayer ✗\n";
                status += anim != null ? "- Animator ✓\n" : "- Animator ✗\n";
                Debug.Log(status);

                // Configurar PhotonView si existe
                if (pv != null && pv.ObservedComponents.Count == 0)
                {
                    Debug.Log("⚙️ Configurando PhotonView del prefab...");
                    if (player != null)
                    {
                        pv.ObservedComponents = new List<Component> { player };
                        pv.Synchronization = ViewSynchronization.UnreliableOnChange;
                    }
                }
            }
        }

        Debug.Log("✅ SimpleFallGuysMultiplayer inicializado correctamente");
    }

    private void SpawnPlayer()
    {
        // Verificar con MasterSpawnController primero
        if (!MasterSpawnController.RequestSpawn("SimpleFallGuysMultiplayer"))
        {
            Debug.Log("🚫 SimpleFallGuysMultiplayer: MasterSpawnController denegó el spawn");
            return;
        }
        
        Debug.Log($"🎮 SimpleFallGuysMultiplayer intentando spawn de jugador...\nEstado actual:\n- En sala: {PhotonNetwork.InRoom}\n- Conectado: {PhotonNetwork.IsConnected}\n- Prefab listo: {playerPrefab != null}");

        // Verificar si ya tenemos un jugador local
        if (hasSpawned || playerList.ContainsKey(PhotonNetwork.LocalPlayer))
        {
            Debug.LogWarning("⚠️ Ya existe un jugador local - Evitando spawn duplicado");
            return;
        }

        // Verificar que estamos en una sala
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("❌ No estamos en una sala - No se puede hacer spawn");
            return;
        }

        // Verificar que el prefab está listo
        if (playerPrefab == null)
        {
            Debug.Log("🔄 Intentando cargar prefab NetworkPlayer...");
            playerPrefab = Resources.Load<GameObject>("NetworkPlayer");
            if (playerPrefab == null)
            {
                Debug.LogError("❌ No se pudo cargar el prefab 'NetworkPlayer'");
                return;
            }
        }

        try
        {
            Debug.Log($"🎯 Iniciando spawn en posición: {GetSpawnPosition()}");
            
            // Instanciar el jugador en la red
            GameObject playerObject = PhotonNetwork.Instantiate(
                "NetworkPlayer", // Usar nombre directo
                GetSpawnPosition(),
                Quaternion.identity,
                0
            );

            if (playerObject != null)
            {
                // Registrar el jugador
                playerList[PhotonNetwork.LocalPlayer] = playerObject;
                hasSpawned = true;
                lastSpawnPosition = playerObject.transform.position;

                // Registrar con MasterSpawnController
                MasterSpawnController.RegisterSpawnedPlayer(playerObject, "SimpleFallGuysMultiplayer");

                // Verificar componentes
                PhotonView pv = playerObject.GetComponent<PhotonView>();
                LHS_MainPlayer player = playerObject.GetComponent<LHS_MainPlayer>();
                Animator anim = playerObject.GetComponentInChildren<Animator>();

                string status = "✅ SimpleFallGuysMultiplayer - Jugador spawneado con:\n";
                status += pv != null ? "- PhotonView ✓\n" : "- PhotonView ✗\n";
                status += player != null ? "- LHS_MainPlayer ✓\n" : "- LHS_MainPlayer ✗\n";
                status += anim != null ? "- Animator ✓\n" : "- Animator ✗\n";
                Debug.Log(status);
            }
            else
            {
                Debug.LogError("❌ PhotonNetwork.Instantiate devolvió null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al hacer spawn: {e.Message}\n{e.StackTrace}");
        }
    }

    private Vector3 GetSpawnPosition()
    {
        // Si hay puntos de spawn definidos, usar uno aleatorio
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return spawnPoint.position;
        }

        // Si no hay puntos de spawn, usar posición aleatoria segura
        return new Vector3(
            Random.Range(-5f, 5f),
            5f,
            Random.Range(-5f, 5f)
        );
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"👥 Jugador entró a la sala: {newPlayer.NickName}");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"👋 Jugador salió de la sala: {otherPlayer.NickName}");
        
        // Limpiar referencia del jugador que se fue
        if (playerList.ContainsKey(otherPlayer))
        {
            playerList.Remove(otherPlayer);
        }
    }

    void OnDrawGizmos()
    {
        if (!enableDebugLogs) return;

        // Visualizar puntos de spawn
        if (spawnPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 1f);
                }
            }
        }

        // Visualizar último punto de spawn
        if (hasSpawned)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(lastSpawnPosition, 1.2f);
        }
    }
} 