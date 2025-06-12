using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DestinationCount : MonoBehaviour
{
    private UIManager uiManager;
    private bool playerHasFinished = false;
    private bool isDebugEnabled = true;

    private void Start()
    {
        // Buscar el UIManager
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("❌ DestinationCount: No se encontró el UIManager");
        }
        
        // Asegurarse de que el collider sea trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"⚠️ DestinationCount: {gameObject.name} - Configurando collider como trigger");
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDebugEnabled)
            Debug.Log($"🎯 Trigger Enter: {other.gameObject.name} (Tag: {other.tag})");
        
        // No procesar si ya terminó
        if (playerHasFinished) return;

        // Verificar si es un jugador o IA
        bool isPlayer = other.CompareTag("Player");
        bool isAI = other.CompareTag("IA");
        
        if (!isPlayer && !isAI)
        {
            if (isDebugEnabled)
                Debug.Log($"🚫 No es jugador ni IA: {other.tag}");
            return;
        }

        // Para jugadores, verificar si es local
        if (isPlayer)
        {
            PhotonView photonView = other.GetComponent<PhotonView>();
            if (photonView != null && !photonView.IsMine)
            {
                if (isDebugEnabled)
                    Debug.Log("🚫 No es el jugador local - ignorando");
                return;
            }
        }
        
        // Marcar como terminado
        playerHasFinished = true;
        
        if (isDebugEnabled)
            Debug.Log($"🎯 {(isPlayer ? "Jugador" : "IA")} clasificado: {other.gameObject.name}");
        
        // Incrementar el ranking y mostrar clasificado
        if (uiManager != null)
        {
            uiManager.CurRank++;
            
            // Solo mostrar UI de clasificado para jugadores reales
            if (isPlayer)
            {
                uiManager.ShowClassifiedImmediate();
                if (isDebugEnabled)
                    Debug.Log("🟡 Mostrando CLASIFICADO - esperando countdown para transición");
            }
            else
            {
                if (isDebugEnabled)
                    Debug.Log($"🤖 IA clasificada - Rank actual: {uiManager.CurRank}");
            }
        }
        else
        {
            Debug.LogError("❌ No se encontró UIManager para actualizar ranking");
        }
        
        // Desactivar movimiento
        DisableMovement(other.gameObject);
    }
    
    private void DisableMovement(GameObject obj)
    {
        if (isDebugEnabled)
            Debug.Log("🚫 Desactivando movimiento...");
        
        // Para jugadores
        LHS_MainPlayer mainPlayer = obj.GetComponent<LHS_MainPlayer>();
        if (mainPlayer != null)
        {
            mainPlayer.enabled = false;
            if (isDebugEnabled)
                Debug.Log("🚫 LHS_MainPlayer desactivado");
        }
        
        // Para IAs
        AINavMesh aiNav = obj.GetComponent<AINavMesh>();
        if (aiNav != null)
        {
            aiNav.enabled = false;
            if (isDebugEnabled)
                Debug.Log("🚫 AINavMesh desactivado");
        }
        
        // Congelar Rigidbody
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            if (isDebugEnabled)
                Debug.Log("🚫 Rigidbody congelado");
        }
        
        // Activar animación de celebración
        Animator animator = obj.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetBool("isClassified", true);
            animator.SetBool("isMoving", false);
            animator.SetBool("isGrounded", true);
            if (isDebugEnabled)
                Debug.Log("🎬 Activando animación de clasificado");
        }
    }
}
