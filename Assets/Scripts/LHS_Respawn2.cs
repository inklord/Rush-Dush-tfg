using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LHS_Respawn2 : MonoBehaviour
{
    [SerializeField] float spawnValue;
    [SerializeField] GameObject player;
    [SerializeField] Transform respawnPoint;

    [Header("🔧 Debug Settings")]
    public bool showDebugLogs = true;

    // Animator reference - YA NO NECESARIO (animaciones eliminadas)
    // Animator anim;

    private RaycastHit hit;
    private int layerMask;
    public float distance = 10;
    AudioSource resp;

    void Awake()
    {
        if (player != null)
        {
            // Animator ya no necesario - animaciones eliminadas
            // anim = player.GetComponentInChildren<Animator>();
            if (showDebugLogs) Debug.Log($"✅ Player asignado: {player.name} (sin animaciones de caída)");
        }
        else
        {
            Debug.LogError("❌ Player no asignado en LHS_Respawn2");
        }
        
        layerMask = 1 << 7; // Layer 7
        resp = GetComponent<AudioSource>();
        
        if (showDebugLogs)
        {
            Debug.Log($"🎯 LHS_Respawn2 inicializado:");
            Debug.Log($"   Layer Mask: {layerMask} (Layer 7)");
            Debug.Log($"   Distance: {distance}");
            Debug.Log($"   Player: {(player != null ? player.name : "NULL")}");
            Debug.Log($"   RespawnPoint: {(respawnPoint != null ? respawnPoint.name : "NULL")}");
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;
        
        // Detectar si el jugador está cayendo (Layer 7 = superficie de caída)
        if (Physics.Raycast(player.transform.position, -player.transform.up, out hit, distance, layerMask))
        {
            // Solo reproducir sonido, SIN animación
            if (showDebugLogs)
            {
                Debug.Log($"🔍 Jugador empezó a caer - Raycast detectó: {hit.collider.name} en layer {hit.collider.gameObject.layer}");
            }
            // DownPlayer(); // DESHABILITADO - No más animación de caída
            if (resp != null) resp.Play();
        }
    }

    void DownPlayer()
    {
        // MÉTODO DESHABILITADO - No más animación de caída
        if (showDebugLogs) Debug.Log("💥 Jugador cayendo - Animación de caída DESHABILITADA");
        
        // Código anterior comentado:
        // if (anim != null)
        // {
        //     anim.SetBool("isFalling", true);
        // }
    }

    // Respawn trigger collision - Reset player to respawn point
    private void OnTriggerEnter(Collider other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"🔍 Trigger detectado: {other.name} con tag: {other.tag}");
        }
        
        if(other.CompareTag("Player"))
        {
            if (showDebugLogs) Debug.Log("✅ Tag Player confirmado - Iniciando respawn");
            
            // Animación de caída eliminada del proyecto
            if (showDebugLogs) Debug.Log("🎬 Respawn completado - SIN animación de caída");
            
            // Código anterior comentado:
            // if (anim != null)
            // {
            //     anim.SetBool("isFalling", false);
            // }

            if (respawnPoint != null)
            {
                Vector3 oldPos = player.transform.position;
                player.transform.position = respawnPoint.transform.position;
                if (showDebugLogs)
                {
                    Debug.Log($"🎯 Jugador respawneado:");
                    Debug.Log($"   De: {oldPos}");
                    Debug.Log($"   A: {respawnPoint.transform.position}");
                }
            }
            else
            {
                Debug.LogError("❌ RespawnPoint no asignado");
            }
            
            // Asegurar que la física se actualice
            Physics.SyncTransforms();
        }
        else
        {
            if (showDebugLogs) Debug.Log($"❌ Tag incorrecto: {other.tag} (esperado: Player)");
        }
    }
}
