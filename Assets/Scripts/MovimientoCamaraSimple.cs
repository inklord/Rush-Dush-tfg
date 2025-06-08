using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 📹 Sistema de Cámara Simple y Suave - Solo Tercera Persona
/// Versión simplificada sin vibraciones, solo movimiento suave
/// </summary>
public class MovimientoCamaraSimple : MonoBehaviour
{
    [Header("🎯 Target & Referencias")]
    public Transform player; // Referencia al jugador
    
    [Header("📐 Posicionamiento")]
    public Vector3 offset = new Vector3(0, 5, -8); // Posición relativa al jugador
    public float smoothSpeed = 5f; // Velocidad de suavizado (más bajo = más suave)
    public float lookAtHeight = 1.5f; // Altura a la que mira la cámara
    
    [Header("🖱️ Control de Mouse")]
    public float mouseSensitivity = 100f; // Sensibilidad del mouse
    public float minYRotation = -30f; // Límite inferior
    public float maxYRotation = 60f; // Límite superior
    
    [Header("💥 Camera Shake")]
    public bool enableShake = true;
    public float shakeIntensity = 1f;
    
    // Variables privadas
    private float mouseX = 0f;
    private float mouseY = 0f;
    private Vector3 currentVelocity;
    
    // Sistema de shake simplificado
    private Vector3 shakeOffset = Vector3.zero;
    private float shakeTimer = 0f;
    private float shakeDuration = 0f;
    
    void Start()
    {
        // El cursor ahora es manejado por CursorManager
        // No configurar cursor aquí
        
        // Buscar jugador si no está asignado
        if (player == null)
        {
            StartCoroutine(FindPlayer());
        }
        
        Debug.Log("📹 Cámara simple inicializada");
    }
    
    IEnumerator FindPlayer()
    {
        while (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("✅ Jugador encontrado automáticamente");
                break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    void Update()
    {
        // Solo actualizar shake
        UpdateShake();
    }
    
    void LateUpdate()
    {
        if (player == null) return;
        
        // Solo procesar input de mouse si el cursor está bloqueado (en juego)
        bool canControlCamera = CursorManager.Instance == null || CursorManager.Instance.cursorLocked;
        
        if (canControlCamera)
        {
            // Input del mouse
            mouseX += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            mouseY = Mathf.Clamp(mouseY, minYRotation, maxYRotation);
        }
        
        // Calcular posición objetivo
        Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0);
        Vector3 targetPosition = player.position - (rotation * Vector3.forward * offset.magnitude) + Vector3.up * offset.y;
        
        // Aplicar suavizado con SmoothDamp (más suave que Lerp)
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition + shakeOffset, ref currentVelocity, 1f / smoothSpeed);
        
        // Mirar al jugador
        Vector3 lookTarget = player.position + Vector3.up * lookAtHeight;
        transform.LookAt(lookTarget);
    }
    
    void UpdateShake()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            
            // Shake simple y suave
            float shakeAmount = shakeIntensity * (shakeTimer / shakeDuration);
            shakeOffset = Random.insideUnitSphere * shakeAmount;
            
            if (shakeTimer <= 0)
            {
                shakeOffset = Vector3.zero;
            }
        }
    }
    
    /// <summary>
    /// 💥 Activar shake de cámara
    /// </summary>
    public void ShakeCamera(float duration = 0.5f, float intensity = 1f)
    {
        if (!enableShake) return;
        
        shakeDuration = duration;
        shakeTimer = duration;
        shakeIntensity = intensity;
    }
    
    /// <summary>
    /// 🎯 Asignar nuevo objetivo
    /// </summary>
    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        Debug.Log($"📹 Nuevo jugador asignado: {newPlayer.name}");
    }
    
    /// <summary>
    /// 🔄 Reset de cámara
    /// </summary>
    public void ResetCamera()
    {
        mouseX = 0f;
        mouseY = 0f;
        shakeOffset = Vector3.zero;
        shakeTimer = 0f;
    }
    
    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            // Mostrar conexión visual
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, player.position);
            
            // Mostrar punto de mira
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position + Vector3.up * lookAtHeight, 0.2f);
        }
    }
} 