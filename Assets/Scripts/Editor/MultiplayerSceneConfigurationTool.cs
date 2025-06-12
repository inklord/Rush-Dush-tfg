using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/// <summary>
/// 🛠️ MULTIPLAYER SCENE CONFIGURATION TOOL
/// Herramienta de editor para configurar automáticamente las escenas para multijugador
/// </summary>
public class MultiplayerSceneConfigurationTool : EditorWindow
{
    private bool showAdvancedOptions = false;
    private string playerPrefabName = "NetworkPlayer";
    
    [MenuItem("Tools/Configure Multiplayer Scenes")]
    public static void ShowWindow()
    {
        GetWindow<MultiplayerSceneConfigurationTool>("Multiplayer Scene Config");
    }

    void OnGUI()
    {
        GUILayout.Label("🎯 MULTIPLAYER SCENE CONFIGURATION", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("Esta herramienta configura automáticamente las escenas Carrera y Hexagonia para que funcionen igual que InGame en multijugador.", MessageType.Info);
        
        GUILayout.Space(10);
        
        // Configuración básica
        GUILayout.Label("⚙️ Configuración", EditorStyles.boldLabel);
        playerPrefabName = EditorGUILayout.TextField("Player Prefab Name:", playerPrefabName);
        
        GUILayout.Space(10);
        
        // Botones principales
        if (GUILayout.Button("🏁 Configurar Escena CARRERA", GUILayout.Height(40)))
        {
            ConfigureScene("Carrera");
        }
        
        if (GUILayout.Button("⬡ Configurar Escena HEXAGONIA", GUILayout.Height(40)))
        {
            ConfigureScene("Hexagonia");
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("🌐 Configurar AMBAS Escenas", GUILayout.Height(40)))
        {
            ConfigureScene("Carrera");
            ConfigureScene("Hexagonia");
        }
        
        GUILayout.Space(20);
        
        // Opciones avanzadas
        showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "🔧 Opciones Avanzadas");
        if (showAdvancedOptions)
        {
            EditorGUI.indentLevel++;
            
            if (GUILayout.Button("🧹 Limpiar Spawners Conflictivos"))
            {
                CleanupCurrentSceneSpawners();
            }
            
            if (GUILayout.Button("📷 Configurar Solo Cámara"))
            {
                SetupCurrentSceneCamera();
            }
            
            if (GUILayout.Button("🎮 Configurar Solo GameManager"))
            {
                SetupCurrentSceneGameManager();
            }
            
            EditorGUI.indentLevel--;
        }
        
        GUILayout.Space(20);
        
        // Estado actual
        GUILayout.Label("📊 Estado Actual", EditorStyles.boldLabel);
        Scene currentScene = EditorSceneManager.GetActiveScene();
        GUILayout.Label($"Escena Activa: {currentScene.name}");
        
        if (IsMultiplayerScene(currentScene.name))
        {
            GUILayout.Label("✅ Esta escena requiere configuración multijugador");
        }
        else
        {
            GUILayout.Label("ℹ️ Esta escena no requiere configuración multijugador");
        }
    }

    /// <summary>
    /// 🛠️ Configurar una escena específica
    /// </summary>
    void ConfigureScene(string sceneName)
    {
        string scenePath = $"Assets/Scenes/{sceneName}.unity";
        
        // Verificar si la escena existe
        if (!System.IO.File.Exists(scenePath))
        {
            EditorUtility.DisplayDialog("Error", $"No se encontró la escena: {scenePath}", "OK");
            return;
        }
        
        // Guardar escena actual si tiene cambios
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            if (EditorUtility.DisplayDialog("Guardar Cambios", 
                "¿Deseas guardar los cambios de la escena actual antes de continuar?", 
                "Guardar", "No Guardar"))
            {
                EditorSceneManager.SaveOpenScenes();
            }
        }
        
