using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealDestPosTrigger : MonoBehaviour
{
    [Header("🎯 Configuración de Meta")]
    public bool isFinishLine = true;           // Si este RealDestPos es una línea de meta
    public string playerTag = "Player";        // Tag del jugador
    
    [Header("🔧 Debug")]
    public bool enableDebugLogs = true;        // Habilitar logs de debug
    
    private bool playerHasFinished = false;    // Si el jugador ya llegó a la meta
    
    private void Start()
    {
        // Verificar que el collider esté configurado como trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"⚠️ RealDestPosTrigger: {gameObject.name} debería tener isTrigger = true para detectar llegada del jugador");
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"🎯 RealDestPosTrigger inicializado en: {gameObject.name}");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Verificar si es el jugador y si no ha terminado ya
        if (other.CompareTag(playerTag) && !playerHasFinished && isFinishLine)
        {
            playerHasFinished = true;
            
            if (enableDebugLogs)
            {
                Debug.Log($"🏁 ¡Jugador ha llegado a la meta! Posición: {other.transform.position}");
            }
            
            // 🚫 Desactivar movimiento del jugador
            DisablePlayerMovement(other.gameObject);
            
            // Buscar el UIManager y activar clasificado (pero sin transición inmediata)
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowClassifiedImmediate();
                
                if (enableDebugLogs)
                {
                    Debug.Log("🟡 Mostrando CLASIFICADO - esperando countdown para transición");
                }
            }
            else
            {
                Debug.LogError("❌ No se encontró UIManager para mostrar clasificado");
            }
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Fallback para colliders que no sean trigger
        if (collision.gameObject.CompareTag(playerTag) && !playerHasFinished && isFinishLine)
        {
            playerHasFinished = true;
            
            if (enableDebugLogs)
            {
                Debug.Log($"🏁 ¡Jugador ha llegado a la meta! (Collision) Posición: {collision.transform.position}");
            }
            
            // 🚫 Desactivar movimiento del jugador
            DisablePlayerMovement(collision.gameObject);
            
            // Buscar el UIManager y activar clasificado
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowClassifiedImmediate();
                
                if (enableDebugLogs)
                {
                    Debug.Log("🟡 Mostrando CLASIFICADO - esperando countdown para transición");
                }
            }
            else
            {
                Debug.LogError("❌ No se encontró UIManager para mostrar clasificado");
            }
        }
    }
    
    /// <summary>
    /// 🚫 Desactivar movimiento del jugador cuando se clasifica
    /// </summary>
    private void DisablePlayerMovement(GameObject player)
    {
        if (enableDebugLogs)
        {
            Debug.Log("🚫 Desactivando movimiento del jugador clasificado...");
        }
        
        // Buscar y desactivar scripts de movimiento comunes
        MonoBehaviour[] movementScripts = player.GetComponents<MonoBehaviour>();
        
        foreach (MonoBehaviour script in movementScripts)
        {
            // Desactivar scripts de movimiento conocidos
            if (script.GetType().Name.Contains("Movement") || 
                script.GetType().Name.Contains("Player") ||
                script.GetType().Name.Contains("Controller") ||
                script.GetType().Name.Contains("movimiento"))
            {
                script.enabled = false;
                if (enableDebugLogs)
                {
                    Debug.Log($"🚫 Script desactivado: {script.GetType().Name}");
                }
            }
        }
        
        // Congelar Rigidbody si existe
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            
            if (enableDebugLogs)
            {
                Debug.Log("🚫 Rigidbody congelado");
            }
        }
        
        // Desactivar CharacterController si existe
        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
            
            if (enableDebugLogs)
            {
                Debug.Log("🚫 CharacterController desactivado");
            }
        }
        
        // Opcional: Reproducir animación de celebración/idle
        Animator animator = player.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            // Activar animación de celebración si existe
            animator.SetBool("isClassified", true);
            animator.SetBool("isMoving", false);
            animator.SetBool("isGrounded", true);
            
            if (enableDebugLogs)
            {
                Debug.Log("🎬 Activando animación de clasificado");
            }
        }
    }
    
    /// <summary>
    /// Resetear el estado (útil para testing)
    /// </summary>
    public void ResetFinishState()
    {
        playerHasFinished = false;
        
        if (enableDebugLogs)
        {
            Debug.Log("🔄 Estado de meta reseteado");
        }
    }
} 