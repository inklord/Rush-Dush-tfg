using UnityEngine;
using Photon.Pun;

/// <summary>
/// 📷 CONFIGURACIÓN AUTOMÁTICA DE CÁMARA
/// Se asegura de que la cámara siempre siga al jugador correcto
/// </summary>
public class CameraAutoSetup : MonoBehaviour
{
    [Header("📷 Configuración Automática")]
    public bool setupOnStart = true;
    public bool continuousCheck = true;
    public float checkInterval = 2f;
    
    private MovimientoCamaraSimple cameraScript;
    private float nextCheckTime = 0f;
    
    void Start()
    {
        
        
        cameraScript = FindObjectOfType<MovimientoCamaraSimple>();
        if (cameraScript == null)
        {
            
            return;
        }
        
        if (setupOnStart)
        {
            Invoke("ConfigurarCamara", 1f); // Delay para que se spawnen los jugadores
        }
    }
    
    void Update()
    {
        if (continuousCheck && Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            VerificarYConfigurarCamara();
        }
    }
    
    /// <summary>
    /// 📷 CONFIGURAR CÁMARA PARA SEGUIR AL JUGADOR CORRECTO
    /// </summary>
    public void ConfigurarCamara()
    {
        if (cameraScript == null) return;
        
        GameObject miJugador = EncontrarMiJugador();
        if (miJugador == null)
        {
            
            return;
        }
        
        cameraScript.SetPlayer(miJugador.transform);
        
        
        // Asegurar que la cámara esté activa
        Camera mainCamera = Camera.main;
        if (mainCamera != null && !mainCamera.enabled)
        {
            mainCamera.enabled = true;
            
        }
    }
    
    /// <summary>
    /// 🔍 VERIFICAR Y CONFIGURAR CÁMARA CONTINUAMENTE
    /// </summary>
    void VerificarYConfigurarCamara()
    {
        if (cameraScript == null) return;
        
        GameObject miJugador = EncontrarMiJugador();
        if (miJugador == null) return;
        
        // Si la cámara no tiene player o sigue al jugador incorrecto
        if (cameraScript.player == null || cameraScript.player.gameObject != miJugador)
        {
            
            cameraScript.SetPlayer(miJugador.transform);
        }
    }
    
    /// <summary>
    /// 🎮 ENCONTRAR MI JUGADOR
    /// </summary>
    GameObject EncontrarMiJugador()
    {
        // En modo multijugador - buscar por PhotonView
        if (PhotonNetwork.IsConnected)
        {
            PhotonView[] views = FindObjectsOfType<PhotonView>();
            foreach (PhotonView pv in views)
            {
                if (pv.IsMine && pv.gameObject.CompareTag("Player"))
                {
                    LHS_MainPlayer playerScript = pv.GetComponent<LHS_MainPlayer>();
                    if (playerScript != null)
                    {
                        return pv.gameObject;
                    }
                }
            }
        }
        else
        {
            // En modo single player - buscar jugador sin PhotonView
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (player.GetComponent<LHS_MainPlayer>() != null)
                {
                    PhotonView pv = player.GetComponent<PhotonView>();
                    if (pv == null) // Sin PhotonView = single player
                    {
                        return player;
                    }
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 🔧 MÉTODO PÚBLICO PARA FORZAR CONFIGURACIÓN
    /// </summary>
    public void ForzarConfiguracionCamara()
    {
        ConfigurarCamara();
    }
} 
