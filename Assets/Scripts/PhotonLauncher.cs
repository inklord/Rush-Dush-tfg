using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// 🚀 PHOTON LAUNCHER SIMPLE
/// Basado en tutorial estándar de Photon - Enfoque minimalista
/// </summary>
public class PhotonLauncher : MonoBehaviourPunCallbacks
{
    [Header("🎮 Player Setup")]
    public Transform spawnPoint;
    
    [Header("🔧 Debug")]
    public bool showDebugInfo = true;
    
    private bool hasSpawned = false;
    
    void Start()
    {
        Debug.Log("🚀 PhotonLauncher iniciado");
        
        // Conectar usando la configuración ya establecida
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Debug.Log("✅ Ya conectado y en sala - Spawning jugador");
            SpawnPlayer();
        }
        else
        {
            Debug.Log("⏳ Esperando conexión a Photon...");
        }
    }
    
    public override void OnConnectedToMaster()
    {
        Debug.Log("🌐 Conectado al Master Server");
        PhotonNetwork.JoinRandomOrCreateRoom();
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log("🎮 Entré a la sala - Spawning jugador");
        SpawnPlayer();
    }
    
    /// <summary>
    /// 🎯 Spawnear jugador en el punto designado
    /// </summary>
    void SpawnPlayer()
    {
        // ⭐ CRÍTICO: Solo spawnear UN jugador por cliente
        if (hasSpawned)
        {
            Debug.LogWarning("⚠️ Ya spawneé un jugador - ABORTANDO");
            return;
        }
        
        // Verificar que no hayamos spawneado ya
        GameObject[] existingPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject existingPlayer in existingPlayers)
        {
            PhotonView pv = existingPlayer.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                Debug.LogWarning($"⚠️ Ya tengo un jugador con PhotonView.IsMine: {existingPlayer.name}");
                hasSpawned = true;
                SetupCameraForPlayer(existingPlayer);
                return;
            }
        }
        
        // Encontrar punto de spawn único para este jugador
        Vector3 spawnPosition = GetUniqueSpawnPosition();
        
        // Remover IA del spawn point si existe
        RemoveAIFromSpawnPoint(spawnPosition);
        
        // 🎯 SPAWN ÚNICO: Solo crear MI jugador
        GameObject player = PhotonNetwork.Instantiate("NetworkPlayer", spawnPosition, Quaternion.identity);
        
        if (player != null)
        {
            hasSpawned = true;
            Debug.Log($"✅ MI jugador spawneado en: {spawnPosition} | PhotonView.IsMine: {player.GetComponent<PhotonView>().IsMine}");
            
            // Configurar cámara SOLO para MI jugador
            SetupCameraForPlayer(player);
        }
        else
        {
            Debug.LogError("❌ Error al spawnear jugador");
        }
    }
    
    /// <summary>
    /// 📍 Obtener posición de spawn única para cada jugador
    /// </summary>
    Vector3 GetUniqueSpawnPosition()
    {
        // Usar el Actor Number de Photon para posiciones únicas
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        
        if (spawnPoint != null)
        {
            // Offset único por jugador
            Vector3 basePos = spawnPoint.position;
            Vector3 offset = new Vector3(playerIndex * 3f, 0, 0); // 3 metros de separación
            return basePos + offset;
        }
        
        // Buscar punto de spawn automáticamente
        GameObject spawnObj = GameObject.FindGameObjectWithTag("Respawn");
        if (spawnObj != null)
        {
            Vector3 basePos = spawnObj.transform.position;
            Vector3 offset = new Vector3(playerIndex * 3f, 0, 0);
            return basePos + offset;
        }
        
        // Posición por defecto con offset único
        return new Vector3(playerIndex * 3f, 1, 0);
    }
    
    /// <summary>
    /// 🤖 Remover IA del punto de spawn
    /// </summary>
    void RemoveAIFromSpawnPoint(Vector3 spawnPosition)
    {
        // Buscar AIs cerca del punto de spawn
        Collider[] nearbyObjects = Physics.OverlapSphere(spawnPosition, 2f);
        
        foreach (Collider obj in nearbyObjects)
        {
            // Buscar objetos con tag "AI" o que contengan "AI" en el nombre
            if (obj.CompareTag("AI") || obj.name.ToLower().Contains("ai"))
            {
                Debug.Log($"🤖 Removiendo IA: {obj.name}");
                Destroy(obj.gameObject);
            }
        }
    }
    
    /// <summary>
    /// 📷 Configurar cámara para seguir al jugador
    /// </summary>
    void SetupCameraForPlayer(GameObject player)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        
        // El script SimplePlayerMovement ya configura la cámara automáticamente
        Debug.Log("📷 Cámara será configurada automáticamente por SimplePlayerMovement");
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Box("🚀 PHOTON LAUNCHER");
        
        GUILayout.Label($"Conectado: {PhotonNetwork.IsConnected}");
        GUILayout.Label($"En sala: {PhotonNetwork.InRoom}");
        GUILayout.Label($"Jugador spawneado: {hasSpawned}");
        
        if (PhotonNetwork.InRoom)
        {
            GUILayout.Label($"Jugadores en sala: {PhotonNetwork.PlayerList.Length}");
        }
        
        GUILayout.EndArea();
    }
} 