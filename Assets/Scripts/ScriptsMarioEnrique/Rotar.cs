using UnityEngine;
using Photon.Pun;

public class RotarCilindro : MonoBehaviour
{
    public float rotationSpeed = 100f; // Velocidad de rotación
    public float forceMagnitude = 5f; // Fuerza aplicada al jugador
    public Vector3 rotationAxis = Vector3.up; // Eje de rotación

    private void Update()
    {
        // Rotar el cilindro continuamente
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }

    private void OnCollisionStay(Collision collision)
    {
        // Verifica si el objeto tiene un PhotonView y es el dueño de la instancia
        PhotonView playerPhotonView = collision.collider.GetComponent<PhotonView>();

        if (playerPhotonView != null && playerPhotonView.IsMine)
        {
            Rigidbody playerRb = collision.collider.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // Generamos un empuje en la dirección de la rotación para afectar el equilibrio
                Vector3 pushDirection = transform.right * Mathf.Sin(Time.time * rotationSpeed);
                playerRb.AddForce(pushDirection * forceMagnitude, ForceMode.Acceleration);
            }
        }
    }
}
