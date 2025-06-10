using UnityEngine;
using Photon.Pun;
using System.Collections;

/// <summary>
/// 🎯 PHOTON SPAWN CONTROLLER - Previene duplicación de jugadores
/// Sistema avanzado para garantizar un solo jugador por cliente
/// </summary>
public class PhotonSpawnController : MonoBehaviourPunCallbacks
{
    [Header("🎮 Spawn Settings")]
    public Transform[] spawnPoints;
    public string playerPrefabName = "NetworkPlayer";
    
    [Header("🔧 Debug")]
    public bool showDebugInfo = true;
    
    // Estado del spawn
    private bool hasSpawnedPlayer = false;
    private GameObject myPlayer = null;
    private static PhotonSpawnController instance;
    
    // Prevención de spam
    private float lastSpawnAttempt = 0f;
    private const float SPAWN_COOLDOWN = 2f;
    
    void Awake()
    {
        // Singleton para evitar múltiples controladores
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        Debug.Log("🎯 PhotonSpawnController iniciado");
        
        // Verificar si ya hay un jugador spawneado
        if (MasterSpawnController.HasSpawnedPlayer())
        {
            Debug.Log("🚫 PhotonSpawnController: Ya existe jugador, desactivando spawner");
            enabled = false;
            return;
        }
        
        // Solo spawnear si estamos conectados y en sala
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            StartCoroutine(DelayedSpawn());
        }
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log("🎮 Entré a la sala - Iniciando spawn con delay");
        StartCoroutine(DelayedSpawn());
    }
    
    /// <summary>
    /// 🕐 Spawn con delay para evitar problemas de timing
    /// </summary>
    IEnumerator DelayedSpawn()
    {
        // Esperar un poco para que la red se estabilice
        yield return new WaitForSeconds(0.5f);
        
        // Verificar nuevamente antes de spawnear
        if (ShouldSpawnPlayer())
        {
            SpawnMyPlayer();
        }
    }
    
    /// <summary>
    /// 🤔 Verificar si debería spawnear un jugador
    /// </summary>
    bool ShouldSpawnPlayer()
    {
        // Cooldown de spam
        if (Time.time - lastSpawnAttempt < SPAWN_COOLDOWN)
        {
            Debug.LogWarning("⏰ Spawn en cooldown");
            return false;
        }
        
        // Ya spawneé
        if (hasSpawnedPlayer && myPlayer != null)
        {
            Debug.LogWarning("✅ Ya tengo un jugador activo");
            return false;
        }
        
        // Verificar jugadores existentes con mi ownership
        GameObject[] existingPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in existingPlayers)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                Debug.LogWarning($"⚠️ Ya tengo un jugador: {player.name}");
                hasSpawnedPlayer = true;
                myPlayer = player;
                return false;
            }
        }
        
        // No estoy en sala
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("❌ No estoy en sala");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 🎯 Spawnear MI jugador único
    /// </summary>
    void SpawnMyPlayer()
    {
        // Verificar con MasterSpawnController primero
        if (!MasterSpawnController.RequestSpawn("PhotonSpawnController"))
        {
            Debug.Log("🚫 PhotonSpawnController: MasterSpawnController denegó el spawn");
            return;
        }
        
        lastSpawnAttempt = Time.time;
        
        // Obtener posición de spawn única
        Vector3 spawnPosition = GetUniqueSpawnPosition();
        
        Debug.Log($"🎯 PhotonSpawnController spawneando jugador en: {spawnPosition}");
        
        try
        {
            // Spawnear jugador
            GameObject player = PhotonNetwork.Instantiate(playerPrefabName, spawnPosition, Quaternion.identity);
            
            if (player != null)
            {
                hasSpawnedPlayer = true;
                myPlayer = player;
                
                Debug.Log($"✅ PhotonSpawnController - Jugador spawneado exitosamente: {player.name}");
                
                // Registrar con MasterSpawnController
                MasterSpawnController.RegisterSpawnedPlayer(player, "PhotonSpawnController");
                
                // Configurar cámara
                SetupCameraForPlayer(player);
                
                // Configurar tag si no lo tiene
                if (player.tag != "Player")
                {
                    player.tag = "Player";
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al spawnear jugador: {e.Message}");
        }
    }
    
    /// <summary>
    /// 📍 Obtener posición de spawn única
    /// </summary>
    Vector3 GetUniqueSpawnPosition()
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        
        // Usar puntos de spawn predefinidos
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int spawnIndex = playerIndex % spawnPoints.Length;
            return spawnPoints[spawnIndex].position;
        }
        
        // Buscar puntos de spawn en la escena
        GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        if (respawnPoints.Length > 0)
        {
            int spawnIndex = playerIndex % respawnPoints.Length;
            return respawnPoints[spawnIndex].transform.position;
        }
        
        // Posición por defecto con offset
        return new Vector3(playerIndex * 3f, 1f, 0f);
    }
    
    /// <summary>
    /// 📷 Configurar cámara para seguir al jugador
    /// </summary>
    void SetupCameraForPlayer(GameObject player)
    {
        // La cámara se configura automáticamente via MovimientoCamaraSimple
        Debug.Log("📷 Cámara se configurará automáticamente");
    }
    
    /// <summary>
    /// 🧹 Limpiar jugador al salir
    /// </summary>
    public override void OnLeftRoom()
    {
        if (myPlayer != null)
        {
            PhotonNetwork.Destroy(myPlayer);
            myPlayer = null;
        }
        hasSpawnedPlayer = false;
        
        Debug.Log("🧹 Jugador limpiado al salir de sala");
    }
    
    /// <summary>
    /// 🔄 Respawn manual (para botón respawn)
    /// </summary>
    public void RespawnPlayer()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("❌ No puedo respawnear - No estoy en sala");
            return;
        }
        
        // Verificar cooldown
        if (Time.time - lastSpawnAttempt < SPAWN_COOLDOWN)
        {
            Debug.LogWarning("⏰ Respawn en cooldown");
            return;
        }
        
        // Destruir jugador actual si existe
        if (myPlayer != null)
        {
            PhotonNetwork.Destroy(myPlayer);
            myPlayer = null;
        }
        
        hasSpawnedPlayer = false;
        
        // Spawnear nuevo jugador
        StartCoroutine(DelayedSpawn());
        
        Debug.Log("🔄 Respawn iniciado");
    }
    
    /// <summary>
    /// 📊 Debug info
    /// </summary>
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 300, 10, 290, 150));
        GUILayout.Box("🎯 SPAWN CONTROLLER");
        
        GUILayout.Label($"Has Spawned: {hasSpawnedPlayer}");
        GUILayout.Label($"My Player: {(myPlayer != null ? myPlayer.name : "None")}");
        GUILayout.Label($"In Room: {PhotonNetwork.InRoom}");
        GUILayout.Label($"Actor Number: {PhotonNetwork.LocalPlayer?.ActorNumber ?? 0}");
        
        if (GUILayout.Button("🔄 Force Respawn") && PhotonNetwork.InRoom)
        {
            RespawnPlayer();
        }
        
        GUILayout.EndArea();
    }
} 