using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class WaitingUserDisplay : MonoBehaviourPunCallbacks
{
    [Header("Visual Elements")]
    public Image backgroundImage;
    public Text mainMessageText;
    public TextMeshProUGUI mainMessageTMP;
    public GameObject loadingSpinner;
    
    [Header("Colors")]
    public Color hostColor = Color.yellow;
    public Color clientColor = Color.cyan;
    
    private string baseMessage = "Esperando a otros jugadores...";
    private float animationTimer = 0f;

    void Start()
    {
        UpdateDisplay();
        InvokeRepeating(nameof(AnimateMessage), 0f, 0.5f);
    }
    
    void Update()
    {
        animationTimer += Time.deltaTime;
    }
    
    void UpdateDisplay()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            SetMessage("❌ No conectado a la sala");
            return;
        }
        
        int playerCount = PhotonNetwork.PlayerList.Length;
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        
        string role = PhotonNetwork.IsMasterClient ? "HOST" : "CLIENTE";
        string message = $"[{role}] {baseMessage}\n{playerCount}/{maxPlayers} jugadores conectados";
        
        // Añadir información específica del rol
        if (PhotonNetwork.IsMasterClient)
        {
            message += $"\n\n🎮 Eres el anfitrión de la partida";
            message += $"\n⏳ El juego comenzará automáticamente";
        }
        else
        {
            var masterClient = PhotonNetwork.MasterClient;
            string hostName = masterClient?.NickName ?? "Desconocido";
            message += $"\n\n👑 Anfitrión: {hostName}";
            message += $"\n⏳ Esperando que inicie la partida...";
        }
        
        SetMessage(message);
        SetRoleColor();
    }
    
    void SetMessage(string message)
    {
        if (mainMessageTMP != null)
        {
            mainMessageTMP.text = message;
        }
        else if (mainMessageText != null)
        {
            mainMessageText.text = message;
        }
    }
    
    void SetRoleColor()
    {
        Color targetColor = PhotonNetwork.IsMasterClient ? hostColor : clientColor;
        
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0.3f);
        }
        
        if (mainMessageTMP != null)
        {
            mainMessageTMP.color = targetColor;
        }
        else if (mainMessageText != null)
        {
            mainMessageText.color = targetColor;
        }
    }
    
    void AnimateMessage()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom) return;
        
        // Añadir puntos animados al final del mensaje base
        int dots = ((int)(animationTimer * 2f) % 4);
        string animatedDots = new string('.', dots);
        
        string currentMessage = GetBaseMessage();
        currentMessage = currentMessage.Replace("...", animatedDots.PadRight(3));
        
        SetMessage(currentMessage);
    }
    
    string GetBaseMessage()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            return "❌ No conectado a la sala";
        }
        
        int playerCount = PhotonNetwork.PlayerList.Length;
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        
        string role = PhotonNetwork.IsMasterClient ? "HOST" : "CLIENTE";
        string message = $"[{role}] {baseMessage}\n{playerCount}/{maxPlayers} jugadores conectados";
        
        if (PhotonNetwork.IsMasterClient)
        {
            message += $"\n\n🎮 Eres el anfitrión de la partida";
            message += $"\n⏳ El juego comenzará automáticamente";
        }
        else
        {
            var masterClient = PhotonNetwork.MasterClient;
            string hostName = masterClient?.NickName ?? "Desconocido";
            message += $"\n\n👑 Anfitrión: {hostName}";
            message += $"\n⏳ Esperando que inicie la partida...";
        }
        
        return message;
    }
    
    #region Photon Callbacks
    
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"🎮 Display: Jugador entró - {newPlayer.NickName}");
        UpdateDisplay();
    }
    
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"🎮 Display: Jugador salió - {otherPlayer.NickName}");
        UpdateDisplay();
    }
    
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log($"🎮 Display: Nuevo Master Client - {newMasterClient.NickName}");
        UpdateDisplay();
    }
    
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("GameState"))
        {
            string gameState = (string)propertiesThatChanged["GameState"];
            Debug.Log($"🎮 Display: Estado del juego cambió a - {gameState}");
            
            if (gameState == "Starting" || gameState == "Loading")
            {
                SetMessage("🚀 ¡Iniciando partida!\n\nCargando...");
                
                if (loadingSpinner != null)
                {
                    loadingSpinner.SetActive(true);
                }
            }
        }
    }
    
    #endregion
} 