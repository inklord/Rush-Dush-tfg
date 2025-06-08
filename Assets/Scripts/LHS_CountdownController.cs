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
        //Bar.SetActive(true);

        while (countdownTime > 0)
        {
            //Bar.SetActive(true);

            ChangeImage();

            if (countdownDisplay != null)
            {
                countdownDisplay.text = countdownTime.ToString();
            }

            // 1 
            yield return new WaitForSecondsRealtime(1f);

            countdownTime--;
        }

        //   
        if (countdownDisplay != null)
        {
            countdownDisplay.text = "GO!";
        }
        //Num_GO.SetActive(false);

        Time.timeScale = 1;

        yield return new WaitForSecondsRealtime(1f);

        if (countdownDisplay != null)
        {
            countdownDisplay.gameObject.SetActive(false);
        }
    }

    void ChangeImage()
    {
        int i = countdownTime;

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
            
            // ✅ Verificación de null antes de reproducir sonido
            PlaySoundSafe(startsfx);
        }

        if (i == 3)
        {
            //Num_C.SetActive(false);
            if (Num_B != null)
            {
                Num_B.SetActive(true);
            }
            //animator.SetBool("Num3", true);
            
            // ✅ Verificación de null antes de reproducir sonido
            PlaySoundSafe(startsfx);
        }

        if (i == 2)
        {
            //Num_B.SetActive(false);
            if (Num_A != null)
            {
                Num_A.SetActive(true);
            }
            //animator.SetBool("Num3", true);
            
            // ✅ Verificación de null antes de reproducir sonido
            PlaySoundSafe(startsfx);
        }

        if (i == 1)
        {
            //Num_A.SetActive(false);
            if (Num_GO != null)
            {
                Num_GO.SetActive(true);
            }
            //animator.SetBool("Num3", true);
            
            // ✅ Verificación de null antes de reproducir sonido
            PlaySoundSafe(gosfx);
        }
    }
    
    /// <summary>
    /// ✅ Método seguro para reproducir sonidos con verificación de null
    /// </summary>
    private void PlaySoundSafe(AudioClip clip)
    {
        // Verificar que tanto el AudioSource como el AudioClip no sean null
        if (mysfx != null && clip != null)
        {
            try
            {
                mysfx.PlayOneShot(clip);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"LHS_CountdownController: Error al reproducir sonido: {e.Message}");
            }
        }
        else
        {
            if (mysfx == null)
            {
                Debug.LogWarning("LHS_CountdownController: AudioSource (mysfx) es null - no se puede reproducir sonido");
            }
            if (clip == null)
            {
                Debug.LogWarning("LHS_CountdownController: AudioClip es null - no se puede reproducir sonido");
            }
        }
    }
}

