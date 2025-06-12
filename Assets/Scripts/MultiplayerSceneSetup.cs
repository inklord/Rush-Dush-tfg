using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

/// <summary>
/// 🎯 MULTIPLAYER SCENE SETUP - Configuración automática para escenas
/// Asegura que Carrera y Hexagonia tengan la misma funcionalidad multijugador que InGame
/// </summary>
public class MultiplayerSceneSetup : MonoBehaviourPunCallbacks
{
    [Header("🎮 Configuración Automática")]
    public bool setupOnStart = true;
    public bool forceCreateComponents = true;
    
    [Header("🔧 Componentes Requeridos")]
    public string playerPrefabName = "NetworkPlayer";
    public Transform[] spawnPoints;
    
    [Header("📊 Debug")]
    public bool showDebugInfo = true;
    
    // Estado del setup
    private bool isSetupComplete = false;
    private string currentSceneName;

    void Awake()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"🎯 MultiplayerSceneSetup iniciado en escena: {currentSceneName}");
    }

    void Start()
    {
        if (setupOnStart)
        {
            SetupMultiplayerScene();
        }
    }

    /// <summary>
    /// 🛠️ Configurar escena para multijugador
    /// </summary>
    public void SetupMultiplayerScene()
    {
        if (isSetupComplete)
        {
            Debug.Log("✅ Setup ya completado para esta escena");
            return;
        }

        Debug.Log($"🛠️ Configurando escena '{currentSceneName}' para multijugador...");

        // 1. Asegurar que existe MasterSpawnController
        EnsureMasterSpawnController();

        // 2. Configurar spawn points automáticamente
        SetupSpawnPoints();

        // 3. Asegurar que existe GameManager apropiado
        EnsureGameManager();

        // 4. Configurar cámara para multijugador
        SetupCamera();

        // 5. Limpiar spawners problemáticos
        CleanupConflictingSpawners();

        // 6. Configurar UI específica de la escena
        SetupSceneSpecificUI();

        isSetupComplete = true;
        Debug.Log($"✅ Setup multijugador completado para '{currentSceneName}'");
    }

    /// <summary>
    /// 🎯 Asegurar que existe MasterSpawnController
    /// </summary>
    void EnsureMasterSpawnController()
    {
        MasterSpawnController existing = FindObjectOfType<MasterSpawnController>();
        
        if (existing == null && forceCreateComponents)
        {
            GameObject masterSpawnerObj = new GameObject("MasterSpawnController");
            MasterSpawnController masterSpawner = masterSpawnerObj.AddComponent<MasterSpawnController>();
            masterSpawner.playerPrefabName = playerPrefabName;
            masterSpawner.showDebugInfo = showDebugInfo;
            
            Debug.Log("🎯 MasterSpawnController creado automáticamente");
        }
        else if (existing != null)
        {
            Debug.Log("🎯 MasterSpawnController ya existe");
        }
    }

    /// <summary>
    /// 📍 Configurar puntos de spawn automáticamente
    /// </summary>
    void SetupSpawnPoints()
    {
        // Buscar puntos de spawn existentes
        GameObject[] respawnObjects = GameObject.FindGameObjectsWithTag("Respawn");
        Transform[] playerStarts = FindObjectsOfType<Transform>();
        
        // Filtrar transforms que contengan "spawn" o "start" en el nombre
        var validSpawnPoints = new System.Collections.Generic.List<Transform>();
        
        // Agregar objetos con tag Respawn
        foreach (GameObject respawn in respawnObjects)
        {
            validSpawnPoints.Add(respawn.transform);
        }
        
        // Buscar objetos con nombres que sugieran spawn points
        foreach (Transform t in playerStarts)
        {
            string name = t.name.ToLower();
            if ((name.Contains("spawn") || name.Contains("start") || name.Contains("player")) 
                && !validSpawnPoints.Contains(t))
            {
                validSpawnPoints.Add(t);
            }
        }
        
        // Si no hay spawn points, crear algunos por defecto
        if (validSpawnPoints.Count == 0)
        {
            CreateDefaultSpawnPoints();
        }
        else
        {
            spawnPoints = validSpawnPoints.ToArray();
            Debug.Log($"📍 Encontrados {spawnPoints.Length} puntos de spawn");
        }
    }

    /// <summary>
    /// 📍 Crear puntos de spawn por defecto
    /// </summary>
    void CreateDefaultSpawnPoints()
    {
        GameObject spawnParent = new GameObject("Auto_SpawnPoints");
        var spawnList = new System.Collections.Generic.List<Transform>();
        
        // Crear 8 spawn points en círculo
        for (int i = 0; i < 8; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
            spawnPoint.transform.parent = spawnParent.transform;
            spawnPoint.tag = "Respawn";
            
            // Posición en círculo
            float angle = i * (360f / 8f) * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * 10f,
                2f, // Altura segura
                Mathf.Sin(angle) * 10f
            );
            
            spawnPoint.transform.position = position;
            spawnList.Add(spawnPoint.transform);
        }
        
        spawnPoints = spawnList.ToArray();
        Debug.Log($"📍 Creados {spawnPoints.Length} puntos de spawn por defecto");
    }

    /// <summary>
    /// 🎮 Asegurar GameManager apropiado
    /// </summary>
    void EnsureGameManager()
    {
        GameManager existing = FindObjectOfType<GameManager>();
        
        if (existing == null && forceCreateComponents)
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            GameManager gameManager = gameManagerObj.AddComponent<GameManager>();
            
            // Configurar según la escena
            if (currentSceneName.ToLower().Contains("hexagon"))
            {
                gameManager.isHexagoniaLevel = true;
                gameManager.hexagoniaTimerDuration = 180f;
            }
            
            Debug.Log($"🎮 GameManager creado para '{currentSceneName}'");
        }
        else if (existing != null)
        {
            // Configurar el existente según la escena
            if (currentSceneName.ToLower().Contains("hexagon") && !existing.isHexagoniaLevel)
            {
                existing.isHexagoniaLevel = true;
                existing.hexagoniaTimerDuration = 180f;
                Debug.Log("🎮 GameManager configurado para Hexagonia");
            }
        }
    }

    /// <summary>
    /// 📷 Configurar cámara para multijugador
    /// </summary>
    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("⚠️ No se encontró Camera.main");
            return;
        }

        // Asegurar que tiene los componentes necesarios para seguir al jugador
        MovimientoCamaraSimple cameraScript = mainCamera.GetComponent<MovimientoCamaraSimple>();
        if (cameraScript == null && forceCreateComponents)
        {
            cameraScript = mainCamera.gameObject.AddComponent<MovimientoCamaraSimple>();
            Debug.Log("📷 MovimientoCamaraSimple agregado a la cámara");
        }

        // Eliminar LHS_Camera si existe para evitar conflictos
        LHS_Camera lhsCamera = mainCamera.GetComponent<LHS_Camera>();
        if (lhsCamera != null)
        {
            if (Application.isPlaying)
                Destroy(lhsCamera);
            else
                DestroyImmediate(lhsCamera);
            Debug.Log("🧹 LHS_Camera eliminado para evitar conflictos");
        }
    }

    /// <summary>
    /// 🧹 Limpiar spawners que pueden causar conflictos
    /// </summary>
    void CleanupConflictingSpawners()
    {
        // Buscar y desactivar spawners problemáticos
        SimplePlayerSpawner[] simpleSpawners = FindObjectsOfType<SimplePlayerSpawner>();
        foreach (var spawner in simpleSpawners)
        {
            if (spawner.gameObject != gameObject)
            {
                spawner.enabled = false;
                Debug.Log($"🧹 Desactivado SimplePlayerSpawner: {spawner.name}");
            }
        }

        PhotonLauncher[] photonLaunchers = FindObjectsOfType<PhotonLauncher>();
        foreach (var launcher in photonLaunchers)
        {
            launcher.enabled = false;
            Debug.Log($"🧹 Desactivado PhotonLauncher: {launcher.name}");
        }

        // Permitir que estos funcionen con MasterSpawnController
        PhotonSpawnController[] photonSpawners = FindObjectsOfType<PhotonSpawnController>();
        SimpleFallGuysMultiplayer[] multiplayers = FindObjectsOfType<SimpleFallGuysMultiplayer>();
        
        Debug.Log($"🔧 Manteniendo activos: {photonSpawners.Length} PhotonSpawnController, {multiplayers.Length} SimpleFallGuysMultiplayer");
    }

    /// <summary>
    /// 🎨 Configurar UI específica de la escena
    /// </summary>
    void SetupSceneSpecificUI()
    {
        switch (currentSceneName.ToLower())
        {
            case "carrera":
                SetupCarreraUI();
                break;
            case "hexagonia":
                SetupHexagoniaUI();
                break;
            default:
                Debug.Log($"🎨 No hay UI específica para '{currentSceneName}'");
                break;
        }
    }

    /// <summary>
    /// 🏁 Configurar UI específica de Carrera
    /// </summary>
    void SetupCarreraUI()
    {
        // Buscar o crear UI de timer/contador para carreras
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            // Crear canvas básico si no existe
            canvas = new GameObject("Canvas");
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        Debug.Log("🏁 UI de Carrera configurada");
    }

    /// <summary>
    /// ⬡ Configurar UI específica de Hexagonia
    /// </summary>
    void SetupHexagoniaUI()
    {
        // Buscar o crear UI de timer específica para Hexagonia
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            // Crear canvas básico si no existe
            canvas = new GameObject("Canvas");
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Buscar GameManager y configurar UI del timer
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null && gameManager.hexagoniaTimerText == null)
        {
            // Buscar text existente o crear uno nuevo
            UnityEngine.UI.Text[] texts = FindObjectsOfType<UnityEngine.UI.Text>();
            foreach (var text in texts)
            {
                if (text.name.ToLower().Contains("timer") || text.name.ToLower().Contains("time"))
                {
                    gameManager.hexagoniaTimerText = text;
                    break;
                }
            }
        }

        Debug.Log("⬡ UI de Hexagonia configurada");
    }

    /// <summary>
    /// 🔄 Reconfigurar escena si es necesario
    /// </summary>
    public void ReconfigureScene()
    {
        isSetupComplete = false;
        SetupMultiplayerScene();
    }

    /// <summary>
    /// 📊 Debug GUI
    /// </summary>
    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 300, 200));
        GUILayout.Box("🎯 MULTIPLAYER SCENE SETUP");
        
        GUILayout.Label($"Escena: {currentSceneName}");
        GUILayout.Label($"Setup Completo: {(isSetupComplete ? "✅" : "❌")}");
        GUILayout.Label($"Conectado: {PhotonNetwork.IsConnected}");
        GUILayout.Label($"En Sala: {PhotonNetwork.InRoom}");
        
        if (spawnPoints != null)
        {
            GUILayout.Label($"Spawn Points: {spawnPoints.Length}");
        }
        
        if (GUILayout.Button("Reconfigurar"))
        {
            ReconfigureScene();
        }
        
        GUILayout.EndArea();
    }

    #region Photon Callbacks

    public override void OnJoinedRoom()
    {
        Debug.Log("🎮 Entré a la sala - Verificando setup de escena");
        if (!isSetupComplete)
        {
            SetupMultiplayerScene();
        }
    }

    #endregion
} 