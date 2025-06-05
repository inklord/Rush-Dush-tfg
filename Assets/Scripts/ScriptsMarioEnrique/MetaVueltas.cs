using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // Necesario para manejar la lista de jugadores

public class MetaControl : MonoBehaviour
{
    private float firstPassTime = -1f;  // Tiempo del primer paso
    private bool firstPassCompleted = false;  // Si el jugador ha pasado por la meta por primera vez
    private int vueltasCompletadas = 0;  // Número de veces que el jugador ha pasado la meta
    private static List<GameObject> jugadoresClasificados = new List<GameObject>();  // Lista de jugadores clasificados
    public int maxClasificados = 5;  // Número máximo de jugadores clasificados

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Verificamos si el jugador ya ha sido clasificado
            if (jugadoresClasificados.Contains(other.gameObject))
            {
                return;  // Si ya está clasificado, no hacer nada
            }

            // Primer paso
            if (!firstPassCompleted)
            {
                firstPassTime = Time.time;
                firstPassCompleted = true;
            }
            else
            {
                // Si han pasado más de 60 segundos desde el primer paso
                if (Time.time - firstPassTime >= 60f)
                {
                    vueltasCompletadas++;
                    if (vueltasCompletadas >= 2)
                    {
                        // Añadimos al jugador a la lista de clasificados si no están ya
                        if (!jugadoresClasificados.Contains(other.gameObject))
                        {
                            jugadoresClasificados.Add(other.gameObject);
                            Debug.Log($"{other.gameObject.name} ha clasificado!");

                            // Si ya hay 5 jugadores clasificados, terminamos la carrera
                            if (jugadoresClasificados.Count >= maxClasificados)
                            {
                                FinDeCarrera();
                            }
                        }
                    }
                }
            }
        }
    }

    private void FinDeCarrera()
    {
        Debug.Log("🏁 ¡Carrera Terminada! Los 5 primeros jugadores han clasificado.");
        // Eliminar a los jugadores que no estén clasificados
        foreach (GameObject jugador in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (!jugadoresClasificados.Contains(jugador))
            {
                Debug.Log($"{jugador.name} ha sido eliminado.");
                Destroy(jugador);  // Elimina al jugador que no se ha clasificado
            }
        }

        // Cargar la siguiente escena o finalizar el juego
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("¡No hay más escenas para cargar!");
        }
    }
}
