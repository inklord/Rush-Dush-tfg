using System.Collections;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// 🎮 MOVIMIENTO SIMPLE DE JUGADOR - Adaptado para Photon
/// Versión ultra-simplificada del LHS_MainPlayer para máxima compatibilidad
/// </summary>
public class SimplePlayerMovement : MonoBehaviourPun, IPunObservable
{
    [Header("🎮 Movimiento")]
    public float speed = 10f;
    public float jumpPower = 15f;
    public float rotateSpeed = 5f;
    
    [Header("🎯 Referencias")]
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
    
    // Variables de red
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private bool networkGrounded;
    private float networkSpeed;
    
    void Start()
    {
        // Obtener componentes
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        currentCamera = Camera.main;
        
        // Solo el owner controla este jugador
        if (photonView.IsMine)
        {
            // Configurar cámara para seguir a este jugador
            SetupCamera();
            Debug.Log("✅ Mi jugador - Controles activados");
        }
        else
        {
            Debug.Log("👥 Jugador remoto - Solo visualización");
        }
    }
    
    void Update()
    {
        if (photonView.IsMine)
        {
            // Solo el owner controla
            HandleInput();
            CheckGrounded();
        }
        else
        {
            // Interpolar movimiento para jugadores remotos
            InterpolateMovement();
        }
        
        UpdateAnimations();
    }
    
    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            // Solo el owner se mueve
            Move();
            Jump();
        }
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
        
        if (isGrounded && dustEffect != null && rb.velocity.magnitude > 2f)
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
            photonView.RPC("NetworkShakeCamera", RpcTarget.All, 0.3f, 1f);
            
            Debug.Log("🚀 ¡Salto!");
        }
    }
    
    /// <summary>
    /// 🎭 Actualizar animaciones
    /// </summary>
    void UpdateAnimations()
    {
        if (anim != null)
        {
            float animSpeed = photonView.IsMine ? 
                new Vector3(horizontal, 0, vertical).magnitude : 
                networkSpeed;
                
            anim.SetFloat("Speed", animSpeed);
            anim.SetBool("Grounded", photonView.IsMine ? isGrounded : networkGrounded);
        }
    }
    
    /// <summary>
    /// 🌐 Interpolación para jugadores remotos
    /// </summary>
    void InterpolateMovement()
    {
        // Interpolar posición y rotación suavemente
        transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
        transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
    }
    
    /// <summary>
    /// 📷 Configurar cámara para seguir este jugador
    /// </summary>
    void SetupCamera()
    {
        if (!photonView.IsMine)
        {
            Debug.Log("❌ No configurar cámara para jugador remoto");
            return;
        }

        if (currentCamera != null)
        {
            MovimientoCamaraSimple cameraScript = currentCamera.GetComponent<MovimientoCamaraSimple>();
            if (cameraScript == null)
            {
                cameraScript = currentCamera.gameObject.AddComponent<MovimientoCamaraSimple>();
            }

            // Verificar si la cámara ya está siguiendo a otro jugador
            if (cameraScript.player != null && cameraScript.player != transform)
            {
                Debug.LogWarning("⚠️ La cámara ya está siguiendo a otro jugador");
                return;
            }

            cameraScript.SetPlayer(transform);
            Debug.Log($"✅ Cámara asignada al jugador: {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró la cámara principal");
        }
    }
    
    /// <summary>
    /// 💥 Shake de cámara via RPC
    /// </summary>
    [PunRPC]
    void NetworkShakeCamera(float duration, float intensity)
    {
        var cameraScript = FindObjectOfType<MovimientoCamaraSimple>();
        if (cameraScript != null)
        {
            cameraScript.ShakeCamera(duration, intensity);
        }
    }
    
    /// <summary>
    /// 📡 Sincronización de red
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Enviar datos
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(isGrounded);
            stream.SendNext(new Vector3(horizontal, 0, vertical).magnitude);
        }
        else
        {
            // Recibir datos
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkGrounded = (bool)stream.ReceiveNext();
            networkSpeed = (float)stream.ReceiveNext();
        }
    }
    
    /// <summary>
    /// ⚡ Manejar colisiones simples
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        // Solo el owner procesa colisiones
        if (!photonView.IsMine) return;
        
        // Si golpea una pared, aplicar pequeño rebote
        if (collision.gameObject.CompareTag("Wall"))
        {
            Vector3 bounceDirection = Vector3.Reflect(transform.forward, collision.contacts[0].normal);
            rb.AddForce(bounceDirection * 5f, ForceMode.Impulse);
            
            // Shake de cámara pequeño
            photonView.RPC("NetworkShakeCamera", RpcTarget.All, 0.2f, 0.5f);
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