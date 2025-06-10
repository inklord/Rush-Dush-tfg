using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class WaitingUserSpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawn Configuration")]
    public Transform[] spawnPoints;
    public GameObject playerPrefab;
    public string playerPrefabName = "NetworkPlayer";
    
    [Header("Spawn Settings")]
    public float spawnDelay = 0.5f;
    public float spawnRadius = 5f;
    public bool autoFindSpawnPoints = true;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private bool hasSpawned = false;
    private GameObject spawnedPlayer;
    private Dictionary<int, GameObject> playerInstances = new Dictionary<int, GameObject>();

    void Start()
    {
        bool isMultiplayer = PhotonNetwork.IsConnected && PhotonNetwork.InRoom;
        bool isSingleplayer = !PhotonNetwork.IsConnected;
        
        Debug.Log($"🚀 WaitingUserSpawner iniciado - Modo: {(isMultiplayer ? "MULTIJUGADOR" : "SINGLEPLAYER")}");
        
        // Verificar si ya hay un jugador spawneado (solo en multiplayer)
        if (isMultiplayer && MasterSpawnController.HasSpawnedPlayer())
        {
            Debug.Log("🚫 WaitingUserSpawner: Ya existe jugador, desactivando spawner");
            enabled = false;
            return;
        }
        
        if (isMultiplayer)
        {
            Debug.Log($"👥 Multijugador - MasterClient: {PhotonNetwork.IsMasterClient}, Jugadores: {PhotonNetwork.PlayerList.Length}");
        }
        else if (isSingleplayer)
        {
            Debug.Log("🎮 Modo Singleplayer detectado");
        }
        else
        {
            Debug.LogWarning("⚠️ Estado indeterminado - Conectado a Photon pero no en sala");
            // Intentar continuar de todas formas
        }
        
        // Buscar puntos de spawn si están vacíos
        if (autoFindSpawnPoints && (spawnPoints == null || spawnPoints.Length == 0))
        {
            FindSpawnPoints();
        }
        
        // Esperar un frame para asegurar que todo esté inicializado
        StartCoroutine(DelayedSpawn());
    }
    
    IEnumerator DelayedSpawn()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(spawnDelay);
        
        SpawnLocalPlayer();
    }
    
    void FindSpawnPoints()
    {
        List<Transform> foundSpawns = new List<Transform>();
        
        // Buscar objetos con tag "Respawn"
        GameObject[] respawnObjects = GameObject.FindGameObjectsWithTag("Respawn");
        foreach (GameObject obj in respawnObjects)
        {
            foundSpawns.Add(obj.transform);
        }
        
        // Si no hay puntos de spawn, crear algunos por defecto
        if (foundSpawns.Count == 0)
        {
            Debug.Log("🔍 No se encontraron puntos de spawn, creando por defecto");
            CreateDefaultSpawnPoints();
        }
        else
        {
            spawnPoints = foundSpawns.ToArray();
            Debug.Log($"✅ Encontrados {spawnPoints.Length} puntos de spawn");
        }
    }
    
    void CreateDefaultSpawnPoints()
    {
        GameObject spawnContainer = new GameObject("DefaultSpawnPoints");
        List<Transform> defaultSpawns = new List<Transform>();
        
        // Crear 8 puntos de spawn en círculo
        for (int i = 0; i < 8; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
            spawnPoint.transform.SetParent(spawnContainer.transform);
            
            float angle = (360f / 8f) * i * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * spawnRadius,
                1f, // Altura fija
                Mathf.Sin(angle) * spawnRadius
            );
            
            spawnPoint.transform.position = position;
            spawnPoint.tag = "Respawn";
            defaultSpawns.Add(spawnPoint.transform);
        }
        
        spawnPoints = defaultSpawns.ToArray();
        Debug.Log($"✅ Creados {spawnPoints.Length} puntos de spawn por defecto");
    }
    
    void SpawnLocalPlayer()
    {
        // Verificar que no hayamos spawneado ya
        if (hasSpawned || spawnedPlayer != null)
        {
            Debug.LogWarning("⚠️ Ya tengo un jugador spawneado");
            return;
        }
        
        bool isMultiplayer = PhotonNetwork.IsConnected && PhotonNetwork.InRoom;
        bool isSingleplayer = !PhotonNetwork.IsConnected;
        
        // Para multiplayer, verificar con MasterSpawnController
        if (isMultiplayer && !MasterSpawnController.RequestSpawn("WaitingUserSpawner"))
        {
            Debug.Log("🚫 WaitingUserSpawner: MasterSpawnController denegó el spawn");
            return;
        }
        
        // Verificar si ya existe un jugador nuestro
        GameObject[] existingPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject existing in existingPlayers)
        {
            if (isMultiplayer)
            {
                PhotonView pv = existing.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    Debug.Log($"✅ Ya tengo un jugador existente (MP): {existing.name}");
                    spawnedPlayer = existing;
                    hasSpawned = true;
                    SetupSpawnedPlayer(existing);
                    return;
                }
            }
            else if (isSingleplayer)
            {
                // En singleplayer, cualquier jugador existente es nuestro
                Debug.Log($"✅ Ya existe un jugador (SP): {existing.name}");
                spawnedPlayer = existing;
                hasSpawned = true;
                SetupSpawnedPlayer(existing);
                return;
            }
        }
        
        // Obtener posición de spawn
        Vector3 spawnPosition = GetSpawnPosition();
        
        // Crear el jugador según el modo
        try
        {
            Debug.Log($"🎮 Spawneando jugador en: {spawnPosition} (Modo: {(isMultiplayer ? "MP" : "SP")})");
            
            GameObject player = null;
            
            if (isMultiplayer)
            {
                // Modo multijugador - usar Photon
                player = PhotonNetwork.Instantiate(playerPrefabName, spawnPosition, Quaternion.identity);
                
                if (player != null)
                {
                    // Registrar en diccionario
                    playerInstances[PhotonNetwork.LocalPlayer.ActorNumber] = player;
                    
                    Debug.Log($"✅ Jugador MP spawneado - ActorNumber: {PhotonNetwork.LocalPlayer.ActorNumber}");
                    
                    // Notificar a otros jugadores
                    if (photonView != null)
                    {
                        photonView.RPC("OnPlayerSpawned", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, spawnPosition);
                    }
                }
            }
            else
            {
                // Modo singleplayer - usar Unity normal
                GameObject prefabToSpawn = null;
                
                // Buscar el prefab en Resources
                prefabToSpawn = Resources.Load<GameObject>(playerPrefabName);
                
                if (prefabToSpawn == null && playerPrefab != null)
                {
                    prefabToSpawn = playerPrefab;
                }
                
                if (prefabToSpawn != null)
                {
                    player = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
                    Debug.Log($"✅ Jugador SP spawneado desde prefab");
                }
                else
                {
                    Debug.LogWarning($"⚠️ No se encontró prefab '{playerPrefabName}', intentando crear jugador básico");
                    
                    // Crear un jugador básico como fallback
                    player = CreateBasicPlayer(spawnPosition);
                }
            }
            
            if (player != null)
            {
                spawnedPlayer = player;
                hasSpawned = true;
                
                // Registrar con MasterSpawnController (solo en multiplayer)
                if (isMultiplayer)
                {
                    MasterSpawnController.RegisterSpawnedPlayer(player, "WaitingUserSpawner");
                }
                
                SetupSpawnedPlayer(player);
                Debug.Log($"✅ Jugador spawneado exitosamente");
            }
            else
            {
                Debug.LogError("❌ Error: No se pudo crear el jugador");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al spawnear jugador: {e.Message}");
        }
    }
    
    /// <summary>
    /// Crear un jugador básico como fallback
    /// </summary>
    GameObject CreateBasicPlayer(Vector3 position)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.transform.position = position;
        player.name = "BasicPlayer";
        player.tag = "Player";
        
        // Añadir componentes básicos
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.drag = 5f;
        
        // Crear material visual
        Renderer renderer = player.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material playerMaterial = new Material(Shader.Find("Standard"));
            playerMaterial.color = Color.blue;
            renderer.material = playerMaterial;
        }
        
        Debug.Log("🎯 Jugador básico creado como fallback");
        return player;
    }
    
    Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int playerIndex = 0;
            
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                // Multijugador - usar Actor Number
                playerIndex = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
            }
            else
            {
                // Singleplayer - usar primer punto de spawn
                playerIndex = 0;
            }
            
            Vector3 basePosition = spawnPoints[playerIndex].position;
            
            // Añadir pequeño offset aleatorio para evitar superposición
            Vector3 randomOffset = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            );
            
            return basePosition + randomOffset;
        }
        
        // Posición por defecto si no hay puntos de spawn
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            return new Vector3(actorNumber * 2f, 1f, 0f);
        }
        else
        {
            // Singleplayer - posición fija
            return new Vector3(0f, 1f, 0f);
        }
    }
    
    void SetupSpawnedPlayer(GameObject player)
    {
        if (player == null) return;
        
        // Asegurar que tiene PhotonView
        PhotonView pv = player.GetComponent<PhotonView>();
        if (pv == null)
        {
            Debug.LogWarning("⚠️ Jugador spawneado sin PhotonView");
            return;
        }
        
        // Verificar que es nuestro
        if (!pv.IsMine)
        {
            Debug.LogWarning("⚠️ Intentando configurar un jugador que no es nuestro");
            return;
        }
        
        // Configurar nombre del jugador
        string playerName = PhotonNetwork.LocalPlayer.NickName;
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = $"Player{PhotonNetwork.LocalPlayer.ActorNumber}";
        }
        
        // Buscar componentes comunes del jugador para configurar nombre
        // Intentar con diferentes tipos de componentes que puedan manejar el nombre
        var textMesh = player.GetComponentInChildren<TextMesh>();
        if (textMesh != null)
        {
            textMesh.text = playerName;
        }
        
        var tmpText = player.GetComponentInChildren<TMPro.TextMeshPro>();
        if (tmpText != null)
        {
            tmpText.text = playerName;
        }
        
        // Si el jugador tiene un componente específico del juego, configurarlo aquí
        // Ejemplo: var playerController = player.GetComponent<SimplePlayerMovement>();
        
        Debug.Log($"🎯 Jugador configurado: {playerName} (ActorNumber: {PhotonNetwork.LocalPlayer.ActorNumber})");
    }
    
    [PunRPC]
    void OnPlayerSpawned(int actorNumber, Vector3 position)
    {
        Debug.Log($"📡 Jugador {actorNumber} spawneado en posición: {position}");
    }
    
    #region Photon Callbacks
    
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"👤 Nuevo jugador entró: {newPlayer.NickName} (ActorNumber: {newPlayer.ActorNumber})");
    }
    
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"👋 Jugador salió: {otherPlayer.NickName} (ActorNumber: {otherPlayer.ActorNumber})");
        
        // Limpiar referencia si existe
        if (playerInstances.ContainsKey(otherPlayer.ActorNumber))
        {
            playerInstances.Remove(otherPlayer.ActorNumber);
        }
    }
    
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log($"👑 Nuevo Master Client: {newMasterClient.NickName}");
    }
    
    #endregion
    
    #region Public Methods
    
    public bool HasSpawnedPlayer()
    {
        return hasSpawned && spawnedPlayer != null;
    }
    
    public GameObject GetSpawnedPlayer()
    {
        return spawnedPlayer;
    }
    
    public void RespawnPlayer()
    {
        if (spawnedPlayer != null && spawnedPlayer.GetComponent<PhotonView>().IsMine)
        {
            PhotonNetwork.Destroy(spawnedPlayer);
            spawnedPlayer = null;
            hasSpawned = false;
            
            StartCoroutine(DelayedSpawn());
        }
    }
    
    #endregion
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        bool isMultiplayer = PhotonNetwork.IsConnected && PhotonNetwork.InRoom;
        bool isSingleplayer = !PhotonNetwork.IsConnected;
        
        GUILayout.BeginArea(new Rect(10, 120, 300, 170));
        GUILayout.Box("🚀 WAITING USER SPAWNER");
        
        GUILayout.Label($"Modo: {(isMultiplayer ? "MULTIJUGADOR" : isSingleplayer ? "SINGLEPLAYER" : "INDETERMINADO")}");
        GUILayout.Label($"Conectado a Photon: {PhotonNetwork.IsConnected}");
        GUILayout.Label($"En sala: {PhotonNetwork.InRoom}");
        GUILayout.Label($"Jugador spawneado: {hasSpawned}");
        
        if (isMultiplayer)
        {
            GUILayout.Label($"MasterClient: {PhotonNetwork.IsMasterClient}");
            GUILayout.Label($"Jugadores en sala: {PhotonNetwork.PlayerList.Length}");
        }
        
        if (spawnPoints != null)
        {
            GUILayout.Label($"Puntos de spawn: {spawnPoints.Length}");
        }
        
        GUILayout.EndArea();
    }
}
