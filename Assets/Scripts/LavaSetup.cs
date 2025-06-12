using UnityEngine;
using UnityEngine.UI;

public class LavaSetup : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject deathImage;
    public AudioSource deathSound;
    public AudioSource backgroundMusic;
    
    [Header("Efectos")]
    public GameObject lavaDeathEffect;
    public Material lavaMaterial;
    public ParticleSystem lavaParticles;

    void Start()
    {
        // Asegurar que tenga el tag correcto
        if (!gameObject.CompareTag("Lava"))
        {
            gameObject.tag = "Lava";
            Debug.Log("🔥 Tag 'Lava' asignado al objeto");
        }

        // Asegurar que tenga collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            Debug.Log("🔥 BoxCollider añadido al objeto de lava");
        }
        col.isTrigger = true; // Importante: usar trigger para mejor detección

        // Configurar MuerteLava si no existe
        MuerteLava muerteLava = GetComponent<MuerteLava>();
        if (muerteLava == null)
        {
            muerteLava = gameObject.AddComponent<MuerteLava>();
            Debug.Log("🔥 Componente MuerteLava añadido");
        }

        // Configurar referencias en MuerteLava
        if (deathImage != null) muerteLava.deathImage = deathImage;
        if (deathSound != null) muerteLava.deathSound = deathSound;
        if (backgroundMusic != null) muerteLava.backgroundMusic = backgroundMusic;

        // Configurar LavaTag si no existe
        LavaTag lavaTag = GetComponent<LavaTag>();
        if (lavaTag == null)
        {
            lavaTag = gameObject.AddComponent<LavaTag>();
            Debug.Log("🔥 Componente LavaTag añadido");
        }

        // Configurar referencias en LavaTag
        if (lavaMaterial != null)
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = lavaMaterial;
                Debug.Log("🔥 Material de lava asignado");
            }
        }

        if (lavaParticles != null)
        {
            lavaTag.lavaEffect = lavaParticles;
            Debug.Log("🔥 Efecto de partículas de lava asignado");
        }

        Debug.Log("🔥 Configuración de lava completada");
    }
} 