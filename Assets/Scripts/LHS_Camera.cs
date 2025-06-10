using UnityEngine;
using Photon.Pun;

public class LHS_Camera : MonoBehaviour
{
    public GameObject player;
    public float distance = 10f;
    public float height = 5f;
    public float smoothSpeed = 5f;
    
    private Vector3 targetPosition;
    
    void Start()
    {
        // Si no hay jugador asignado, intentar encontrar el jugador local
        if (player == null)
        {
            FindLocalPlayer();
        }
    }
    
    void LateUpdate()
    {
        // Si no hay jugador asignado, intentar encontrarlo
        if (player == null)
        {
            FindLocalPlayer();
            return;
        }
        
        // Calcular la posición objetivo de la cámara
        targetPosition = player.transform.position;
        targetPosition.y += height;
        targetPosition.z -= distance;
        
        // Mover la cámara suavemente hacia la posición objetivo
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        
        // Hacer que la cámara mire al jugador
        transform.LookAt(player.transform.position);
    }
    
    void FindLocalPlayer()
    {
        // Buscar todos los PhotonViews en la escena
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach (PhotonView view in views)
        {
            // Buscar el jugador que nos pertenece
            if (view.IsMine && view.gameObject.GetComponent<LHS_MainPlayer>() != null)
            {
                player = view.gameObject;
                Debug.Log("✅ Cámara: Jugador local encontrado");
                return;
            }
        }
    }
}
