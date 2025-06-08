using UnityEngine;
using Photon.Pun;

/// <summary>
/// 🎯 SIMPLE PLAYER SPAWNER - Versión simplificada que garantiza compilación
/// Soluciona el problema de "No tengo ningún jugador!"
/// </summary>
public class SimplePlayerSpawner : MonoBehaviourPunCallbacks
{
    [Header("🎯 Simple Spawn Settings")]
    public bool autoSpawn = true;
    public string playerPrefabName = "Player";
    public bool showDebug = true;
    
    private bool hasMyPlayer = false;
    
    void Start()
    {
        Debug.Log("🎯 SimplePlayerSpawner iniciado");
        
        if (autoSpawn && PhotonNetwork.IsConnected)
        {
            Invoke("CheckAndSpawn", 1f);
        }
        
        // Verificar cada 3 segundos
        InvokeRepeating("CheckMyPlayer", 3f, 3f);
    }
    
    void CheckAndSpawn()
    {
        if (!PhotonNetwork.IsConnected) return;
        
        GameObject myPlayer = FindMyPlayer();
        if (myPlayer == null)
        {
            Debug.Log("🚨 NO TENGO JUGADOR - Spawneando...");
            SpawnPlayer();
        }
        else
        {
            hasMyPlayer = true;
            Debug.Log($"✅ Jugador encontrado: {myPlayer.name}");
        }
    }
    
    GameObject FindMyPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        foreach (GameObject player in players)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                if (player.GetComponent<LHS_MainPlayer>() != null)
                {
                    return player;
                }
            }
        }
        
        return null;
    }
    
    void SpawnPlayer()
    {
        if (!PhotonNetwork.IsConnected) return;
        
        Vector3 spawnPos = GetSpawnPosition();
        
        try
        {
            GameObject player = PhotonNetwork.Instantiate(playerPrefabName, spawnPos, Quaternion.identity);
            if (player != null)
            {
                hasMyPlayer = true;
                Debug.Log($"✅ Jugador spawneado: {player.name}");
                
                // Configurar cámara
                MovimientoCamaraSimple camera = FindObjectOfType<MovimientoCamaraSimple>();
                if (camera != null)
                {
                    camera.SetPlayer(player.transform);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"🚨 Error spawn: {e.Message}");
        }
    }
    
    Vector3 GetSpawnPosition()
    {
        float offset = (PhotonNetwork.LocalPlayer.ActorNumber - 1) * 3f;
        return new Vector3(offset, 1f, 0f);
    }
    
    void CheckMyPlayer()
    {
        GameObject myPlayer = FindMyPlayer();
        bool currentHas = (myPlayer != null);
        
        if (hasMyPlayer != currentHas)
        {
            hasMyPlayer = currentHas;
            if (!hasMyPlayer)
            {
                Debug.Log("🚨 Perdí mi jugador - Respawning...");
                CheckAndSpawn();
            }
        }
    }
    
    public void ForceRespawn()
    {
        Debug.Log("🎮 Force respawn solicitado");
        hasMyPlayer = false;
        CheckAndSpawn();
    }
    
    void OnGUI()
    {
        if (!showDebug) return;
        
        GUILayout.BeginArea(new Rect(10, Screen.height - 120, 300, 110));
        GUILayout.Box("🎯 SIMPLE PLAYER SPAWNER");
        
        GUILayout.Label($"✅ Tengo jugador: {hasMyPlayer}");
        GUILayout.Label($"🌐 Conectado: {PhotonNetwork.IsConnected}");
        
        if (PhotonNetwork.IsConnected)
        {
            GUILayout.Label($"🎯 ActorNumber: {PhotonNetwork.LocalPlayer.ActorNumber}");
        }
        
        if (GUILayout.Button("🎮 FORCE RESPAWN"))
        {
            ForceRespawn();
        }
        
        GUILayout.EndArea();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ForceRespawn();
        }
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log("🎯 OnJoinedRoom - Spawning player...");
        Invoke("CheckAndSpawn", 1f);
    }
} 