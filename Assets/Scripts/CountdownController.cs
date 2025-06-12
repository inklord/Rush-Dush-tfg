using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;

public class CountdownController : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public Text countdownText;        // Texto que mostrará la cuenta regresiva
    public float countdownDuration = 1f;  // Duración de cada número
    public float delayBeforeStart = 1f;   // Espera antes de iniciar la cuenta regresiva

    [Header("Audio (Opcional)")]
    public AudioSource audioSource;    // Para efectos de sonido
    public AudioClip countdownSound;   // Sonido para cada número
    public AudioClip startSound;       // Sonido para "GO!"

    private void Start()
    {
        // Solo el Master Client inicia la cuenta regresiva
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartCountdown", RpcTarget.All);
        }

        // Asegurarse de que el texto esté vacío al inicio
        if (countdownText != null)
        {
            countdownText.text = "";
        }
    }

    [PunRPC]
    private void StartCountdown()
    {
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        // Cuenta regresiva: 3, 2, 1
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                
                // Reproducir sonido si está configurado
                if (audioSource != null && countdownSound != null)
                {
                    audioSource.PlayOneShot(countdownSound);
                }
            }
            
            yield return new WaitForSeconds(countdownDuration);
        }

        // Mostrar "¡GO!"
        if (countdownText != null)
        {
            countdownText.text = "¡GO!";
            
            // Reproducir sonido de inicio si está configurado
            if (audioSource != null && startSound != null)
            {
                audioSource.PlayOneShot(startSound);
            }
        }

        // Esperar un momento antes de ocultar el texto
        yield return new WaitForSeconds(countdownDuration);

        // Ocultar el texto
        if (countdownText != null)
        {
            countdownText.text = "";
        }

        // Notificar al HexagoniaGameManager que la cuenta regresiva terminó
        if (HexagoniaGameManager.Instance != null)
        {
            HexagoniaGameManager.Instance.OnCountdownFinished();
        }
    }
} 