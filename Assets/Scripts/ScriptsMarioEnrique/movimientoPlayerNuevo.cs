using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movimientoPlayerNuevo : MonoBehaviour
{
    private bool jumping; // Booleano por saber si está saltando
    private Rigidbody rb; // Referencia al componente Rigidbody
    public bool movimientoAxis; // Si utilizamos movimiento por axis o teclas
    public float speed;
    public enum tipoFuerza
    {
        fuerzaCoordenasasAbsolutas, fuerzaCoordenadasRelativas, fuerzaTorsionCoordenadasAbsolutas,
        fuerzaTorsionCoordenadasRelativas, fuerzaEnPosicion
    }

    // Enumeración para establecer los modos de fuerza
    public enum modoFuerza
    {
        Force, Acceleration, Impulse, VelocityChange
    }

    public tipoFuerza fuerza;
    public modoFuerza fuerzaSalto;

    private Transform camTransform; // Referencia a la cámara

    void Start()
    {
        // Guardamos en rb el componente Rigidbody del objeto
        rb = GetComponent<Rigidbody>();
        jumping = false;
        movimientoAxis = false;
        speed = 20.0f;
        fuerza = tipoFuerza.fuerzaCoordenasasAbsolutas; // Para aplicar por defecto AddForce
        fuerzaSalto = modoFuerza.Force;

        // Referencia a la cámara
        camTransform = Camera.main.transform;
    }

    // Detección de colisiones
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Superficie"))
        {
            jumping = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Destructor")
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        Vector3 vectorMovimiento = Vector3.zero;

        if (!movimientoAxis)
        {
            // Movimiento hacia la izquierda (A o flecha izquierda)
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                // Movimiento en relación con la cámara
                Vector3 cameraDir = camTransform.TransformDirection(-1.0f, 0, 0);
                vectorMovimiento = new Vector3(cameraDir.x, 0, cameraDir.z);
            }

            // Movimiento hacia la derecha (D o flecha derecha)
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                // Movimiento en relación con la cámara
                Vector3 cameraDir = camTransform.TransformDirection(1.0f, 0, 0);
                vectorMovimiento = new Vector3(cameraDir.x, 0, cameraDir.z);
            }

            // Movimiento hacia adelante (W o flecha arriba)
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                // Movimiento en relación con la cámara
                Vector3 cameraDir = camTransform.TransformDirection(0, 0, 1);
                vectorMovimiento = new Vector3(cameraDir.x, 0, cameraDir.z);
            }

            // Movimiento hacia atrás (X o flecha abajo)
            if (Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.DownArrow))
            {
                // Movimiento en relación con la cámara
                Vector3 cameraDir = camTransform.TransformDirection(0, 0, -1);
                vectorMovimiento = new Vector3(cameraDir.x, 0, cameraDir.z);
            }
        }
        else
        {   
            // Movimiento utilizando los ejes de entrada (para movimientos por axis)
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");
            vectorMovimiento = new Vector3(moveHorizontal, 0.0f, moveVertical);
        }

        // Aplicación del movimiento

        switch (fuerza)
        {
            case tipoFuerza.fuerzaCoordenasasAbsolutas:
                rb.AddForce(vectorMovimiento * speed);
                break;
            case tipoFuerza.fuerzaCoordenadasRelativas:
                rb.AddRelativeForce(vectorMovimiento * speed);
                break;
            case tipoFuerza.fuerzaTorsionCoordenadasAbsolutas:
                rb.AddTorque(vectorMovimiento * speed);
                break;
            case tipoFuerza.fuerzaTorsionCoordenadasRelativas:
                rb.AddRelativeTorque(vectorMovimiento * speed);
                break;
            case tipoFuerza.fuerzaEnPosicion:
                rb.AddForceAtPosition(vectorMovimiento * speed, new Vector3(0, 0, 0));
                break;
        }

        // Gestión del salto
        if (Input.GetKeyDown(KeyCode.Z) && jumping == false)
        {
            jumping = true;
            if (rb != null)
            {
                switch (fuerzaSalto)
                {
                    case modoFuerza.Force:
                        rb.AddForce(new Vector3(0.0f, 300.0f, 0.0f), ForceMode.Force);
                        break;
                    case modoFuerza.Acceleration:
                        rb.AddForce(new Vector3(0.0f, 300.0f, 0.0f), ForceMode.Acceleration);
                        break;
                    case modoFuerza.Impulse:
                        rb.AddForce(new Vector3(0.0f, 6.0f, 0.0f), ForceMode.Impulse);
                        break;
                    case modoFuerza.VelocityChange:
                        rb.AddForce(new Vector3(0.0f, 6.0f, 0.0f), ForceMode.VelocityChange);
                        break;
                }
            }
        }
    }
}
