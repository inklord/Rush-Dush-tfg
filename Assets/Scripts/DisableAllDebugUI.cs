using UnityEngine;

/// <summary>
/// 🧹 DISABLE ALL DEBUG UI - Desactiva automáticamente toda la información de debug en pantalla
/// </summary>
public class DisableAllDebugUI : MonoBehaviour
{
    [Header("🧹 Configuración")]
    public bool executeOnStart = true;
    public bool showProgress = true;

    void Start()
    {
        if (executeOnStart)
        {
            DisableAllDebug();
            
            // Auto-destruirse después de la limpieza
            Destroy(gameObject);
        }

        // Desactivar debug UI en HexagoniaGameManager
        HexagoniaGameManager hexagoniaManager = FindObjectOfType<HexagoniaGameManager>();
        if (hexagoniaManager != null)
        {
            hexagoniaManager.enableDebugLogs = false;
        }

        // Desactivar otros debug UIs según sea necesario
        // ...
    }

    [ContextMenu("Disable All Debug")]
    public void DisableAllDebug()
    {
        if (showProgress) Debug.Log("🧹 Desactivando toda la información de debug en pantalla...");
        
        int totalDisabled = 0;

        // Desactivar debug en LHS_MainPlayer
        LHS_MainPlayer[] players = FindObjectsOfType<LHS_MainPlayer>();
        foreach (var player in players)
        {
            if (player.showDebugInfo)
            {
                player.showDebugInfo = false;
                totalDisabled++;
                if (showProgress) Debug.Log($"✅ Debug desactivado en LHS_MainPlayer: {player.name}");
            }
        }

        // Desactivar debug en MovimientoCamaraSimple
        MovimientoCamaraSimple[] cameras = FindObjectsOfType<MovimientoCamaraSimple>();
        foreach (var camera in cameras)
        {
            if (camera.showDebugInfo)
            {
                camera.showDebugInfo = false;
                totalDisabled++;
                if (showProgress) Debug.Log($"✅ Debug desactivado en MovimientoCamaraSimple: {camera.name}");
            }
        }

        // Desactivar debug en MasterSpawnController
        MasterSpawnController[] spawners = FindObjectsOfType<MasterSpawnController>();
        foreach (var spawner in spawners)
        {
            if (spawner.showDebugInfo)
            {
                spawner.showDebugInfo = false;
                totalDisabled++;
                if (showProgress) Debug.Log($"✅ Debug desactivado en MasterSpawnController: {spawner.name}");
            }
        }

        // Desactivar debug en SimplePlayerSpawner
        SimplePlayerSpawner[] simpleSpawners = FindObjectsOfType<SimplePlayerSpawner>();
        foreach (var spawner in simpleSpawners)
        {
            if (spawner.showDebugInfo)
            {
                spawner.showDebugInfo = false;
                totalDisabled++;
                if (showProgress) Debug.Log($"✅ Debug desactivado en SimplePlayerSpawner: {spawner.name}");
            }
        }

        // Desactivar debug en CarreraForceMultiplayerFix
        CarreraForceMultiplayerFix[] carreraFixes = FindObjectsOfType<CarreraForceMultiplayerFix>();
        foreach (var fix in carreraFixes)
        {
            if (fix.showDebug)
            {
                fix.showDebug = false;
                totalDisabled++;
                if (showProgress) Debug.Log($"✅ Debug desactivado en CarreraForceMultiplayerFix: {fix.name}");
            }
        }

        // Desactivar debug en GameManager
        GameManager[] gameManagers = FindObjectsOfType<GameManager>();
        foreach (var gm in gameManagers)
        {
            if (gm.enableDebugLogs)
            {
                gm.enableDebugLogs = false;
                totalDisabled++;
                if (showProgress) Debug.Log($"✅ Debug desactivado en GameManager: {gm.name}");
            }
        }

        // Desactivar debug en PersistentSettingsManager
        PersistentSettingsManager[] settingsManagers = FindObjectsOfType<PersistentSettingsManager>();
        foreach (var sm in settingsManagers)
        {
            if (sm.enableDebugLogs || sm.showDebugUI)
            {
                sm.enableDebugLogs = false;
                sm.showDebugUI = false;
                totalDisabled++;
                if (showProgress) Debug.Log($"✅ Debug desactivado en PersistentSettingsManager: {sm.name}");
            }
        }

        // Desactivar debug en UniversalMultiplayerSetup
        UniversalMultiplayerSetup[] universalSetups = FindObjectsOfType<UniversalMultiplayerSetup>();
        foreach (var setup in universalSetups)
        {
            if (setup.showDebugGUI || setup.showDebugInfo)
            {
                setup.showDebugGUI = false;
                setup.showDebugInfo = false;
                totalDisabled++;
                if (showProgress) Debug.Log($"✅ Debug desactivado en UniversalMultiplayerSetup: {setup.name}");
            }
        }

        Debug.Log($"🧹 Limpieza de debug completada: {totalDisabled} componentes limpiados");
        
        if (totalDisabled == 0)
        {
            Debug.Log("✅ No se encontraron componentes con debug activo");
        }
    }

    void OnGUI()
    {
        if (!executeOnStart)
        {
            GUI.Box(new Rect(10, 10, 300, 80), "🧹 DISABLE ALL DEBUG UI");
            GUI.Label(new Rect(20, 35, 280, 20), "Desactiva todos los debug de pantalla");
            
            if (GUI.Button(new Rect(20, 55, 150, 25), "Desactivar Todo"))
            {
                DisableAllDebug();
            }
            
            if (GUI.Button(new Rect(180, 55, 100, 25), "Cerrar"))
            {
                Destroy(gameObject);
            }
        }
    }
} 