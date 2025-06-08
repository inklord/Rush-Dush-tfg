using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// 🔧 Solucionador automático de eventos duplicados en el Lobby
/// Limpia eventos conflictivos y configura correctamente los botones
/// </summary>
public class LobbyEventFixer : MonoBehaviour
{
    [Header("🔧 Configuración Automática")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool showDebugInfo = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixLobbyEvents();
        }
    }
    
    [ContextMenu("🔧 Arreglar Eventos del Lobby")]
    public void FixLobbyEvents()
    {
        Debug.Log("🔧 Iniciando reparación de eventos del Lobby...");
        
        // 1. Buscar y limpiar Input Fields problemáticos
        FixInputFieldEvents();
        
        // 2. Buscar y limpiar botones duplicados
        FixButtonEvents();
        
        // 3. Configurar LobbyManager correctamente
        ConfigureLobbyManager();
        
        Debug.Log("✅ Reparación completada!");
    }
    
    void FixInputFieldEvents()
    {
        Debug.Log("🔍 Buscando Input Fields con eventos problemáticos...");
        
        // Buscar todos los TMP Input Fields
        TMP_InputField[] inputFields = FindObjectsOfType<TMP_InputField>();
        
        foreach (var input in inputFields)
        {
            // Limpiar eventos que puedan estar ejecutándose automáticamente
            if (input.onEndEdit.GetPersistentEventCount() > 0)
            {
                Debug.Log($"🧹 Limpiando eventos OnEndEdit de: {input.name}");
                input.onEndEdit.RemoveAllListeners();
            }
            
            if (input.onValueChanged.GetPersistentEventCount() > 0)
            {
                Debug.Log($"🧹 Limpiando eventos OnValueChanged de: {input.name}");
                input.onValueChanged.RemoveAllListeners();
            }
            
            if (input.onSubmit.GetPersistentEventCount() > 0)
            {
                Debug.Log($"🧹 Limpiando eventos OnSubmit de: {input.name}");
                input.onSubmit.RemoveAllListeners();
            }
        }
        
        // También limpiar Input Fields normales (por si acaso)
        InputField[] oldInputFields = FindObjectsOfType<InputField>();
        foreach (var input in oldInputFields)
        {
            input.onEndEdit.RemoveAllListeners();
            input.onValueChanged.RemoveAllListeners();
        }
        
        Debug.Log($"✅ {inputFields.Length + oldInputFields.Length} Input Fields limpiados");
    }
    
    void FixButtonEvents()
    {
        Debug.Log("🔍 Buscando TODOS los botones con eventos...");
        
        // Buscar TODOS los botones del lobby
        Button[] allButtons = FindObjectsOfType<Button>();
        
        foreach (var button in allButtons)
        {
            // Limpiar TODOS los botones que tengan eventos
            if (button.onClick.GetPersistentEventCount() > 0)
            {
                Debug.Log($"🧹 Limpiando eventos de botón: {button.name} ({button.onClick.GetPersistentEventCount()} eventos)");
                
                // Limpiar TODOS los eventos existentes
                button.onClick.RemoveAllListeners();
                
                if (showDebugInfo)
                {
                    Debug.Log($"  - Eventos limpiados en: {button.name}");
                }
            }
        }
        
        Debug.Log($"✅ Todos los eventos de botones limpiados");
    }
    
    void ConfigureLobbyManager()
    {
        Debug.Log("🔍 Buscando y configurando LobbyManager...");
        
        // Buscar el LobbyManager
        LobbyManager lobbyManager = FindObjectOfType<LobbyManager>();
        
        if (lobbyManager == null)
        {
            Debug.LogError("❌ LobbyManager no encontrado! Asegúrate de tener el script en la escena.");
            return;
        }
        
        // Buscar y conectar botones automáticamente
        ConnectButtonsToLobbyManager(lobbyManager);
        
        // Buscar y conectar Input Fields automáticamente
        ConnectInputFieldsToLobbyManager(lobbyManager);
        
        Debug.Log("✅ LobbyManager configurado correctamente");
    }
    
    void ConnectButtonsToLobbyManager(LobbyManager lobbyManager)
    {
        Button[] allButtons = FindObjectsOfType<Button>();
        
        // Variables para asegurar que solo conectamos UN botón por función
        bool createRoomConnected = false;
        bool joinRandomConnected = false;
        bool backToMenuConnected = false;
        bool startGameConnected = false;
        bool multiplayerConnected = false;
        
        foreach (var button in allButtons)
        {
            string buttonName = button.name.ToLower();
            
            // Conectar SOLO EL PRIMER botón de cada tipo
            if (buttonName == "createroombutton" && !createRoomConnected)
            {
                button.onClick.AddListener(lobbyManager.CreateRoom);
                Debug.Log($"  ✅ Conectado: {button.name} → CreateRoom");
                createRoomConnected = true;
            }
            else if (buttonName == "joinrandombutton" && !joinRandomConnected)
            {
                button.onClick.AddListener(lobbyManager.JoinRandomRoom);
                Debug.Log($"  ✅ Conectado: {button.name} → JoinRandomRoom");
                joinRandomConnected = true;
            }
            else if (buttonName == "backtomenubutton" && !backToMenuConnected)
            {
                button.onClick.AddListener(lobbyManager.BackToMainMenu);
                Debug.Log($"  ✅ Conectado: {button.name} → BackToMainMenu");
                backToMenuConnected = true;
            }
            else if (buttonName == "startgamebutton" && !startGameConnected)
            {
                button.onClick.AddListener(lobbyManager.StartMultiplayerGame);
                Debug.Log($"  ✅ Conectado: {button.name} → StartMultiplayerGame");
                startGameConnected = true;
            }
            else if (buttonName == "multiplayerbutton" && !multiplayerConnected)
            {
                button.onClick.AddListener(lobbyManager.OpenMultiplayerMenu);
                Debug.Log($"  ✅ Conectado: {button.name} → OpenMultiplayerMenu");
                multiplayerConnected = true;
            }
            else
            {
                // Mostrar botones duplicados que NO conectamos
                if (showDebugInfo && (buttonName.Contains("create") || buttonName.Contains("join") || 
                    buttonName.Contains("back") || buttonName.Contains("lobby") || buttonName.Contains("multiplayer")))
                {
                    Debug.Log($"  ⚠️ Botón duplicado NO conectado: {button.name} (nombre: '{buttonName}')");
                }
            }
        }
        
        // Mostrar resumen de conexiones
        Debug.Log($"📊 Resumen de conexiones:");
        Debug.Log($"  - CreateRoom: {(createRoomConnected ? "✅" : "❌")}");
        Debug.Log($"  - JoinRandom: {(joinRandomConnected ? "✅" : "❌")}");
        Debug.Log($"  - BackToMenu: {(backToMenuConnected ? "✅" : "❌")}");
        Debug.Log($"  - StartGame: {(startGameConnected ? "✅" : "❌")}");
        Debug.Log($"  - Multiplayer: {(multiplayerConnected ? "✅" : "❌")}");
    }
    
    void ConnectInputFieldsToLobbyManager(LobbyManager lobbyManager)
    {
        // Buscar Input Fields TMP
        TMP_InputField[] tmpInputs = FindObjectsOfType<TMP_InputField>();
        
        foreach (var input in tmpInputs)
        {
            string inputName = input.name.ToLower();
            
            // Conectar Input Fields al LobbyManager usando reflexión
            if (inputName.Contains("player") && inputName.Contains("name"))
            {
                // Conectar playerNameInput
                var field = typeof(LobbyManager).GetField("playerNameInput", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(lobbyManager, input);
                    Debug.Log($"  ✅ Conectado: {input.name} → playerNameInput");
                }
            }
            else if (inputName.Contains("room") && inputName.Contains("name"))
            {
                // Conectar roomNameInput
                var field = typeof(LobbyManager).GetField("roomNameInput", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(lobbyManager, input);
                    Debug.Log($"  ✅ Conectado: {input.name} → roomNameInput");
                }
            }
        }
    }
    
    [ContextMenu("🔍 Diagnosticar Eventos")]
    public void DiagnoseEvents()
    {
        Debug.Log("🔍 === DIAGNÓSTICO DE EVENTOS ===");
        
        // Diagnosticar Input Fields
        TMP_InputField[] inputs = FindObjectsOfType<TMP_InputField>();
        foreach (var input in inputs)
        {
            Debug.Log($"📝 Input Field: {input.name}");
            Debug.Log($"  - OnEndEdit eventos: {input.onEndEdit.GetPersistentEventCount()}");
            Debug.Log($"  - OnValueChanged eventos: {input.onValueChanged.GetPersistentEventCount()}");
            Debug.Log($"  - OnSubmit eventos: {input.onSubmit.GetPersistentEventCount()}");
        }
        
        // Diagnosticar Botones
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (var button in buttons)
        {
            Debug.Log($"🔘 Botón: {button.name}");
            Debug.Log($"  - OnClick eventos: {button.onClick.GetPersistentEventCount()}");
        }
        
        Debug.Log("🔍 === FIN DIAGNÓSTICO ===");
    }
} 