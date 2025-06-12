using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Photon.Pun;

public class MuerteLava : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject deathImage;
    public AudioSource deathSound;
    public AudioSource backgroundMusic;
    public float fadeDuration = 1.5f;
    public float deathScreenDuration = 2f;
    
    [Header("Debug")]
    public bool enableDebugLogs = true;

    private bool transitionStarted = false;
    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        
        if (deathImage != null)
        {
            deathImage.SetActive(false);
        }

        if (backgroundMusic != null && PhotonNetwork.IsMasterClient)
        {
            backgroundMusic.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si es un jugador, destruirlo y cambiar a escena de fracaso
        if (other.CompareTag("Player"))
        {
            // Destruir el jugador
            if (PhotonNetwork.IsConnected)
            {
                PhotonView playerView = other.GetComponent<PhotonView>();
                if (playerView != null && playerView.IsMine)
                {
                    PhotonNetwork.Destroy(other.gameObject);
                    // Ir a escena de fracaso
                    SceneChange sceneChanger = FindObjectOfType<SceneChange>();
                    if (sceneChanger != null)
                    {
                        sceneChanger.GoToEndingFailure();
                    }
                }
            }
            else
            {
                Destroy(other.gameObject);
                // Ir a escena de fracaso
                SceneChange sceneChanger = FindObjectOfType<SceneChange>();
                if (sceneChanger != null)
                {
                    sceneChanger.GoToEndingFailure();
                }
            }
        }
        // Si es una IA, solo destruirla
        else if (other.CompareTag("IA"))
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Destroy(other.gameObject);
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }

    private IEnumerator ShowDeathEffects()
    {
        // Reproducir sonido de muerte
        if (deathSound != null && !deathSound.isPlaying)
        {
            deathSound.Play();
        }

        // Detener música de fondo
        if (backgroundMusic != null)
        {
            StartCoroutine(FadeOutMusic());
        }

        // Mostrar y animar imagen de muerte
        if (deathImage != null)
        {
            Image img = deathImage.GetComponent<Image>();
            if (img != null)
            {
                deathImage.SetActive(true);
                
                // Fade in
                float elapsedTime = 0f;
                Color startColor = img.color;
                while (elapsedTime < fadeDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                    img.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                    yield return null;
                }
            }
        }
    }

    private IEnumerator FadeOutMusic()
    {
        if (backgroundMusic == null) yield break;

        float startVolume = backgroundMusic.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            backgroundMusic.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        backgroundMusic.Stop();
    }

    private IEnumerator TransitionToFinalFracaso()
    {
        Debug.Log("MuerteLava: Iniciando transición a FinalFracaso");
        
        // Esperar a que se muestren los efectos de muerte
        yield return new WaitForSeconds(deathScreenDuration);

        Debug.Log("MuerteLava: Tiempo de espera completado, procediendo con la transición");

        // Si estamos en red, usar PhotonNetwork para cambiar la escena
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("MuerteLava: Cargando FinalFracaso a través de PhotonNetwork");
            PhotonNetwork.LoadLevel("FinalFracaso");
        }
        else
        {
            // Intentar usar SceneChange primero
            SceneChange sceneChanger = FindObjectOfType<SceneChange>();
            if (sceneChanger != null)
            {
                Debug.Log("MuerteLava: Usando SceneChange para ir a FinalFracaso");
                sceneChanger.GoToEndingFailure();
            }
            else
            {
                // Fallback: cargar escena directamente
                Debug.Log("MuerteLava: Cargando FinalFracaso directamente");
                SceneManager.LoadScene("FinalFracaso");
            }
        }
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
                
                PhotonNetwork.Destroy(randomIA);
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
                
                PhotonNetwork.Destroy(randomPlayer);
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
            
            if (HexagoniaGameManager.Instance != null)
            {
                Debug.Log($"HexagoniaGameManager - Jugadores activos: {HexagoniaGameManager.Instance.GetPlayersAlive()}");
            }
            else
            {
                Debug.Log("HexagoniaGameManager no encontrado!");
            }
        }
    }

    void OnGUI()
    {
        if (enableDebugLogs)
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 100, 300, 90));
            GUILayout.Box("🔥 MUERTE LAVA DEBUG");
            
            if (GUILayout.Button("Test: Eliminar IA Random"))
            {
                GameObject[] ias = GameObject.FindGameObjectsWithTag("IA");
                if (ias.Length > 0)
                {
                    GameObject randomIA = ias[Random.Range(0, ias.Length)];
                    if (enableDebugLogs)
                        Debug.Log($"MuerteLava: TEST - Eliminando IA {randomIA.name} manualmente");
                    
                    PhotonNetwork.Destroy(randomIA);
                }
                else
                {
                    Debug.Log("No se encontraron IAs para eliminar");
                }
            }
            
            if (GUILayout.Button("Test: Eliminar Player Random"))
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                if (players.Length > 0)
                {
                    GameObject randomPlayer = players[Random.Range(0, players.Length)];
                    if (enableDebugLogs)
                        Debug.Log($"MuerteLava: TEST - Eliminando Player {randomPlayer.name} manualmente");
                    
                    PhotonNetwork.Destroy(randomPlayer);
                }
                else
                {
                    Debug.Log("No se encontraron Players para eliminar");
                }
            }
            
            if (enableDebugLogs)
            {
                if (HexagoniaGameManager.Instance != null)
                {
                    Debug.Log($"HexagoniaGameManager - Jugadores activos: {HexagoniaGameManager.Instance.GetPlayersAlive()}");
                }
                else
                {
                    Debug.Log("HexagoniaGameManager no encontrado!");
                }
            }
            
            GUILayout.EndArea();
        }
    }
}
