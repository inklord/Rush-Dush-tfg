using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class WaitingUserManager : MonoBehaviourPunCallbacks
{
    private float waitTime = 3f;
    private float timer = 0f;
    private bool isTransitioning = false;
    private new PhotonView photonView;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (photonView == null)
        {
            photonView = gameObject.AddComponent<PhotonView>();
            photonView.ViewID = 999; // ID fijo para este manager
        }
    }

    void Start()
    {
        // Asegurar que estamos conectados y en una sala
        if (!PhotonNetwork.IsConnected || PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogWarning("⚠️ No hay conexión con Photon o no estamos en una sala");
            SceneManager.LoadScene("Lobby"); // Volver al lobby si no hay conexión
            return;
        }

        // Asegurar que la sincronización automática está activada
        PhotonNetwork.AutomaticallySyncScene = true;

        // Iniciar el temporizador solo si somos el MasterClient
        if (PhotonNetwork.IsMasterClient)
        {
            timer = 0f;
            isTransitioning = false;
            
            // Establecer las propiedades iniciales de la sala
            Hashtable props = new Hashtable();
            props.Add("GameState", "WaitingUser");
            props.Add("WaitingStartTime", (float)PhotonNetwork.Time);
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            
            Debug.Log($"🎮 MasterClient configurando WaitingUser - Jugadores: {PhotonNetwork.PlayerList.Length}");
        }
        else
        {
            Debug.Log($"🎮 Cliente conectado a WaitingUser - MasterClient: {PhotonNetwork.MasterClient?.NickName}");
        }

        Debug.Log($"🎮 WaitingUser iniciado - MasterClient: {PhotonNetwork.IsMasterClient}, Jugadores en sala: {PhotonNetwork.PlayerList.Length}");
        
        // Mostrar información de todos los jugadores en la sala
        foreach (var player in PhotonNetwork.PlayerList)
        {
            string role = player.IsMasterClient ? "[HOST]" : "[CLIENT]";
            string isLocal = player.IsLocal ? " (TU)" : "";
            Debug.Log($"👤 {role} {player.NickName ?? $"Player{player.ActorNumber}"}{isLocal}");
        }
    }

    void Update()
    {
        if (!isTransitioning && PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            timer += Time.deltaTime;

            if (timer >= waitTime)
            {
                isTransitioning = true;
                StartGameForAll();
            }
        }
    }

    private void StartGameForAll()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("🎮 WaitingUser: Iniciando transición a Intro para todos los jugadores");
        
        // Actualizar estado del juego
        Hashtable props = new Hashtable();
        props.Add("GameState", "StartingIntro");
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        // Notificar a todos los jugadores
        photonView.RPC("PrepareForIntro", RpcTarget.All);
    }

    [PunRPC]
    private void PrepareForIntro()
    {
        Debug.Log("🎬 Preparando transición a Intro...");
        
        // Solo el MasterClient carga la siguiente escena
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Intro");
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"👤 Jugador entró a WaitingUser: {newPlayer.NickName}");
        
        // Sincronizar el estado actual con el nuevo jugador
        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable props = PhotonNetwork.CurrentRoom.CustomProperties;
            if (!props.ContainsKey("GameState"))
            {
                props = new Hashtable();
                props.Add("GameState", "WaitingUser");
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"❌ Desconectado de Photon: {cause}");
        // Si nos desconectamos, volver al lobby
        SceneManager.LoadScene("Lobby");
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log($"👑 Nuevo Master Client en WaitingUser: {newMasterClient.NickName}");
        
        // Si nos convertimos en el nuevo MasterClient y el juego no ha comenzado
        if (PhotonNetwork.IsMasterClient)
        {
            string gameState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameState"];
            if (gameState == "WaitingUser")
            {
                timer = 0f;
                isTransitioning = false;
            }
        }
    }
} 