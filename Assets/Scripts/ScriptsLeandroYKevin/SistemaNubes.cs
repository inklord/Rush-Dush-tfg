using UnityEngine;

public class SistemaNubes : MonoBehaviour
{
    private float tiempoEntreNubes = 0.5f;
    private float velocidadNubes = 2f;
    private float tiempoSiguienteNube = 0f;
    private float limitesMapa = 100f; // Tamaño del área donde aparecen nubes

    void Start()
    {
        // Genera nubes iniciales
        for (int i = 0; i < 50; i++)
        {
            CrearNube(true);
        }
    }

    void Update()
    {
        // Genera nueva nube cuando sea el momento
        if (Time.time >= tiempoSiguienteNube)
        {
            CrearNube(false);
            tiempoSiguienteNube = Time.time + tiempoEntreNubes;
        }

        // Mueve todas las nubes existentes
        foreach (Transform nube in transform)
        {
            // Mueve la nube en una dirección aleatoria
            nube.Translate(nube.right * velocidadNubes * Time.deltaTime);

            // Si la nube se va muy lejos, la reposicionamos al otro lado
            if (Mathf.Abs(nube.position.x) > limitesMapa || 
                Mathf.Abs(nube.position.z) > limitesMapa)
            {
                ReposicionarNube(nube);
            }
        }
    }

    void CrearNube(bool esInicial)
    {
        GameObject nube = new GameObject("Nube");
        nube.transform.parent = transform;

        // Si es inicial, posición aleatoria en todo el mapa
        // Si no, genera en los bordes
        if (esInicial)
        {
            nube.transform.position = new Vector3(
                Random.Range(-limitesMapa, limitesMapa),
                Random.Range(10f, 20f),
                Random.Range(-limitesMapa, limitesMapa)
            );
        }
        else
        {
            // Genera en un borde aleatorio
            float x = Random.Range(-limitesMapa, limitesMapa);
            float z = Random.Range(-limitesMapa, limitesMapa);
            
            if (Random.value > 0.5f)
            {
                x = Random.value > 0.5f ? limitesMapa : -limitesMapa;
            }
            else
            {
                z = Random.value > 0.5f ? limitesMapa : -limitesMapa;
            }

            nube.transform.position = new Vector3(x, Random.Range(10f, 20f), z);
        }

        // Rotación aleatoria para dirección de movimiento
        nube.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        // Crea las esferas que forman la nube
        for (int i = 0; i < 5; i++)
        {
            GameObject esfera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            esfera.transform.parent = nube.transform;
            
            esfera.transform.localPosition = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-0.5f, 0.5f),
                Random.Range(-1f, 1f)
            );
            
            float escala = Random.Range(1.5f, 2.5f);
            esfera.transform.localScale = new Vector3(escala, escala * 0.6f, escala);

            Renderer rend = esfera.GetComponent<Renderer>();
            rend.material.color = Color.white;
        }
    }

    void ReposicionarNube(Transform nube)
    {
        // Reposiciona la nube en el lado opuesto del mapa
        Vector3 pos = nube.position;
        if (Mathf.Abs(pos.x) > limitesMapa)
        {
            pos.x = -Mathf.Sign(pos.x) * limitesMapa;
        }
        if (Mathf.Abs(pos.z) > limitesMapa)
        {
            pos.z = -Mathf.Sign(pos.z) * limitesMapa;
        }
        nube.position = pos;
    }
}