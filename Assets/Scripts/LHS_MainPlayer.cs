using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

/// <summary>
/// 🎮 MAIN PLAYER CONTROLLER - Movimiento y cámara
/// </summary>
public class LHS_MainPlayer : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("🎮 Movimiento")]
    public float speed = 10f;
    public float rotateSpeed = 5f;
    public float jumpPower = 5f;
    public bool UseCameraRotation = true;

    [Header("🎯 Configuración")]
    public float groundCheckDistance = 2f;
    public LayerMask groundLayerMask = -1;
    public bool showDebugInfo = true;
    public float jumpCooldown = 0.1f;
    public float coyoteTime = 0.1f;
    public float respawnHeight = -10f;

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
        // Obtener referencias
        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        anim = GetComponentInChildren<Animator>();

        // Inicializar variables de red
        networkPosition = transform.position;
        networkRotation = transform.rotation;
        networkLerpTime = 0f;

        // Configuración basada en propiedad
        if (photonView.IsMine)
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

        LHS_Camera cameraScript = currentCamera.GetComponent<LHS_Camera>();
        if (cameraScript == null)
        {
            cameraScript = currentCamera.gameObject.AddComponent<LHS_Camera>();
        }

        cameraScript.player = gameObject;
        Debug.Log($"📷 Cámara configurada para seguir a: {gameObject.name}");
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            // Interpolación suave para jugadores remotos
            UpdateRemotePlayer();
            return;
        }

        // Control local
        UpdateLocalPlayer();
    }

    void UpdateLocalPlayer()
    {
        // Ground check
        wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayerMask);

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            lastGroundedPosition = transform.position;
        }

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
        // Interpolar posición y rotación
        transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * NETWORK_SMOOTHING);
        transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * NETWORK_SMOOTHING);

        // Actualizar animaciones
        if (anim != null)
        {
            anim.SetBool("isMove", networkVelocity.magnitude > 0.1f);
            anim.SetBool("isJump", networkIsJumping);
        }
    }

    Vector3 CalculateMoveDirection(float horizontal, float vertical)
    {
        Vector3 moveDirection;
        if (UseCameraRotation && currentCamera != null)
        {
            Vector3 forward = currentCamera.transform.forward;
            Vector3 right = currentCamera.transform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            moveDirection = forward * vertical + right * horizontal;
        }
        else
        {
            moveDirection = new Vector3(horizontal, 0, vertical);
        }

        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        return moveDirection;
    }

    void ApplyMovement(Vector3 moveDirection)
    {
        if (!photonView.IsMine || rb == null) return;

        Vector3 targetVelocity = moveDirection * speed;
        targetVelocity.y = rb.velocity.y;
        rb.velocity = targetVelocity;

        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotateSpeed * Time.deltaTime);
        }
    }

    void ProcessJump()
    {
        if (!photonView.IsMine) return;

        if (Input.GetButtonDown("Jump") && Time.time > lastJumpTime + jumpCooldown)
        {
            if (isGrounded || Time.time < lastGroundedTime + coyoteTime)
            {
                // Realizar salto
                photonView.RPC("DoJump", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void DoJump()
    {
        if (rb != null)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            lastJumpTime = Time.time;
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
        if (!photonView.IsMine) return;

        Vector3 respawnPosition = lastGroundedPosition;
        if (respawnPosition.y < respawnHeight)
        {
            respawnPosition = Vector3.up * 5f;
        }

        photonView.RPC("NetworkRespawn", RpcTarget.All, respawnPosition);
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
        if (stream.IsWriting)
        {
            // Datos que enviamos
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(rb.velocity);
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

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lastGroundedPosition, 0.5f);
    }
}



  





