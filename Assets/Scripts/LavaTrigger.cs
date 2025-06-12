using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

/// <summary>
/// Script simple para la lava. Solo necesita:
/// 1. Un objeto con tag "Lava"
/// 2. Un Collider marcado como "Is Trigger"
/// 3. Este script adjunto
/// </summary>
public class LavaTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"LavaTrigger: Colisión detectada con {other.name} (Tag: {other.tag})");

        // Si es un jugador, destruirlo y cambiar a escena de fracaso
        if (other.CompareTag("Player"))
        {
            Debug.Log($"LavaTrigger: Jugador {other.name} tocó la lava");
            
            // Solo procesar si es nuestro jugador local
            PhotonView playerView = other.GetComponent<PhotonView>();
            if (playerView != null && playerView.IsMine)
            {
                Debug.Log("LavaTrigger: Es nuestro jugador local, procesando muerte");
                
                // Notificar al HexagoniaGameManager
                if (HexagoniaGameManager.Instance != null)
                {
                    Debug.Log("LavaTrigger: Notificando muerte al HexagoniaGameManager");
                    HexagoniaGameManager.Instance.OnPlayerDeath(other.gameObject);
                }
                
                // Destruir el jugador
                PhotonNetwork.Destroy(other.gameObject);
                
                // Cambiar a escena de fracaso
                if (PhotonNetwork.IsConnected)
                {
                    Debug.Log("LavaTrigger: Cambiando a FinalFracaso (Multiplayer)");
                    PhotonNetwork.LoadLevel("FinalFracaso");
                }
                else
                {
                    Debug.Log("LavaTrigger: Cambiando a FinalFracaso (Singleplayer)");
                    SceneManager.LoadScene("FinalFracaso");
                }
            }
        }
        // Si es una IA, solo destruirla
        else if (other.CompareTag("IA"))
        {
            Debug.Log($"LavaTrigger: IA {other.name} tocó la lava");
            
            // Notificar al HexagoniaGameManager
            if (HexagoniaGameManager.Instance != null)
            {
                Debug.Log("LavaTrigger: Notificando muerte de IA al HexagoniaGameManager");
                HexagoniaGameManager.Instance.OnPlayerDeath(other.gameObject);
            }

            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Destroy(other.gameObject);
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }
} 