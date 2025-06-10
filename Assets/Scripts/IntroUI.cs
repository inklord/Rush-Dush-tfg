using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using Photon.Pun;

/// <summary>
/// 🎬 Controlador de la escena Intro con Timeline/Cinemachine
/// Detecta automáticamente cuando termina la intro y cambia a InGame
/// </summary>
public class IntroUI : MonoBehaviourPunCallbacks
{
    [Header("🎬 Intro UI Components")]
    public GameObject missionUI;
    public GameObject missionPos;
    public float speed = 3f;
    
    [Header("⏱️ Auto Scene Change")]
    public float introDuration = 14f;           // Duración total de la intro en segundos
    public string nextScene = "InGame";          // Escena a cargar después
    public bool autoChangeScene = true;          // Cambiar escena automáticamente
    
    [Header("🎮 Optional Timeline")]
    public PlayableDirector timelineDirector;   // Timeline director (opcional)
    
    Vector3 dir;
    private float introTimer = 0f;
    private bool hasChangedScene = false;
    
    void Start()
    {
        Debug.Log("🎬 Intro iniciada");
        
        // Buscar Timeline automáticamente si no está asignado
        if (timelineDirector == null)
        {
            timelineDirector = FindObjectOfType<PlayableDirector>();
            if (timelineDirector != null)
            {
                Debug.Log($"🎬 Timeline encontrado: {timelineDirector.name}");
                introDuration = (float)timelineDirector.duration;
                Debug.Log($"⏱️ Duración detectada: {introDuration} segundos");
            }
        }
        
        // Iniciar timer
        introTimer = 0f;
        hasChangedScene = false;
    }

    void Update()
    {
        // Movimiento de UI (código original)
        if (missionUI != null && missionPos != null)
        {
            dir = missionPos.transform.position - missionUI.transform.position;
            missionUI.transform.position += dir * speed * Time.deltaTime;
        }
        
        // Control de cambio de escena automático
        if (autoChangeScene && !hasChangedScene)
        {
            introTimer += Time.deltaTime;
            
            // Verificar si el Timeline ha terminado (método preferido)
            if (timelineDirector != null)
            {
                if (timelineDirector.state != PlayState.Playing && introTimer > 1f)
                {
                    ChangeToInGame();
                }
            }
            // Fallback: usar timer manual
            else if (introTimer >= introDuration)
            {
                ChangeToInGame();
            }
        }
        
        // Permitir saltar la intro con tecla (opcional)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
        {
            ChangeToInGame();
        }
    }
    
    /// <summary>
    /// 🚀 Cambiar a la escena de juego
    /// </summary>
    void ChangeToInGame()
    {
        if (hasChangedScene) return;
        
        hasChangedScene = true;
        Debug.Log($"🚀 Cambiando a escena: {nextScene}");
        
        // Solo el MasterClient puede cambiar la escena
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(nextScene);
        }
    }
    
    /// <summary>
    /// 🎬 Método público para terminar intro manualmente
    /// </summary>
    public void EndIntro()
    {
        ChangeToInGame();
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.LogWarning($"❌ Desconectado de Photon: {cause}");
        // Si nos desconectamos, volver al lobby
        SceneManager.LoadScene("Lobby");
    }
}
