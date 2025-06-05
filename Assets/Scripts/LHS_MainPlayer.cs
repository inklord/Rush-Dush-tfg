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

    Animator anim;
    Rigidbody rigid;

    // Control de salto
    bool isGrounded = false;
    bool wasGrounded = false;
    bool jDown;
    
    

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
    }

    private void Update()
    {
        CheckGrounded();
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

        // Debug detallado
        if (showDebugInfo && (wasGrounded != isGrounded || Vector3.Distance(lastPosition, transform.position) > 1f))
        {
            Debug.Log($"Pos: {transform.position:F1} | Grounded: {isGrounded} | Distance: {minDistance:F2} | VelY: {rigid.velocity.y:F2}");
            
            if (hit.collider != null)
            {
                Debug.Log($"Hit Object: {hit.collider.name} | Tag: {hit.collider.tag} | Layer: {hit.collider.gameObject.layer}");
            }
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
        Jump();
        
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
        jDown = Input.GetButtonDown("Jump");
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

    void Jump()
    {
        // Solo salta si está en el suelo y presiona el botón de salto
        if (jDown && isGrounded)
        {
            Debug.Log("Saltando!"); // Debug para confirmar que salta
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);

            // Forzar el estado de no estar en el suelo temporalmente
            isGrounded = false;
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            mysfx.PlayOneShot(jumpfx);
            dust.Play();
        }
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

        // Paredes (colisión con rebote)
        if (collision.collider.tag == "Wall")
        {
            

            rigid.velocity = new Vector3(0, 0, 0);
            rigid.AddForce(Vector3.back * bounceForce, ForceMode.Impulse);

            mysfx.PlayOneShot(bouncefx);
            bounce.Play();

            bounce.transform.position = transform.position;
        }
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
    }
}



  



