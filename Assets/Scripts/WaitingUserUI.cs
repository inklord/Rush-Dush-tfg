using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class WaitingUserUI : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public Text waitingText;
    public Text playerCountText;
    public Text playerListText;
    public GameObject playerListPanel;
    
    [Header("Settings")]
    public bool useTextMeshPro = false;
    
    // TextMeshPro alternativas (opcional)
    [Header("TextMeshPro References (Optional)")]
    public TextMeshProUGUI waitingTextTMP;
    public TextMeshProUGUI playerCountTextTMP;
    public TextMeshProUGUI playerListTextTMP;
    
    private float updateInterval = 1f;
    private float lastUpdateTime = 0f;

    void Start()
    {
        // Buscar automáticamente componentes si no están asignados
        FindUIComponents();
        
        // Actualizar UI inmediatamente
        UpdatePlayerInfo();
        
        Debug.Log($"🎮 WaitingUserUI iniciado - MasterClient: {PhotonNetwork.IsMasterClient}, Jugadores: {PhotonNetwork.PlayerList.Length}");
    }
    
    void Update()
    {
        // Actualizar UI periódicamente
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdatePlayerInfo();
            lastUpdateTime = Time.time;
        }
    }
    
    void FindUIComponents()
    {
        // Buscar componentes automáticamente por nombre
        if (waitingText == null && waitingTextTMP == null)
        {
            // Buscar por nombres comunes
            Text[] texts = FindObjectsOfType<Text>();
            foreach (Text text in texts)
            {
                string name = text.name.ToLower();
                if (name.Contains("waiting") || name.Contains("esperando"))
                {
                    waitingText = text;
                    Debug.Log($"✅ Encontrado waitingText: {text.name}");
                    break;
                }
            }
            
            // Si no se encontró Text, buscar TextMeshPro
            if (waitingText == null)
            {
                TextMeshProUGUI[] tmpTexts = FindObjectsOfType<TextMeshProUGUI>();
                foreach (TextMeshProUGUI tmpText in tmpTexts)
                {
                    string name = tmpText.name.ToLower();
                    if (name.Contains("waiting") || name.Contains("esperando"))
                    {
                        waitingTextTMP = tmpText;
                        useTextMeshPro = true;
                        Debug.Log($"✅ Encontrado waitingTextTMP: {tmpText.name}");
                        break;
                    }
                }
            }
        }
        
        // Crear elementos UI faltantes si es necesario
        CreateMissingUIElements();
    }
    
    void CreateMissingUIElements()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("⚠️ No se encontró Canvas en la escena");
            return;
        }
        
        // Crear contador de jugadores si no existe
        if (playerCountText == null && playerCountTextTMP == null)
        {
            GameObject countObj = new GameObject("PlayerCountText");
            countObj.transform.SetParent(canvas.transform, false);
            
            Text countText = countObj.AddComponent<Text>();
            countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            countText.fontSize = 24;
            countText.color = Color.white;
            countText.alignment = TextAnchor.UpperRight;
            countText.text = "Jugadores: 0/20";
            
            RectTransform rectTransform = countObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.anchoredPosition = new Vector2(-20, -20);
            rectTransform.sizeDelta = new Vector2(200, 50);
            
            playerCountText = countText;
            Debug.Log("✅ Creado PlayerCountText automáticamente");
        }
        
        // Crear lista de jugadores si no existe
        if (playerListText == null && playerListTextTMP == null)
        {
            GameObject listObj = new GameObject("PlayerListText");
            listObj.transform.SetParent(canvas.transform, false);
            
            Text listText = listObj.AddComponent<Text>();
            listText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            listText.fontSize = 18;
            listText.color = Color.cyan;
            listText.alignment = TextAnchor.UpperLeft;
            listText.text = "Jugadores en sala:\n• Ninguno";
            
            RectTransform rectTransform = listObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(20, -80);
            rectTransform.sizeDelta = new Vector2(300, 400);
            
            playerListText = listText;
            Debug.Log("✅ Creado PlayerListText automáticamente");
        }
    }
    
    void UpdatePlayerInfo()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            Debug.LogWarning("⚠️ No conectado a Photon o no en sala");
            return;
        }
        
        int playerCount = PhotonNetwork.PlayerList.Length;
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        
        // Actualizar texto de espera
        string waitingMessage = $"Esperando a otros jugadores...\n{playerCount}/{maxPlayers} conectados";
        UpdateText(waitingText, waitingTextTMP, waitingMessage);
        
        // Actualizar contador de jugadores
        string countMessage = $"{playerCount} / {maxPlayers}";
        UpdateText(playerCountText, playerCountTextTMP, countMessage);
        
        // Actualizar lista de jugadores
        string playerListMessage = BuildPlayerList();
        UpdateText(playerListText, playerListTextTMP, playerListMessage);
        
        Debug.Log($"🔄 UI actualizada - Jugadores: {playerCount}, MasterClient: {PhotonNetwork.IsMasterClient}");
    }
    
    string BuildPlayerList()
    {
        if (PhotonNetwork.PlayerList == null || PhotonNetwork.PlayerList.Length == 0)
        {
            return "Jugadores en sala:\n• Ninguno";
        }
        
        string playerList = "Jugadores en sala:\n";
        
        // Ordenar jugadores: MasterClient primero
        List<Photon.Realtime.Player> sortedPlayers = new List<Photon.Realtime.Player>(PhotonNetwork.PlayerList);
        sortedPlayers.Sort((p1, p2) => {
            if (p1.IsMasterClient && !p2.IsMasterClient) return -1;
            if (!p1.IsMasterClient && p2.IsMasterClient) return 1;
            return p1.ActorNumber.CompareTo(p2.ActorNumber);
        });
        
        foreach (Photon.Realtime.Player player in sortedPlayers)
        {
            string prefix = player.IsMasterClient ? "👑 " : "👤 ";
            string playerName = string.IsNullOrEmpty(player.NickName) ? $"Player{player.ActorNumber}" : player.NickName;
            
            // Marcar al jugador local
            if (player.IsLocal)
            {
                playerList += $"{prefix}{playerName} (TU)\n";
            }
            else
            {
                playerList += $"{prefix}{playerName}\n";
            }
        }
        
        return playerList;
    }
    
    void UpdateText(Text regularText, TextMeshProUGUI tmpText, string message)
    {
        if (useTextMeshPro && tmpText != null)
        {
            tmpText.text = message;
        }
        else if (regularText != null)
        {
            regularText.text = message;
        }
    }
    
    #region Photon Callbacks
    
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"👤 Jugador entró a WaitingUser: {newPlayer.NickName}");
        UpdatePlayerInfo();
    }
    
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"👋 Jugador salió de WaitingUser: {otherPlayer.NickName}");
        UpdatePlayerInfo();
    }
    
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log($"👑 Nuevo Master Client en WaitingUser: {newMasterClient.NickName}");
        UpdatePlayerInfo();
    }
    
    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // Actualizar si cambian las propiedades del jugador
        UpdatePlayerInfo();
    }
    
    #endregion
} 