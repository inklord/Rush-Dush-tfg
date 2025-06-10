using UnityEngine;
using Photon.Pun;

/// <summary>
/// 🚩 CHECKPOINT SYSTEM - Sistema unificado de checkpoints y respawn
/// </summary>
public class CheckpointSystem : MonoBehaviourPunCallbacks
{
    [Header("🚩 Configuración de Checkpoints")]
    public float respawnHeight = -10f;
    public float respawnDelay = 1f;
    public bool showDebugGizmos = true;

    [Header("🎮 Efectos")]
    public ParticleSystem respawnEffect;
    public AudioClip respawnSound;
    public AudioClip checkpointSound;

    // Referencias privadas
    private Vector3 lastCheckpoint;
    private bool hasCheckpoint = false;
    private AudioSource audioSource;
    private Rigidbody rb;
    private Animator anim;

    void Start()
    {
        // Inicializar referencias
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (respawnSound != null || checkpointSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        // Guardar posición inicial como primer checkpoint
        lastCheckpoint = transform.position;
        Debug.Log($"🚩 Checkpoint inicial establecido en: {lastCheckpoint}");
    }

    void Update()
    {
        // Solo el dueño del objeto puede manejar el respawn
        if (!photonView.IsMine) return;

        // Verificar si el jugador cayó por debajo del límite
        if (transform.position.y < respawnHeight)
        {
            Respawn();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Solo el dueño del objeto puede activar checkpoints
        if (!photonView.IsMine) return;

        // Verificar si es un checkpoint
        if (other.CompareTag("Checkpoint"))
        {
            UpdateCheckpoint(other.transform.position);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Solo el dueño del objeto puede manejar el respawn
        if (!photonView.IsMine) return;

        // Si choca con una plataforma mortal
        if (collision.gameObject.CompareTag("PlataformaMortal"))
        {
            Respawn();
        }
    }

    /// <summary>
    /// 🚩 Actualizar posición del checkpoint
    /// </summary>
    void UpdateCheckpoint(Vector3 newCheckpoint)
    {
        lastCheckpoint = newCheckpoint;
        hasCheckpoint = true;

        // Efectos de checkpoint
        if (checkpointSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(checkpointSound);
        }

        Debug.Log($"🚩 Nuevo checkpoint establecido en: {lastCheckpoint}");
    }

    /// <summary>
    /// 🔄 Respawnear al jugador
    /// </summary>
    void Respawn()
    {
        // Detener movimiento
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Activar animación de caída si existe
        if (anim != null)
        {
            anim.SetBool("isFalling", true);
        }

        // Teletransportar al último checkpoint
        transform.position = lastCheckpoint;

        // Efectos de respawn
        if (respawnEffect != null)
        {
            respawnEffect.Play();
        }

        if (respawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }

        // Resetear animación después del respawn
        if (anim != null)
        {
            anim.SetBool("isFalling", false);
        }

        Debug.Log($"🔄 Jugador respawneado en: {lastCheckpoint}");
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Visualizar último checkpoint
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lastCheckpoint, 0.5f);

        // Visualizar altura de respawn
        Gizmos.color = Color.red;
        Vector3 respawnLine = transform.position;
        respawnLine.y = respawnHeight;
        Gizmos.DrawLine(transform.position, respawnLine);
    }
} 