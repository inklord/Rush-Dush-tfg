using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;

/// <summary>
/// 🎮 MAIN PLAYER CONTROLLER - Movimiento y cámara
/// </summary>
public class LHS_MainPlayer : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("🎮 Movimiento")]
    public float speed = 10f;
    public float rotateSpeed = 5f;
    public float jumpPower = 5f;
    public bool UseCameraRotation = false;

    [Header("🖱️ Control de Rotación")]
    public float mouseSensitivity = 3f;
    public bool invertMouseY = false;

    [Header("🎯 Configuración")]
    public float groundCheckDistance = 1.1f;
    public LayerMask groundLayerMask;
    public bool showDebugInfo = false;
    public float jumpCooldown = 0.1f;
    public float coyoteTime = 0.1f;
    public float respawnHeight = -10f;
    
    [Header("🚀 Doble Salto")]
    public bool enableDoubleJump = false;
    public float secondJumpMultiplier = 0.8f; // Segundo salto más débil

    [Header("📷 Cámara")]
    public float cameraDistance = 5f;
    public float cameraHeight = 2f;
    public float cameraSmoothSpeed = 5f;

    [Header("🎯 Efectos")]
    public ParticleSystem dust;
    public UnityEngine.UI.Image bar;
    public string playerTag = "";
    public float bounceForce;
    public AudioSource bounce;
    public AudioSource mysfx;
    public AudioClip jumpfx;
    public AudioClip bouncefx;

    // Propiedades públicas para sincronización
    public bool IsMoving => rb != null && rb.velocity.magnitude > 0.1f;
    public bool IsJumping => !isGrounded;
    public bool IsGrounded => isGrounded;
    public float CurrentSpeed => rb != null ? rb.velocity.magnitude : 0f;

    // Referencias privadas
    private Rigidbody rb;
    private new PhotonView photonView;
    private Camera currentCamera;
    private Vector3 lastGroundedPosition;
    private float lastJumpTime;
    private float lastGroundedTime;
    private bool isGrounded;
    private bool wasGrounded;
    private Animator anim;
    private Vector3 lastPosition;
    private float lastGroundDistance;
    private bool jumpRequested;
    private bool canJump = true;
    private int jumpCount = 0; // Contador de saltos
    private int maxJumps = 1; // Máximo saltos permitidos (se ajusta dinámicamente)

    // Variables de red
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Vector3 networkVelocity;
    private bool networkIsGrounded;
    private bool networkIsJumping;
    private float networkLerpTime;
    private const float NETWORK_SMOOTHING = 10f;

    void Awake()
    {
        // Configurar LayerMask para Ground
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1)
        {
            groundLayerMask = 1 << groundLayer;
        }
        else
        {
            Debug.LogWarning("⚠️ Layer 'Ground' no encontrado, usando layer Default");
            groundLayerMask = 1 << LayerMask.NameToLayer("Default");
        }

        // Obtener referencias
        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        anim = GetComponentInChildren<Animator>();

        // Configurar Rigidbody para mejor detección de colisiones
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.mass = 1f; // Masa normal
            rb.drag = 0f; // Sin drag para movimiento más preciso
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            
            if (showDebugInfo)
                Debug.Log("🎮 Rigidbody configurado para mejor detección de colisiones");
        }

        // Configurar CapsuleCollider para mejor precisión
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            capsule.material = new PhysicMaterial
            {
                dynamicFriction = 0f, // Reducir fricción para evitar "pegarse"
                staticFriction = 0f,
                frictionCombine = PhysicMaterialCombine.Minimum,
                bounceCombine = PhysicMaterialCombine.Minimum,
                bounciness = 0f
            };
            
            if (showDebugInfo)
                Debug.Log("🎮 CapsuleCollider configurado con material físico optimizado");
        }

        // Inicializar variables de red
        networkPosition = transform.position;
        networkRotation = transform.rotation;
        networkLerpTime = 0f;

        // Configuración basada en propiedad
        if (photonView == null || photonView.IsMine)
        {
            SetupLocalPlayer();
        }
        else
        {
            SetupRemotePlayer();
        }
    }

    void SetupLocalPlayer()
    {
        // Activar control local
        enabled = true;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        // Configurar cursor para control con ratón
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Configurar cámara
        SetupCamera();
        
        Debug.Log($"✅ Jugador local inicializado: {gameObject.name}");
    }

    void SetupRemotePlayer()
    {
        // Desactivar control local
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        Debug.Log($"👥 Jugador remoto inicializado: {gameObject.name}");
    }

    void SetupCamera()
    {
        currentCamera = Camera.main;
        if (currentCamera == null)
        {
            Debug.LogError("❌ No se encontró la cámara principal");
            return;
        }

        // Solo usar MovimientoCamaraSimple
        MovimientoCamaraSimple cameraScript = currentCamera.GetComponent<MovimientoCamaraSimple>();
        if (cameraScript == null)
        {
            cameraScript = currentCamera.gameObject.AddComponent<MovimientoCamaraSimple>();
        }

        cameraScript.SetPlayer(transform);
        Debug.Log($"📷 Cámara MovimientoCamaraSimple configurada para: {gameObject.name}");
    }

    void Update()
    {
        // Manejar modo singleplayer y multiplayer
        if (photonView != null && PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !photonView.IsMine)
        {
            // Interpolación suave para jugadores remotos (solo en multiplayer)
            UpdateRemotePlayer();
            return;
        }

        // Control local (singleplayer o jugador propio en multiplayer)
        UpdateLocalPlayer();
    }

    void UpdateLocalPlayer()
    {
        // Control de cursor con Escape
        HandleCursorToggle();
        
        // Ground check estricto solo para layer "Ground"
        wasGrounded = isGrounded;
        
        // Verificar layer Ground con múltiples puntos de detección
        Vector3[] checkPoints = new Vector3[]
        {
            transform.position, // Centro
            transform.position + transform.forward * 0.3f, // Frente
            transform.position - transform.forward * 0.3f, // Atrás
            transform.position + transform.right * 0.3f, // Derecha
            transform.position - transform.right * 0.3f  // Izquierda
        };

        bool raycastGrounded = false;
        foreach (Vector3 point in checkPoints)
        {
            if (Physics.Raycast(point, Vector3.down, groundCheckDistance, groundLayerMask))
            {
                raycastGrounded = true;
                break;
            }
        }

        // Verificación adicional con overlap box para mejor precisión
        Vector3 boxCenter = transform.position + Vector3.down * (groundCheckDistance * 0.5f);
        Vector3 boxHalfExtents = new Vector3(0.4f, groundCheckDistance * 0.5f, 0.4f);
        bool boxGrounded = Physics.CheckBox(boxCenter, boxHalfExtents, Quaternion.identity, groundLayerMask);

        // Verificar también objetos con tag Hexagono
        bool hexagonGrounded = false;
        foreach (Vector3 point in checkPoints)
        {
            RaycastHit hit;
            if (Physics.Raycast(point, Vector3.down, out hit, groundCheckDistance))
            {
                if (hit.collider.CompareTag("Hexagono"))
                {
                    hexagonGrounded = true;
                    break;
                }
            }
        }

        // Considerar en suelo si cualquiera de las verificaciones es positiva
        isGrounded = raycastGrounded || boxGrounded || hexagonGrounded;

        if (showDebugInfo && wasGrounded != isGrounded)
        {
            Debug.Log($"🌍 CAMBIO DE SUELO - Anterior: {wasGrounded}, Actual: {isGrounded} " +
                     $"(Raycast: {raycastGrounded}, Box: {boxGrounded}, Hexagono: {hexagonGrounded})");
        }

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            lastGroundedPosition = transform.position;
            
            // Resetear contador de saltos cuando está en el suelo
            if (jumpCount > 0)
            {
                jumpCount = 0;
                if (showDebugInfo) Debug.Log("🚀 Saltos reseteados - En suelo");
            }
        }

        // Control de rotación del jugador con ratón
        HandleMouseLook();

        // Input y movimiento
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calcular dirección de movimiento
        Vector3 moveDirection = CalculateMoveDirection(horizontal, vertical);

        // Aplicar movimiento
        ApplyMovement(moveDirection);

        // Procesar salto
        ProcessJump();

        // Actualizar animaciones
        UpdateAnimations(moveDirection);

        // Check respawn
        if (transform.position.y < respawnHeight)
        {
            Respawn();
        }
    }

    void UpdateRemotePlayer()
    {
        // Interpolación suave para jugadores remotos (solo en multiplayer)
        if (photonView == null) return;
        
        networkLerpTime += Time.deltaTime * NETWORK_SMOOTHING;
        transform.position = Vector3.Lerp(transform.position, networkPosition, networkLerpTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, networkLerpTime);
        
        if (networkLerpTime >= 1f)
            networkLerpTime = 1f;
    }

    /// <summary>
    /// 🖱️ Control de rotación del jugador con ratón (Fall Guys style)
    /// </summary>
    void HandleMouseLook()
    {
        // Solo procesar si el cursor está bloqueado
        if (Cursor.lockState != CursorLockMode.Locked) return;

        // Obtener input del ratón
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        
        // Rotar al jugador horizontalmente (izquierda/derecha)
        transform.Rotate(Vector3.up * mouseX);
    }

    Vector3 CalculateMoveDirection(float horizontal, float vertical)
    {
        Vector3 moveDirection;
        
        // Movimiento relativo al jugador (no a la cámara)
        // Forward = hacia donde mira el jugador
        // Right = lado derecho del jugador
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        
        // Asegurar que no hay componente Y
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        // Calcular dirección final
        moveDirection = forward * vertical + right * horizontal;

        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        return moveDirection;
    }

    void ApplyMovement(Vector3 moveDirection)
    {
        if (photonView != null && PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !photonView.IsMine) return;
        if (rb == null) return;

        // Calcular velocidad objetivo
        Vector3 targetVelocity = moveDirection * speed;
        targetVelocity.y = rb.velocity.y;

        // Aplicar velocidad directamente para mejor respuesta
        rb.velocity = targetVelocity;

        // Limitar velocidad máxima horizontal
        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0;
        if (horizontalVelocity.magnitude > speed)
        {
            horizontalVelocity = horizontalVelocity.normalized * speed;
            rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
        }

        if (showDebugInfo)
        {
            Debug.Log($"🎮 Velocidad: {rb.velocity.magnitude:F2}");
        }
    }

    void ProcessJump()
    {
        // Verificar si debe procesar el salto (singleplayer o jugador propio)
        if (photonView != null && PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !photonView.IsMine) 
        {
            if (showDebugInfo && Input.GetButtonDown("Jump"))
                Debug.Log("❌ SALTO IGNORADO - No es jugador propio (PhotonView)");
            return;
        }

        // Debug detallado del input
        bool jumpInputPressed = Input.GetButtonDown("Jump");
        bool cooldownOk = Time.time > lastJumpTime + jumpCooldown;
        
        if (showDebugInfo && jumpInputPressed)
        {
            Debug.Log($"🎮 INPUT JUMP DETECTADO - Cooldown OK: {cooldownOk}, Grounded: {isGrounded}, JumpCount: {jumpCount}");
        }

        if (jumpInputPressed && cooldownOk)
        {
            bool canPerformJump = false;
            bool isFirstJump = false;
            
            // Ajustar máximo de saltos basado en configuración
            int currentMaxJumps = enableDoubleJump ? 2 : 1;
            
            // Verificar límite de saltos
            if (jumpCount >= currentMaxJumps)
            {
                if (showDebugInfo) Debug.Log($"❌ LÍMITE DE SALTOS ALCANZADO - JumpCount: {jumpCount}/{currentMaxJumps}");
                return;
            }
            
            // Primer salto: en el suelo o coyote time
            if ((isGrounded || Time.time < lastGroundedTime + coyoteTime) && jumpCount == 0)
            {
                canPerformJump = true;
                isFirstJump = true;
                if (showDebugInfo) Debug.Log("✅ PRIMER SALTO APROBADO - En suelo o coyote time");
            }
            // Doble salto: en el aire y doble salto habilitado
            else if (enableDoubleJump && jumpCount == 1 && !isGrounded)
            {
                canPerformJump = true;
                isFirstJump = false;
                if (showDebugInfo) Debug.Log("✅ DOBLE SALTO APROBADO - En aire");
            }
            else
            {
                if (showDebugInfo) 
                {
                    Debug.Log($"❌ SALTO RECHAZADO - Grounded: {isGrounded}, JumpCount: {jumpCount}, DoubleJump: {enableDoubleJump}, MaxJumps: {currentMaxJumps}");
                    Debug.Log($"    LastGroundedTime: {lastGroundedTime:F2}, CurrentTime: {Time.time:F2}, CoyoteWindow: {Time.time < lastGroundedTime + coyoteTime}");
                }
            }

            if (canPerformJump)
            {
                if (showDebugInfo) Debug.Log($"🚀 EJECUTANDO SALTO - Photon: {photonView != null}, RB: {rb != null}");
                
                // Realizar salto
                if (photonView != null && PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
                {
                    // Multiplayer: usar RPC
                    photonView.RPC("DoJump", RpcTarget.All, isFirstJump);
                }
                else
                {
                    // Singleplayer: llamar directamente
                    DoJump(isFirstJump);
                }
            }
        }
        else if (showDebugInfo && jumpInputPressed && !cooldownOk)
        {
            Debug.Log($"❌ SALTO EN COOLDOWN - Tiempo restante: {(lastJumpTime + jumpCooldown - Time.time):F2}s");
        }
    }

    [PunRPC]
    void DoJump(bool isFirstJump = true)
    {
        if (rb != null)
        {
            // Calcular fuerza del salto
            float currentJumpPower = isFirstJump ? jumpPower : (jumpPower * secondJumpMultiplier);
            
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(Vector3.up * currentJumpPower, ForceMode.Impulse);
            lastJumpTime = Time.time;
            
            // Incrementar contador de saltos
            jumpCount++;
            
            if (showDebugInfo)
            {
                Debug.Log($"🚀 Salto ejecutado #{jumpCount} - Potencia: {currentJumpPower:F1} - Tipo: {(isFirstJump ? "Primer" : "Doble")}");
            }
        }

        // Efectos
        if (anim != null)
        {
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
        }

        if (mysfx != null && jumpfx != null)
        {
            mysfx.PlayOneShot(jumpfx);
        }

        if (dust != null)
        {
            dust.Play();
        }
    }

    void UpdateAnimations(Vector3 moveDirection)
    {
        if (anim == null) return;

        if (wasGrounded != isGrounded)
        {
            anim.SetBool("isJump", !isGrounded);
        }

        bool isMoving = moveDirection.magnitude > 0.1f;
        anim.SetBool("isMove", isMoving);
    }

    void Respawn()
    {
        // Verificar si debe procesar el respawn (singleplayer o jugador propio)
        if (photonView != null && PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !photonView.IsMine) return;

        Vector3 respawnPosition = lastGroundedPosition;
        if (respawnPosition.y < respawnHeight)
        {
            respawnPosition = Vector3.up * 5f;
        }

        if (photonView != null && PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            // Multiplayer: usar RPC
            photonView.RPC("NetworkRespawn", RpcTarget.All, respawnPosition);
        }
        else
        {
            // Singleplayer: llamar directamente
            NetworkRespawn(respawnPosition);
        }
    }

    [PunRPC]
    void NetworkRespawn(Vector3 position)
    {
        transform.position = position;
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Solo funciona en modo multiplayer
        if (photonView == null) return;
        
        if (stream.IsWriting)
        {
            // Datos que enviamos
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(rb != null ? rb.velocity : Vector3.zero);
            stream.SendNext(isGrounded);
            stream.SendNext(!isGrounded); // isJumping
        }
        else
        {
            // Datos que recibimos
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkVelocity = (Vector3)stream.ReceiveNext();
            networkIsGrounded = (bool)stream.ReceiveNext();
            networkIsJumping = (bool)stream.ReceiveNext();
            networkLerpTime = 0f;
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // Visualizar puntos de detección de suelo
        Vector3[] checkPoints = new Vector3[]
        {
            transform.position,
            transform.position + transform.forward * 0.3f,
            transform.position - transform.forward * 0.3f,
            transform.position + transform.right * 0.3f,
            transform.position - transform.right * 0.3f
        };

        // Dibujar rayos de detección
        Gizmos.color = isGrounded ? Color.green : Color.red;
        foreach (Vector3 point in checkPoints)
        {
            Gizmos.DrawLine(point, point + Vector3.down * groundCheckDistance);
            Gizmos.DrawWireSphere(point + Vector3.down * groundCheckDistance, 0.1f);
        }
        
        // Dibujar caja de detección
        Gizmos.color = Color.yellow;
        Vector3 boxCenter = transform.position + Vector3.down * (groundCheckDistance * 0.5f);
        Vector3 boxHalfExtents = new Vector3(0.4f, groundCheckDistance * 0.5f, 0.4f);
        Gizmos.DrawWireCube(boxCenter, boxHalfExtents * 2);
        
        // Posición de último suelo conocido
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(lastGroundedPosition, Vector3.one * 0.5f);
    }
    
    void OnGUI()
    {
        if (!showDebugInfo || !Application.isPlaying) return;
        
        // Panel de información detallada
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        
        GUILayout.Label("=== DEBUG JUGADOR PRINCIPAL ===");
        GUILayout.Label($"Nombre: {gameObject.name}");
        GUILayout.Label($"Activo: {enabled}");
        GUILayout.Label($"PhotonView: {(photonView != null ? "SÍ" : "NO")} | IsMine: {(photonView != null ? photonView.IsMine.ToString() : "N/A")}");
        
        GUILayout.Label("--- ESTADO ---");
        GUILayout.Label($"En suelo: {isGrounded}");
        GUILayout.Label($"Saltos: {jumpCount}/{(enableDoubleJump ? 2 : 1)}");
        GUILayout.Label($"Último salto: {Time.time - lastJumpTime:F2}s atrás");
        GUILayout.Label($"Último suelo: {Time.time - lastGroundedTime:F2}s atrás");
        GUILayout.Label($"Cooldown: {(Time.time > lastJumpTime + jumpCooldown ? "OK" : "ESPERANDO")}");
        
        GUILayout.Label("--- INPUT ---");
        GUILayout.Label($"Jump Input: {Input.GetButton("Jump")}");
        GUILayout.Label($"Jump Down: {Input.GetButtonDown("Jump")}");
        GUILayout.Label($"Cursor Lock: {Cursor.lockState}");
        
        GUILayout.Label("--- FÍSICA ---");
        GUILayout.Label($"Rigidbody: {(rb != null ? "OK" : "NULL")}");
        GUILayout.Label($"Velocidad: {(rb != null ? rb.velocity.ToString("F1") : "N/A")}");
        GUILayout.Label($"Ground Distance: {groundCheckDistance}m");
        GUILayout.Label($"Ground LayerMask: {groundLayerMask.value} (Layer: {LayerMask.LayerToName(Mathf.RoundToInt(Mathf.Log(groundLayerMask.value, 2)))})");
        
        // Verificar si hay objetos Ground detectados
        Vector3 rayOrigin = transform.position;
        bool rayHit = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayerMask);
        if (rayHit)
        {
            GUILayout.Label($"Ground Hit: {hit.collider.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
            GUILayout.Label($"Hit Distance: {hit.distance:F2}m");
        }
        else
        {
            GUILayout.Label("Ground Hit: NONE");
        }
        
        // Detectar conflictos
        GUILayout.Label("--- CONFLICTOS ---");
        var otherMovements = FindObjectsOfType<MonoBehaviour>().Where(mb => 
            mb != this && 
            (mb.GetType().Name.Contains("Movement") || 
             mb.GetType().Name.Contains("Player") || 
             mb.GetType().Name.Contains("Jump"))
        ).ToArray();
        
        if (otherMovements.Length > 0)
        {
            GUILayout.Label($"Scripts potencialmente conflictivos: {otherMovements.Length}");
            foreach (var script in otherMovements.Take(3))
            {
                GUILayout.Label($"  - {script.GetType().Name} en {script.gameObject.name}");
            }
        }
        else
        {
            GUILayout.Label("No hay scripts conflictivos detectados");
        }
        
        GUILayout.EndArea();
    }

    /// <summary>
    /// 🔓 Control de cursor con Escape
    /// </summary>
    void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (showDebugInfo) Debug.Log("🔓 Cursor desbloqueado");
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (showDebugInfo) Debug.Log("🔒 Cursor bloqueado");
            }
        }
    }
}



  





