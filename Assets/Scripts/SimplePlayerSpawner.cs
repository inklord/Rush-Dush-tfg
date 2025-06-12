using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// 🎯 SIMPLE PLAYER SPAWNER - Versión simplificada que garantiza compilación
/// Soluciona el problema de "No tengo ningún jugador!"
/// </summary>
public class SimplePlayerSpawner : MonoBehaviourPunCallbacks
{
    [Header("🎮 Player Settings")]
    public string playerPrefabName = "NetworkPlayer";
    public float respawnHeight = -10f;
    public bool showDebugInfo = false;
    
    private bool hasMyPlayer = false;
    private GameObject myPlayerInstance;
    private static SimplePlayerSpawner instance;
    
    void Awake()
    {
        // Singleton para evitar múltiples spawners
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Suscribirse al evento de cambio de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        // Desuscribirse del evento al destruir el objeto
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Limpiar referencias al cambiar de escena
        if (myPlayerInstance != null)
        {
            PhotonNetwork.Destroy(myPlayerInstance);
        }
        hasMyPlayer = false;
        myPlayerInstance = null;

        // Intentar spawnear después de un breve delay
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            StartCoroutine(DelayedSpawn());
        }
    }
    
    void Start()
    {
        Debug.Log($"🎮 SimplePlayerSpawner Start - IsConnected: {PhotonNetwork.IsConnected}, InRoom: {PhotonNetwork.InRoom}");
        
        // Verificar si ya hay un jugador spawneado
        if (MasterSpawnController.HasSpawnedPlayer())
        {
            Debug.Log("🚫 SimplePlayerSpawner: Ya existe jugador, desactivando spawner");
            enabled = false;
            return;
        }
        
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !hasMyPlayer)
        {
            StartCoroutine(DelayedSpawn());
        }
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log($"🎮 OnJoinedRoom - ActorNumber: {PhotonNetwork.LocalPlayer.ActorNumber}");
        if (!hasMyPlayer)
        {
            StartCoroutine(DelayedSpawn());
        }
    }
    
    IEnumerator DelayedSpawn()
    {
        Debug.Log("⏳ Iniciando spawn con delay...");
        // Esperar un momento para asegurar que la red esté lista
        yield return new WaitForSeconds(0.5f);
        
        // Verificar nuevamente las condiciones
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            Debug.LogWarning("❌ No se puede spawnear - No estamos conectados o en una sala");
            yield break;
        }
        
        SpawnPlayer();
    }
    
    void SpawnPlayer()
    {
        // Verificar con MasterSpawnController primero
        if (!MasterSpawnController.RequestSpawn("SimplePlayerSpawner"))
        {
            Debug.Log("🚫 SimplePlayerSpawner: MasterSpawnController denegó el spawn");
            return;
        }
        
        try
        {
            if (hasMyPlayer && myPlayerInstance != null)
            {
                Debug.LogWarning("Ya existe un jugador para esta instancia.");
                return;
            }

            // Limpiar estado si el jugador anterior ya no existe
            if (myPlayerInstance == null)
            {
                hasMyPlayer = false;
            }

            // Verificar si el prefab existe
            if (string.IsNullOrEmpty(playerPrefabName))
            {
                Debug.LogError("❌ Nombre de prefab no configurado");
                return;
            }

            Debug.Log($"🎮 SimplePlayerSpawner spawneando jugador - ActorNumber: {PhotonNetwork.LocalPlayer.ActorNumber}");

            // Buscar un punto de spawn válido
            Vector3 spawnPosition = GetSpawnPosition();

            // Intentar spawnear el jugador
            myPlayerInstance = PhotonNetwork.Instantiate(playerPrefabName, spawnPosition, Quaternion.identity);
            
            if (myPlayerInstance != null)
            {
                Debug.Log($"✅ SimplePlayerSpawner - Jugador spawneado: {myPlayerInstance.name}");
                hasMyPlayer = true;
                
                // Registrar con MasterSpawnController
                MasterSpawnController.RegisterSpawnedPlayer(myPlayerInstance, "SimplePlayerSpawner");
                
                // Asignar la cámara al jugador local
                var mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    var cameraScript = mainCamera.GetComponent<LHS_Camera>();
                    if (cameraScript != null)
                    {
                        cameraScript.player = myPlayerInstance;
                        Debug.Log("✅ Cámara asignada al jugador local");
                    }
                }

                // Asegurarse de que los componentes estén correctamente configurados
                PhotonView pv = myPlayerInstance.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    var mainPlayer = myPlayerInstance.GetComponent<LHS_MainPlayer>();
                    if (mainPlayer != null)
                    {
                        mainPlayer.enabled = true;
                    }
                }
            }
            else
            {
                Debug.LogError("❌ Error al spawnear el jugador");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error en SpawnPlayer: {e.Message}");
        }
    }
    
    Vector3 GetSpawnPosition()
    {
        // Posición de spawn aleatoria en un área segura
        float randomX = Random.Range(-5f, 5f);
        float randomZ = Random.Range(-5f, 5f);
        return new Vector3(randomX, 2f, randomZ);
    }
    
    public override void OnLeftRoom()
    {
        hasMyPlayer = false;
        myPlayerInstance = null;
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, Screen.height - 150, 300, 140));
        GUILayout.Box("🎯 SIMPLE PLAYER SPAWNER");
        
        GUILayout.Label($"✅ Tengo jugador: {hasMyPlayer}");
        GUILayout.Label($"🌐 Conectado: {PhotonNetwork.IsConnected}");
        GUILayout.Label($"🎮 En sala: {PhotonNetwork.InRoom}");
        
        if (PhotonNetwork.IsConnected)
        {
            GUILayout.Label($"🎯 ActorNumber: {PhotonNetwork.LocalPlayer.ActorNumber}");
            if (PhotonNetwork.InRoom)
            {
                GUILayout.Label($"👥 Jugadores en sala: {PhotonNetwork.CurrentRoom.PlayerCount}");
            }
        }
        
        if (GUILayout.Button("🎮 FORCE RESPAWN"))
        {
            SpawnPlayer();
        }
        
        GUILayout.EndArea();
    }
    
    void Update()
    {
        // Si perdimos la referencia al jugador pero el flag sigue activo
        if (hasMyPlayer && myPlayerInstance == null)
        {
            hasMyPlayer = false;
            SpawnPlayer();
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            SpawnPlayer();
        }
    }
} 