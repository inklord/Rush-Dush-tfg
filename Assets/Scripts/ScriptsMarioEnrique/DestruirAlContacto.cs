using UnityEngine;
using Photon.Pun;



public class DestruirAlContacto : MonoBehaviourPunCallbacks
{
    private Vector3 lastCheckpoint;
    private bool hasCheckpoint = false; // Verifica si el jugador ha tocado un checkpoint

    private void Start()
    {
        lastCheckpoint = transform.position; // Guardar posición inicial como primer checkpoint
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si el objeto con el que colisiona es un checkpoint, actualiza la posición de respawn
        if (other.CompareTag("Checkpoint"))
        {
            lastCheckpoint = other.transform.position;
            hasCheckpoint = true;
        }
    }

   private void OnCollisionEnter(Collision collision)
{
    // "gameObject" es el Player porque el script está en el Player
    if (gameObject.CompareTag("Player") && photonView.IsMine)
    {
        // Si choca con una plataforma mortal
        if (collision.gameObject.CompareTag("PlataformaMortal"))
        {
            Respawn();
        }
    }
}


    private void Respawn()
    {
        if (hasCheckpoint)
        {
            transform.position = lastCheckpoint;
        }
        else
        {
            transform.position = Vector3.zero; // Posición por defecto si no hay checkpoints
        }
    }
}
