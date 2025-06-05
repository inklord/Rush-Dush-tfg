using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBarriers : MonoBehaviour
{
    public enum Axis { X, Y, Z }  // Enum para elegir el eje de movimiento

    public Axis moveAxis = Axis.X; // Eje de movimiento, por defecto en el eje X
    public float moveDistance = 15f; // Distancia máxima que la barrera se moverá
    [Range(0.1f, 10f)]
    public float moveSpeed = 2f; // Velocidad de movimiento
    public bool randomStartPosition = true; // Si las barreras inician en posiciones aleatorias
    private Vector3 startPosition; // Posición inicial de la barrera
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
            switch (moveAxis)
            {
                case Axis.X:
                    transform.localPosition = new Vector3(startPosition.x + randomOffset, startPosition.y, startPosition.z);
                    break;
                case Axis.Y:
                    transform.localPosition = new Vector3(startPosition.x, startPosition.y + randomOffset, startPosition.z);
                    break;
                case Axis.Z:
                    transform.localPosition = new Vector3(startPosition.x, startPosition.y, startPosition.z + randomOffset);
                    break;
            }
        }

        // Calculamos la posición objetivo al mover la barrera
        switch (moveAxis)
        {
            case Axis.X:
                targetPosition = new Vector3(startPosition.x + moveDistance, startPosition.y, startPosition.z);
                break;
            case Axis.Y:
                targetPosition = new Vector3(startPosition.x, startPosition.y + moveDistance, startPosition.z);
                break;
            case Axis.Z:
                targetPosition = new Vector3(startPosition.x, startPosition.y, startPosition.z + moveDistance);
                break;
        }
    }

    void Update()
    {
        // Movemos la barrera entre la posición inicial y la posición objetivo
        if (movingForward)
        {
            // Mueve la barrera hacia la posición objetivo
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);

            // Si la barrera ha llegado a la posición objetivo, invertimos la dirección
            if (Vector3.Distance(transform.localPosition, targetPosition) < 0.1f)
            {
                movingForward = false;
            }
        }
        else
        {
            // Mueve la barrera hacia la posición inicial
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, startPosition, moveSpeed * Time.deltaTime);

            // Si la barrera ha llegado a la posición inicial, invertimos la dirección
            if (Vector3.Distance(transform.localPosition, startPosition) < 0.1f)
            {
                movingForward = true;
            }
        }
    }
}