        try
        {
            // Abrir la escena
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            Debug.Log($"🛠️ Configurando multijugador para escena: {sceneName}");
            
            // Aplicar configuraciones
            ApplyMultiplayerSetup(sceneName);
            
            // Marcar escena como modificada
            EditorSceneManager.MarkSceneDirty(scene);
            
            // Guardar
            EditorSceneManager.SaveScene(scene);
            
            EditorUtility.DisplayDialog("Éxito", 
                $"Escena '{sceneName}' configurada exitosamente para multijugador!", "OK");
                
            Debug.Log($"✅ Configuración completada para {sceneName}");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", 
                $"Error configurando la escena '{sceneName}': {e.Message}", "OK");
            Debug.LogError($"❌ Error configurando {sceneName}: {e.Message}");
        }
    }

    /// <summary>
    /// 🎯 Aplicar configuración multijugador
    /// </summary>
    void ApplyMultiplayerSetup(string sceneName)
    {
        // 1. Crear objeto de configuración universal
        GameObject setupObject = new GameObject("UniversalMultiplayerSetup");
        UniversalMultiplayerSetup setup = setupObject.AddComponent<UniversalMultiplayerSetup>();
        
        // Configurar según la escena
        setup.playerPrefabName = playerPrefabName;
        setup.autoSetupOnStart = true;
        setup.setupCameraAutomatically = true;
        setup.showDebugInfo = true;
        setup.showDebugGUI = true;
        
        if (sceneName.ToLower().Contains("hexagon"))
        {
            setup.isHexagoniaLevel = true;
            setup.hexagoniaTimerDuration = 180f;
        }
        
        // 2. Configurar spawn points si existen
        SetupSpawnPoints(setup);
        
        // 3. Configurar cámara
        SetupCamera();
        
        // 4. Limpiar spawners problemáticos
        CleanupSpawners();
        
        // 5. Configurar GameManager si no existe
        SetupGameManager(sceneName);
        
        Debug.Log($"🎯 Configuración aplicada a {sceneName}");
    }

    /// <summary>
    /// 📍 Configurar spawn points
    /// </summary>
    void SetupSpawnPoints(UniversalMultiplayerSetup setup)
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
            setup.manualSpawnPoints = spawnPoints;
            Debug.Log($"📍 Configurados {spawnPoints.Length} spawn points existentes");
        }
        else
        {
            // Crear spawn points básicos
            CreateDefaultSpawnPoints();
        }
    }

    /// <summary>
    /// 📍 Crear spawn points por defecto
    /// </summary>
    void CreateDefaultSpawnPoints()
    {
        GameObject spawnParent = new GameObject("SpawnPoints");
        
        // Crear 4 spawn points básicos
        for (int i = 0; i < 4; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
            spawnPoint.transform.parent = spawnParent.transform;
            spawnPoint.tag = "Respawn";
            
            // Posición básica
            spawnPoint.transform.position = new Vector3(i * 5f, 2f, 0f);
        }
        
        Debug.Log("📍 Spawn points por defecto creados");
    }

    /// <summary>
    /// 📷 Configurar cámara
    /// </summary>
    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("⚠️ No se encontró Camera.main");
            return;
        }

        // Agregar MovimientoCamaraSimple si no existe
        if (mainCamera.GetComponent<MovimientoCamaraSimple>() == null)
        {
            mainCamera.gameObject.AddComponent<MovimientoCamaraSimple>();
            Debug.Log("📷 MovimientoCamaraSimple agregado");
        }

        // Eliminar LHS_Camera si existe para evitar conflictos
        LHS_Camera lhsCamera = mainCamera.GetComponent<LHS_Camera>();
        if (lhsCamera != null)
        {
            DestroyImmediate(lhsCamera);
            Debug.Log("🧹 LHS_Camera eliminado para evitar conflictos");
        }
    }

    /// <summary>
    /// 🧹 Limpiar spawners problemáticos
    /// </summary>
    void CleanupSpawners()
    {
        int cleaned = 0;
        
        // Desactivar SimplePlayerSpawner
        SimplePlayerSpawner[] simpleSpawners = FindObjectsOfType<SimplePlayerSpawner>();
        foreach (var spawner in simpleSpawners)
        {
            spawner.enabled = false;
            cleaned++;
        }

        // Desactivar PhotonLauncher
        PhotonLauncher[] launchers = FindObjectsOfType<PhotonLauncher>();
        foreach (var launcher in launchers)
        {
            launcher.enabled = false;
            cleaned++;
        }

        // Desactivar WaitingUserSpawner
        WaitingUserSpawner[] waitingSpawners = FindObjectsOfType<WaitingUserSpawner>();
        foreach (var spawner in waitingSpawners)
        {
            spawner.enabled = false;
            cleaned++;
        }

        if (cleaned > 0)
        {
            Debug.Log($"🧹 {cleaned} spawners problemáticos desactivados");
        }
    }

    /// <summary>
    /// 🎮 Configurar GameManager
    /// </summary>
    void SetupGameManager(string sceneName)
    {
        GameManager existing = FindObjectOfType<GameManager>();
        
        if (existing == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            GameManager gameManager = gmObj.AddComponent<GameManager>();
            
            if (sceneName.ToLower().Contains("hexagon"))
            {
                gameManager.isHexagoniaLevel = true;
                gameManager.hexagoniaTimerDuration = 180f;
            }
            
            Debug.Log($"🎮 GameManager creado para {sceneName}");
        }
        else
        {
            if (sceneName.ToLower().Contains("hexagon") && !existing.isHexagoniaLevel)
            {
                existing.isHexagoniaLevel = true;
                existing.hexagoniaTimerDuration = 180f;
                Debug.Log("🎮 GameManager configurado para Hexagonia");
            }
        }
    }

    /// <summary>
    /// 🧹 Limpiar spawners de la escena actual
    /// </summary>
    void CleanupCurrentSceneSpawners()
    {
        CleanupSpawners();
        EditorUtility.DisplayDialog("Limpieza Completada", 
            "Spawners conflictivos desactivados en la escena actual.", "OK");
    }

    /// <summary>
    /// 📷 Configurar cámara de la escena actual
    /// </summary>
    void SetupCurrentSceneCamera()
    {
        SetupCamera();
        EditorUtility.DisplayDialog("Cámara Configurada", 
            "Sistema de cámara configurado para la escena actual.", "OK");
    }

    /// <summary>
    /// 🎮 Configurar GameManager de la escena actual
    /// </summary>
    void SetupCurrentSceneGameManager()
    {
        Scene currentScene = EditorSceneManager.GetActiveScene();
        SetupGameManager(currentScene.name);
        EditorSceneManager.MarkSceneDirty(currentScene);
        EditorUtility.DisplayDialog("GameManager Configurado", 
            "GameManager configurado para la escena actual.", "OK");
    }

    /// <summary>
    /// ✅ Verificar si la escena es multijugador
    /// </summary>
    bool IsMultiplayerScene(string sceneName)
    {
        string[] multiplayerScenes = { "InGame", "Carrera", "Hexagonia" };
        foreach (string scene in multiplayerScenes)
        {
            if (sceneName.Equals(scene, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
} 