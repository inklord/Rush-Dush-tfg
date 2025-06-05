using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class EliminationSystem : MonoBehaviourPunCallbacks
{
    public Text playerCountText;  // UI de jugadores restantes
    public GameObject losePanel;  // Panel de derrota
    public GameObject winPanel;   // Panel de victoria

    private int playerID; // ID del jugador local

    void Start()
    {
        if (losePanel != null) losePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        if (playerCountText == null) Debug.LogError("playerCountText no asignado en el Inspector");
        if (losePanel == null) Debug.LogError("losePanel no asignado en el Inspector");
        if (winPanel == null) Debug.LogError("winPanel no asignado en el Inspector");

        playerID = PhotonNetwork.LocalPlayer.ActorNumber; // Asigna un ID único según Photon

        if (PhotonNetwork.IsMasterClient) // Solo el host controla la eliminación
        {
            InvokeRepeating("EliminatePlayers", 3f, 3f); // Cada 3 segundos elimina jugadores
        }

        UpdatePlayerCountUI();
    }

    [PunRPC]
    void UpdatePlayerCountUI()
    {
        if (playerCountText != null)
        {
            playerCountText.text = "Jugadores restantes: " + PhotonNetwork.PlayerList.Length;
        }
    }

    void EliminatePlayers()
    {
        if (!PhotonNetwork.IsMasterClient) return; // Solo el host elimina jugadores

        int currentPlayers = PhotonNetwork.PlayerList.Length;
        int eliminated = 0;

        if (currentPlayers > 15)
        {
            eliminated = Random.Range(1, currentPlayers - 15 + 1);
        }
        else if (currentPlayers > 10)
        {
            eliminated = Random.Range(1, currentPlayers - 10 + 1);
        }
        else if (currentPlayers > 5)
        {
            eliminated = Random.Range(1, currentPlayers - 5 + 1);
        }
        else if (currentPlayers > 1)
        {
            eliminated = Random.Range(1, currentPlayers - 1 + 1);
        }

        if (eliminated > 0)
        {
            Photon.Realtime.Player playerToEliminate = PhotonNetwork.PlayerList[Random.Range(0, currentPlayers)];
            photonView.RPC("EliminatePlayer", RpcTarget.All, playerToEliminate.ActorNumber);
        }
    }

    [PunRPC]
    void EliminatePlayer(int eliminatedPlayerID)
    {
        if (playerID == eliminatedPlayerID)
        {
            losePanel.SetActive(true);
            PhotonNetwork.LeaveRoom();
        }

        photonView.RPC("UpdatePlayerCountUI", RpcTarget.All);
        CheckGameState();
    }

    void CheckGameState()
    {
        if (PhotonNetwork.PlayerList.Length == 1)
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                winPanel.SetActive(true);
            }
        }
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); // Regresa al menú principal
    }
}
