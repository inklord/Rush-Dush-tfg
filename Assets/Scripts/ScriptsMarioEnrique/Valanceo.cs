using UnityEngine;

public class valanceoaxe : MonoBehaviour
{
    public float swingSpeed = 2f; // Velocidad del balanceo
    public float swingAngle = 45f; // �ngulo m�ximo del balanceo
    public float forceMagnitude = 15f; // Fuerza aplicada al jugador

    private Quaternion startRotation;
    private float timeCounter;

    private void Start()
    {
        startRotation = transform.rotation; // Guardamos la rotaci�n inicial
    }

    private void Update()
    {
        // Movimiento pendular con seno para balanceo suave
        timeCounter += Time.deltaTime * swingSpeed;
        float angle = Mathf.Sin(timeCounter) * swingAngle;
        transform.rotation = startRotation * Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Obtener el Rigidbody del objeto impactado
        Rigidbody playerRb = other.GetComponent<Rigidbody>();

        if (playerRb != null) // Si tiene un Rigidbody, le aplicamos fuerza
        {
            // Calcular direcci�n del golpe, pero solo en el eje X (hacia los lados)
            Vector3 hitDirection = other.transform.position - transform.position;
            hitDirection.y = 0f; // No afectamos el eje Y (sin empuje hacia arriba)

            // Si quieres empujar hacia el lado, puedes usar `hitDirection.x`
            playerRb.AddForce(hitDirection.normalized * forceMagnitude, ForceMode.Impulse);
        }
    }
}
