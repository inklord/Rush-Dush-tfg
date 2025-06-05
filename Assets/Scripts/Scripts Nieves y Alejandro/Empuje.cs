using UnityEngine;

public class EmpujarAlContacto : MonoBehaviour
{
    public float fuerzaEmpuje = 50f; // La fuerza con la que se empujará el objeto
    public float tiempoDeEmpuje = 0.1f; // El tiempo entre cada empuje continuo

    private void OnCollisionStay(Collision collision)
    {
        // Verifica si el objeto que colisiona tiene un Rigidbody
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Calcula la dirección del empuje, en este caso se empuja en la dirección del objeto que colisiona
            Vector3 direccionEmpuje = (collision.transform.position - transform.position).normalized;
            
            // Aplica la fuerza al objeto en la dirección calculada, de forma continua mientras haya contacto
            rb.AddForce(direccionEmpuje * fuerzaEmpuje, ForceMode.Force);
        }
    }
}
