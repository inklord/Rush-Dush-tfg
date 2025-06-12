using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;

/// <summary>
/// 🌐 UNIVERSAL MULTIPLAYER SETUP - Configuración automática para cualquier escena
/// Este script debe ser colocado en las escenas Carrera y Hexagonia para que funcionen igual que InGame
/// </summary>
public class UniversalMultiplayerSetup : MonoBehaviourPunCallbacks
{
    [Header("🎯 Configuración Universal")]
    [Tooltip("Se aplicará automáticamente al cargar la escena")]
    public bool autoSetupOnStart = true;
    
    [Header("🎮 Configuración de Spawn")]
    public string playerPrefabName = "NetworkPlayer";
    public Transform[] manualSpawnPoints;
    
    [Header("📷 Configuración de Cámara")]
    public bool setupCameraAutomatically = true;
    public Camera targetCamera;
    
    [Header("🎮 Configuración de GameManager")]
    public bool isHexagoniaLevel = false;
    public float hexagoniaTimerDuration = 180f;
    
    [Header("🔧 Debug")]
    public bool showDebugGUI = false;
    public bool showDebugInfo = false;
    
    // Estado interno
    private bool setupCompleted = false;
    private string currentSceneName;

    void Awake()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        
        // Auto-detectar si es Hexagonia
        if (currentSceneName.ToLower().Contains("hexagon"))
        {
            isHexagoniaLevel = true;
        }
        
