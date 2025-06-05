using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaBalanceo : MonoBehaviour
{
    public float velocidadBalanceo = 2f; // Velocidad de balanceo de la plataforma
    public float anguloMaximo = 20f; // Máximo ángulo de inclinación
    public Transform centroPlataforma; // Centro de la plataforma (puede ser un objeto vacío en el centro)

    private List<Transform> jugadoresIzquierda = new List<Transform>();
    private List<Transform> jugadoresDerecha = new List<Transform>();

    private Rigidbody rb;
    private float inclinacionActual = 0f;
    private float inclinacionDeseada = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (centroPlataforma == null)
        {
            centroPlataforma = transform; // Si no se asigna, se usa el transform de la plataforma
        }
    }

    void Update()
    {
        CalcularPeso();
        BalancearPlataforma();
    }

    // Calcula el número de jugadores en cada lado de la plataforma
    private void CalcularPeso()
    {
        jugadoresIzquierda.Clear();
        jugadoresDerecha.Clear();

        // Iterar sobre todos los jugadores que estén sobre la plataforma
        foreach (var jugador in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (IsJugadorSobrePlataforma(jugador.transform))
            {
                float ladoDelJugador = jugador.transform.position.x - centroPlataforma.position.x;

                if (ladoDelJugador < 0)
                {
                    jugadoresIzquierda.Add(jugador.transform); // Jugador en el lado izquierdo
                }
                else
                {
                    jugadoresDerecha.Add(jugador.transform); // Jugador en el lado derecho
                }
            }
        }
    }

    // Verifica si el jugador está sobre la plataforma
    private bool IsJugadorSobrePlataforma(Transform jugador)
    {
        // Compara si el jugador está en el rango de la plataforma (puedes ajustar esto según el tamaño de la plataforma)
        return Mathf.Abs(jugador.position.y - transform.position.y) < 2f; // Ajusta el umbral según sea necesario
    }

    // Realiza el balanceo de la plataforma hacia el lado con más peso
    private void BalancearPlataforma()
    {
        // Calculamos el peso en cada lado
        float pesoIzquierda = jugadoresIzquierda.Count;
        float pesoDerecha = jugadoresDerecha.Count;

        // Determinamos la inclinación deseada
        if (pesoIzquierda > pesoDerecha)
        {
            inclinacionDeseada = anguloMaximo;
        }
        else if (pesoDerecha > pesoIzquierda)
        {
            inclinacionDeseada = -anguloMaximo;
        }
        else
        {
            inclinacionDeseada = 0f; // Sin inclinación si el peso es igual
        }

        // Suavizamos la inclinación actual para no hacer el movimiento tan brusco
        inclinacionActual = Mathf.Lerp(inclinacionActual, inclinacionDeseada, Time.deltaTime * velocidadBalanceo);

        // Aplicamos la rotación final de la plataforma
        transform.rotation = Quaternion.Euler(0, 0, inclinacionActual);
    }
}
