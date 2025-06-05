using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public float limitTime;
    public Text textTimer;
    int min;
    float sec;
    public GameObject roundOver;
    public GameObject success;
    public GameObject failure;
    public GameObject player;
    public GameObject destPos;
    public GameObject boxTriggerPoint;
    int curRank = 0;
    public Text curRankUI;

    // Variables para controlar el estado del juego
    private bool gameEnded = false;
    private bool resultShown = false;
    private float gameEndTime = 0f;

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
        Debug.Log($"UIManager inicializado como instancia única en: {gameObject.name}");
    }
    public int CurRank
    {
        get 
        { 
            return curRank;
        }
        set
        {
            curRank = value;
            curRankUI.text = curRank + " / 20 ";
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        // Buscar el jugador si no está asignado
        if (player == null)
        {
            player = GameObject.Find("Player");
            if (player == null)
            {
                player = GameObject.FindWithTag("Player");
            }
        }
        
        // Verificar que se encontró el jugador
        if (player != null)
        {
            Debug.Log($"Jugador encontrado: {player.name} en posición: {player.transform.position}");
        }
        else
        {
            Debug.LogError("No se pudo encontrar al jugador! Verifica que tenga el tag 'Player' o el nombre 'Player'");
        }
        
        // Debug: Verificar si hay scripts LHS_Particle que puedan interferir
        LHS_Particle particleScript = FindObjectOfType<LHS_Particle>();
        if (particleScript != null)
        {
            Debug.LogWarning($"DETECTADO LHS_Particle script en: {particleScript.gameObject.name}");
            if (particleScript.winUI != null)
            {
                Debug.LogWarning($"LHS_Particle.winUI está conectado a: {particleScript.winUI.name}");
                if (particleScript.winUI == success)
                {
                    Debug.LogError("¡CONFLICTO! LHS_Particle.winUI es el mismo que nuestro success panel!");
                }
            }
        }
        
        // Asegurar que todos los paneles estén desactivados al inicio
        roundOver.SetActive(false);
        success.SetActive(false);
        failure.SetActive(false);
        
        // Resetear variables de estado
        gameEnded = false;
        resultShown = false;
        gameEndTime = 0f;
        
        Debug.Log("UIManager inicializado correctamente");
    }

    // Update is called once per frame
    void Update()
    {
        Timer();
        
        // Debug: Verificar si los paneles se activan inesperadamente
        if (gameEnded && resultShown)
        {
            // Si ya se mostró el resultado, verificar que solo uno esté activo
            bool successActive = success.activeInHierarchy;
            bool failureActive = failure.activeInHierarchy;
            
            if (successActive && failureActive)
            {
                Debug.LogError("¡PROBLEMA DETECTADO! Ambos paneles están activos al mismo tiempo:");
                Debug.LogError($"Success activo: {successActive}, Failure activo: {failureActive}");
                
                // Forzar que solo esté activo el correcto
                if (player.transform.position.z <= 560)
                {
                    Debug.LogError("Forzando solo failure activo...");
                    success.SetActive(false);
                    failure.SetActive(true);
                }
            }
        }
    }

    void Timer()
    {
        // Solo actualizar el timer si el juego no ha terminado
        if (!gameEnded)
        {
            limitTime -= Time.deltaTime;

            if (limitTime >= 60f)
            {
                min = (int)limitTime / 60;
                sec = limitTime % 60;
                textTimer.text = min + " : " + (int)sec;
            }
            if (limitTime < 60f)
                textTimer.text = "<color=white>" + (int)limitTime + "</color>";
            if (limitTime < 30f)
                textTimer.text = "<color=red>" + (int)limitTime + "</color>";
            
            // Cuando el tiempo se acaba
            if (limitTime <= 0)
            {
                textTimer.text = "<color=red>" + "Time Over" + "</color>";
                gameEnded = true;
                gameEndTime = Time.time;
                roundOver.SetActive(true);
                
                // Debug: Verificar la posición del jugador cuando termina el tiempo
                Debug.Log($"TIEMPO AGOTADO - Posición del jugador: {player.transform.position}");
                Debug.Log($"Posición Z del jugador: {player.transform.position.z}");
                Debug.Log($"¿Mayor que 560? {player.transform.position.z > 560}");
            }
        }
        else if (!resultShown)
        {
            // Manejar la secuencia después de que termine el tiempo
            float timeSinceGameEnd = Time.time - gameEndTime;
            
            // Mostrar roundOver por 2 segundos
            if (timeSinceGameEnd >= 2f)
            {
                roundOver.SetActive(false);
                
                // Debug adicional antes de mostrar resultado
                Debug.Log($"MOSTRANDO RESULTADO - Posición actual: {player.transform.position}");
                Debug.Log($"Posición Z: {player.transform.position.z}");
                
                // Desactivar los animators temporalmente para evitar activaciones automáticas
                Animator successAnimator = success.GetComponent<Animator>();
                Animator failureAnimator = failure.GetComponent<Animator>();
                
                if (successAnimator != null) successAnimator.enabled = false;
                if (failureAnimator != null) failureAnimator.enabled = false;
                
                // También deshabilitar LHS_Particle si existe para evitar interferencias
                LHS_Particle particleScript = FindObjectOfType<LHS_Particle>();
                if (particleScript != null)
                {
                    Debug.LogWarning("Deshabilitando temporalmente LHS_Particle para evitar interferencias...");
                    particleScript.enabled = false;
                }
                
                // Asegurar que ambos paneles estén desactivados antes de decidir
                success.SetActive(false);
                failure.SetActive(false);
                
                // Esperar un frame para asegurar que se desactivaron completamente
                StartCoroutine(ShowResultAfterFrame());
                
                resultShown = true; // Evitar que se vuelva a ejecutar
                Debug.Log("Resultado mostrado. No se volverá a ejecutar esta lógica.");
            }
        }
    }
    
    IEnumerator ShowResultAfterFrame()
    {
        yield return null; // Esperar un frame
        
        // Verificar la posición del jugador y mostrar el resultado apropiado
        if (player.transform.position.z > 560)
        {
            Debug.Log("ACTIVANDO success panel...");
            success.SetActive(true);
            
            // Reactivar animator después de activar el panel
            Animator successAnimator = success.GetComponent<Animator>();
            if (successAnimator != null) successAnimator.enabled = true;
            
            Debug.Log("RESULTADO: ¡ÉXITO! Jugador cruzó la meta a tiempo.");
        }
        else
        {
            Debug.Log("ACTIVANDO failure panel...");
            failure.SetActive(true);
            
            // Reactivar animator después de activar el panel
            Animator failureAnimator = failure.GetComponent<Animator>();
            if (failureAnimator != null) failureAnimator.enabled = true;
            
            Debug.Log("RESULTADO: FRACASO - Jugador no llegó a la meta a tiempo.");
        }
        
        // Esperar un poco más antes de reactivar LHS_Particle para evitar conflictos
        yield return new WaitForSeconds(1f);
        
        // Reactivar LHS_Particle después de mostrar el resultado
        LHS_Particle particleScript = FindObjectOfType<LHS_Particle>();
        if (particleScript != null)
        {
            Debug.Log("Reactivando LHS_Particle...");
            particleScript.enabled = true;
        }
    }
}
