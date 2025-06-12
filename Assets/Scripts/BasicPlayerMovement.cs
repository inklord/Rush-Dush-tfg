using UnityEngine;

/// <summary>
/// 🎮 MOVIMIENTO BÁSICO SIN RED - Para verificar funcionamiento
/// Versión ultra-simple sin dependencias de Photon
/// </summary>
public class BasicPlayerMovement : MonoBehaviour
{
    [Header("🎮 Movimiento")]
    public float speed = 10f;
    public float jumpPower = 15f;
    public float rotateSpeed = 5f;
    
    [Header("🎯 Referencias Opcionales")]
    public ParticleSystem dustEffect;
    public AudioSource audioSource;
    public AudioClip jumpSound;
    
    // Componentes
    private Rigidbody rb;
    private Animator anim;
    private Camera currentCamera;
    
    // Variables de movimiento
    private float horizontal;
    private float vertical;
    private bool isGrounded;
    private bool jumpPressed;
    
    void Start()
    {
        // Obtener componentes
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        currentCamera = Camera.main;
        
        // Configurar cámara para seguir a este jugador
        SetupCamera();
        
    }
    
    void Update()
    {
        HandleInput();
        CheckGrounded();
        UpdateAnimations();
    }
    
    void FixedUpdate()
    {
        Move();
        Jump();
    }
    
    /// <summary>
    /// 🎮 Manejar input del jugador
    /// </summary>
    void HandleInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        jumpPressed = Input.GetButtonDown("Jump");
    }
    
    /// <summary>
    /// 🌍 Verificar si está en el suelo
    /// </summary>
    void CheckGrounded()
    {
        // Raycast simple hacia abajo
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 1.2f);
        
        if (isGrounded && dustEffect != null && rb != null && rb.velocity.magnitude > 2f)
        {
            if (!dustEffect.isPlaying)
                dustEffect.Play();
        }
        else if (dustEffect != null && dustEffect.isPlaying)
        {
            dustEffect.Stop();
        }
    }
    
    /// <summary>
    /// 🏃 Movimiento del jugador
    /// </summary>
    void Move()
    {
        if (rb == null) return;
        
        // Calcular dirección de movimiento
        Vector3 moveDirection = Vector3.zero;
        
        if (currentCamera != null)
        {
            // Movimiento relativo a la cámara
            Vector3 forward = currentCamera.transform.forward;
            Vector3 right = currentCamera.transform.right;
            
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            
            moveDirection = forward * vertical + right * horizontal;
        }
        else
        {
            // Movimiento mundial si no hay cámara
            moveDirection = new Vector3(horizontal, 0, vertical);
        }
        
        // Aplicar movimiento
        if (moveDirection.magnitude > 0.1f)
        {
            // Mover
            Vector3 movement = moveDirection * speed * Time.fixedDeltaTime;
            rb.MovePosition(transform.position + movement);
            
            // Rotar hacia la dirección de movimiento
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
        }
    }
    
    /// <summary>
    /// 🚀 Salto del jugador
    /// </summary>
    void Jump()
    {
        if (rb == null) return;
        
        if (jumpPressed && isGrounded)
        {
            // Aplicar fuerza de salto
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            
            // Reproducir sonido
            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
            
            // Activar shake de cámara
            var cameraScript = FindObjectOfType<MovimientoCamaraSimple>();
            if (cameraScript != null)
            {
                cameraScript.ShakeCamera(0.3f, 1f);
            }
            
            
        }
    }
    
    /// <summary>
    /// 🎭 Actualizar animaciones
    /// </summary>
    void UpdateAnimations()
    {
        if (anim != null)
        {
            float animSpeed = new Vector3(horizontal, 0, vertical).magnitude;
            anim.SetFloat("Speed", animSpeed);
            anim.SetBool("Grounded", isGrounded);
        }
    }
    
    /// <summary>
    /// 📷 Configurar cámara para seguir este jugador
    /// </summary>
    void SetupCamera()
    {
        if (currentCamera != null)
        {
            MovimientoCamaraSimple cameraScript = currentCamera.GetComponent<MovimientoCamaraSimple>();
            if (cameraScript == null)
            {
                cameraScript = currentCamera.gameObject.AddComponent<MovimientoCamaraSimple>();
            }
            cameraScript.SetPlayer(transform);
            
        }
    }
    
    /// <summary>
    /// ⚡ Manejar colisiones simples
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (rb == null) return;
        
        // Si golpea una pared, aplicar pequeño rebote
        if (collision.gameObject.CompareTag("Wall"))
        {
            Vector3 bounceDirection = Vector3.Reflect(transform.forward, collision.contacts[0].normal);
            rb.AddForce(bounceDirection * 5f, ForceMode.Impulse);
            
            // Shake de cámara pequeño
            var cameraScript = FindObjectOfType<MovimientoCamaraSimple>();
            if (cameraScript != null)
            {
                cameraScript.ShakeCamera(0.2f, 0.5f);
            }
        }
    }
    
    /// <summary>
    /// 🎯 Para compatibilidad con sistemas existentes
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }
    
    public float GetCurrentSpeed()
    {
        return rb != null ? rb.velocity.magnitude : 0f;
    }
} 
