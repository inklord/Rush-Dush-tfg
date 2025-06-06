using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Controlador principal del jugador - Se mueve izquierda/derecha y salta.
// Solo salta cuando presiona la tecla de salto.
public class LHS_MainPlayer : MonoBehaviour
{
    
    // Velocidad de movimiento
    public float speed = 10;
    // Velocidad de rotación
    public float rotateSpeed = 5;
    // Fuerza de salto
    public float jumpPower = 5;

    // Cámara
    private Camera currentCamera;
    public bool UseCameraRotation = true;

    // Partículas de polvo
    public ParticleSystem dust;

    // Barra de vida/información
    public GameObject bar;

    // Colisión y rebote 
    public string playerTag;
    public float bounceForce;
    public ParticleSystem bounce;

    // Efectos de sonido
    public AudioSource mysfx;
    public AudioClip jumpfx;
    public AudioClip bouncefx;

    // Detección de suelo mejorada
    [Header("Ground Check")]
    public float groundCheckDistance = 2.0f; // Aumentar distancia
    public LayerMask groundLayerMask = -1; // Por defecto incluye todas las capas
    public bool showDebugInfo = true; // Para mostrar información de debug

    [Header("Jump Settings")]
    public float jumpCooldown = 0.1f; // Tiempo mínimo entre saltos
    public float coyoteTime = 0.1f; // Tiempo extra después de dejar el suelo para saltar

    Animator anim;
    Rigidbody rigid;

    // Control de salto mejorado
    bool isGrounded = false;
    bool wasGrounded = false;
    bool canJump = true; // Puede saltar
    bool jumpRequested = false; // Salto solicitado
    
    float lastGroundedTime = 0f; // Última vez que tocó el suelo
    float lastJumpTime = 0f; // Última vez que saltó

    float hAxis;
    float vAxis;

    Vector3 moveVec;

    // Variables para debug
    private Vector3 lastPosition;
    private float lastGroundDistance = 0f;

