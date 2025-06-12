using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaBalanceo : MonoBehaviour
{
    [Header("Configuración de Balanceo")]
    public float velocidadBalanceo = 2f;
    public float anguloMaximo = 15f;
    public float velocidadRetorno = 2f;
    public float factorDistancia = 1.5f; // Multiplicador de efecto según distancia al centro
    public float amortiguacion = 0.98f; // Factor de amortiguación del movimiento

    [Header("Detección de Jugadores")]
    public float alturaDeteccion = 2f;
    public float anchoPlataforma = 5f;

    [Header("Debug")]
    public bool enableDebugLogs = false;

    private List<(Transform transform, float distanciaCentro)> jugadoresIzquierda = new List<(Transform, float)>();
    private List<(Transform transform, float distanciaCentro)> jugadoresDerecha = new List<(Transform, float)>();
    private Rigidbody rb;
    private float inclinacionActual = 0f;
    private float inclinacionDeseada = 0f;
    private float velocidadAngular = 0f; // Velocidad actual de rotación
    private Quaternion rotacionInicial;
    private Vector3 centroPlataforma;
    private Bounds bounds;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rotacionInicial = transform.rotation;
        
        bounds = GetComponent<Collider>().bounds;
        centroPlataforma = bounds.center;
        anchoPlataforma = bounds.size.z; // Usar Z para el ancho lateral
    }

    void Update()
    {
        CalcularPeso();
        BalancearPlataforma();
    }

    private void CalcularPeso()
    {
        jugadoresIzquierda.Clear();
        jugadoresDerecha.Clear();

        foreach (var jugador in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (IsJugadorSobrePlataforma(jugador.transform))
            {
                float distanciaAlCentro = jugador.transform.position.z - centroPlataforma.z;
                float distanciaNormalizada = Mathf.Abs(distanciaAlCentro) / (anchoPlataforma * 0.5f);

                if (distanciaAlCentro < 0)
                {
                    jugadoresIzquierda.Add((jugador.transform, distanciaNormalizada));
                }
                else
                {
                    jugadoresDerecha.Add((jugador.transform, distanciaNormalizada));
                }
            }
        }
    }

    private void BalancearPlataforma()
    {
        // Calcular momento total considerando la distancia al centro
        float momentoIzquierda = 0f;
        float momentoDerecha = 0f;

        foreach (var (_, distancia) in jugadoresIzquierda)
        {
            momentoIzquierda += distancia * factorDistancia;
        }

        foreach (var (_, distancia) in jugadoresDerecha)
        {
            momentoDerecha += distancia * factorDistancia;
        }

        float momentoTotal = momentoIzquierda + momentoDerecha;
        
        if (momentoTotal > 0)
        {
            // Calcular la diferencia de momento
            float diferenciaMomento = momentoIzquierda - momentoDerecha;
            
            // La inclinación deseada depende de la diferencia de momentos
            inclinacionDeseada = -anguloMaximo * (diferenciaMomento / momentoTotal);
            
            // Aplicar física simple
            float fuerzaRotacion = (inclinacionDeseada - inclinacionActual) * velocidadBalanceo;
            velocidadAngular += fuerzaRotacion * Time.deltaTime;
            
            // Aplicar amortiguación
            velocidadAngular *= amortiguacion;
        }
        else
        {
            // Retorno a posición inicial más suave
            inclinacionDeseada = 0f;
            velocidadAngular += (-inclinacionActual * velocidadRetorno) * Time.deltaTime;
            velocidadAngular *= amortiguacion;
        }

        // Actualizar inclinación con física
        inclinacionActual += velocidadAngular * Time.deltaTime;
        inclinacionActual = Mathf.Clamp(inclinacionActual, -anguloMaximo, anguloMaximo);

        // Aplicar rotación
        transform.rotation = rotacionInicial * Quaternion.Euler(inclinacionActual, 0, 0);
        
        if (enableDebugLogs)
        {
            Debug.Log($"🎮 Balanceo - MomentoIzq: {momentoIzquierda:F2}, MomentoDer: {momentoDerecha:F2}, " +
                     $"Velocidad: {velocidadAngular:F2}, Inclinación: {inclinacionActual:F2}");
        }
    }

    private bool IsJugadorSobrePlataforma(Transform jugador)
    {
        bool dentroDeAncho = Mathf.Abs(jugador.position.z - centroPlataforma.z) < anchoPlataforma * 0.5f;
        bool dentroDeAlto = jugador.position.y - centroPlataforma.y < alturaDeteccion;
        bool encimaDePlataforma = jugador.position.y > centroPlataforma.y;

        if (dentroDeAncho && dentroDeAlto && encimaDePlataforma)
        {
            RaycastHit hit;
            if (Physics.Raycast(jugador.position, Vector3.down, out hit, alturaDeteccion))
            {
                return hit.transform == this.transform;
            }
        }
        return false;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.yellow;
        Vector3 centro = centroPlataforma;
        Vector3 tamanio = new Vector3(bounds.size.x, alturaDeteccion, anchoPlataforma);
        Gizmos.DrawWireCube(centro + Vector3.up * alturaDeteccion * 0.5f, tamanio);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(centro, centro + Vector3.up * alturaDeteccion);
    }
}
