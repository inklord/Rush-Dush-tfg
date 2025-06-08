using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

// Controlador principal del jugador - Se mueve izquierda/derecha y salta.
// Solo salta cuando presiona la tecla de salto.
// Adaptado para Photon Multiplayer
public class LHS_MainPlayer : MonoBehaviourPun, IPunObservable
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
    
    // Variables de red (Photon)
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private float networkSpeed;
    private bool networkGrounded;
    private bool networkJumping;

    // Se llama antes del primer frame
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        // Activar barra de información solo si está asignada
        if (bar != null)
        {
            bar.SetActive(true);
            bar.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 3.35f, 0));
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning($"⚠️ {gameObject.name}: Bar no asignado - continuando sin UI");
        }
    }

    private void Start()
    {
        currentCamera = FindObjectOfType<Camera>();
        lastGroundedTime = Time.time;
    }

    private void Update()
    {
        // 🚨 CRÍTICO: Solo el owner puede controlar su jugador
        bool iAmTheOwner = photonView == null || photonView.IsMine;
        
        if (!iAmTheOwner)
        {
            // ❌ NO SOY EL OWNER - Solo interpolar movimiento remoto
            InterpolateNetworkMovement();
            
            // 🚫 BLOQUEAR TODO INPUT para jugadores remotos
            if (showDebugInfo && Input.anyKeyDown)
            {
                Debug.Log($"🚫 {gameObject.name} es REMOTO - Input BLOQUEADO! (PhotonView.IsMine = false)");
            }
            return; // ⭐ SALIR INMEDIATAMENTE
        }
        
        // ✅ SOY EL OWNER - Controlar normalmente
        if (showDebugInfo && Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log($"✅ {gameObject.name} es MÍO - Controlando! (PhotonView.IsMine = {photonView?.IsMine ?? true})");
        }
        
        // Verificar si tenemos PhotonView
        if (photonView == null)
        {
            // Modo single player - comportamiento normal
            CheckGrounded();
            HandleJumpInput();
            UpdateAnimations();
        }
        else if (photonView.IsMine)
        {
            // Solo el dueño del personaje controla el input
            CheckGrounded();
            HandleJumpInput();
            UpdateAnimations();
        }
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
        // 🚨 CRÍTICO: Solo el owner puede mover su jugador
        bool iAmTheOwner = photonView == null || photonView.IsMine;
        
        if (!iAmTheOwner)
        {
            // ❌ NO SOY EL OWNER - NO hacer nada
            return; // ⭐ SALIR INMEDIATAMENTE - Los remotos no se mueven desde aquí
        }
        
        // ✅ SOY EL OWNER - Mover normalmente
        // Verificar si tenemos PhotonView
        if (photonView == null)
        {
            // Modo single player - comportamiento normal
            FreezeRotation();
            GetInput();
            Move();
            Turn();
            Expression();
        }
        else if (photonView.IsMine)
        {
            // Solo el dueño controla el movimiento
            FreezeRotation();
            GetInput();
            Move();
            Turn();
            Expression();
        }
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
        // Solo procesar colisiones para el owner (o si no hay PhotonView)
        if (photonView != null && !photonView.IsMine) return;

        // --- NUEVO: Manejo especial para puertas ---
        if (collision.gameObject.CompareTag("Puerta"))
        {
            Puerta puerta = collision.gameObject.GetComponent<Puerta>();
            if (puerta != null)
            {
                // Si es real y NO está rota, rebota y cancela salto
                if (puerta.esReal && !puerta.EstaRota())
                {
                    // Rebote hacia atrás
                    Vector3 rebote = -transform.forward * bounceForce;
                    rigid.velocity = Vector3.zero;
                    rigid.AddForce(rebote + Vector3.up * 2f, ForceMode.Impulse);

                    // Efectos
                    if (mysfx != null && bouncefx != null)
                        mysfx.PlayOneShot(bouncefx);
                    if (bounce != null)
                    {
                        bounce.Play();
                        bounce.transform.position = collision.contacts[0].point;
                    }

                    // Cancelar salto
                    canJump = false;
                    isGrounded = false;
                    anim.SetBool("isJump", false);

                    Debug.Log("🚪 Colisión con puerta real NO rota: rebote aplicado");
                    return; // No procesar más
                }
                // Si la puerta es real y ya está rota, dejar pasar (no hacer nada especial)
            }
        }

        // Verificar si es suelo para forzar actualización
        if (collision.gameObject.layer == 0 || collision.gameObject.tag == "Floor" || collision.gameObject.tag == "Platform")
        {
            StartCoroutine(ForceGroundCheck());
        }

        // Sistema mejorado de colisiones con diferentes tipos de obstáculos
        HandleObstacleCollision(collision.gameObject, collision.contacts[0].point);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Solo procesar triggers para el owner (o si no hay PhotonView)
        if (photonView != null && !photonView.IsMine) return;

        // --- NUEVO: Manejo especial para puertas (por si alguna puerta es trigger) ---
        if (other.CompareTag("Puerta"))
        {
            Puerta puerta = other.GetComponent<Puerta>();
            if (puerta != null && puerta.esReal && !puerta.EstaRota())
            {
                // Rebote hacia atrás
                Vector3 rebote = -transform.forward * bounceForce;
                rigid.velocity = Vector3.zero;
                rigid.AddForce(rebote + Vector3.up * 2f, ForceMode.Impulse);

                if (mysfx != null && bouncefx != null)
                    mysfx.PlayOneShot(bouncefx);
                if (bounce != null)
                {
                    bounce.Play();
                    bounce.transform.position = other.transform.position;
                }

                canJump = false;
                isGrounded = false;
                anim.SetBool("isJump", false);

                Debug.Log("🚪 Trigger con puerta real NO rota: rebote aplicado");
                return;
            }
        }

        Debug.Log($"🎯 TRIGGER detectado: {other.name} con tag '{other.tag}'");
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
        if (photonView != null)
        {
            // Modo multiplayer - usar RPC
            photonView.RPC("NetworkShakeCamera", RpcTarget.All, 1.0f, 2.5f);
        }
        else
        {
            // Modo single player - shake directo
            var camera = FindObjectOfType<MovimientoCamaraSimple>();
            if (camera != null)
            {
                camera.ShakeCamera(1.0f, 2.5f);
            }
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
        // Raycast principal
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance);
        
        // Raycasts adicionales
        float offset = 0.3f;
        Vector3[] checkPoints = {
            rayStart + Vector3.forward * offset,
            rayStart + Vector3.back * offset,
            rayStart + Vector3.left * offset,
            rayStart + Vector3.right * offset
        };
        
        Gizmos.color = Color.blue;
        foreach (Vector3 point in checkPoints)
        {
            Gizmos.DrawLine(point, point + Vector3.down * groundCheckDistance);
        }
        
        // Distancia actual al suelo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * lastGroundDistance, 0.1f);
        
        // Estado del jugador (solo en editor)
        #if UNITY_EDITOR
        if (showDebugInfo)
        {
            string debugText = $"Grounded: {isGrounded}\nCanJump: {canJump}\nDistance: {lastGroundDistance:F2}";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2, debugText);
        }
        #endif
    }

    #region Network Compatibility Methods
    
    /// <summary>
    /// 🌍 Verificar si el jugador está en el suelo (para NetworkPlayerController)
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }
    
    /// <summary>
    /// 🔄 Reiniciar estado del jugador (para NetworkPlayerController)
    /// </summary>
    public void ResetPlayerState()
    {
        // Reiniciar variables de movimiento
        hAxis = 0f;
        vAxis = 0f;
        moveVec = Vector3.zero;
        
        // Reiniciar estado de salto
        jumpRequested = false;
        canJump = true;
        isGrounded = false;
        wasGrounded = false;
        
        // Reiniciar tiempos
        lastGroundedTime = Time.time;
        lastJumpTime = 0f;
        
        // Reiniciar rigidbody
        if (rigid != null)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
        
        // Reiniciar animaciones
        if (anim != null)
        {
            anim.SetFloat("Speed", 0f);
            anim.SetBool("isJump", false);
            anim.SetBool("isMove", false);
        }
        
        Debug.Log("🔄 Estado del jugador reiniciado");
    }
    
    /// <summary>
    /// 🎮 Obtener estado de velocidad actual (para animaciones de red)
    /// </summary>
    public float GetCurrentSpeed()
    {
        return rigid != null ? rigid.velocity.magnitude : 0f;
    }
    
    /// <summary>
    /// 🎯 Forzar eliminación del jugador (para sistemas multijugador)
    /// </summary>
    public void ForceElimination()
    {
        // Verificar si hay NetworkPlayerController
        NetworkPlayerController networkController = GetComponent<NetworkPlayerController>();
        if (networkController != null)
        {
            networkController.EliminateFromNetwork();
        }
        else
        {
            // Fallback para modo single player
            Debug.Log($"💀 Jugador {gameObject.name} eliminado (modo single player)");
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 📊 Obtener información de debug del jugador
    /// </summary>
    public string GetPlayerDebugInfo()
    {
        return $"Player: {gameObject.name}\n" +
               $"Position: {transform.position}\n" +
               $"Grounded: {isGrounded}\n" +
               $"CanJump: {canJump}\n" +
               $"Velocity: {(rigid != null ? rigid.velocity : Vector3.zero)}\n" +
               $"Speed: {GetCurrentSpeed():F2}";
    }
    
    #endregion
    
    #region Photon Network Methods
    
    /// <summary>
    /// 🌐 Interpolación MEJORADA para jugadores remotos
    /// </summary>
    void InterpolateNetworkMovement()
    {
        // Verificar que tenemos datos válidos
        if (networkPosition == Vector3.zero && networkRotation == Quaternion.identity)
        {
            return; // No interpolar sin datos válidos
        }
        
        // Calcular distancia para detectar teleport
        float distance = Vector3.Distance(transform.position, networkPosition);
        
        // Si la distancia es muy grande, teleportear en lugar de interpolar
        if (distance > 10f)
        {
            transform.position = networkPosition;
            transform.rotation = networkRotation;
            
            if (showDebugInfo)
            {
                Debug.Log($"🌐 {gameObject.name} TELEPORT: Distancia={distance:F1}");
            }
        }
        else
        {
            // Interpolación suave adaptativa
            float posLerpSpeed = Mathf.Clamp(distance * 2f, 5f, 20f); // Velocidad basada en distancia
            float rotLerpSpeed = 15f;
            
            // Interpolación con smoothing mejorado
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * posLerpSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * rotLerpSpeed);
        }
        
        // Actualizar animaciones con filtrado
        if (anim != null)
        {
            // Suavizar cambios de velocidad para evitar jitter
            float currentAnimSpeed = anim.GetFloat("Speed");
            float smoothedSpeed = Mathf.Lerp(currentAnimSpeed, networkSpeed, Time.deltaTime * 5f);
            
            anim.SetFloat("Speed", smoothedSpeed);
            anim.SetBool("isJump", networkJumping);
            anim.SetBool("isMove", smoothedSpeed > 0.1f);
            anim.SetBool("isGrounded", networkGrounded);
        }
        
        // Debug cada 2 segundos
        if (showDebugInfo && Time.time % 2f < 0.1f)
        {
            Debug.Log($"🌐 {gameObject.name} INTERPOLANDO: Pos={transform.position:F1} → {networkPosition:F1} | Dist={distance:F2}");
        }
    }
    
    /// <summary>
    /// 📡 Sincronización MEJORADA de datos con Photon
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (rigid == null) return; // Verificación de seguridad
        
        if (stream.IsWriting)
        {
            // ✅ SOY EL OWNER - Enviar mis datos
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(rigid.velocity); // Enviar velocidad completa
            stream.SendNext(rigid.velocity.magnitude); // Speed separado
            stream.SendNext(isGrounded);
            stream.SendNext(jumpRequested || anim.GetBool("isJump"));
            stream.SendNext(hAxis); // Input horizontal
            stream.SendNext(vAxis); // Input vertical
            
            if (showDebugInfo && Time.time % 3f < 0.1f) // Log cada 3 segundos
            {
                Debug.Log($"📡 {gameObject.name} ENVIANDO: Pos={transform.position:F1}, Vel={rigid.velocity:F1}, Grounded={isGrounded}");
            }
        }
        else
        {
            // 🌐 SOY REMOTO - Recibir datos del owner
            Vector3 receivedPosition = (Vector3)stream.ReceiveNext();
            Quaternion receivedRotation = (Quaternion)stream.ReceiveNext();
            Vector3 receivedVelocity = (Vector3)stream.ReceiveNext();
            float receivedSpeed = (float)stream.ReceiveNext();
            bool receivedGrounded = (bool)stream.ReceiveNext();
            bool receivedJumping = (bool)stream.ReceiveNext();
            float receivedHAxis = (float)stream.ReceiveNext();
            float receivedVAxis = (float)stream.ReceiveNext();
            
            // Interpolar suavemente para evitar teleport
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            
            // Compensar por lag de red
            networkPosition = receivedPosition + receivedVelocity * lag;
            networkRotation = receivedRotation;
            networkSpeed = receivedSpeed;
            networkGrounded = receivedGrounded;
            networkJumping = receivedJumping;
            
            // Aplicar velocidad directamente para física más realista
            if (rigid != null)
            {
                rigid.velocity = Vector3.Lerp(rigid.velocity, receivedVelocity, Time.deltaTime * 15f);
            }
            
            // Actualizar animaciones con datos de red
            if (anim != null)
            {
                anim.SetFloat("Speed", receivedSpeed);
                anim.SetBool("isJump", receivedJumping);
                anim.SetBool("isMove", receivedSpeed > 0.1f);
                anim.SetBool("isGrounded", receivedGrounded);
            }
            
            if (showDebugInfo && Time.time % 3f < 0.1f) // Log cada 3 segundos
            {
                Debug.Log($"📡 {gameObject.name} RECIBIENDO: Pos={receivedPosition:F1}, Vel={receivedVelocity:F1}, Lag={lag:F3}s");
            }
        }
    }
    
    /// <summary>
    /// 🎮 Solo el owner puede controlar las colisiones
    /// </summary>
    [PunRPC]
    void NetworkShakeCamera(float duration, float intensity)
    {
        var camera = FindObjectOfType<MovimientoCamaraSimple>();
        if (camera != null)
        {
            camera.ShakeCamera(duration, intensity);
        }
    }
    

    
    #endregion
}



  




