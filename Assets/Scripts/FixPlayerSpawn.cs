using UnityEngine;
using Photon.Pun;

public class FixPlayerSpawn : MonoBehaviour
{
    public bool autoSpawn = true;
    
    void Start()
    {
        Debug.Log("FixPlayerSpawn iniciado");
        
        if (autoSpawn)
        {
            Invoke("SpawnMyPlayer", 2f);
        }
    }
    
    void SpawnMyPlayer()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("No conectado a Photon");
            return;
        }
        
        // Buscar si ya tengo un jugador
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        bool tengoJugador = false;
        
        foreach (GameObject player in players)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                tengoJugador = true;
                Debug.Log("Ya tengo jugador: " + player.name);
                break;
            }
        }
        
        if (!tengoJugador)
        {
            Debug.Log("NO TENGO JUGADOR - Spawneando...");
            
            Vector3 pos = new Vector3(0, 1, 0);
            if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                pos = new Vector3(3, 1, 0);
            }
            
            PhotonNetwork.Instantiate("Player", pos, Quaternion.identity);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            SpawnMyPlayer();
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 100));
        GUILayout.Box("FIX PLAYER SPAWN");
        
        if (GUILayout.Button("SPAWN PLAYER"))
        {
            SpawnMyPlayer();
        }
        
        GUILayout.EndArea();
    }
} 