    // Se llama antes del primer frame
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        // Activar barra de información
        bar.SetActive(true);
        bar.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 3.35f, 0));
    }

    private void Start()
    {
        currentCamera = FindObjectOfType<Camera>();
        lastGroundedTime = Time.time;
    }

    private void Update()
    {
        CheckGrounded();
        HandleJumpInput();
        UpdateAnimations();
    }

    void CheckGrounded()
    {
        wasGrounded = isGrounded;
        lastPosition = transform.position;
        
        // Múltiples puntos de verificación para mayor precisión
        Vector3 center = transform.position;
        Vector3 rayStart = center + Vector3.up * 0.1f;
        
        // Raycast principal desde el centro
        RaycastHit hit;
        bool centerHit = Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayerMask);
        
        // Raycasts adicionales desde las esquinas para mayor precisión
        float offset = 0.3f;
        Vector3[] checkPoints = {
            rayStart + Vector3.forward * offset,
            rayStart + Vector3.back * offset,
            rayStart + Vector3.left * offset,
            rayStart + Vector3.right * offset
        };
        
        bool anyHit = centerHit;
        float minDistance = centerHit ? hit.distance : float.MaxValue;
        
        // Verificar puntos adicionales
        foreach (Vector3 point in checkPoints)
        {
            if (Physics.Raycast(point, Vector3.down, out RaycastHit cornerHit, groundCheckDistance, groundLayerMask))
            {
                anyHit = true;
                if (cornerHit.distance < minDistance)
                {
                    minDistance = cornerHit.distance;
                    hit = cornerHit;
                }
            }
        }
        
        isGrounded = anyHit;
        lastGroundDistance = minDistance;
        
        // Verificación adicional por velocidad si no detecta suelo
        if (!isGrounded && rigid.velocity.y <= 0.1f && rigid.velocity.y >= -2f)
        {
            // Extender la búsqueda si la velocidad es baja
            isGrounded = Physics.Raycast(rayStart, Vector3.down, groundCheckDistance * 1.5f, groundLayerMask);
        }

        // Actualizar estado de salto
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            
            // Restaurar capacidad de salto cuando toque el suelo
            if (!wasGrounded)
            {
                canJump = true;
                Debug.Log("🌍 Tocó el suelo - Puede saltar de nuevo!");
            }
        }

        // Debug detallado
        if (showDebugInfo && (wasGrounded != isGrounded || Vector3.Distance(lastPosition, transform.position) > 1f))
        {
            Debug.Log($"Pos: {transform.position:F1} | Grounded: {isGrounded} | CanJump: {canJump} | Distance: {minDistance:F2} | VelY: {rigid.velocity.y:F2}");
            
            if (hit.collider != null)
            {
                Debug.Log($"Hit Object: {hit.collider.name} | Tag: {hit.collider.tag} | Layer: {hit.collider.gameObject.layer}");
            }
        }
    }

    void HandleJumpInput()
    {
        // Detectar input de salto
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
            Debug.Log("🚀 ¡Espaciadora presionada!");
        }

        // Procesar salto si es posible
        if (jumpRequested)
        {
            bool canPerformJump = false;

            // Verificar si puede saltar (en el suelo O dentro del coyote time)
            if (isGrounded && canJump)
            {
                canPerformJump = true;
            }
            else if (!isGrounded && canJump && (Time.time - lastGroundedTime) <= coyoteTime)
            {
                canPerformJump = true;
                Debug.Log("🦊 Coyote time jump!");
            }

            // Verificar cooldown
            if (canPerformJump && (Time.time - lastJumpTime) >= jumpCooldown)
            {
                PerformJump();
            }
            else if (!canJump)
            {
                Debug.Log("❌ No puede saltar - Debe tocar el suelo primero");
            }
            else if ((Time.time - lastJumpTime) < jumpCooldown)
            {
                Debug.Log("⏰ Esperando cooldown de salto");
            }
            else
            {
                Debug.Log("❌ No está en el suelo y coyote time expirado");
            }

            // Resetear solicitud
            jumpRequested = false;
        }
    }

    void PerformJump()
    {
        Debug.Log("🚀 ¡SALTANDO!");
        
        // Aplicar fuerza de salto
        rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        
        // Actualizar estados
        canJump = false; // No puede saltar hasta tocar el suelo
        isGrounded = false; // Ya no está en el suelo
        lastJumpTime = Time.time;
        
        // Animaciones y efectos
        anim.SetBool("isJump", true);
        anim.SetTrigger("doJump");
        
        // Sonidos y partículas
        if (mysfx != null && jumpfx != null)
        {
            mysfx.PlayOneShot(jumpfx);
        }
        if (dust != null)
        {
            dust.Play();
        }
    }

    void UpdateAnimations()
    {
        // Actualizar animación de salto solo cuando cambie el estado
        if (wasGrounded != isGrounded)
        {
            anim.SetBool("isJump", !isGrounded);
        }
    }

    private void FixedUpdate()
    {
        FreezeRotation();
        GetInput();
        Move();
        Turn();
        
        Expression();
    }

    void FreezeRotation()
    {
        // Evita que el personaje rote después de colisiones
        // Mantiene estable la rotación del personaje
        rigid.angularVelocity = Vector3.zero;
    }

    void GetInput()
    {
        hAxis = Input.GetAxis("Horizontal");
        vAxis = Input.GetAxis("Vertical");
    }

    void Move()
    {
        // Movimiento
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        // Aplica la rotación de la cámara al movimiento
        if (UseCameraRotation)
        {
            // Obtiene la rotación Y de la cámara
            Quaternion v3Rotation = Quaternion.Euler(0f, currentCamera.transform.eulerAngles.y, 0f);
            // Aplica la rotación al vector de movimiento
            moveVec = v3Rotation * moveVec;
        }

        // Aplicar movimiento
        transform.position += moveVec * speed * Time.deltaTime;

        // Activar animación de movimiento
        anim.SetBool("isMove", moveVec != Vector3.zero);
    }

    void Turn()
    {
        // Rotación hacia la dirección de movimiento
        if (hAxis == 0 && vAxis == 0)
            return;
        Quaternion newRotation = Quaternion.LookRotation(moveVec);
        rigid.rotation = Quaternion.Slerp(rigid.rotation, newRotation, rotateSpeed * Time.deltaTime);
    }

    // Maneja diferentes tipos de colisiones + sonidos / partículas 
    private void OnCollisionEnter(Collision collision)
    {
        // Verificar si es suelo para forzar actualización
        if (collision.gameObject.layer == 0 || collision.gameObject.tag == "Floor" || collision.gameObject.tag == "Platform")
        {
            // Forzar verificación de suelo en el próximo frame
            StartCoroutine(ForceGroundCheck());
        }

        // Sistema mejorado de colisiones con diferentes tipos de obstáculos
        HandleObstacleCollision(collision.gameObject, collision.contacts[0].point);
    }

    /// <summary>
    /// 🎯 Manejo de triggers (para objetos configurados como Trigger)
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🎯 TRIGGER detectado: {other.name} con tag '{other.tag}'");
        
        // Usar transform.position como punto de contacto para triggers
        HandleObstacleCollision(other.gameObject, other.transform.position);
    }

    /// <summary>
    /// 🚧 Sistema unificado de manejo de colisiones con obstáculos
    /// </summary>
    void HandleObstacleCollision(GameObject collisionObject, Vector3 contactPoint)
    {
        string obstacleTag = collisionObject.tag;
        
        Debug.Log($"🚧 Procesando colisión: {collisionObject.name} - Tag: '{obstacleTag}'");
        
        switch (obstacleTag)
        {
            case "Wall":
                HandleWallCollision(collisionObject, contactPoint);
                break;
                
            case "SpinningHammer":
                HandleSpinningHammerCollision(collisionObject, contactPoint);
                break;
                
            case "MovingObstacle":
                HandleMovingObstacleCollision(collisionObject, contactPoint);
                break;
                
            case "Bouncer":
                HandleBouncerCollision(collisionObject, contactPoint);
                break;
                
            case "Pusher":
                HandlePusherCollision(collisionObject, contactPoint);
                break;
                
            default:
                // Verificar si tiene componente de obstáculo dinámico
                DynamicObstacle dynamicObstacle = collisionObject.GetComponent<DynamicObstacle>();
                if (dynamicObstacle != null)
                {
                    HandleDynamicObstacleCollision(collisionObject, dynamicObstacle, contactPoint);
                }
                else
                {
                    Debug.Log($"⚠️ Objeto sin handler específico: {collisionObject.name} - Tag: '{obstacleTag}'");
                }
                break;
        }
    }

    /// <summary>
    /// 🧱 Manejo de colisión con paredes (comportamiento original)
    /// </summary>
    void HandleWallCollision(GameObject collisionObject, Vector3 contactPoint)
    {
        rigid.velocity = new Vector3(0, 0, 0);
        rigid.AddForce(Vector3.back * bounceForce, ForceMode.Impulse);

        if (mysfx != null && bouncefx != null)
        {
            mysfx.PlayOneShot(bouncefx);
        }
        if (bounce != null)
        {
            bounce.Play();
            bounce.transform.position = contactPoint;
        }
        
        Debug.Log("💥 Colisión con pared - Rebote aplicado");
    }

    /// <summary>
    /// 🔨 Manejo de colisión con martillos giratorios
    /// </summary>
    void HandleSpinningHammerCollision(GameObject collisionObject, Vector3 contactPoint)
    {
        // Obtener el componente del martillo
        SpinningHammer hammer = collisionObject.GetComponent<SpinningHammer>();
        
        if (hammer != null)
        {
            // Usar el nuevo sistema de fuerza mejorado
            Vector3 launchForce = hammer.GetLaunchForce(transform.position);
            
            // Aplicar efecto de aturdimiento con duración del martillo
            StartCoroutine(StunEffect(hammer.stunDuration));
            
            // Resetear velocidad y aplicar fuerza de lanzamiento
            rigid.velocity = Vector3.zero;
            rigid.AddForce(launchForce, ForceMode.Impulse);
            
            Debug.Log($"🚀 ¡LANZADO POR MARTILLO! Fuerza total: {launchForce.magnitude:F1}");
        }
        else
        {
            // Fallback al sistema anterior si no hay componente SpinningHammer
            Vector3 knockbackDirection = (transform.position - collisionObject.transform.position).normalized;
            float knockbackForce = bounceForce * 3f; // Aumentar fuerza de fallback
            
            rigid.velocity = Vector3.zero;
            Vector3 forceVector = knockbackDirection * knockbackForce;
            forceVector.y = Mathf.Max(forceVector.y, knockbackForce * 0.6f); // Más fuerza vertical
            
            rigid.AddForce(forceVector, ForceMode.Impulse);
            
            Debug.Log($"🔨 Martillo sin componente - Fuerza fallback: {knockbackForce}");
        }
        
        // Efectos visuales y sonoros
        if (mysfx != null && bouncefx != null)
        {
            mysfx.pitch = Random.Range(0.8f, 1.2f); // Variación de pitch
            mysfx.PlayOneShot(bouncefx);
        }
        if (bounce != null)
        {
            bounce.Play();
            bounce.transform.position = contactPoint;
        }
        
        // Activar shake de cámara más intenso
        var camera = FindObjectOfType<MovimientoCamaraNuevo>();
        if (camera != null)
        {
            camera.ShakeCamera(1.0f, 2.5f); // Shake más intenso y duradero
        }
    }

    /// <summary>
    /// 📦 Manejo de colisión con obstáculos en movimiento
    /// </summary>
    void HandleMovingObstacleCollision(GameObject collisionObject, Vector3 contactPoint)
    {
        Vector3 obstacleVelocity = Vector3.zero;
        Rigidbody obstacleRb = collisionObject.GetComponent<Rigidbody>();
        
        if (obstacleRb != null)
        {
            obstacleVelocity = obstacleRb.velocity;
        }
        
        // Transferir parte del momentum del obstáculo al jugador
        Vector3 transferForce = obstacleVelocity * 0.5f;
        rigid.AddForce(transferForce, ForceMode.VelocityChange);
        
        Debug.Log($"📦 Colisión con obstáculo móvil - Momentum transferido: {transferForce.magnitude:F1}");
    }

    /// <summary>
    /// 🏀 Manejo de colisión con objetos que rebotan
    /// </summary>
    void HandleBouncerCollision(GameObject collisionObject, Vector3 contactPoint)
    {
        Vector3 bounceDirection = Vector3.up + (transform.position - collisionObject.transform.position).normalized * 0.5f;
        float bounceIntensity = bounceForce * 1.5f;
        
        rigid.velocity = Vector3.zero;
        rigid.AddForce(bounceDirection.normalized * bounceIntensity, ForceMode.Impulse);
        
        // Efectos
        if (bounce != null)
        {
            bounce.Play();
            bounce.transform.position = contactPoint;
        }
        
        Debug.Log($"🏀 Rebote aplicado - Dirección: {bounceDirection}");
    }

    /// <summary>
    /// 👋 Manejo de colisión con empujadores
    /// </summary>
    void HandlePusherCollision(GameObject collisionObject, Vector3 contactPoint)
    {
        Vector3 pushDirection = collisionObject.transform.forward;
        float pushForce = bounceForce * 1.2f;
        
        rigid.AddForce(pushDirection * pushForce, ForceMode.Impulse);
        
        Debug.Log($"👋 Empuje aplicado - Dirección: {pushDirection}");
    }

    /// <summary>
    /// ⚙️ Manejo de colisión con obstáculos dinámicos (componente personalizado)
    /// </summary>
    void HandleDynamicObstacleCollision(GameObject collisionObject, DynamicObstacle obstacle, Vector3 contactPoint)
    {
        Vector3 forceDirection = obstacle.GetForceDirection(transform.position);
        float forceAmount = obstacle.GetForceAmount();
        ObstacleEffectType effectType = obstacle.GetEffectType();
        
        // Aplicar efecto según el tipo
        switch (effectType)
        {
            case ObstacleEffectType.Knockback:
                rigid.velocity = Vector3.zero;
                rigid.AddForce(forceDirection * forceAmount, ForceMode.Impulse);
                break;
                
            case ObstacleEffectType.Push:
                rigid.AddForce(forceDirection * forceAmount, ForceMode.Force);
                break;
                
            case ObstacleEffectType.Stun:
                StartCoroutine(StunEffect(obstacle.GetStunDuration()));
                break;
                
            case ObstacleEffectType.Bounce:
                Vector3 bounceDir = Vector3.up + forceDirection * 0.7f;
                rigid.velocity = Vector3.zero;
                rigid.AddForce(bounceDir.normalized * forceAmount, ForceMode.Impulse);
                break;
        }
        
        // Efectos opcionales
        if (obstacle.HasSoundEffect() && mysfx != null && bouncefx != null)
        {
            mysfx.PlayOneShot(bouncefx);
        }
        
        if (obstacle.HasParticleEffect() && bounce != null)
        {
            bounce.Play();
            bounce.transform.position = contactPoint;
        }
        
        Debug.Log($"⚙️ Obstáculo dinámico - Tipo: {effectType}, Fuerza: {forceAmount}");
    }

    /// <summary>
    /// 😵 Efecto de aturdimiento temporal
    /// </summary>
    System.Collections.IEnumerator StunEffect(float duration)
    {
        // Reducir velocidad de movimiento temporalmente
        float originalSpeed = speed;
        speed *= 0.3f; // Reducir velocidad al 30%
        
        // Indicador visual (opcional)
        Debug.Log($"😵 Jugador aturdido por {duration} segundos");
        
        yield return new WaitForSeconds(duration);
        
        // Restaurar velocidad
        speed = originalSpeed;
        Debug.Log("✅ Aturdimiento terminado");
    }

    IEnumerator ForceGroundCheck()
    {
        yield return new WaitForFixedUpdate();
        CheckGrounded();
    }

    // Expresiones/Gestos del personaje
    void Expression()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            anim.SetTrigger("doDance01");
        }

        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            anim.SetTrigger("doDance02");
        }

        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            anim.SetTrigger("doVictory");
        }
    }

    // Visualizar el raycast en el editor
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // Configurar colores
        Gizmos.color = isGrounded ? Color.green : Color.red;
        
        // Raycast principal
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance);
        
        // Raycasts adicionales
        float offset = 0.3f;
        Vector3[] checkPoints = {
            rayStart + Vector3.forward * offset,
            rayStart + Vector3.back * offset,
            rayStart + Vector3.left * offset,
            rayStart + Vector3.right * offset
        };
        
        Gizmos.color = isGrounded ? Color.green : Color.yellow;
        foreach (Vector3 point in checkPoints)
        {
            Gizmos.DrawLine(point, point + Vector3.down * groundCheckDistance);
        }
        
        // Mostrar punto final del raycast principal
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(rayStart + Vector3.down * lastGroundDistance, 0.1f);
        
        // Área de detección
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(offset * 2, 0.1f, offset * 2));

        // Mostrar estado de salto
        if (canJump)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.3f);
        }
    }
}



  



