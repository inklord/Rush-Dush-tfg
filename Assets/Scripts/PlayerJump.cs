using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Verifica si el jugador está en el suelo
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Evita doble salto con impulso acumulado
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
