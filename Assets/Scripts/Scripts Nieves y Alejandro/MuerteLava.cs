using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;

public class MuerteLava : MonoBehaviourPunCallbacks
{
    public GameObject deathImage;
    public AudioSource deathSound;
    public AudioSource backgroundMusic;
    public float fadeDuration = 1.5f;
    public float deathScreenDuration = 5f;
    
    [Header("Configuración de Eliminación")]
    public bool enableDebugLogs = true;

    private void Start()
    {
        if (deathImage != null)
        {
            deathImage.SetActive(false);
        }

        if (backgroundMusic != null && PhotonNetwork.IsMasterClient)
        {
            backgroundMusic.Play();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject objeto = collision.gameObject;
        
        if (enableDebugLogs)
            Debug.Log($"MuerteLava: Colisión detectada con {objeto.name}, Tag: {objeto.tag}");

        // ✅ INTEGRACIÓN CON LAVA GAME MANAGER (SOLO PARA PLAYERS)
        if (objeto.CompareTag("Player") && LavaGameManager.Instance != null)
        {
            // Notificar al LavaGameManager SOLO cuando es un Player
            LavaGameManager.Instance.HandleLavaDeath(objeto);
            
            if (enableDebugLogs)
                Debug.Log($"MuerteLava: Notificando a LavaGameManager sobre muerte de PLAYER {objeto.name}");
        }

        // ✅ DETECCIÓN MEJORADA PARA JUGADORES REALES
        if (objeto.CompareTag("Player"))
        {
            PhotonView photonView = objeto.GetComponent<PhotonView>();
            
            if (enableDebugLogs)
            {
                Debug.Log($"MuerteLava: Jugador detectado - {objeto.name}");
                Debug.Log($"MuerteLava: Tiene PhotonView: {photonView != null}");
                if (photonView != null)
                {
                    Debug.Log($"MuerteLava: PhotonView.IsMine: {photonView.IsMine}");
                    Debug.Log($"MuerteLava: PhotonView.ViewID: {photonView.ViewID}");
                }
            }
            
            // ✅ ELIMINACIÓN MÁS PERMISIVA para Players
            if (photonView != null && photonView.IsMine)
            {
                // Método original con PhotonView
                MostrarMuerteJugador(objeto);
            }
            else if (photonView == null)
            {
                // Player sin PhotonView (modo local/testing)
                if (enableDebugLogs)
                    Debug.Log($"MuerteLava: Player sin PhotonView, eliminando localmente");
                
                MostrarMuerteJugadorLocal(objeto);
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log($"MuerteLava: Player no es mío (IsMine=false), ignorando");
            }
        }
        
        // ✅ SOPORTE PARA IAs (Solo GameManager original, NO LavaGameManager)
        else if (objeto.CompareTag("IA"))
        {
            if (enableDebugLogs)
                Debug.Log($"MuerteLava: IA {objeto.name} tocó lava - usando GameManager original");
            
            MostrarMuerteIA(objeto);
        }
        
        // ✅ DETECCIÓN ALTERNATIVA por componentes
        else if (objeto.GetComponent<IAPlayerSimple>() != null)
        {
            if (enableDebugLogs)
                Debug.Log($"MuerteLava: IA detectada por componente IAPlayerSimple");
            MostrarMuerteIA(objeto);
        }
        
        // ✅ DETECCIÓN GENÉRICA como último recurso
        else
        {
            // Buscar otros componentes que indiquen que es un jugador
            bool isPlayer = false;
            string componentFound = "";
            
            if (objeto.GetComponent<CharacterController>() != null)
            {
                isPlayer = true;
                componentFound = "CharacterController";
            }
            else if (objeto.GetComponent<Rigidbody>() != null && objeto.name.ToLower().Contains("player"))
            {
                isPlayer = true;
                componentFound = "Rigidbody + nombre contiene 'player'";
            }
            
            if (isPlayer)
            {
                if (enableDebugLogs)
                    Debug.Log($"MuerteLava: Jugador detectado por {componentFound}, eliminando como jugador local");
                
                // Solo notificar al LavaGameManager si es realmente un Player
                if (objeto.CompareTag("Player") && LavaGameManager.Instance != null)
                {
                    LavaGameManager.Instance.HandleLavaDeath(objeto);
                }
                
                MostrarMuerteJugadorLocal(objeto);
            }
            else if (enableDebugLogs)
            {
                Debug.Log($"MuerteLava: Objeto {objeto.name} no reconocido como jugador o IA");
            }
        }
    }
    
    private void MostrarMuerteJugadorLocal(GameObject jugador)
    {
        if (enableDebugLogs)
            Debug.Log($"MuerteLava: Jugador local {jugador.name} eliminado por lava");
        
        // Efectos visuales y sonoros
        MostrarEfectosMuerte();
        
        // ✅ INTEGRACIÓN CON GAMEMANAGER para jugadores locales
        if (GameManager.Instance != null)
        {
            // Usar GameManager para jugadores locales también
            GameManager.Instance.ForceEliminatePlayer(jugador);
        }
        else
        {
            // Fallback: destruir directamente
            StartCoroutine(EliminatePlayerWithEffect(jugador));
        }

        // Configurar cámara después de muerte
        ConfigurarCamaraDespuesMuerte();
    }
    
    private IEnumerator EliminatePlayerWithEffect(GameObject player)
    {
        // Cambiar color antes de destruir
        Renderer playerRenderer = player.GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            playerRenderer.material.color = Color.red; // Color de muerte por lava
        }
        
        // Esperar un poco antes de destruir
        yield return new WaitForSeconds(1f);
        
        // Destruir el jugador
        Destroy(player);
    }

