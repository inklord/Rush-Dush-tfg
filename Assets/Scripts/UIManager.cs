using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("⏱️ Timer Configuration")]
    public float limitTime;
    public Text textTimer;
    
    [Header("🎯 Result Panels")]
    public GameObject success;   // Panel de éxito/clasificado
    public GameObject failure;   // Panel de fracaso - va a escena FinalFracaso
    
    [Header("🎮 Game Objects")]
    public GameObject player;
    public GameObject destPos;
    public GameObject boxTriggerPoint;
    
    [Header("📊 Ranking")]
    public Text curRankUI;
    
    [Header("🔧 Debug")]
    public bool enableDebugLogs = true;  // Habilitar logs de debug
    
    // Variables internas
    int min;
    float sec;
    int curRank = 0;
    private bool gameEnded = false;
    private bool resultShown = false;
    
    // Variables para determinar tipo de nivel
    private bool isHexagoniaLevel = false;
    private bool isClassificationLevel = false; // InGame, Carrera

    // Variables para clasificado inmediato
    private bool playerClassified = false;      // Si el jugador ya se clasificó (llegó a la meta)
    private bool classifiedPanelShown = false; // Si ya se mostró el panel de clasificado

    public static UIManager Instance;
    
    private void Awake()
    {
        // Verificar si ya existe una instancia
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"¡MÚLTIPLES UIManagers DETECTADOS! Destruyendo duplicado en: {gameObject.name}");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // Auto-detectar tipo de nivel
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (currentScene == "Hexagonia")
        {
            isHexagoniaLevel = true;
            Debug.Log($"🔵 UIManager: Nivel FINAL detectado - {currentScene}");
        }
        else if (currentScene == "InGame" || currentScene == "Carrera")
        {
            isClassificationLevel = true;
            Debug.Log($"🟡 UIManager: Nivel CLASIFICATORIO detectado - {currentScene}");
        }
        
        Debug.Log($"UIManager inicializado como instancia única en: {gameObject.name}");
    }
    
    public int CurRank
    {
        get { return curRank; }
        set
        {
            curRank = value;
            if (curRankUI != null)
            {
                // Solo mostrar el número de clasificados
                curRankUI.text = $"{curRank}";
                
                if (enableDebugLogs)
                    Debug.Log($"📊 Jugadores clasificados: {curRank}");
            }
        }
    }

    void Start()
    {
        // Buscar el jugador si no está asignado
        FindPlayer();
        
        // Asegurar que todos los paneles estén desactivados al inicio
        if (success != null) success.SetActive(false);
        if (failure != null) failure.SetActive(false);
        
        // Resetear variables de estado
        gameEnded = false;
        resultShown = false;
        playerClassified = false;
        classifiedPanelShown = false;
        
        // Inicializar el contador
        CurRank = 0;
        
        if (isHexagoniaLevel)
        {
            Debug.Log("🔵 UIManager: Hexagonia - El GameManager manejará las condiciones de victoria");
        }
        else if (isClassificationLevel)
        {
            Debug.Log("🟡 UIManager: Nivel clasificatorio - Clasificación por posición o tiempo");
        }
        else
        {
            Debug.Log("✅ UIManager: Nivel estándar inicializado correctamente");
        }
    }

    void FindPlayer()
    {
        if (player == null)
        {
            // Buscar por nombre exacto primero
            player = GameObject.Find("Player");
            
            // Si no lo encuentra, buscar por tag
            if (player == null)
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                if (players.Length > 0)
                {
                    // Tomar el primer jugador encontrado
                    player = players[0];
                    Debug.Log($"🎮 Jugador encontrado por tag: {player.name}");
                }
            }
            
            // Si aún no lo encuentra, buscar por componente LHS_MainPlayer
            if (player == null)
            {
                LHS_MainPlayer mainPlayer = FindObjectOfType<LHS_MainPlayer>();
                if (mainPlayer != null)
                {
                    player = mainPlayer.gameObject;
                    Debug.Log($"🎮 Jugador encontrado por componente: {player.name}");
                }
            }
        }
        
        if (player != null)
        {
            Debug.Log($"🎮 Jugador encontrado: {player.name} en posición: {player.transform.position}");
        }
        else
        {
            Debug.LogWarning("⚠️ No se pudo encontrar al jugador! Se reintentará en el próximo frame");
            // Reintentar en el siguiente frame
            StartCoroutine(RetryFindPlayer());
        }
    }

    IEnumerator RetryFindPlayer()
    {
        yield return null;
        FindPlayer();
    }

    void Update()
    {
        if (!gameEnded)
        {
            UpdateTimer();
        }
    }

    void UpdateTimer()
    {
        limitTime -= Time.deltaTime;

        // Actualizar display del timer
        if (limitTime >= 60f)
        {
            min = (int)limitTime / 60;
            sec = limitTime % 60;
            textTimer.text = min + " : " + (int)sec;
        }
        else if (limitTime >= 30f)
        {
            textTimer.text = "<color=white>" + (int)limitTime + "</color>";
        }
        else if (limitTime > 0)
        {
            textTimer.text = "<color=red>" + (int)limitTime + "</color>";
        }
        else
        {
            // ⏰ TIEMPO AGOTADO - Mostrar resultado y hacer transición
            textTimer.text = "<color=red>Time Over</color>";
            gameEnded = true;
            
            if (enableDebugLogs)
                Debug.Log($"⏰ Tiempo agotado - Total clasificados: {curRank}");
            
            // Programar transición después de un breve delay
            StartCoroutine(TransitionToNextLevelAfterDelay(2f));
        }
    }

    /// <summary>
    /// 🟡 Mostrar clasificado inmediatamente cuando el jugador llega a la meta
    /// Pero esperar al countdown antes de hacer transición
    /// </summary>
    public void ShowClassifiedImmediate()
    {
        // Solo para niveles clasificatorios
        if (!isClassificationLevel)
        {
            Debug.LogWarning("⚠️ ShowClassifiedImmediate llamado en nivel no clasificatorio");
            return;
        }
        
        // Evitar múltiples activaciones
        if (playerClassified)
        {
            Debug.Log("🟡 Jugador ya clasificado - ignorando");
            return;
        }
        
        // Marcar como clasificado y mostrar panel
        playerClassified = true;
        
        if (success != null && !classifiedPanelShown)
        {
            success.SetActive(true);
            classifiedPanelShown = true;
            Debug.Log("🟡 Panel de CLASIFICADO activado");
            
            // Reactivar animator del éxito
            Animator successAnimator = success.GetComponent<Animator>();
            if (successAnimator != null)
            {
                successAnimator.enabled = true;
                Debug.Log("🎬 Animación de clasificado activada");
        }
        }
        
        // NO programar transición - esperar a que se acabe el tiempo
        if (enableDebugLogs)
            Debug.Log("⏰ Esperando a que se acabe el tiempo para transición...");
    }

    /// <summary>
    /// Mostrar resultado inmediatamente basado en la posición del jugador
    /// </summary>
    void ShowGameResult()
    {
        if (resultShown) return;
        resultShown = true;

        // Si no tenemos jugador, intentar encontrarlo
        if (player == null)
        {
            FindPlayer();
        if (player == null)
        {
            Debug.LogError("❌ No se puede mostrar resultado: jugador no encontrado");
            return;
            }
        }

        // Deshabilitar scripts que puedan interferir
        DisableInterferingScripts();

        // 🟡 Si el jugador ya se clasificó, solo hacer la transición
        if (playerClassified && isClassificationLevel)
        {
            Debug.Log("🟡 Jugador ya clasificado - ejecutando transición al siguiente nivel...");
            StartCoroutine(TransitionToNextLevelAfterDelay(2f)); // Transición más rápida
            return;
        }

        // En niveles clasificatorios, esperar a que el jugador llegue a la meta
        if (isClassificationLevel)
        {
            Debug.Log("🟡 Esperando a que el jugador llegue a la meta...");
            return;
        }

        // Para otros niveles, mostrar fracaso
        Debug.Log("💀 Mostrando panel de FRACASO (tiempo agotado)");
        StartCoroutine(ShowResultWithDelay(false));
    }

    void DisableInterferingScripts()
    {
        // Deshabilitar LHS_Particle si existe para evitar interferencias
        LHS_Particle particleScript = FindObjectOfType<LHS_Particle>();
        if (particleScript != null)
        {
            Debug.Log("⚙️ Deshabilitando LHS_Particle para evitar interferencias...");
            particleScript.enabled = false;
        }
    }

    IEnumerator ShowResultWithDelay(bool isSuccess)
    {
        // Esperar un frame para asegurar estabilidad
        yield return null;
        
        // Asegurar que ambos paneles estén desactivados
        if (success != null) success.SetActive(false);
        if (failure != null) failure.SetActive(false);
        
        // Mostrar el panel correspondiente
        if (isSuccess)
        {
            if (isClassificationLevel)
            {
                Debug.Log("🟡 Mostrando panel de CLASIFICADO (nivel clasificatorio)");
            }
            else
            {
                Debug.Log("🏆 Mostrando panel de ÉXITO");
            }
            
            if (success != null)
            {
                success.SetActive(true);
                
                // Reactivar animator del éxito
                Animator successAnimator = success.GetComponent<Animator>();
                if (successAnimator != null) 
                {
                    successAnimator.enabled = true;
                    Debug.Log("🎬 Animación de éxito/clasificado activada");
                }
            }
            
            // Programar transición después de la animación
            if (isClassificationLevel)
            {
                StartCoroutine(TransitionToNextLevelAfterDelay(4f));
            }
            else
            {
                StartCoroutine(TransitionToEndingAfterDelay(true, 4f));
            }
        }
        else
        {
            Debug.Log("💀 Mostrando panel de FRACASO");
            if (failure != null)
            {
                failure.SetActive(true);
                
                // Reactivar animator del fracaso
                Animator failureAnimator = failure.GetComponent<Animator>();
                if (failureAnimator != null) 
                {
                    failureAnimator.enabled = true;
                    Debug.Log("🎬 Animación de fracaso activada");
                }
            }
            
            // Programar transición a escena FinalFracaso después de la animación
            StartCoroutine(TransitionToEndingAfterDelay(false, 4f));
        }
        
        // Reactivar LHS_Particle después de mostrar el resultado
        yield return new WaitForSeconds(1f);
        
        LHS_Particle particleScript = FindObjectOfType<LHS_Particle>();
        if (particleScript != null)
        {
            Debug.Log("🎆 Reactivando LHS_Particle...");
            particleScript.enabled = true;
        }
    }

    /// <summary>
    /// 🟡 Transición a siguiente nivel (para niveles clasificatorios)
    /// </summary>
    IEnumerator TransitionToNextLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Buscar el SceneChange component
        SceneChange sceneChanger = FindObjectOfType<SceneChange>();
        
        if (sceneChanger != null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            
            if (currentScene == "InGame")
            {
                Debug.Log("🟡 Transicionando de InGame a Carrera (clasificado)...");
                sceneChanger.InGameSceneChange();
            }
            else if (currentScene == "Carrera")
            {
                Debug.Log("🟡 Transicionando de Carrera a Hexagonia (clasificado)...");
                sceneChanger.CarreraSceneChange();
            }
            else
            {
                Debug.LogWarning($"⚠️ Escena clasificatoria no reconocida: {currentScene}");
                sceneChanger.GoToEndingSuccess();
            }
        }
        else
        {
            // Fallback: detectar escena actual y cargar la siguiente
            string currentScene = SceneManager.GetActiveScene().name;
            string nextScene = GetNextScene(currentScene);
            
            Debug.Log($"🔄 Fallback: Cargando {nextScene} desde {currentScene}...");
            SceneManager.LoadScene(nextScene);
        }
    }
    
    /// <summary>
    /// 🎯 Obtener la siguiente escena en el flujo clasificatorio
    /// </summary>
    string GetNextScene(string currentScene)
    {
        switch (currentScene)
        {
            case "InGame": return "Carrera";
            case "Carrera": return "Hexagonia";
            default: return "Ending"; // Fallback
        }
    }

    IEnumerator TransitionToEndingAfterDelay(bool isSuccess, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Buscar el SceneChange component
        SceneChange sceneChanger = FindObjectOfType<SceneChange>();
        
        if (sceneChanger != null)
        {
            if (isSuccess)
            {
                Debug.Log("🏆 Transicionando a escena de ÉXITO (Ending)...");
                sceneChanger.GoToEndingSuccess();
            }
            else
            {
                Debug.Log("💀 Transicionando a escena de FRACASO (FinalFracaso)...");
                sceneChanger.GoToEndingFailure();
            }
        }
        else
        {
            // Fallback: cargar directamente con SceneManager
            string targetScene = isSuccess ? "Ending" : "FinalFracaso";
            Debug.Log($"🔄 Fallback: Cargando escena {targetScene} directamente...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        }
    }

    /// <summary>
    /// Método público para forzar mostrar resultado (para trigger zones, etc.)
    /// </summary>
    public void ForceShowResult()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            ShowGameResult();
        }
    }

    /// <summary>
    /// Método público para mostrar éxito inmediatamente
    /// </summary>
    public void ForceSuccess()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            resultShown = true;
            
            if (isClassificationLevel)
            {
                Debug.Log("🟡 Forzando CLASIFICACIÓN...");
                StartCoroutine(ShowResultWithDelay(true));
            }
            else
            {
                Debug.Log("🏆 Forzando ÉXITO...");
                StartCoroutine(ShowResultWithDelay(true));
            }
        }
    }

    /// <summary>
    /// Método público para mostrar fracaso inmediatamente
    /// </summary>
    public void ForceFailure()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            resultShown = true;
            StartCoroutine(ShowResultWithDelay(false));
        }
    }
}
