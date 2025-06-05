using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPillars : MonoBehaviour
{
    public float moveDistance = 15f; // Distancia máxima por defecto que el pilar se moverá
    [Range(0.1f, 10f)]
    public float moveSpeed = 2f; // Velocidad de movimiento, ahora puede ser mayor a 1
    public bool randomStartPosition = true; // Si los pilares inician en posiciones aleatorias
    private Vector3 startPosition; // Posición inicial del pilar
    private Vector3 targetPosition; // Posición objetivo
    private bool movingForward = true; // Dirección de movimiento

    void Start()
    {
        // Guardamos la posición inicial
        startPosition = transform.localPosition;

        // Si es necesario, asignamos una posición aleatoria al inicio
        if (randomStartPosition)
        {
            float randomOffset = Random.Range(-moveDistance, moveDistance);
            transform.localPosition = new Vector3(startPosition.x + randomOffset, startPosition.y, startPosition.z);
        }

        // Calculamos la posición objetivo al mover el pilar
        targetPosition = new Vector3(startPosition.x + moveDistance, startPosition.y, startPosition.z);
    }

    void Update()
    {
        // Movemos el pilar entre la posición inicial y la posición objetivo
        if (movingForward)
        {
            // Mueve el pilar hacia la posición objetivo
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);

            // Si el pilar ha llegado a la posición objetivo, invertimos la dirección
            if (Vector3.Distance(transform.localPosition, targetPosition) < 0.1f)
            {
                movingForward = false;
            }
        }
        else
        {
            // Mueve el pilar hacia la posición inicial
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, startPosition, moveSpeed * Time.deltaTime);

            // Si el pilar ha llegado a la posición inicial, invertimos la dirección
            if (Vector3.Distance(transform.localPosition, startPosition) < 0.1f)
            {
                movingForward = true;
            }
        }
    }
}
