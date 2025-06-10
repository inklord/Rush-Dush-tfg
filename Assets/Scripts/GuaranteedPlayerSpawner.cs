using UnityEngine;
using Photon.Pun;
using System.Collections;

/// <summary>
/// 🎯 GUARANTEED PLAYER SPAWNER - Garantiza que cada jugador tenga su player
/// Soluciona el problema de "No tengo ningún jugador!"
/// </summary>
public class GuaranteedPlayerSpawner : MonoBehaviourPunCallbacks
{
    [Header("🎯 Guaranteed Spawn Settings")]
    public bool autoSpawnOnJoin = true;
    public bool forceRespawnIfMissing = true;
    public float respawnCheckInterval = 2f;
    public bool showDebugInfo = true;
    
    [Header("🎮 Player Prefab")]
    public string playerPrefabName = "Player"; // Nombre del prefab en Resources
    
    [Header("📍 Spawn Points")]
    public Transform[] spawnPoints;
    
    private bool hasMyPlayer = false;
    private float lastCheckTime = 0f;
    
    void Start()
    {
        Debug.Log("🎯 === GUARANTEED PLAYER SPAWNER INICIADO ===");
        
        // Verificar si ya hay un jugador spawneado
        if (MasterSpawnController.HasSpawnedPlayer())
        {
            Debug.Log("🚫 GuaranteedPlayerSpawner: Ya existe jugador, desactivando spawner");
            enabled = false;
            return;
        }
        
        // Auto-spawn si está activado
        if (autoSpawnOnJoin && PhotonNetwork.IsConnected)
        {
            StartCoroutine(DelayedSpawnCheck());
        }
        
        // Verificación continua
        if (forceRespawnIfMissing)
        {
            InvokeRepeating("CheckMyPlayer", respawnCheckInterval, respawnCheckInterval);
        }
    }
    
    IEnumerator DelayedSpawnCheck()
    {
        yield return new WaitForSeconds(1f); // Esperar estabilización de red
        CheckAndSpawnMyPlayer();
    }
    
