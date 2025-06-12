using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 3, 2 ,1 , GO!�� �� �� ������ �����ϰ� �ʹ
//   ִ´
// 1  3, 2, 1 ´
// ٲ  UI   Ѵ
//ð   GO! UI 
// ٽ  ش

public class LHS_CountdownController : MonoBehaviour
{
    public int countdownTime = 4;

    public Text countdownDisplay;
    public GameObject anim;

    public GameObject Num_A;   //1
    public GameObject Num_B;   //2
    public GameObject Num_C;   //3
    public GameObject Num_GO;
    //public GameObject Bar;
    
    // ؽƮ ȿ
    Animator animator;

    //  ȿ
    public AudioSource mysfx;
    public AudioClip startsfx;
    public AudioClip gosfx;

    // Game state
    private bool countdownFinished = false;
    private HexagoniaGameManager hexagoniaManager;

    private void Awake()
    {
        // ✅ Verificar componentes críticos
        if (countdownDisplay == null)
        {
            Debug.LogError("LHS_CountdownController: countdownDisplay no está asignado!");
            return;
        }
        
        if (anim != null)
        {
            animator = anim.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("LHS_CountdownController: No se encontró Animator en el objeto anim");
            }
        }
        else
        {
            Debug.LogWarning("LHS_CountdownController: anim GameObject no está asignado");
        }
        
        // ✅ Verificar componentes de audio (no críticos)
        if (mysfx == null)
        {
            Debug.LogWarning("LHS_CountdownController: AudioSource (mysfx) no está asignado - sonidos deshabilitados");
        }
        if (startsfx == null)
        {
            Debug.LogWarning("LHS_CountdownController: AudioClip startsfx no está asignado");
        }
        if (gosfx == null)
        {
            Debug.LogWarning("LHS_CountdownController: AudioClip gosfx no está asignado");
        }
        
        // Buscar HexagoniaGameManager
        hexagoniaManager = FindObjectOfType<HexagoniaGameManager>();
        if (hexagoniaManager == null)
        {
            Debug.LogWarning("LHS_CountdownController: No se encontró HexagoniaGameManager");
        }
        
        StartCoroutine(CountdownToStart());

        // ✅ Verificar GameObjects UI antes de usarlos
        if (Num_A != null) Num_A.SetActive(false); //1
        if (Num_B != null) Num_B.SetActive(false); //2
        if (Num_C != null) Num_C.SetActive(false); //3
        if (Num_GO != null) Num_GO.SetActive(false);

        Time.timeScale = 0;
    }

    //ڷƾ Լ 
    IEnumerator CountdownToStart()
    {
        // Asegurarse de que todos los números estén ocultos al inicio
        if (Num_A != null) Num_A.SetActive(false);
        if (Num_B != null) Num_B.SetActive(false);
        if (Num_C != null) Num_C.SetActive(false);
        if (Num_GO != null) Num_GO.SetActive(false);

        while (countdownTime > 0)
        {
            ChangeImage();

            if (countdownDisplay != null)
            {
                countdownDisplay.text = countdownTime.ToString();
            }

            yield return new WaitForSecondsRealtime(1f);
            countdownTime--;
        }

        // Mostrar GO!
        if (countdownDisplay != null)
        {
            countdownDisplay.text = "GO!";
        }
        
        if (Num_GO != null)
        {
            // Asegurarse de que GO! sea el único visible
            Num_A.SetActive(false);
            Num_B.SetActive(false);
            Num_C.SetActive(false);
            Num_GO.SetActive(true);
        }

        PlaySoundSafe(gosfx);
        Time.timeScale = 1;
        countdownFinished = true;

        yield return new WaitForSecondsRealtime(1f);

        // Ocultar todo al final
        if (countdownDisplay != null)
        {
            countdownDisplay.gameObject.SetActive(false);
        }
        
        if (Num_GO != null) Num_GO.SetActive(false);

        // Notificar al HexagoniaGameManager que el countdown terminó
        if (hexagoniaManager != null)
        {
            hexagoniaManager.OnCountdownFinished();
        }
    }

    void ChangeImage()
    {
        int i = countdownTime;

        // Primero, ocultar todos los números
        if (Num_A != null) Num_A.SetActive(false);
        if (Num_B != null) Num_B.SetActive(false);
        if (Num_C != null) Num_C.SetActive(false);
        if (Num_GO != null) Num_GO.SetActive(false);

        // Luego, mostrar solo el número actual
        if (i == 4)
        {
            if (Num_C != null)
            {
                Num_C.SetActive(true);
            }
            
            if (animator != null)
            {
                animator.SetBool("Num3", true);
            }
            
            PlaySoundSafe(startsfx);
        }
        else if (i == 3)
        {
            if (Num_B != null)
            {
                Num_B.SetActive(true);
            }
            
            PlaySoundSafe(startsfx);
        }
        else if (i == 2)
        {
            if (Num_A != null)
            {
                Num_A.SetActive(true);
            }
            
            PlaySoundSafe(startsfx);
        }
        else if (i == 1)
        {
            if (Num_GO != null)
            {
                Num_GO.SetActive(true);
            }
            
            PlaySoundSafe(gosfx);
        }
    }
    
    /// <summary>
    /// ✅ Método seguro para reproducir sonidos con verificación de null
    /// </summary>
    private void PlaySoundSafe(AudioClip clip)
    {
        if (mysfx != null && clip != null && !mysfx.isPlaying)
        {
            mysfx.clip = clip;
            mysfx.Play();
        }
    }

    public bool IsCountdownFinished()
    {
        return countdownFinished;
    }
}

