using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmpujeVentilador : MonoBehaviour
{
    public float fuerzaEmpuje = 10f; // Intensidad del empuje
    public Vector3 direccionEmpuje = Vector3.forward; // Dirección del viento

    private void OnTriggerStay(Collider other)
    {
        // Verifica si el objeto que entra es el jugador
        if (other.CompareTag("Player")) 
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Aplica una fuerza continua en la dirección del ventilador
                rb.AddForce(direccionEmpuje.normalized * fuerzaEmpuje, ForceMode.Acceleration);
            }
        }
    }
}
