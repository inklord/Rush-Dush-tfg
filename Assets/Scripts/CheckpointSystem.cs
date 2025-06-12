using UnityEngine;
using Photon.Pun;

/// <summary>
/// 🚩 CHECKPOINT SYSTEM - Sistema unificado de checkpoints y respawn
/// </summary>
public class CheckpointSystem : MonoBehaviourPunCallbacks
{
    [Header("🚩 Configuración de Checkpoints")]
    public Transform lastCheckpoint;
    public float respawnHeight = -10f;
    public float respawnDelay = 1f;
    public bool showDebugInfo = true;

    [Header("🎮 Efectos")]
    public ParticleSystem respawnEffect;
    public AudioClip respawnSound;
    public AudioClip checkpointSound;

    // Referencias privadas
    private bool isRespawning = false;
    private AudioSource audioSource;
    private Rigidbody rb;
    // private Animator anim; // YA NO NECESARIO - animaciones eliminadas

    void Start()
    {
        // Inicializar referencias
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (respawnSound != null || checkpointSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        rb = GetComponent<Rigidbody>();
        // anim = GetComponentInChildren<Animator>(); // YA NO NECESARIO
    }

    void Update()
    {
        // Si el jugador cae por debajo de cierta altura, respawnear
        if (transform.position.y < respawnHeight && !isRespawning)
        {
            if (showDebugInfo)
                Debug.Log("🔄 Jugador cayó, iniciando respawn...");
            
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
            SetCheckpoint(other.transform);
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
    public void SetCheckpoint(Transform checkpoint)
    {
        if (checkpoint != null)
        {
            lastCheckpoint = checkpoint;
            if (showDebugInfo)
                Debug.Log($"✅ Nuevo checkpoint establecido en: {checkpoint.position}");
        }
    }

    /// <summary>
    /// 🔄 Respawnear al jugador
    /// </summary>
    void Respawn()
    {
        if (isRespawning) return;
        isRespawning = true;

        if (lastCheckpoint != null)
        {
            if (showDebugInfo)
                Debug.Log($"🔄 Respawneando en último checkpoint: {lastCheckpoint.position}");

            // Desactivar física temporalmente
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
            }

            // Teletransportar al checkpoint
            transform.position = lastCheckpoint.position;
            transform.rotation = lastCheckpoint.rotation;

            // Reactivar física
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No hay checkpoint establecido para respawn");
        }

        isRespawning = false;

        // Efectos de respawn
        if (respawnEffect != null)
        {
            respawnEffect.Play();
        }

        if (respawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }
    }

    void OnDrawGizmos()
    {
        // Dibujar línea de altura de respawn
        Vector3 playerPos = transform.position;
        Vector3 respawnLineStart = new Vector3(playerPos.x - 5f, respawnHeight, playerPos.z);
        Vector3 respawnLineEnd = new Vector3(playerPos.x + 5f, respawnHeight, playerPos.z);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(respawnLineStart, respawnLineEnd);

        // Dibujar checkpoint actual si existe
        if (lastCheckpoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(lastCheckpoint.position, 1f);
        }
    }
} 