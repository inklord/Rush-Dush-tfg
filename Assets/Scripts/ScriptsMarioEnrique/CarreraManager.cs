using System.Collections;
using UnityEngine;
using TMPro;
using Photon.Pun; // Necesario para Photon

public class CarreraManager : MonoBehaviour
{
    public int vueltasMaximas = 2; // Número de vueltas máximas por jugador
    public TextMeshProUGUI textoVueltas; // Referencia al texto que muestra las vueltas
    public CanvasGroup gameOverPanel; // Panel de GameOver
    public GameObject[] jugadores;  // Array para almacenar los jugadores

    private bool carreraTerminada = false; // Estado de la carrera
    private int jugadoresTerminados = 0;  // Número de jugadores que han completado la carrera

    void Start()
    {
        // Asignar referencias si no están asignadas en el Inspector
        if (textoVueltas == null)
        {
            textoVueltas = GameObject.Find("TextoVueltas")?.GetComponent<TextMeshProUGUI>();
        }
        if (gameOverPanel == null)
        {
            GameObject panel = GameObject.Find("GameOverPanel");
            if (panel != null) gameOverPanel = panel.GetComponent<CanvasGroup>();
        }

        // Ocultar GameOver al inicio
        if (gameOverPanel != null)
        {
            gameOverPanel.alpha = 0;
            gameOverPanel.interactable = false;
            gameOverPanel.blocksRaycasts = false;
        }

        // Encontrar todos los jugadores en la escena
        jugadores = GameObject.FindGameObjectsWithTag("Player");

        // Verificar que los jugadores están siendo encontrados
        Debug.Log($"Jugadores encontrados: {jugadores.Length}");

       
    }

   private void OnTriggerEnter(Collider other)
{
    // Comprobamos si el objeto tiene la etiqueta "Player" y si la carrera no ha terminado
    if (other.CompareTag("Player") && !carreraTerminada)
    {
        // Comprobamos si el jugador tocó un plano con la etiqueta "Vueltas"
        if (other.CompareTag("Vueltas"))
        {
            // Obtener el componente Player del objeto que tocó el trigger
            Player playerScript = other.GetComponent<Player>();

            if (playerScript != null) // Si el jugador tiene el componente Player
            {
                // Si el jugador no ha completado todas las vueltas
                if (playerScript.vueltasCompletadas < vueltasMaximas)
                {
                    playerScript.vueltasCompletadas++;  // Aumentamos las vueltas
                    Debug.Log($"{other.gameObject.name} ha completado vuelta: {playerScript.vueltasCompletadas}");

                    // Actualizamos la UI
                    ActualizarUI(playerScript); // Actualiza la UI con las nuevas vueltas

                    // Verificamos si el jugador ha completado todas las vueltas
                    if (playerScript.vueltasCompletadas >= vueltasMaximas)
                    {
                        jugadoresTerminados++;  // Aumentamos el contador de jugadores terminados

                        Debug.Log($"{other.gameObject.name} ha completado la carrera!");

                        // Verificamos si todos los jugadores han terminado
                        if (jugadoresTerminados >= jugadores.Length)
                        {
                            FinDeCarrera(); // Finalizamos la carrera
                        }
                    }
                }
            }
        }
    }
}



    // Actualizar el texto de la UI con las vueltas completadas
    void ActualizarUI(Player playerScript)
    {
        if (textoVueltas != null)
        {
            // Mostrar el número de vueltas completadas de cada jugador
            textoVueltas.text = $"{playerScript.vueltasCompletadas} / {vueltasMaximas} Vueltas";
        }
        else
        {
            Debug.LogWarning("⚠️ Falta asignar el TextoVueltas en el Inspector.");
        }
    }

    void FinDeCarrera()
    {
        carreraTerminada = true;
        Debug.Log("🏁 ¡Carrera Terminada!");

        // Opcional: Aquí puedes eliminar jugadores no terminados o realizar otras acciones
        StartCoroutine(MostrarGameOver());
    }

    IEnumerator MostrarGameOver()
    {
        if (gameOverPanel != null)
        {
            float tiempoFade = 1.5f; // Duración del fade-in
            float tiempo = 0f;
            while (tiempo < tiempoFade)
            {
                tiempo += Time.deltaTime;
                gameOverPanel.alpha = Mathf.Lerp(0, 1, tiempo / tiempoFade);
                yield return null;
            }
            gameOverPanel.interactable = true;
            gameOverPanel.blocksRaycasts = true;
        }
        else
        {
            Debug.LogError("❌ No se encontró el GameOverPanel en la escena.");
        }
    }
}
