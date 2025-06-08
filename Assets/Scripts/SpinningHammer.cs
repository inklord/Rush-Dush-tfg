using System.Collections;
using UnityEngine;

/// <summary>
/// 🔨 Martillo Giratorio Profesional
/// Características: Rotación continua, fuerza variable, efectos visuales, sonidos
/// </summary>
public class SpinningHammer : MonoBehaviour
{
    [Header("🔄 Rotation Settings")]
    public float rotationSpeed = 180f; // Grados por segundo
    public Vector3 rotationAxis = Vector3.up; // Eje de rotación
    public bool randomStartRotation = true; // Rotación inicial aleatoria
    
    [Header("💥 Impact Settings")]
    public float baseKnockbackForce = 35f; // Fuerza base de empuje (incrementada)
    public float maxKnockbackForce = 70f; // Fuerza máxima de empuje (incrementada)
    public float forceMultiplierBySpeed = 2f; // Multiplicador basado en velocidad (incrementado)
    public float stunDuration = 1.2f; // Duración del aturdimiento (incrementada)
    public float minVerticalForce = 15f; // Fuerza vertical mínima (incrementada significativamente)
    public float verticalForceMultiplier = 0.8f; // Multiplicador para fuerza vertical
    
    [Header("📊 Physics Settings")]
    public bool useVariableForce = true; // Fuerza variable según velocidad
    public float impactRadius = 2f; // Radio de impacto
    public LayerMask playerLayer = -1; // Capas de jugadores
    
    [Header("🎨 Visual Effects")]
    public bool enableTrailEffect = true;
    public GameObject impactEffect; // Efecto de partículas al impacto
    public Color hammerColor = Color.red;
    public bool enableSpeedIndicator = true; // Indicador visual de velocidad
    
    [Header("🔊 Audio Settings")]
    public AudioSource audioSource;
    public AudioClip swingSound; // Sonido de giro
    public AudioClip impactSound; // Sonido de impacto
    public float swingSoundInterval = 2f; // Intervalo entre sonidos de giro
    
    [Header("⚙️ Advanced Settings")]
    public bool enableWarningZone = true; // Zona de advertencia
    public float warningRadius = 3f; // Radio de zona de advertencia
    public bool pauseOnImpact = false; // Pausar brevemente al impactar
    public float pauseDuration = 0.1f;
    
    // Variables privadas
    private float currentSpeed; // Velocidad actual
    private float lastSwingSoundTime;
    private bool isPaused = false;
    private TrailRenderer[] trails;
    private Renderer hammerRenderer;
    private Material originalMaterial;
    private float speedIndicatorTimer = 0f;
    
    // Componentes
    private Rigidbody rb;
    private Collider hammerCollider;
    
    void Start()
    {
        SetupComponents();
        SetupVisualEffects();
        SetupAudio();
        
        // 🏷️ Asegurar que tiene el tag correcto
        if (!gameObject.CompareTag("SpinningHammer"))
        {
            gameObject.tag = "SpinningHammer";
            Debug.Log("🔨 Tag 'SpinningHammer' asignado automáticamente");
        }
        
        if (randomStartRotation)
        {
            // Rotación aleatoria solo en el eje especificado
            float randomAngle = Random.Range(0f, 360f);
            transform.Rotate(rotationAxis, randomAngle, Space.Self);
        }
        
        currentSpeed = rotationSpeed;
        Debug.Log($"🔨 Martillo giratorio inicializado - Velocidad: {rotationSpeed}°/s, Fuerza: {GetKnockbackForce()}");
    }
    
    void SetupComponents()
    {
        // Obtener componentes principales
        rb = GetComponent<Rigidbody>();
        hammerCollider = GetComponent<Collider>();
        
        // Configurar como trigger si no tiene Rigidbody, pero solo si es compatible
        if (rb == null && hammerCollider != null)
        {
            // Verificar si el collider puede ser trigger
            if (CanBeUsedAsTrigger(hammerCollider))
            {
                hammerCollider.isTrigger = true;
                Debug.Log($"🔨 {gameObject.name}: Collider configurado como trigger");
            }
            else
            {
                // Para MeshColliders cóncavos, crear un trigger adicional
                CreateTriggerCollider();
                Debug.Log($"🔨 {gameObject.name}: Trigger adicional creado para MeshCollider cóncavo");
            }
        }
        
        // Buscar renderer
        hammerRenderer = GetComponentInChildren<Renderer>();
        if (hammerRenderer != null)
        {
            originalMaterial = hammerRenderer.material;
        }
    }
    
    /// <summary>
    /// 🔍 Verificar si un collider puede usarse como trigger
    /// </summary>
    bool CanBeUsedAsTrigger(Collider collider)
    {
        // MeshCollider cóncavo no puede ser trigger
        if (collider is MeshCollider meshCollider)
        {
            return meshCollider.convex;
        }
        
        // Otros tipos de colliders sí pueden ser triggers
        return true;
    }
    
