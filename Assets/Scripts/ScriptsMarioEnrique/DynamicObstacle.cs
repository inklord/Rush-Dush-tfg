using UnityEngine;

/// <summary>
/// 🎯 Tipos de efectos que puede tener un obstáculo
/// </summary>
public enum ObstacleEffectType
{
    Knockback,  // Empuje direccional fuerte
    Push,       // Empuje continuo
    Stun,       // Aturdimiento
    Bounce,     // Rebote hacia arriba
    Slow,       // Ralentizar jugador
    Stop        // Parar completamente
}

/// <summary>
/// ⚙️ Sistema de Obstáculos Dinámicos Universal
/// Permite configurar cualquier tipo de obstáculo con efectos personalizados
/// </summary>
public class DynamicObstacle : MonoBehaviour
{
    [Header("🎯 Obstacle Configuration")]
    public ObstacleEffectType effectType = ObstacleEffectType.Knockback;
    public float forceAmount = 15f; // Intensidad del efecto
    public float stunDuration = 1f; // Duración del aturdimiento (si aplica)
    
    [Header("📐 Force Direction")]
    public bool useCustomDirection = false;
    public Vector3 customForceDirection = Vector3.forward;
    public bool useObjectForward = true; // Usar transform.forward del objeto
    public bool usePlayerDirection = false; // Dirección desde objeto hacia jugador
    public bool useRandomDirection = false; // Dirección aleatoria
    
    [Header("⚡ Advanced Physics")]
    public bool scaleForceByDistance = false; // Escalar fuerza por distancia
    public float maxEffectDistance = 5f;
    public bool addVerticalComponent = false; // Añadir componente vertical
    public float verticalForceRatio = 0.3f; // Ratio de fuerza vertical
    
    [Header("🎨 Effects")]
    public bool enableSoundEffect = true;
    public bool enableParticleEffect = true;
    public bool enableScreenShake = true;
    public float shakeIntensity = 1f;
    public float shakeDuration = 0.5f;
    
    [Header("🔄 Dynamic Behavior")]
    public bool changeEffectOverTime = false;
    public float effectChangeInterval = 3f; // Cambiar efecto cada X segundos
    public ObstacleEffectType[] alternativeEffects; // Efectos alternativos
    
    [Header("📊 Conditional Activation")]
    public bool onlyAffectMovingPlayers = false; // Solo afectar jugadores en movimiento
    public float minPlayerSpeed = 1f; // Velocidad mínima del jugador
    public bool useActivationCooldown = true; // Cooldown entre activaciones
    public float cooldownDuration = 0.5f; // Duración del cooldown
    
    [Header("🎮 Player Interaction")]
    public bool allowMultipleHits = false; // Permitir múltiples impactos
    public float multiHitInterval = 1f; // Intervalo entre impactos múltiples
    public bool affectOnlyGroundedPlayers = false; // Solo jugadores en el suelo
    
    // Variables privadas
    private float lastActivationTime = 0f;
    private float lastEffectChangeTime = 0f;
    private int currentEffectIndex = 0;
    private float lastPlayerHitTime = 0f;
    
    // Referencias
    private AudioSource audioSource;
    private ParticleSystem particles;
    
    void Start()
    {
        SetupComponents();
        
        if (changeEffectOverTime && alternativeEffects != null && alternativeEffects.Length > 0)
        {
            InvokeRepeating(nameof(ChangeEffect), effectChangeInterval, effectChangeInterval);
        }
        
        Debug.Log($"⚙️ Obstáculo dinámico configurado - Tipo: {effectType}");
    }
    
