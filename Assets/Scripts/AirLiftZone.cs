using UnityEngine;
using System.Collections;

/// <summary>
/// 🌪️ Zona de Elevación por Aire
/// Crea un efecto de ventilador que eleva al jugador manteniendo su capacidad de movimiento
/// </summary>
public class AirLiftZone : MonoBehaviour
{
    [Header("🌪️ Configuración de Elevación")]
    public float liftForce = 15f; // Fuerza de elevación
    public float maxLiftHeight = 5f; // Altura máxima de elevación
    public float smoothLiftFactor = 2f; // Suavizado de la elevación
    public float airControlMultiplier = 0.8f; // Control en el aire (0-1)
    
    [Header("🎮 Configuración de Movimiento")]
    public float horizontalDrag = 0.5f; // Resistencia horizontal en el aire
    public float verticalDrag = 0.2f; // Resistencia vertical en el aire
    public float rotationSpeed = 2f; // Velocidad de rotación del jugador
    
    [Header("🎨 Efectos Visuales")]
    public ParticleSystem airParticles; // Partículas de aire
    public float particleIntensity = 1f; // Intensidad de las partículas
    
    [Header("🔊 Efectos de Sonido")]
    public AudioSource windSound; // Sonido del viento
    public float maxWindVolume = 0.7f; // Volumen máximo del sonido
    
    // Variables privadas
    private Vector3 targetPosition;
    private bool isPlayerInside = false;
    private LHS_MainPlayer playerController;
    private Rigidbody playerRb;
    private Animator playerAnimator;
    private float originalGravity;
    private float originalDrag;
    
    void Start()
    {
        // Configurar el collider como trigger
        if (GetComponent<Collider>() != null)
        {
            GetComponent<Collider>().isTrigger = true;
        }
        
        // Inicializar efectos visuales
        if (airParticles != null)
        {
            var emission = airParticles.emission;
            emission.rateOverTime = 0; // Inicialmente desactivado
        }
        
        // Configurar sonido
        if (windSound != null)
        {
            windSound.volume = 0;
            windSound.loop = true;
            windSound.Play();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerController = other.GetComponent<LHS_MainPlayer>();
            playerRb = other.GetComponent<Rigidbody>();
            playerAnimator = other.GetComponent<Animator>();
            
            if (playerRb != null)
            {
                // Guardar valores originales
                originalGravity = playerRb.useGravity ? 9.81f : 0f;
                originalDrag = playerRb.drag;
                
                // Configurar física para el aire
                playerRb.useGravity = false;
                playerRb.drag = horizontalDrag;
            }
            
            // Activar efectos
            StartCoroutine(ActivateEffects(true));
            
            // Activar animación de vuelo
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("IsFlying", true);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            
            if (playerRb != null)
            {
                // Restaurar valores originales
                playerRb.useGravity = true;
                playerRb.drag = originalDrag;
            }
            
            // Desactivar efectos
            StartCoroutine(ActivateEffects(false));
            
            // Desactivar animación de vuelo
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("IsFlying", false);
            }
        }
    }
    
    void FixedUpdate()
    {
        if (isPlayerInside && playerRb != null)
        {
            // Calcular posición objetivo
            targetPosition = transform.position + Vector3.up * maxLiftHeight;
            
            // Aplicar fuerza de elevación
            float distanceToTarget = Vector3.Distance(playerRb.position, targetPosition);
            float liftMultiplier = Mathf.Clamp01(1f - (distanceToTarget / maxLiftHeight));
            
            Vector3 liftForceVector = Vector3.up * liftForce * liftMultiplier;
            playerRb.AddForce(liftForceVector, ForceMode.Acceleration);
            
            // Aplicar resistencia vertical
            playerRb.AddForce(-playerRb.velocity * verticalDrag, ForceMode.Acceleration);
            
            // Rotar al jugador suavemente
            if (playerController != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(playerController.transform.forward, Vector3.up);
                playerController.transform.rotation = Quaternion.Slerp(
                    playerController.transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                );
            }
        }
    }
    
    IEnumerator ActivateEffects(bool activate)
    {
        float targetParticleRate = activate ? 50f * particleIntensity : 0f;
        float targetVolume = activate ? maxWindVolume : 0f;
        float currentParticleRate = airParticles != null ? airParticles.emission.rateOverTime.constant : 0f;
        float currentVolume = windSound != null ? windSound.volume : 0f;
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Interpolar partículas
            if (airParticles != null)
            {
                var emission = airParticles.emission;
                emission.rateOverTime = Mathf.Lerp(currentParticleRate, targetParticleRate, t);
            }
            
            // Interpolar sonido
            if (windSound != null)
            {
                windSound.volume = Mathf.Lerp(currentVolume, targetVolume, t);
            }
            
            yield return null;
        }
    }
    
    void OnDrawGizmos()
    {
        // Visualizar la zona de elevación
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, GetComponent<Collider>().bounds.extents.magnitude);
        
        // Visualizar la altura máxima
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.2f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * maxLiftHeight);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * maxLiftHeight, 0.5f);
    }
} 