    /// <summary>
    /// 🛠️ Crear un trigger adicional para colliders que no pueden ser triggers
    /// </summary>
    void CreateTriggerCollider()
    {
        // Crear un GameObject hijo para el trigger
        GameObject triggerObject = new GameObject($"{gameObject.name}_Trigger");
        triggerObject.transform.SetParent(transform);
        triggerObject.transform.localPosition = Vector3.zero;
        triggerObject.transform.localRotation = Quaternion.identity;
        triggerObject.transform.localScale = Vector3.one;
        
        // Añadir un BoxCollider como trigger (alternativa eficiente)
        BoxCollider triggerCollider = triggerObject.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        
        // Ajustar el tamaño del trigger basado en el collider original
        if (hammerCollider != null)
        {
            Bounds bounds = hammerCollider.bounds;
            triggerCollider.size = bounds.size * 1.1f; // Ligeramente más grande
        }
        else
        {
            // Tamaño por defecto
            triggerCollider.size = Vector3.one * 2f;
        }
        
        // Añadir el script de trigger al objeto hijo
        SpinningHammerTrigger triggerScript = triggerObject.AddComponent<SpinningHammerTrigger>();
        triggerScript.parentHammer = this;
        
        Debug.Log($"✅ Trigger BoxCollider creado para {gameObject.name}");
    }
    
    void SetupVisualEffects()
    {
        if (enableTrailEffect)
        {
            trails = GetComponentsInChildren<TrailRenderer>();
            
            // Si no hay trails, crear uno automáticamente
            if (trails.Length == 0)
            {
                CreateTrailEffect();
            }
        }
    }
    
    void CreateTrailEffect()
    {
        GameObject trailObject = new GameObject("HammerTrail");
        trailObject.transform.SetParent(transform);
        trailObject.transform.localPosition = Vector3.zero;
        
        TrailRenderer trail = trailObject.AddComponent<TrailRenderer>();
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = hammerColor;
        trail.endColor = new Color(hammerColor.r, hammerColor.g, hammerColor.b, 0f);
        trail.startWidth = 0.5f;
        trail.endWidth = 0.1f;
        trail.time = 0.3f;
        
        trails = new TrailRenderer[] { trail };
    }
    
