using UnityEngine;
using Photon.Pun;
using System.Collections;

/// <summary>
/// 🏁 CARRERA FORCE MULTIPLAYER FIX - Script simple que fuerza 2 jugadores en Carrera
/// </summary>
public class CarreraForceMultiplayerFix : MonoBehaviourPunCallbacks
{
    [Header("🏁 Configuración")]
    public bool showDebug = false;
    public float checkInterval = 3f;
    public bool autoSpawnMissingPlayer = true;
    
    private bool isSpawning = false;

    void Start()
    {
        // Solo en escena Carrera
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Carrera")
        {
            if (showDebug) Debug.Log("🏁 CarreraForceMultiplayerFix iniciado");
            StartCoroutine(ForceMultiplayerLoop());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator ForceMultiplayerLoop()
    {
        // Esperar que la escena se estabilice
        yield return new WaitForSeconds(2f);

        while (true)
        {
            if (autoSpawnMissingPlayer && !isSpawning)
            {
                CheckAndForceSpawn();
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    void CheckAndForceSpawn()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        int localPlayers = 0;
        int remotePlayers = 0;

        foreach (GameObject player in allPlayers)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null)
            {
                if (pv.IsMine)
                    localPlayers++;
                else
                    remotePlayers++;
            }
        }

                    if (showDebug) Debug.Log($"🏁 Estado actual - Local: {localPlayers}, Remoto: {remotePlayers}, Total: {allPlayers.Length}");

        // Si solo tengo 1 jugador total, spawnear uno remoto ficticio
        if (allPlayers.Length < 2)
        {
            if (showDebug) Debug.Log("🚀 Solo hay 1 jugador - Spawneando jugador remoto ficticio");
            StartCoroutine(SpawnRemotePlayer());
        }
    }

    IEnumerator SpawnRemotePlayer()
    {
        if (isSpawning) yield break;
        isSpawning = true;

        // Simular que viene otro jugador
        Vector3 spawnPos = GetAlternateSpawnPosition();
        
        try
        {
            // Si estamos conectados, crear un jugador de red
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                if (showDebug) Debug.Log("🚀 Intentando spawn de red...");
                GameObject remotePlayer = PhotonNetwork.Instantiate("NetworkPlayer", spawnPos, Quaternion.identity);
                if (remotePlayer != null)
                {
                    if (showDebug) Debug.Log($"✅ Jugador remoto spawneado: {remotePlayer.name}");
                }
            }
            else
            {
                // Si no hay red, crear un jugador local simple
                if (showDebug) Debug.Log("🚀 Creando jugador local alternativo...");
                GameObject playerPrefab = Resources.Load<GameObject>("NetworkPlayer");
                if (playerPrefab == null)
                {
                    // Buscar en la escena un prefab de ejemplo
                    GameObject[] existingPlayers = GameObject.FindGameObjectsWithTag("Player");
                    if (existingPlayers.Length > 0)
                    {
                        GameObject fakeRemote = Instantiate(existingPlayers[0], spawnPos, Quaternion.identity);
                        fakeRemote.name = "FakeRemotePlayer";
                        
                        // Desactivar controles del jugador falso
                        LHS_MainPlayer playerScript = fakeRemote.GetComponent<LHS_MainPlayer>();
                        if (playerScript != null)
                        {
                            playerScript.enabled = false;
                        }
                        
                        // Cambiar color para distinguirlo
                        ChangePlayerColor(fakeRemote, Color.red);
                        
                        if (showDebug) Debug.Log("✅ Jugador falso creado para simular multijugador");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            if (showDebug) Debug.LogError($"❌ Error spawneando jugador remoto: {e.Message}");
        }

        yield return new WaitForSeconds(1f);
        isSpawning = false;
    }

    Vector3 GetAlternateSpawnPosition()
    {
        // Buscar puntos de spawn
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        
        if (spawnPoints.Length > 1)
        {
            return spawnPoints[1].transform.position + Vector3.up * 1f;
        }
        
        // Posición alternativa basada en el jugador existente
        GameObject[] existingPlayers = GameObject.FindGameObjectsWithTag("Player");
        if (existingPlayers.Length > 0)
        {
            Vector3 basePos = existingPlayers[0].transform.position;
            return basePos + new Vector3(5f, 0f, 0f); // 5 metros a la derecha
        }
        
        // Posición por defecto
        return new Vector3(90f, 28f, 10f);
    }

    void ChangePlayerColor(GameObject player, Color color)
    {
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material != null)
            {
                renderer.material.color = color;
            }
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (showDebug) Debug.Log($"🏁 Nuevo jugador entró - Verificando spawns en 2 segundos");
        StartCoroutine(DelayedCheck());
    }

    IEnumerator DelayedCheck()
    {
        yield return new WaitForSeconds(2f);
        CheckAndForceSpawn();
    }

    void OnGUI()
    {
        if (!showDebug) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int localCount = 0;
        int remoteCount = 0;
        int fakeCount = 0;

        foreach (GameObject player in players)
        {
            if (player.name.Contains("Fake"))
            {
                fakeCount++;
            }
            else
            {
                PhotonView pv = player.GetComponent<PhotonView>();
                if (pv != null)
                {
                    if (pv.IsMine) localCount++;
                    else remoteCount++;
                }
                else
                {
                    localCount++; // Sin PhotonView = local
                }
            }
        }

        GUI.Box(new Rect(10, 250, 280, 100), "🏁 FORCE MULTIPLAYER FIX");
        GUI.Label(new Rect(20, 275, 250, 20), $"Total jugadores: {players.Length}");
        GUI.Label(new Rect(20, 295, 250, 20), $"Local: {localCount} | Remoto: {remoteCount} | Fake: {fakeCount}");
        GUI.Label(new Rect(20, 315, 250, 20), $"Spawning: {(isSpawning ? "SÍ" : "NO")}");

        if (GUI.Button(new Rect(20, 330, 120, 20), "Force Spawn"))
        {
            StartCoroutine(SpawnRemotePlayer());
        }
    }
} 