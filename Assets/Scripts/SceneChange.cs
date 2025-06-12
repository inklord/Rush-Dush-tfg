using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class SceneChange : MonoBehaviour
{
    [Header("Transición por Lava")]
    public float delayBeforeFracaso = 2f; // Tiempo de espera antes de ir a FinalFracaso

    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (currentScene == "Intro")
        {
            // En Intro: permite BackQuote, tecla º/~ y CLICK para continuar
            if (Input.GetKeyDown(KeyCode.BackQuote) || 
                Input.GetKeyDown(KeyCode.Tilde) || 
                Input.GetMouseButtonDown(0))
            {
                Debug.Log("🎬 SceneChange: Saltando desde Intro a InGame");
                LoadScene("InGame");
            }
        }

        if (currentScene == "InGame")
        {
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Tilde))
            {
                Debug.Log("🏁 SceneChange: Cambiando de InGame a Carrera");
                LoadScene("Carrera");
            }
        }

        if (currentScene == "Carrera")
        {
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Tilde))
            {
                Debug.Log("⬡ SceneChange: Cambiando de Carrera a Hexagonia");
                LoadScene("Hexagonia");
            }
        }

        if (currentScene == "Hexagonia")
        {
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Tilde))
            {
                Debug.Log("🏆 SceneChange: Cambiando de Hexagonia a Ending");
                LoadScene("Ending");
            }
        }
    }

    /// <summary>
    /// 🌐 Método unificado para cargar escenas (soporta multijugador)
    /// </summary>
    private void LoadScene(string sceneName)
    {
        // Verificar si estamos en modo multijugador
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            // 🌐 MODO MULTIJUGADOR - Solo el MasterClient puede cambiar escenas
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log($"🌐 [MULTIJUGADOR] MasterClient cambiando a escena: {sceneName}");
                PhotonNetwork.LoadLevel(sceneName);
            }
            else
            {
                Debug.Log($"⚠️ [MULTIJUGADOR] Solo el host puede cambiar de escena (tecla º ignorada)");
                // Opcional: mostrar mensaje en pantalla para jugadores no-host
                ShowNonHostMessage();
            }
        }
        else
        {
            // 🎮 MODO SINGLEPLAYER - Cambio normal
            Debug.Log($"🎮 [SINGLEPLAYER] Cambiando a escena: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// ⚠️ Mostrar mensaje cuando jugador no-host intenta cambiar escena
    /// </summary>
    private void ShowNonHostMessage()
    {
        // Opcional: mostrar un mensaje temporal en pantalla
        // StartCoroutine(ShowTemporaryMessage("Solo el host puede cambiar de escena"));
    }

    public void LoginSceneChange()
    {
        LoadScene("Lobby");
    }

    public void LobbySceneChange()
    {
        LoadScene("Intro");
    }

    public void StartGameSceneChange()
    {
        LoadScene("Intro");
    }

    public void IntroSceneChange()
    {
        LoadScene("InGame");
    }

    public void InGameSceneChange()
    {
        LoadScene("Carrera");
    }

    public void CarreraSceneChange()
    {
        LoadScene("Hexagonia");
    }

    public void HexagoniaSceneChange()
    {
        LoadScene("Ending");
    }

    public void EndingSceneChange()
    {
        LoadScene("Lobby");
    }

    public void GoToCarrera()
    {
        LoadScene("Carrera");
    }

    public void GoToHexagonia()
    {
        LoadScene("Hexagonia");
    }

    public void GoToInGame()
    {
        LoadScene("InGame");
    }

    public void GoToEnding()
    {
        LoadScene("Ending");
    }

    public void RestartGame()
    {
        LoadScene("Lobby");
    }

    public void GoToEndingSuccess()
    {
        LoadScene("Ending");
    }
    
    public void GoToEndingFailure()
    {
        StartCoroutine(GoToFinalFracasoWithDelay());
    }

    private IEnumerator GoToFinalFracasoWithDelay()
    {
        Debug.Log("💀 SceneChange: Iniciando transición a FinalFracaso...");
        
        // Esperar el delay configurado
        yield return new WaitForSeconds(delayBeforeFracaso);
        
        Debug.Log("💀 SceneChange: Cargando escena FinalFracaso");
        LoadScene("FinalFracaso");
    }

    public void FinalFracasoSceneChange()
    {
        LoadScene("Lobby");
    }

    // Nuevo método específico para muerte por lava
    public void HandleLavaDeath()
    {
        Debug.Log("💀 SceneChange: Jugador tocó lava, iniciando transición a FinalFracaso");
        StartCoroutine(GoToFinalFracasoWithDelay());
    }
}