    /// <summary>
    /// 🔍 VERIFICAR Y SPAWN MI JUGADOR
    /// </summary>
    public void CheckAndSpawnMyPlayer()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("🎯 No conectado a Photon, saltando spawn");
            return;
        }
        
        // Buscar mi jugador actual
        GameObject myPlayer = FindMyPlayer();
        
        if (myPlayer == null)
        {
            Debug.Log("🚨 NO TENGO JUGADOR PROPIO - Spawneando...");
            SpawnMyPlayer();
        }
        else
        {
            hasMyPlayer = true;
            if (showDebugInfo)
            {
                Debug.Log($"✅ MI JUGADOR ENCONTRADO: {myPlayer.name}");
            }
        }
    }
    
    /// <summary>
    /// 🔍 ENCONTRAR MI JUGADOR
    /// </summary>
    GameObject FindMyPlayer()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        
        foreach (GameObject player in allPlayers)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            
            if (pv != null && pv.IsMine)
            {
                // Verificar que es un jugador real, no AI
                if (!player.name.Contains("IAPlayerSimple") && 
                    !player.name.Contains("AI") &&
                    player.GetComponent<LHS_MainPlayer>() != null)
                {
                    return player;
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 🎮 SPAWN MI JUGADOR
    /// </summary>
    void SpawnMyPlayer()
    {
        // Verificar con MasterSpawnController primero
        if (!MasterSpawnController.RequestSpawn("GuaranteedPlayerSpawner"))
        {
            Debug.Log("🚫 GuaranteedPlayerSpawner: MasterSpawnController denegó el spawn");
            return;
        }
        
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("🚨 No conectado a Photon - No se puede spawnear");
            return;
        }
        
        Vector3 spawnPosition = GetSpawnPosition();
        Quaternion spawnRotation = Quaternion.identity;
        
        Debug.Log($"🎮 GuaranteedPlayerSpawner spawneando jugador en posición: {spawnPosition}");
        Debug.Log($"🎮 ActorNumber: {PhotonNetwork.LocalPlayer.ActorNumber}");
        
        try
        {
            GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabName, spawnPosition, spawnRotation);
            
            if (playerObj != null)
            {
                hasMyPlayer = true;
                Debug.Log($"✅ GuaranteedPlayerSpawner - Jugador spawneado exitosamente: {playerObj.name}");
                
                // Registrar con MasterSpawnController
                MasterSpawnController.RegisterSpawnedPlayer(playerObj, "GuaranteedPlayerSpawner");
                
                // Configurar cámara
                ConfigureCamera(playerObj);
                
                // Configurar tags
                if (!playerObj.CompareTag("Player"))
                {
                    playerObj.tag = "Player";
                }
            }
            else
            {
                Debug.LogError("🚨 SPAWN FALLÓ - Objeto nulo retornado");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"🚨 ERROR EN SPAWN: {e.Message}");
        }
    }
    
    /// <summary>
    /// 📍 OBTENER POSICIÓN DE SPAWN
    /// </summary>
    Vector3 GetSpawnPosition()
    {
        // Si tenemos spawn points configurados
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int index = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
            return spawnPoints[index].position;
        }
        
        // Posición basada en ActorNumber
        float offset = (PhotonNetwork.LocalPlayer.ActorNumber - 1) * 3f;
        return new Vector3(offset, 1f, 0f);
    }
    
    /// <summary>
    /// 📷 CONFIGURAR CÁMARA
    /// </summary>
    void ConfigureCamera(GameObject player)
    {
        // Buscar cámara simple
        MovimientoCamaraSimple cameraScript = FindObjectOfType<MovimientoCamaraSimple>();
        if (cameraScript != null)
        {
            cameraScript.SetPlayer(player.transform);
            Debug.Log("📷 Cámara configurada para nuevo jugador");
        }
    }
    
    /// <summary>
    /// 🔄 VERIFICACIÓN CONTINUA
    /// </summary>
    void CheckMyPlayer()
    {
        if (Time.time - lastCheckTime < respawnCheckInterval) return;
        lastCheckTime = Time.time;
        
        GameObject myPlayer = FindMyPlayer();
        bool currentHasPlayer = (myPlayer != null);
        
        if (hasMyPlayer != currentHasPlayer)
        {
            hasMyPlayer = currentHasPlayer;
            
            if (!hasMyPlayer)
            {
                Debug.Log("🚨 PERDÍ MI JUGADOR - Intentando respawn...");
                CheckAndSpawnMyPlayer();
            }
        }
    }
    
    /// <summary>
    /// 🔄 FORCE RESPAWN - Manual
    /// </summary>
    public void ForceRespawn()
    {
        Debug.Log("🎮 FORCE RESPAWN solicitado por usuario");
        hasMyPlayer = false;
        CheckAndSpawnMyPlayer();
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, Screen.height - 150, 350, 140));
        
        GUIStyle headerStyle = new GUIStyle(GUI.skin.box);
        headerStyle.fontSize = 12;
        headerStyle.fontStyle = FontStyle.Bold;
        
        GUILayout.Box("🎯 GUARANTEED PLAYER SPAWNER", headerStyle);
        
        // Estado actual
        GUILayout.Label($"✅ Tengo jugador: {hasMyPlayer}");
        GUILayout.Label($"🌐 Conectado: {PhotonNetwork.IsConnected}");
        GUILayout.Label($"🎯 ActorNumber: {PhotonNetwork.LocalPlayer.ActorNumber}");
        
        // Botones de control
        if (GUILayout.Button("🎮 FORCE RESPAWN"))
        {
            ForceRespawn();
        }
        
        if (GUILayout.Button("🔄 CHECK PLAYER"))
        {
            CheckAndSpawnMyPlayer();
        }
        
        GUILayout.EndArea();
    }
    
    void Update()
    {
        // Atajos de teclado
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ForceRespawn();
        }
    }
    
    #region Photon Callbacks
    
    public override void OnJoinedRoom()
    {
        Debug.Log("🎯 OnJoinedRoom - Verificando spawn...");
        StartCoroutine(DelayedSpawnCheck());
    }
    
    #endregion
} 