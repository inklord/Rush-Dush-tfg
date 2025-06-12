using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;

/// <summary>
/// 🔧 AUTO SCENE MULTIPLAYER FIXER - Se ejecuta automáticamente en todas las escenas
/// Detecta automáticamente si una escena necesita configuración multijugador y la aplica
/// </summary>
[DefaultExecutionOrder(-100)] // Ejecutar antes que otros scripts
public class AutoSceneMultiplayerFixer : MonoBehaviourPunCallbacks
{
    // Configuración estática para asegurar consistencia
    private static readonly string[] MULTIPLAYER_SCENES = { "InGame", "Carrera", "Hexagonia" };
    private static readonly string PLAYER_PREFAB_NAME = "NetworkPlayer";
    
    // Estado global
    private static bool hasRunInThisScene = false;
    private static string lastProcessedScene = "";
    
    void Awake()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        // Resetear flag si cambiamos de escena
        if (lastProcessedScene != currentScene)
        {
            hasRunInThisScene = false;
            lastProcessedScene = currentScene;
        }
        
        // Solo procesar una vez por escena
        if (hasRunInThisScene)
        {
            return;
        }
        
        if (IsMultiplayerScene(currentScene))
        {
            Debug.Log($"🔧 AutoSceneMultiplayerFixer detectó escena multijugador: {currentScene}");
            
            // 🔍 DETECCIÓN TEMPRANA DE MODO
            bool isLikelyMultiplayer = DetectIfLikelyMultiplayer();
            
            if (!isLikelyMultiplayer)
            {
                Debug.Log($"🎮 Modo singleplayer detectado - Configuración mínima para '{currentScene}'");
                StartCoroutine(SetupSingleplayerSceneDelayed());
            }
            else
            {
                Debug.Log($"🌐 Modo multijugador detectado - Configuración completa para '{currentScene}'");
                StartCoroutine(SetupSceneDelayed());
            }
        }
    }

    /// <summary>
    /// 🔍 Detectar si probablemente estamos en multijugador
    /// </summary>
    bool DetectIfLikelyMultiplayer()
    {
        // Si Photon está conectado, definitivamente multijugador
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            return true;
        }
        
        // Si hay spawners de red activos, probablemente multijugador
        if (FindObjectsOfType<PhotonSpawnController>().Length > 0 ||
            FindObjectsOfType<SimpleFallGuysMultiplayer>().Length > 0 ||
            FindObjectsOfType<PhotonLauncher>().Length > 0)
        {
            return true;
        }
        
        // Si no hay indicios claros, asumir singleplayer por ahora
        return false;
    }

    /// <summary>
    /// 🎮 Setup mínimo para singleplayer con delay
    /// </summary>
    IEnumerator SetupSingleplayerSceneDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        
        string currentScene = SceneManager.GetActiveScene().name;
        
        Debug.Log($"🎮 Aplicando configuración SINGLEPLAYER mínima a '{currentScene}'");
        
        // Solo configurar lo esencial para singleplayer
        SetupCameraSystem();
        SetupGameManager(currentScene);
        CleanupMultiplayerSpawners(); // Limpiar spawners de red innecesarios
        
        hasRunInThisScene = true;
        
        Debug.Log($"✅ AutoSceneMultiplayerFixer SINGLEPLAYER completado para '{currentScene}'");
    }

    /// <summary>
    /// 🧹 Limpiar spawners de multijugador en modo singleplayer
    /// </summary>
    void CleanupMultiplayerSpawners()
    {
        int cleaned = 0;
        
        // En singleplayer, desactivar todos los spawners de red
        PhotonSpawnController[] photonSpawners = FindObjectsOfType<PhotonSpawnController>();
        foreach (var spawner in photonSpawners)
        {
            spawner.enabled = false;
            cleaned++;
            Debug.Log($"🧹 [SP] Desactivado PhotonSpawnController: {spawner.name}");
        }

        SimpleFallGuysMultiplayer[] multiplayers = FindObjectsOfType<SimpleFallGuysMultiplayer>();
        foreach (var mp in multiplayers)
        {
            mp.enabled = false;
            cleaned++;
            Debug.Log($"🧹 [SP] Desactivado SimpleFallGuysMultiplayer: {mp.name}");
        }

        PhotonLauncher[] launchers = FindObjectsOfType<PhotonLauncher>();
        foreach (var launcher in launchers)
        {
            launcher.enabled = false;
            cleaned++;
            Debug.Log($"🧹 [SP] Desactivado PhotonLauncher: {launcher.name}");
        }

        SimplePlayerSpawner[] simpleSpawners = FindObjectsOfType<SimplePlayerSpawner>();
        foreach (var spawner in simpleSpawners)
        {
            spawner.enabled = false;
            cleaned++;
            Debug.Log($"🧹 [SP] Desactivado SimplePlayerSpawner: {spawner.name}");
        }

        WaitingUserSpawner[] waitingSpawners = FindObjectsOfType<WaitingUserSpawner>();
        foreach (var spawner in waitingSpawners)
        {
            spawner.enabled = false;
            cleaned++;
            Debug.Log($"🧹 [SP] Desactivado WaitingUserSpawner: {spawner.name}");
        }

        if (cleaned > 0)
        {
            Debug.Log($"🧹 [SP] Limpieza singleplayer completada - {cleaned} spawners de red desactivados");
        }
    }

    /// <summary>
    /// 🕐 Setup con delay para asegurar que todo esté cargado
    /// </summary>
    IEnumerator SetupSceneDelayed()
    {
        // Esperar que se complete la carga de la escena
        yield return new WaitForSeconds(0.1f);
        
        string currentScene = SceneManager.GetActiveScene().name;
        
        Debug.Log($"🛠️ Aplicando configuración multijugador automática a '{currentScene}'");
        
        // 1. Configurar MasterSpawnController global
        SetupMasterSpawnController();
        
        // 2. Configurar cámara automáticamente
        SetupCameraSystem();
        
        // 3. Limpiar spawners conflictivos
        CleanupConflictingSpawners();
        
        // 4. Configurar GameManager si es necesario
        SetupGameManager(currentScene);
        
        // 5. Crear spawner de respaldo si es necesario
        SetupBackupSpawner();
        
        hasRunInThisScene = true;
        
        Debug.Log($"✅ AutoSceneMultiplayerFixer completado para '{currentScene}'");
    }

    /// <summary>
    /// 🎯 Configurar MasterSpawnController si no existe
    /// </summary>
    void SetupMasterSpawnController()
    {
        // Verificar si ya existe un MasterSpawnController
        MasterSpawnController existing = FindObjectOfType<MasterSpawnController>();
        
        if (existing == null)
        {
            // Crear uno nuevo
            GameObject masterObj = new GameObject("Auto_MasterSpawnController");
            MasterSpawnController master = masterObj.AddComponent<MasterSpawnController>();
            master.playerPrefabName = PLAYER_PREFAB_NAME;
            master.showDebugInfo = true;
            
            // Hacer persistente
            DontDestroyOnLoad(masterObj);
            
            Debug.Log("🎯 MasterSpawnController creado automáticamente");
        }
        else
        {
            Debug.Log("🎯 MasterSpawnController ya existe - OK");
        }
    }

    /// <summary>
    /// 📷 Configurar sistema de cámara
    /// </summary>
    void SetupCameraSystem()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("⚠️ No se encontró Camera.main");
            return;
        }

        // Solo usar MovimientoCamaraSimple
        MovimientoCamaraSimple cameraScript = mainCamera.GetComponent<MovimientoCamaraSimple>();
        if (cameraScript == null)
        {
            cameraScript = mainCamera.gameObject.AddComponent<MovimientoCamaraSimple>();
            Debug.Log("📷 MovimientoCamaraSimple agregado automáticamente");
        }

        // Eliminar LHS_Camera si existe para evitar conflictos
        LHS_Camera lhsCamera = mainCamera.GetComponent<LHS_Camera>();
        if (lhsCamera != null)
        {
            if (Application.isPlaying)
                Destroy(lhsCamera);
            else
                DestroyImmediate(lhsCamera);
            Debug.Log("🧹 [Auto] LHS_Camera eliminado para evitar conflictos");
        }

        // Configurar valores por defecto
        if (cameraScript != null)
        {
            // Asegurar que tiene valores sensatos
            cameraScript.enabled = true;
        }
    }

    /// <summary>
    /// 🧹 Limpiar spawners que pueden causar problemas
    /// </summary>
    void CleanupConflictingSpawners()
    {
        int cleaned = 0;
        
        // Desactivar SimplePlayerSpawner que no sean el master
        SimplePlayerSpawner[] simpleSpawners = FindObjectsOfType<SimplePlayerSpawner>();
        foreach (var spawner in simpleSpawners)
        {
            spawner.enabled = false;
            cleaned++;
            Debug.Log($"🧹 Desactivado SimplePlayerSpawner: {spawner.name}");
        }

        // Desactivar PhotonLauncher
        PhotonLauncher[] launchers = FindObjectsOfType<PhotonLauncher>();
        foreach (var launcher in launchers)
        {
            launcher.enabled = false;
            cleaned++;
            Debug.Log($"🧹 Desactivado PhotonLauncher: {launcher.name}");
        }

        // Desactivar WaitingUserSpawner si estamos en escenas de juego
        WaitingUserSpawner[] waitingSpawners = FindObjectsOfType<WaitingUserSpawner>();
        foreach (var spawner in waitingSpawners)
        {
            spawner.enabled = false;
            cleaned++;
            Debug.Log($"🧹 Desactivado WaitingUserSpawner: {spawner.name}");
        }

        if (cleaned > 0)
        {
            Debug.Log($"🧹 Limpieza completada - {cleaned} spawners problemáticos desactivados");
        }
    }

    /// <summary>
    /// 🎮 Configurar GameManager apropiado
    /// </summary>
    void SetupGameManager(string sceneName)
    {
        GameManager existing = FindObjectOfType<GameManager>();
        
        if (existing == null)
        {
            // Crear GameManager si no existe
            GameObject gmObj = new GameObject("Auto_GameManager");
            GameManager gameManager = gmObj.AddComponent<GameManager>();
            
            // Configurar según escena
            if (sceneName.ToLower().Contains("hexagon"))
            {
                gameManager.isHexagoniaLevel = true;
                gameManager.hexagoniaTimerDuration = 180f;
                Debug.Log("🎮 GameManager configurado para Hexagonia");
            }
            
            Debug.Log($"🎮 GameManager creado automáticamente para '{sceneName}'");
        }
        else
        {
            // Configurar el existente si es necesario
            if (sceneName.ToLower().Contains("hexagon") && !existing.isHexagoniaLevel)
            {
                existing.isHexagoniaLevel = true;
                existing.hexagoniaTimerDuration = 180f;
                Debug.Log("🎮 GameManager existente configurado para Hexagonia");
            }
        }
    }

    /// <summary>
    /// 🛡️ Crear spawner de respaldo si es necesario
    /// </summary>
    void SetupBackupSpawner()
    {
        // Solo crear si no hay ningún spawner activo que funcione
        PhotonSpawnController photonSpawner = FindObjectOfType<PhotonSpawnController>();
        SimpleFallGuysMultiplayer fallGuysMP = FindObjectOfType<SimpleFallGuysMultiplayer>();
        
        if (photonSpawner == null && fallGuysMP == null)
        {
            // Crear PhotonSpawnController de respaldo
            GameObject spawnerObj = new GameObject("Auto_PhotonSpawnController");
            PhotonSpawnController spawner = spawnerObj.AddComponent<PhotonSpawnController>();
            spawner.playerPrefabName = PLAYER_PREFAB_NAME;
            spawner.showDebugInfo = true;
            
            // Configurar spawn points automáticamente
            SetupSpawnPoints(spawner);
            
            Debug.Log("🛡️ PhotonSpawnController de respaldo creado");
        }
    }

    /// <summary>
    /// 📍 Configurar spawn points automáticamente
    /// </summary>
    void SetupSpawnPoints(PhotonSpawnController spawner)
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
            spawner.spawnPoints = spawnPoints;
            Debug.Log($"📍 Configurados {spawnPoints.Length} spawn points automáticamente");
        }
        else
        {
            // Crear spawn points por defecto si no hay ninguno
            CreateDefaultSpawnPoints(spawner);
        }
    }

    /// <summary>
    /// 📍 Crear spawn points por defecto
    /// </summary>
    void CreateDefaultSpawnPoints(PhotonSpawnController spawner)
    {
        GameObject spawnParent = new GameObject("Auto_SpawnPoints");
        Transform[] spawnPoints = new Transform[4]; // 4 jugadores max por defecto
        
        for (int i = 0; i < 4; i++)
        {
            GameObject spawnPoint = new GameObject($"AutoSpawn_{i}");
            spawnPoint.transform.parent = spawnParent.transform;
            spawnPoint.tag = "Respawn";
            
            // Posición en línea con separación
            spawnPoint.transform.position = new Vector3(i * 5f, 2f, 0f);
            spawnPoints[i] = spawnPoint.transform;
        }
        
        spawner.spawnPoints = spawnPoints;
        Debug.Log("📍 Spawn points por defecto creados");
    }

    /// <summary>
    /// ✅ Verificar si la escena requiere configuración multijugador
    /// </summary>
    static bool IsMultiplayerScene(string sceneName)
    {
        foreach (string scene in MULTIPLAYER_SCENES)
        {
            if (sceneName.Equals(scene, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    #region Photon Callbacks

    public override void OnJoinedRoom()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (IsMultiplayerScene(currentScene))
        {
            Debug.Log($"🎮 Entré a sala en escena multijugador '{currentScene}' - Verificando configuración");
            
            // Verificar que todo esté bien configurado
            if (!hasRunInThisScene)
            {
                StartCoroutine(SetupSceneDelayed());
            }
        }
    }

    #endregion

    #region Debug

    void OnGUI()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (!IsMultiplayerScene(currentScene)) return;

        // Detectar modo actual
        bool isLikelyMP = DetectIfLikelyMultiplayer();
        string modeLabel = isLikelyMP ? "🌐 MULTIJUGADOR" : "🎮 SINGLEPLAYER";

        GUILayout.BeginArea(new Rect(10, Screen.height - 140, 450, 120));
        GUILayout.Box($"🔧 AUTO FIXER - {modeLabel}");
        
        GUILayout.Label($"Escena: {currentScene} {(hasRunInThisScene ? "✅" : "⏳")}");
        GUILayout.Label($"Modo Detectado: {modeLabel}");
        GUILayout.Label($"Photon: {(PhotonNetwork.IsConnected ? "🟢 Conectado" : "🔴 Desconectado")} | Sala: {(PhotonNetwork.InRoom ? "🟢" : "🔴")}");
        
        if (GUILayout.Button("Reconfigurar"))
        {
            hasRunInThisScene = false;
            if (isLikelyMP)
            {
                StartCoroutine(SetupSceneDelayed());
            }
            else
            {
                StartCoroutine(SetupSingleplayerSceneDelayed());
            }
        }
        
        GUILayout.EndArea();
    }

    #endregion
} 