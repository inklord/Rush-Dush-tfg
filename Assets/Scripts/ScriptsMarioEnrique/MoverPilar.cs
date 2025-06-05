using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPillars : MonoBehaviour
{
    public float moveDistance = 15f; // Distancia m�xima por defecto que el pilar se mover�
    [Range(0.1f, 10f)]
    public float moveSpeed = 2f; // Velocidad de movimiento, ahora puede ser mayor a 1
    public bool randomStartPosition = true; // Si los pilares inician en posiciones aleatorias
    private Vector3 startPosition; // Posici�n inicial del pilar
    private Vector3 targetPosition; // Posici�n objetivo
    private bool movingForward = true; // Direcci�n de movimiento

    void Start()
    {
        // Guardamos la posici�n inicial
        startPosition = transform.localPosition;

        // Si es necesario, asignamos una posici�n aleatoria al inicio
        if (randomStartPosition)
        {
            float randomOffset = Random.Range(-moveDistance, moveDistance);
            transform.localPosition = new Vector3(startPosition.x + randomOffset, startPosition.y, startPosition.z);
        }

        // Calculamos la posici�n objetivo al mover el pilar
        targetPosition = new Vector3(startPosition.x + moveDistance, startPosition.y, startPosition.z);
    }

    void Update()
    {
        // Movemos el pilar entre la posici�n inicial y la posici�n objetivo
        if (movingForward)
        {
            // Mueve el pilar hacia la posici�n objetivo
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);

            // Si el pilar ha llegado a la posici�n objetivo, invertimos la direcci�n
            if (Vector3.Distance(transform.localPosition, targetPosition) < 0.1f)
            {
                movingForward = false;
            }
        }
        else
        {
            // Mueve el pilar hacia la posici�n inicial
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, startPosition, moveSpeed * Time.deltaTime);

            // Si el pilar ha llegado a la posici�n inicial, invertimos la direcci�n
            if (Vector3.Distance(transform.localPosition, startPosition) < 0.1f)
            {
                movingForward = true;
            }
        }
    }
}
