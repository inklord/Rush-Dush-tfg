using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

public class IAPuertas : MonoBehaviour
{
    public float rangoBusqueda = 20f;
    public float distanciaComprobacion = 1.5f;
    public float tiempoBloqueoParaDescartar = 1.2f;
    public string tagPuerta = "Puerta";
    public string tagDestinoFinal = "RealDestPos";
    public bool esLider = false; // Indica si esta IA es la que sabe qué puerta es la correcta
    [Header("Configuración de Comportamiento")]
    public float tiempoMinEspera = 0.2f;
    public float tiempoMaxEspera = 1.0f;
    public float distanciaDeteccionPuertaRota = 10f;
    public float velocidadCambioObjetivo = 2f;
    private bool haAtravesadoPuerta = false;
    private float tiempoEsperaProximaFila = 1f;

    private NavMeshAgent agent;
    private Rigidbody rigid;
    private GameObject destinoFinal;
    private Puerta puertaActual;
    private Vector3 ultimaPosicion;
    private float tiempoAtascado = 0f;
    private bool yendoAlDestino = false;
    private List<Puerta> puertasVisitadas = new List<Puerta>();
    private FilaPuertas filaActual;
    private float tiempoEsperaAntesDeEntrar;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rigid = GetComponent<Rigidbody>();
        destinoFinal = GameObject.FindGameObjectWithTag(tagDestinoFinal);
        ultimaPosicion = transform.position;
        tiempoEsperaAntesDeEntrar = Random.Range(0.2f, 1f);
        Invoke("BuscarPuertaInicial", 0.2f);
    }

    void FixedUpdate()
    {
        if (yendoAlDestino)
        {
            if (destinoFinal != null)
            {
                agent.SetDestination(destinoFinal.transform.position);
                // Verificar si hemos llegado al final de la fila actual
                if (!haAtravesadoPuerta && Vector3.Distance(transform.position, puertaActual.transform.position) > 5f)
                {
                    haAtravesadoPuerta = true;
                    StartCoroutine(BuscarSiguienteFila());
                }
            }
            return;
        }

        if (puertaActual != null)
        {
            agent.SetDestination(puertaActual.transform.position);

            float dist = Vector3.Distance(transform.position, puertaActual.transform.position);
            if (dist <= distanciaComprobacion)
            {
                ComprobarPuerta();
            }

            if (Vector3.Distance(transform.position, ultimaPosicion) < 0.05f)
            {
                tiempoAtascado += Time.deltaTime;
                if (tiempoAtascado >= tiempoBloqueoParaDescartar)
                {
                    CambiarDePuerta();
                }
            }
            else
            {
                tiempoAtascado = 0f;
            }

            ultimaPosicion = transform.position;
        }

        FreezeRotation();
    }

    IEnumerator BuscarSiguienteFila()
    {
        yield return new WaitForSeconds(tiempoEsperaProximaFila);

        // Resetear variables para la siguiente fila
        yendoAlDestino = false;
        haAtravesadoPuerta = false;
        puertasVisitadas.Clear();

        // Buscar la siguiente fila
        FilaPuertas[] todasLasFilas = FindObjectsOfType<FilaPuertas>();
        FilaPuertas siguienteFila = null;
        float distanciaMinima = float.MaxValue;

        foreach (FilaPuertas fila in todasLasFilas)
        {
            if (fila == filaActual) continue; // Ignorar la fila actual

            float dist = Vector3.Distance(transform.position, fila.transform.position);
            // Solo considerar filas que estén adelante del personaje
            if (dist < distanciaMinima && fila.transform.position.z > transform.position.z)
            {
                distanciaMinima = dist;
                siguienteFila = fila;
            }
        }

        if (siguienteFila != null)
        {
            filaActual = siguienteFila;
            BuscarPuertaInicial();
        }
    }

    void ComprobarPuerta()
    {
        if (puertaActual == null) return;

        Rigidbody puertaRb = puertaActual.GetComponent<Rigidbody>();
        if (puertaRb != null)
        {
            if (!puertaRb.isKinematic) // Si ya está rota
            {
                yendoAlDestino = true;
                haAtravesadoPuerta = false; // Resetear para la nueva fila
                if (destinoFinal != null)
                    agent.SetDestination(destinoFinal.transform.position);
            }
            else if (puertaActual.esReal && esLider) // Si es la puerta real y somos el líder
            {
                puertaRb.isKinematic = false;
                puertaRb.AddForce(-transform.forward * puertaActual.fuerzaCaida);
                puertaActual.filaPadre.NotificarPuertaRota(puertaActual);
                yendoAlDestino = true;
                haAtravesadoPuerta = false;
            }
        }
    }

    void BuscarPuertaInicial()
    {

        // Buscar la fila de puertas más cercana
        if (filaActual == null)
        {
            FilaPuertas[] filasPuertas = FindObjectsOfType<FilaPuertas>();
            float distanciaMinima = float.MaxValue;

            foreach (FilaPuertas fila in filasPuertas)
            {
                float dist = Vector3.Distance(transform.position, fila.transform.position);
                // Solo considerar filas que estén adelante del personaje
                if (dist < distanciaMinima && fila.transform.position.z > transform.position.z)
                {
                    distanciaMinima = dist;
                    filaActual = fila;
                }
            }
        }

        if (filaActual == null || filaActual.puertas.Count == 0)
        {
            Debug.LogWarning($"{name} no encontró puertas disponibles.");
            return;
        }

        if (esLider)
        {
            puertaActual = filaActual.ObtenerPuertaReal();
        }
        else
        {
            // Excluir la puerta rota si existe
            List<Puerta> puertasDisponibles = new List<Puerta>();
            foreach (Puerta p in filaActual.puertas)
            {
                if (!puertasVisitadas.Contains(p) &&
                    (filaActual.puertaRota == null || p != filaActual.puertaRota))
                {
                    puertasDisponibles.Add(p);
                }
            }

            // Si no hay puertas disponibles, reiniciar la búsqueda
            if (puertasDisponibles.Count == 0)
            {
                puertasVisitadas.Clear();
                foreach (Puerta p in filaActual.puertas)
                {
                    if (filaActual.puertaRota == null || p != filaActual.puertaRota)
                    {
                        puertasDisponibles.Add(p);
                    }
                }
            }

            if (puertasDisponibles.Count > 0)
            {
                puertaActual = puertasDisponibles[Random.Range(0, puertasDisponibles.Count)];
            }
        }

        if (puertaActual != null)
        {
            agent.SetDestination(puertaActual.transform.position);
        }
    }

    System.Collections.IEnumerator EsperarYSeguirPuerta(Puerta puerta)
    {
        yield return new WaitForSeconds(tiempoEsperaAntesDeEntrar);
        if (!yendoAlDestino && puerta != null)
        {
            puertaActual = puerta;
            agent.SetDestination(puertaActual.transform.position);
        }
    }

    void CambiarDePuerta()
    {
        if (puertaActual != null && !puertasVisitadas.Contains(puertaActual))
        {
            puertasVisitadas.Add(puertaActual);
        }

        tiempoAtascado = 0f;

        // Si hay una puerta rota cerca, ir hacia ella
        if (!esLider && filaActual != null && filaActual.TienePuertaRota)
        {
            float distanciaAPuertaRota = Vector3.Distance(transform.position, filaActual.puertaRota.transform.position);
            if (distanciaAPuertaRota < distanciaDeteccionPuertaRota)
            {
                StartCoroutine(EsperarYSeguirPuerta(filaActual.puertaRota));
                return;
            }
        }

        BuscarPuertaInicial();
    }

    void FreezeRotation()
    {
        if (rigid != null)
            rigid.angularVelocity = Vector3.zero;
    }
    void OnDrawGizmos()
    {
        if (filaActual != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(filaActual.transform.position, 1f);
        }
    }
}