        Debug.Log($"🌐 UniversalMultiplayerSetup iniciado en '{currentSceneName}'");
    }

    void Start()
    {
        if (autoSetupOnStart)
        {
            StartCoroutine(SetupMultiplayerWithDelay());
        }
    }

    /// <summary>
    /// 🕐 Configurar multijugador con delay
    /// </summary>
    IEnumerator SetupMultiplayerWithDelay()
    {
        // Esperar que la escena se estabilice
        yield return new WaitForSeconds(0.2f);
        
        SetupMultiplayer();
    }

    /// <summary>
    /// 🛠️ Configurar multijugador completo
    /// </summary>
    [ContextMenu("Setup Multiplayer")]
    public void SetupMultiplayer()
    {
        if (setupCompleted)
        {
            Debug.Log("✅ Setup multijugador ya completado");
            return;
        }

        // 🔍 DETECCIÓN INTELIGENTE DE MODO
        bool isMultiplayerMode = DetectMultiplayerMode();
        bool isSingleplayerMode = !isMultiplayerMode;

        if (isSingleplayerMode)
        {
            Debug.Log($"🎮 Modo SINGLEPLAYER detectado en '{currentSceneName}' - Configuración mínima");
            SetupSingleplayerMode();
        }
        else
        {
            Debug.Log($"🌐 Modo MULTIJUGADOR detectado en '{currentSceneName}' - Configuración completa");
            SetupMultiplayerMode();
        }

        setupCompleted = true;
        Debug.Log($"✅ Configuración completada para '{currentSceneName}' (Modo: {(isMultiplayerMode ? "Multijugador" : "Singleplayer")})");
    }

    /// <summary>
    /// 🔍 Detectar si estamos en modo multijugador
    /// </summary>
    bool DetectMultiplayerMode()
    {
        // Si ya estamos conectados a Photon, definitivamente es multijugador
        if (PhotonNetwork.IsConnected)
        {
            return true;
        }

        // Si tenemos spawners multijugador activos, probablemente es multijugador
        if (FindObjectsOfType<PhotonSpawnController>().Length > 0 ||
            FindObjectsOfType<SimpleFallGuysMultiplayer>().Length > 0)
        {
            return true;
        }

        // Si hay múltiples jugadores en escena, probablemente es multijugador
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 1)
        {
            return true;
        }

        // Si no hay indicios de multijugador, asumir singleplayer
        return false;
    }

    /// <summary>
    /// 🎮 Configuración mínima para singleplayer
    /// </summary>
    void SetupSingleplayerMode()
    {
        try
        {
            // 1. Configurar solo cámara (lo más importante)
            SetupCameraSystem();
            
            // 2. Configurar GameManager básico
            SetupGameManager();
            
            // 3. Limpiar spawners multijugador innecesarios
            CleanupMultiplayerOnlySpawners();
            
            // 4. NO crear MasterSpawnController ni spawners de red
            Debug.Log("🎮 Configuración singleplayer aplicada - Solo cámara y GameManager");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error configurando singleplayer: {e.Message}");
        }
    }

    /// <summary>
    /// 🌐 Configuración completa para multijugador
    /// </summary>
    void SetupMultiplayerMode()
    {
        try
        {
            // 1. Configurar MasterSpawnController
            SetupMasterSpawnController();
            
            // 2. Configurar sistema de cámara
            SetupCameraSystem();
            
            // 3. Configurar GameManager
            SetupGameManager();
            
            // 4. Limpiar spawners conflictivos
            CleanupConflictingSpawners();
            
            // 5. Configurar spawner principal
            SetupMainSpawner();
            
            Debug.Log("🌐 Configuración multijugador completa aplicada");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error configurando multijugador: {e.Message}");
        }
    }

    /// <summary>
    /// 🧹 Limpiar solo spawners de multijugador en modo singleplayer
    /// </summary>
    void CleanupMultiplayerOnlySpawners()
    {
        int disabledCount = 0;

        // Desactivar spawners que solo funcionan en multijugador
        PhotonSpawnController[] photonSpawners = FindObjectsOfType<PhotonSpawnController>();
        foreach (var spawner in photonSpawners)
        {
            spawner.enabled = false;
            disabledCount++;
            Debug.Log($"🧹 [SP] Desactivado PhotonSpawnController: {spawner.name}");
        }

        SimpleFallGuysMultiplayer[] fallGuysMP = FindObjectsOfType<SimpleFallGuysMultiplayer>();
        foreach (var mp in fallGuysMP)
        {
            mp.enabled = false;
            disabledCount++;
            Debug.Log($"🧹 [SP] Desactivado SimpleFallGuysMultiplayer: {mp.name}");
        }

        PhotonLauncher[] launchers = FindObjectsOfType<PhotonLauncher>();
        foreach (var launcher in launchers)
        {
            launcher.enabled = false;
            disabledCount++;
            Debug.Log($"🧹 [SP] Desactivado PhotonLauncher: {launcher.name}");
        }

        WaitingUserSpawner[] waitingSpawners = FindObjectsOfType<WaitingUserSpawner>();
        foreach (var spawner in waitingSpawners)
        {
            spawner.enabled = false;
            disabledCount++;
            Debug.Log($"🧹 [SP] Desactivado WaitingUserSpawner: {spawner.name}");
        }

        if (disabledCount > 0)
        {
            Debug.Log($"🧹 [SP] {disabledCount} spawners multijugador desactivados para singleplayer");
        }
    }

    /// <summary>
    /// 🎯 Configurar MasterSpawnController
    /// </summary>
    void SetupMasterSpawnController()
    {
        MasterSpawnController existing = FindObjectOfType<MasterSpawnController>();
        
        if (existing == null)
        {
            // Crear MasterSpawnController
            GameObject masterObj = new GameObject("MasterSpawnController");
            MasterSpawnController master = masterObj.AddComponent<MasterSpawnController>();
            master.playerPrefabName = playerPrefabName;
            master.showDebugInfo = showDebugInfo;
            
            // Configurar para persistir entre escenas
            DontDestroyOnLoad(masterObj);
            
            Debug.Log("🎯 MasterSpawnController creado y configurado");
        }
        else
        {
            Debug.Log("🎯 MasterSpawnController ya existe");
            
            // Asegurar configuración correcta
            existing.playerPrefabName = playerPrefabName;
        }
    }

    /// <summary>
    /// 📷 Configurar sistema de cámara
    /// </summary>
    void SetupCameraSystem()
    {
        if (!setupCameraAutomatically) return;

        // Buscar cámara objetivo
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogWarning("⚠️ No se encontró cámara para configurar");
            return;
        }

        // Solo usar MovimientoCamaraSimple
        MovimientoCamaraSimple cameraMovement = targetCamera.GetComponent<MovimientoCamaraSimple>();
        if (cameraMovement == null)
        {
            cameraMovement = targetCamera.gameObject.AddComponent<MovimientoCamaraSimple>();
            Debug.Log("📷 MovimientoCamaraSimple agregado a la cámara");
        }

        // Eliminar LHS_Camera si existe para evitar conflictos
        LHS_Camera lhsCamera = targetCamera.GetComponent<LHS_Camera>();
        if (lhsCamera != null)
        {
            if (Application.isPlaying)
                Destroy(lhsCamera);
            else
                DestroyImmediate(lhsCamera);
            Debug.Log("🧹 LHS_Camera eliminado para evitar conflictos");
        }

        // Configurar parámetros por defecto
        if (cameraMovement != null)
        {
            // Los valores por defecto del script son adecuados
            cameraMovement.enabled = true;
        }

        Debug.Log("📷 Sistema de cámara configurado");
    }

    /// <summary>
    /// 🎮 Configurar GameManager
    /// </summary>
    void SetupGameManager()
    {
        GameManager existing = FindObjectOfType<GameManager>();
        
        if (existing == null)
        {
            // Crear GameManager
            GameObject gmObj = new GameObject("GameManager");
            GameManager gameManager = gmObj.AddComponent<GameManager>();
            
            // Configurar para Hexagonia si es necesario
            gameManager.isHexagoniaLevel = isHexagoniaLevel;
            gameManager.hexagoniaTimerDuration = hexagoniaTimerDuration;
            gameManager.enableDebugLogs = showDebugInfo;
            
            Debug.Log($"🎮 GameManager creado (Hexagonia: {isHexagoniaLevel})");
        }
        else
        {
            // Configurar el existente
            if (isHexagoniaLevel && !existing.isHexagoniaLevel)
            {
                existing.isHexagoniaLevel = true;
                existing.hexagoniaTimerDuration = hexagoniaTimerDuration;
                Debug.Log("🎮 GameManager configurado para Hexagonia");
            }
        }
    }

    /// <summary>
    /// 🧹 Limpiar spawners conflictivos
    /// </summary>
    void CleanupConflictingSpawners()
    {
        int disabledCount = 0;

        // Desactivar SimplePlayerSpawner
        SimplePlayerSpawner[] simpleSpawners = FindObjectsOfType<SimplePlayerSpawner>();
        foreach (var spawner in simpleSpawners)
        {
            spawner.enabled = false;
            disabledCount++;
            Debug.Log($"🧹 Desactivado SimplePlayerSpawner: {spawner.name}");
        }

        // Desactivar PhotonLauncher
        PhotonLauncher[] launchers = FindObjectsOfType<PhotonLauncher>();
        foreach (var launcher in launchers)
        {
            launcher.enabled = false;
            disabledCount++;
            Debug.Log($"🧹 Desactivado PhotonLauncher: {launcher.name}");
        }

        // Desactivar WaitingUserSpawner
        WaitingUserSpawner[] waitingSpawners = FindObjectsOfType<WaitingUserSpawner>();
        foreach (var spawner in waitingSpawners)
        {
            spawner.enabled = false;
            disabledCount++;
            Debug.Log($"🧹 Desactivado WaitingUserSpawner: {spawner.name}");
        }

        // Desactivar GuaranteedPlayerSpawner
        GuaranteedPlayerSpawner[] guaranteedSpawners = FindObjectsOfType<GuaranteedPlayerSpawner>();
        foreach (var spawner in guaranteedSpawners)
        {
            spawner.enabled = false;
            disabledCount++;
            Debug.Log($"🧹 Desactivado GuaranteedPlayerSpawner: {spawner.name}");
        }

        if (disabledCount > 0)
        {
            Debug.Log($"🧹 {disabledCount} spawners conflictivos desactivados");
        }
    }

    /// <summary>
    /// 🎯 Configurar spawner principal
    /// </summary>
    void SetupMainSpawner()
    {
        // Verificar si ya existe un spawner compatible
        PhotonSpawnController photonSpawner = FindObjectOfType<PhotonSpawnController>();
        SimpleFallGuysMultiplayer fallGuysMP = FindObjectOfType<SimpleFallGuysMultiplayer>();

        if (photonSpawner == null && fallGuysMP == null)
        {
            // Crear PhotonSpawnController
            GameObject spawnerObj = new GameObject("PhotonSpawnController");
            PhotonSpawnController spawner = spawnerObj.AddComponent<PhotonSpawnController>();
            spawner.playerPrefabName = playerPrefabName;
            spawner.showDebugInfo = showDebugInfo;

            // Configurar spawn points
            SetupSpawnPoints(spawner);

            Debug.Log("🎯 PhotonSpawnController principal creado");
        }
        else
        {
            Debug.Log($"🎯 Spawner compatible ya existe: {(photonSpawner != null ? "PhotonSpawnController" : "SimpleFallGuysMultiplayer")}");
        }
    }

    /// <summary>
    /// 📍 Configurar spawn points
    /// </summary>
    void SetupSpawnPoints(PhotonSpawnController spawner)
    {
        Transform[] spawnPoints = null;

        // Usar spawn points manuales si están configurados
        if (manualSpawnPoints != null && manualSpawnPoints.Length > 0)
        {
            spawnPoints = manualSpawnPoints;
            Debug.Log($"📍 Usando {spawnPoints.Length} spawn points manuales");
        }
        else
        {
            // Buscar spawn points automáticamente
            spawnPoints = FindSpawnPointsAutomatically();
        }

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            spawner.spawnPoints = spawnPoints;
            Debug.Log($"📍 {spawnPoints.Length} spawn points configurados");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontraron spawn points - usando posiciones por defecto");
        }
    }

    /// <summary>
    /// 📍 Buscar spawn points automáticamente
    /// </summary>
    Transform[] FindSpawnPointsAutomatically()
    {
        // Buscar objetos con tag Respawn
        GameObject[] respawnObjects = GameObject.FindGameObjectsWithTag("Respawn");
        
        if (respawnObjects.Length > 0)
        {
            Transform[] spawnPoints = new Transform[respawnObjects.Length];
            for (int i = 0; i < respawnObjects.Length; i++)
            {
                spawnPoints[i] = respawnObjects[i].transform;
            }
            Debug.Log($"📍 Encontrados {spawnPoints.Length} objetos con tag 'Respawn'");
            return spawnPoints;
        }

        // Buscar por nombres que sugieran spawn points
        Transform[] allTransforms = FindObjectsOfType<Transform>();
        System.Collections.Generic.List<Transform> foundSpawns = new System.Collections.Generic.List<Transform>();

        foreach (Transform t in allTransforms)
        {
            string name = t.name.ToLower();
            if (name.Contains("spawn") || name.Contains("start") || (name.Contains("player") && name.Contains("start")))
            {
                foundSpawns.Add(t);
            }
        }

        if (foundSpawns.Count > 0)
        {
            Debug.Log($"📍 Encontrados {foundSpawns.Count} spawn points por nombre");
            return foundSpawns.ToArray();
        }

        Debug.LogWarning("📍 No se encontraron spawn points automáticamente");
        return null;
    }

    /// <summary>
    /// 🔄 Reconfigurar todo
    /// </summary>
    [ContextMenu("Reconfigure All")]
    public void ReconfigureAll()
    {
        setupCompleted = false;
        SetupMultiplayer();
    }

    #region Photon Callbacks

    public override void OnJoinedRoom()
    {
        Debug.Log($"🎮 Entré a la sala en '{currentSceneName}' - Verificando configuración");
        
        if (!setupCompleted)
        {
            StartCoroutine(SetupMultiplayerWithDelay());
        }
    }

    #endregion

    #region Debug GUI

    void OnGUI()
    {
        if (!showDebugGUI) return;

        // Detectar modo actual
        bool isCurrentlyMultiplayer = DetectMultiplayerMode();
        string currentMode = isCurrentlyMultiplayer ? "🌐 MULTIJUGADOR" : "🎮 SINGLEPLAYER";

        // Posición en la esquina superior derecha
        float width = 320f;
        float height = 200f;
        Rect windowRect = new Rect(Screen.width - width - 10, 10, width, height);

        GUILayout.BeginArea(windowRect);
        GUILayout.Box($"🎯 UNIVERSAL SETUP - {currentMode}");

        GUILayout.Label($"Escena: {currentSceneName}");
        GUILayout.Label($"Modo: {currentMode}");
        GUILayout.Label($"Setup: {(setupCompleted ? "✅ Completado" : "⏳ Pendiente")}");
        GUILayout.Label($"Hexagonia: {(isHexagoniaLevel ? "✅" : "❌")}");
        
        if (PhotonNetwork.IsConnected)
        {
            GUILayout.Label($"Photon: 🟢 Conectado");
            GUILayout.Label($"Sala: {(PhotonNetwork.InRoom ? "🟢 En sala" : "🔴 Sin sala")}");
        }
        else
        {
            GUILayout.Label("Photon: 🔴 Desconectado (Singleplayer)");
        }

        if (GUILayout.Button("Reconfigurar"))
        {
            ReconfigureAll();
        }

        if (GUILayout.Button(showDebugInfo ? "Ocultar Debug" : "Mostrar Debug"))
        {
            showDebugInfo = !showDebugInfo;
        }

        GUILayout.EndArea();
    }

    #endregion
} 