    void SetupComponents()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && enableSoundEffect)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
        
        // Setup particles
        particles = GetComponentInChildren<ParticleSystem>();
    }
    
    void ChangeEffect()
    {
        if (alternativeEffects == null || alternativeEffects.Length == 0) return;
        
        currentEffectIndex = (currentEffectIndex + 1) % alternativeEffects.Length;
        effectType = alternativeEffects[currentEffectIndex];
        
        Debug.Log($"🔄 Efecto de obstáculo cambiado a: {effectType}");
    }
    
    #region Public API - Called by LHS_MainPlayer
    
    /// <summary>
    /// 📐 Obtener dirección de la fuerza a aplicar
    /// </summary>
    public Vector3 GetForceDirection(Vector3 playerPosition)
    {
        Vector3 direction = Vector3.zero;
        
        if (useCustomDirection)
        {
            direction = customForceDirection.normalized;
        }
        else if (useObjectForward)
        {
            direction = transform.forward;
        }
        else if (usePlayerDirection)
        {
            direction = (playerPosition - transform.position).normalized;
        }
        else if (useRandomDirection)
        {
            direction = Random.onUnitSphere;
            direction.y = Mathf.Abs(direction.y); // Siempre hacia arriba
        }
        else
        {
            // Dirección por defecto: alejar del obstáculo
            direction = (playerPosition - transform.position).normalized;
        }
        
        // Añadir componente vertical si está configurado
        if (addVerticalComponent)
        {
            direction.y = Mathf.Max(direction.y, verticalForceRatio);
            direction = direction.normalized;
        }
        
        return direction;
    }
    
    /// <summary>
    /// 💪 Obtener cantidad de fuerza a aplicar
    /// </summary>
    public float GetForceAmount()
    {
        float calculatedForce = forceAmount;
        
        // Escalar por distancia si está configurado
        if (scaleForceByDistance)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                float distanceRatio = Mathf.Clamp01(1f - (distance / maxEffectDistance));
                calculatedForce *= distanceRatio;
            }
        }
        
        return calculatedForce;
    }
    
    /// <summary>
    /// 🎯 Obtener tipo de efecto actual
    /// </summary>
    public ObstacleEffectType GetEffectType()
    {
        return effectType;
    }
    
    /// <summary>
    /// ⏱️ Obtener duración del aturdimiento
    /// </summary>
    public float GetStunDuration()
    {
        return stunDuration;
    }
    
    /// <summary>
    /// 🔊 Verificar si tiene efecto de sonido
    /// </summary>
    public bool HasSoundEffect()
    {
        return enableSoundEffect;
    }
    
    /// <summary>
    /// ✨ Verificar si tiene efecto de partículas
    /// </summary>
    public bool HasParticleEffect()
    {
        return enableParticleEffect;
    }
    
    /// <summary>
    /// ✅ Verificar si el obstáculo puede activarse
    /// </summary>
    public bool CanActivate(GameObject player)
    {
        // Verificar cooldown
        if (useActivationCooldown && Time.time - lastActivationTime < cooldownDuration)
        {
            return false;
        }
        
        // Verificar múltiples impactos
        if (!allowMultipleHits && Time.time - lastPlayerHitTime < multiHitInterval)
        {
            return false;
        }
        
        // Verificar si el jugador está en movimiento (si es requerido)
        if (onlyAffectMovingPlayers)
        {
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null && playerRb.velocity.magnitude < minPlayerSpeed)
            {
                return false;
            }
        }
        
        // Verificar si el jugador está en el suelo (si es requerido)
        if (affectOnlyGroundedPlayers)
        {
            LHS_MainPlayer playerScript = player.GetComponent<LHS_MainPlayer>();
            if (playerScript != null)
            {
                // Esta verificación requeriría acceso a isGrounded del LHS_MainPlayer
                // Por ahora asumimos que está en el suelo
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 🎬 Activar el obstáculo (llamado cuando se usa)
    /// </summary>
    public void Activate()
    {
        lastActivationTime = Time.time;
        lastPlayerHitTime = Time.time;
        
        // Reproducir efectos
        PlayEffects();
        
        // Shake de cámara
        if (enableScreenShake)
        {
            var camera = FindObjectOfType<MovimientoCamaraNuevo>();
            if (camera != null)
            {
                camera.ShakeCamera(shakeDuration, shakeIntensity);
            }
        }
        
        Debug.Log($"⚡ Obstáculo activado - Efecto: {effectType}");
    }
    
    #endregion
    
    #region Effects
    
    void PlayEffects()
    {
        // Reproducir sonido
        if (enableSoundEffect && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.Play();
        }
        
        // Reproducir partículas
        if (enableParticleEffect && particles != null)
        {
            particles.Play();
        }
    }
    
    #endregion
    
    #region Unity Events
    
    void OnTriggerEnter(Collider other)
    {
        HandlePlayerInteraction(other.gameObject);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        HandlePlayerInteraction(collision.gameObject);
    }
    
    void HandlePlayerInteraction(GameObject player)
    {
        // Verificar si es jugador
        if (!player.CompareTag("Player")) return;
        
        // Verificar si puede activarse
        if (!CanActivate(player)) return;
        
        // El manejo de efectos se hace en LHS_MainPlayer.cs
        // Este método principalmente verifica condiciones
        
        Debug.Log($"🎯 Jugador interactuó con obstáculo: {player.name}");
    }
    
    #endregion
    
    #region Debug
    
    void OnDrawGizmos()
    {
        // Mostrar rango de efecto
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxEffectDistance);
        
        // Mostrar dirección de fuerza
        Vector3 forceDir = GetForceDirection(transform.position + Vector3.forward);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, forceDir * 2f);
        
        // Indicador de tipo de efecto
        Gizmos.color = GetEffectColor();
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
    }
    
    void OnDrawGizmosSelected()
    {
        // Información detallada
        if (Application.isPlaying)
        {
            string info = $"{effectType}\nFuerza: {GetForceAmount():F1}";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, info);
        }
    }
    
    Color GetEffectColor()
    {
        switch (effectType)
        {
            case ObstacleEffectType.Knockback: return Color.red;
            case ObstacleEffectType.Push: return new Color(1f, 0.5f, 0f); // Orange color
            case ObstacleEffectType.Stun: return Color.yellow;
            case ObstacleEffectType.Bounce: return Color.green;
            case ObstacleEffectType.Slow: return Color.blue;
            case ObstacleEffectType.Stop: return Color.magenta;
            default: return Color.white;
        }
    }
    
    #endregion
} 