    void SetupAudio()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.maxDistance = 20f;
    }
    
    void Update()
    {
        if (!isPaused)
        {
            RotateHammer();
            UpdateSpeedIndicator();
            HandleSwingSound();
        }
    }
    
    void RotateHammer()
    {
        // Rotar el martillo
        transform.Rotate(rotationAxis * currentSpeed * Time.deltaTime, Space.Self);
        
        // Actualizar efectos visuales basados en velocidad
        UpdateTrailEffects();
    }
    
    void UpdateSpeedIndicator()
    {
        if (!enableSpeedIndicator || hammerRenderer == null) return;
        
        speedIndicatorTimer += Time.deltaTime;
        
        // Cambiar intensidad del color basado en velocidad
        float speedRatio = currentSpeed / rotationSpeed;
        float colorIntensity = 0.5f + (speedRatio * 0.5f);
        
        if (originalMaterial != null)
        {
            Color targetColor = hammerColor * colorIntensity;
            hammerRenderer.material.color = Color.Lerp(hammerRenderer.material.color, targetColor, Time.deltaTime * 2f);
        }
    }
    
    void UpdateTrailEffects()
    {
        if (trails == null) return;
        
        float speedRatio = currentSpeed / rotationSpeed;
        
        foreach (TrailRenderer trail in trails)
        {
            if (trail != null)
            {
                // Ajustar trail basado en velocidad
                trail.startWidth = 0.5f * speedRatio;
                trail.time = 0.3f * speedRatio;
                
                Color trailColor = hammerColor;
                trailColor.a = speedRatio * 0.8f;
                
                // Usar startColor y endColor en lugar de color
                trail.startColor = trailColor;
                trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
            }
        }
    }
    
    void HandleSwingSound()
    {
        if (swingSound != null && audioSource != null)
        {
            if (Time.time - lastSwingSoundTime >= swingSoundInterval)
            {
                audioSource.pitch = Random.Range(0.8f, 1.2f);
                audioSource.PlayOneShot(swingSound, 0.3f);
                lastSwingSoundTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// 💥 Manejo de colisión con trigger
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        HandleImpact(other.gameObject);
    }
    
    /// <summary>
    /// 🔄 Método público para triggers auxiliares
    /// </summary>
    public void HandleTriggerEnter(Collider other)
    {
        HandleImpact(other.gameObject);
    }
    
    /// <summary>
    /// 🔄 Método público para triggers auxiliares (salida)
    /// </summary>
    public void HandleTriggerExit(Collider other)
    {
        // Actualmente no necesitamos lógica especial para OnTriggerExit
        // Pero está aquí para futuras expansiones
    }
    
    /// <summary>
    /// 💥 Manejo de colisión normal
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        HandleImpact(collision.gameObject);
    }
    
    /// <summary>
    /// 🎯 Procesar impacto con jugador
    /// </summary>
    void HandleImpact(GameObject hitObject)
    {
        // Verificar si es jugador
        if (!IsPlayer(hitObject)) return;
        
        // Reproducir sonido de impacto
        PlayImpactSound();
        
        // Mostrar efecto de partículas
        ShowImpactEffect(hitObject.transform.position);
        
        // Pausar temporalmente si está configurado
        if (pauseOnImpact)
        {
            StartCoroutine(PauseRotation());
        }
        
        // El manejo de fuerzas se hace en LHS_MainPlayer.cs
        Debug.Log($"🔨 Martillo impactó a: {hitObject.name}");
    }
    
    bool IsPlayer(GameObject obj)
    {
        return obj.CompareTag("Player") || obj.GetComponent<LHS_MainPlayer>() != null;
    }
    
    void PlayImpactSound()
    {
        if (impactSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(impactSound, 0.7f);
        }
    }
    
    void ShowImpactEffect(Vector3 position)
    {
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
    
    IEnumerator PauseRotation()
    {
        isPaused = true;
        yield return new WaitForSeconds(pauseDuration);
        isPaused = false;
    }
    
    #region Public API
    
    /// <summary>
    /// 📊 Obtener fuerza de empuje calculada
    /// </summary>
    public float GetKnockbackForce()
    {
        if (!useVariableForce)
        {
            return baseKnockbackForce;
        }
        
        // Calcular fuerza basada en velocidad actual
        float speedRatio = currentSpeed / rotationSpeed;
        float calculatedForce = baseKnockbackForce + (speedRatio * forceMultiplierBySpeed * baseKnockbackForce);
        
        return Mathf.Clamp(calculatedForce, baseKnockbackForce, maxKnockbackForce);
    }
    
    /// <summary>
    /// ⬆️ Obtener fuerza vertical garantizada
    /// </summary>
    public float GetVerticalForce()
    {
        float baseForce = GetKnockbackForce();
        float verticalForce = Mathf.Max(minVerticalForce, baseForce * verticalForceMultiplier);
        return verticalForce;
    }
    
    /// <summary>
    /// 🎯 Obtener vector de fuerza completo (horizontal + vertical)
    /// </summary>
    public Vector3 GetLaunchForce(Vector3 playerPosition)
    {
        // Dirección horizontal (alejar del martillo)
        Vector3 horizontalDirection = (playerPosition - transform.position).normalized;
        horizontalDirection.y = 0; // Solo componente horizontal
        
        // Fuerza horizontal
        float horizontalForce = GetKnockbackForce();
        Vector3 horizontalForceVector = horizontalDirection * horizontalForce;
        
        // Fuerza vertical garantizada
        float verticalForce = GetVerticalForce();
        Vector3 verticalForceVector = Vector3.up * verticalForce;
        
        // Combinar fuerzas
        Vector3 totalForce = horizontalForceVector + verticalForceVector;
        
        Debug.Log($"🚀 Fuerza de lanzamiento - Horizontal: {horizontalForce:F1}, Vertical: {verticalForce:F1}, Total: {totalForce.magnitude:F1}");
        return totalForce;
    }
    
    /// <summary>
    /// ⚡ Cambiar velocidad de rotación
    /// </summary>
    public void SetRotationSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
        Debug.Log($"🔨 Velocidad de martillo cambiada a: {newSpeed}°/s");
    }
    
    /// <summary>
    /// ⏸️ Pausar/Reanudar rotación
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused;
    }
    
    /// <summary>
    /// 🎨 Cambiar color del martillo
    /// </summary>
    public void SetHammerColor(Color newColor)
    {
        hammerColor = newColor;
    }
    
    /// <summary>
    /// 📏 Obtener distancia al jugador más cercano
    /// </summary>
    public float GetDistanceToNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDistance = float.MaxValue;
        
        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }
        
        return minDistance;
    }
    
    #endregion
    
    #region Debug and Gizmos
    
    void OnDrawGizmos()
    {
        // Mostrar radio de impacto
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
        
        // Mostrar zona de advertencia
        if (enableWarningZone)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, warningRadius);
        }
        
        // Mostrar dirección de rotación
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, rotationAxis * 2f);
    }
    
    void OnDrawGizmosSelected()
    {
        // Información detallada cuando está seleccionado
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        
        // Mostrar fuerza actual en modo play
        if (Application.isPlaying)
        {
            float currentForce = GetKnockbackForce();
            // Nota: UnityEditor.Handles solo funciona en el editor
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, $"Fuerza: {currentForce:F1}");
            #endif
        }
    }
    
    #endregion
} 