using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "WaitingUser")
        {
            if (Input.GetMouseButtonDown(0))
            {
                SceneManager.LoadScene("Intro");
            }
        }

        if (SceneManager.GetActiveScene().name == "Intro")
        {
            if (Input.GetMouseButtonDown(0))
            {
                SceneManager.LoadScene("InGame");
            }
        }

        if (SceneManager.GetActiveScene().name == "InGame")
        {
            if (Input.GetMouseButtonDown(0))
            {
                SceneManager.LoadScene("Carrera");
            }
        }

        if (SceneManager.GetActiveScene().name == "Carrera")
        {
            if (Input.GetMouseButtonDown(0))
            {
                SceneManager.LoadScene("Hexagonia");
            }
        }

        if (SceneManager.GetActiveScene().name == "Hexagonia")
        {
            if (Input.GetMouseButtonDown(0))
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
        SceneManager.LoadScene("WaitingUser");
    }

    public void WaitingPalyersSceneChange()
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
}
