using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
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
                SceneManager.LoadScene("InGame");
            }
        }

        if (currentScene == "InGame")
        {
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Tilde))
            {
                SceneManager.LoadScene("Carrera");
            }
        }

        if (currentScene == "Carrera")
        {
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Tilde))
            {
                SceneManager.LoadScene("Hexagonia");
            }
        }

        if (currentScene == "Hexagonia")
        {
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Tilde))
            {
                SceneManager.LoadScene("Ending");
            }
        }
    }

    public void LoginSceneChange()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void LobbySceneChange()
    {
        SceneManager.LoadScene("Intro");
    }

    public void StartGameSceneChange()
    {
        SceneManager.LoadScene("Intro");
    }

    public void IntroSceneChange()
    {
        SceneManager.LoadScene("InGame");
    }

    public void InGameSceneChange()
    {
        SceneManager.LoadScene("Carrera");
    }

    public void CarreraSceneChange()
    {
        SceneManager.LoadScene("Hexagonia");
    }

    public void HexagoniaSceneChange()
    {
        SceneManager.LoadScene("Ending");
    }

    public void EndingSceneChange()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void GoToCarrera()
    {
        SceneManager.LoadScene("Carrera");
    }

    public void GoToHexagonia()
    {
        SceneManager.LoadScene("Hexagonia");
    }

    public void GoToInGame()
    {
        SceneManager.LoadScene("InGame");
    }

    public void GoToEnding()
    {
        SceneManager.LoadScene("Ending");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void GoToEndingSuccess()
    {
        SceneManager.LoadScene("Ending");
    }
    
    public void GoToEndingFailure()
    {
        SceneManager.LoadScene("FinalFracaso");
    }

    public void FinalFracasoSceneChange()
    {
        SceneManager.LoadScene("Lobby");
    }
}
