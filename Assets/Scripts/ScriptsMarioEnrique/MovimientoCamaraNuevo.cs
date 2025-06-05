using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoCamaraNuevo : MonoBehaviour
{
    public Transform player; // Referencia al jugador
    public Vector3 offset = new Vector3(0, 5, -10); // Distancia inicial de la cámara
    public float smoothSpeed = 5f; // Velocidad de suavizado de la cámara

    [Header("Rotación con el Ratón")]
    public float sensibilidadX = 200f; // Sensibilidad del mouse en el eje X
    public float sensibilidadY = 150f; // Sensibilidad del mouse en el eje Y
    public float limiteRotacionMin = -30f; // Límite inferior de la cámara
    public float limiteRotacionMax = 70f; // Límite superior de la cámara

    private float rotacionActualX = 0f;
    private float rotacionActualY = 10f; // Inclinación inicial

    [Header("Zoom con la Rueda del Ratón")]
    public float zoomSpeed = 2f;
    public float minZoom = 5f;
    public float maxZoom = 20f;
    private float distanciaActual;

    void Start()
    {
        StartCoroutine(FindPlayer());
        distanciaActual = offset.magnitude; // Distancia inicial
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor para mejorar la experiencia
        Cursor.visible = false; // Oculta el cursor
    }

    IEnumerator FindPlayer()
    {
        while (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("✅ Jugador encontrado y cámara asignada.");
            }
            else
            {
                Debug.LogWarning("⏳ Buscando jugador...");
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Rotación de la cámara con el mouse o joystick derecho
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadY * Time.deltaTime;

        rotacionActualX += mouseX;
        rotacionActualY -= mouseY;
        rotacionActualY = Mathf.Clamp(rotacionActualY, limiteRotacionMin, limiteRotacionMax);

        // Aplicar la rotación
        Quaternion rotacionFinal = Quaternion.Euler(rotacionActualY, rotacionActualX, 0);
        Vector3 nuevaPosicion = player.position - (rotacionFinal * Vector3.forward * distanciaActual) + Vector3.up * 2f;

        // Aplicar zoom con la rueda del ratón
        float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        distanciaActual = Mathf.Clamp(distanciaActual - scroll, minZoom, maxZoom);

        // Suavizar el movimiento de la cámara
        transform.position = Vector3.Lerp(transform.position, nuevaPosicion, smoothSpeed * Time.deltaTime);
        transform.LookAt(player.position + Vector3.up * 2f); // Mirar al jugador
    }
}