    private void MostrarMuerteJugador(GameObject jugador)
    {
        if (enableDebugLogs)
            Debug.Log($"MuerteLava: Jugador real {jugador.name} eliminado por lava");
        
        // Efectos visuales y sonoros para jugador real
        MostrarEfectosMuerte();

        // Destruir usando Photon para jugadores reales
        if (jugador.GetComponent<PhotonView>().IsMine)
        {
            PhotonNetwork.Destroy(jugador);
        }

        // Configurar cámara después de muerte (solo para jugador local)
        ConfigurarCamaraDespuesMuerte();
    }
    
    private void MostrarMuerteIA(GameObject ia)
    {
        if (enableDebugLogs)
            Debug.Log($"MuerteLava: IA {ia.name} eliminada por lava");
        
        // ✅ INTEGRACIÓN CON GAMEMANAGER
        if (GameManager.Instance != null)
        {
            // Usar el sistema de GameManager para eliminar la IA
            GameManager.Instance.ForceEliminatePlayer(ia);
        }
        else
        {
            // Fallback: eliminar directamente si no hay GameManager
            if (enableDebugLogs)
                Debug.LogWarning("MuerteLava: No se encontró GameManager, eliminando IA directamente");
            
            StartCoroutine(EliminateIAWithEffect(ia));
        }
        
        // Efectos de sonido (sin efectos visuales para IAs)
        ReproducirEfectoSonoro();
    }
    
    private IEnumerator EliminateIAWithEffect(GameObject ia)
    {
        // Cambiar color antes de destruir
        Renderer iaRenderer = ia.GetComponent<Renderer>();
        if (iaRenderer != null)
        {
            iaRenderer.material.color = Color.red; // Color de muerte por lava
        }
        
        // Esperar un poco antes de destruir
        yield return new WaitForSeconds(0.5f);
        
        // Destruir la IA
        Destroy(ia);
    }

    private void MostrarEfectosMuerte()
    {
        if (deathImage != null)
        {
            deathImage.SetActive(true);
            StartCoroutine(FadeIn());
        }

        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }

        ReproducirEfectoSonoro();
        
        StartCoroutine(OcultarDeathImage());
    }
    
    private void ReproducirEfectoSonoro()
    {
        if (deathSound != null && !deathSound.isPlaying)
        {
            deathSound.Play();
        }
    }
    
    private void ConfigurarCamaraDespuesMuerte()
    {
        // Set the camera position and rotation after death
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(-53, 130, -73); // Update position
            Camera.main.transform.rotation = Quaternion.Euler(25, 30, 0);  // Update rotation
        }
    }

    IEnumerator FadeIn()
    {
        Image img = deathImage.GetComponent<Image>();
        Color color = img.color;
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            img.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 1;
        img.color = color;
    }

    IEnumerator OcultarDeathImage()
    {
        yield return new WaitForSeconds(deathScreenDuration);

        if (deathImage != null)
        {
            StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeOut()
    {
        if (deathImage == null) yield break;
        Image img = deathImage.GetComponent<Image>();
        if (img == null) yield break;

        Color color = img.color;
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            img.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 0;
        img.color = color;
        deathImage.SetActive(false);
    }
    
    // ✅ MÉTODOS PARA TESTING
    private void Update()
    {
        // TEST: Forzar eliminación de IA con tecla L
        if (Input.GetKeyDown(KeyCode.L))
        {
            GameObject[] ias = GameObject.FindGameObjectsWithTag("IA");
            if (ias.Length > 0)
            {
                GameObject randomIA = ias[Random.Range(0, ias.Length)];
                if (enableDebugLogs)
                    Debug.Log($"MuerteLava: TEST - Eliminando IA {randomIA.name} manualmente");
                
                MostrarMuerteIA(randomIA);
            }
            else
            {
                Debug.Log("MuerteLava: No se encontraron IAs para eliminar");
            }
        }
        
        // TEST: Forzar eliminación de Player con tecla P
        if (Input.GetKeyDown(KeyCode.P))
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length > 0)
            {
                GameObject randomPlayer = players[Random.Range(0, players.Length)];
                if (enableDebugLogs)
                    Debug.Log($"MuerteLava: TEST - Eliminando Player {randomPlayer.name} manualmente");
                
                // Llamar directamente al método apropiado
                PhotonView photonView = randomPlayer.GetComponent<PhotonView>();
                if (photonView != null && photonView.IsMine)
                {
                    MostrarMuerteJugador(randomPlayer);
                }
                else
                {
                    MostrarMuerteJugadorLocal(randomPlayer);
                }
            }
            else
            {
                Debug.Log("MuerteLava: No se encontraron Players para eliminar");
            }
        }
        
        // TEST: Debug completo con tecla D
        if (Input.GetKeyDown(KeyCode.D))
        {
            GameObject[] ias = GameObject.FindGameObjectsWithTag("IA");
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            
            Debug.Log("=== DEBUG COMPLETO MuerteLava ===");
            Debug.Log($"IAs encontradas: {ias.Length}");
            foreach (GameObject ia in ias)
            {
                Debug.Log($"  - IA: {ia.name}, Activa: {ia.activeInHierarchy}");
            }
            
            Debug.Log($"Players encontrados: {players.Length}");
            foreach (GameObject player in players)
            {
                PhotonView pv = player.GetComponent<PhotonView>();
                Debug.Log($"  - Player: {player.name}, Activo: {player.activeInHierarchy}, PhotonView: {pv != null}, IsMine: {pv?.IsMine}");
            }
            
            if (GameManager.Instance != null)
            {
                Debug.Log($"GameManager - Jugadores activos: {GameManager.Instance.GetActivePlayerCount()}");
            }
            else
            {
                Debug.Log("GameManager no encontrado!");
            }
        }
    }
}
