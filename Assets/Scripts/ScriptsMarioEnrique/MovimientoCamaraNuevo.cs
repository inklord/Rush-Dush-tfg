using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 📹 Sistema de Cámara Profesional Mejorado
/// Características: Múltiples modos, detección de colisiones, efectos de shake, transiciones suaves
/// </summary>
public class MovimientoCamaraNuevo : MonoBehaviour
{
    [Header("🎯 Target & References")]
    public Transform player; // Referencia al jugador
    public LayerMask collisionMask = -1; // Capas que pueden bloquear la cámara
    
    [Header("📐 Camera Positioning")]
    public Vector3 offset = new Vector3(0, 5, -10); // Distancia inicial de la cámara
    public float smoothSpeed = 8f; // Velocidad de suavizado de la cámara
    public float lookAtHeight = 1.8f; // Altura a la que mira la cámara del jugador
    
    [Header("🖱️ Mouse Controls")]
    public float sensibilidadX = 300f; // Sensibilidad del mouse en el eje X
    public float sensibilidadY = 200f; // Sensibilidad del mouse en el eje Y
    public float limiteRotacionMin = -45f; // Límite inferior de la cámara
    public float limiteRotacionMax = 80f; // Límite superior de la cámara
    public bool invertYAxis = false; // Invertir eje Y del mouse
    
    [Header("🔍 Zoom Settings")]
    public float zoomSpeed = 3f;
    public float minZoom = 3f;
    public float maxZoom = 25f;
    public float zoomSmoothness = 10f;
    
    [Header("🚧 Collision Detection")]
    public bool enableCollisionDetection = true;
    public float collisionBuffer = 0.3f; // Espacio extra para evitar que la cámara toque paredes
    public float collisionSmoothness = 15f;
    
    [Header("💥 Camera Shake")]
    public bool enableCameraShake = true;
    public float shakeIntensity = 1f;
    
    [Header("🎮 Camera Modes")]
    public CameraMode currentMode = CameraMode.ThirdPerson;
    public float firstPersonHeight = 1.7f;
    public float shoulderOffsetX = 0.5f;
    
    [Header("⚙️ Advanced Settings")]
    public bool enableAutoFocus = true;
    public float autoFocusSpeed = 2f;
    public bool enableSmartPivot = true;
    public float pivotSmoothness = 5f;
    public bool showDebugInfo = false;
    
    [Header("🔧 Input Settings")]
    public KeyCode toggleCursorKey = KeyCode.LeftAlt;
    public KeyCode resetCameraKey = KeyCode.R;
    public KeyCode switchModeKey = KeyCode.C;
    
    // Enumeración para modos de cámara
    public enum CameraMode
    {
        ThirdPerson,
        FirstPerson,
        OverShoulder,
        Free,
        Cinematic
    }
    
    // Variables privadas
    private float rotacionActualX = 0f;
    private float rotacionActualY = 15f; // Inclinación inicial mejorada
    private float distanciaActual;
    private float targetDistance;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    
    // Sistema de shake
    private Vector3 shakeOffset = Vector3.zero;
    private float shakeTimer = 0f;
    private float shakeDuration = 0f;
    private float currentShakeIntensity = 0f;
    
    // Control de cursor
    private bool cursorLocked = true;
    private bool cursorVisible = false;
    
    // Pivot inteligente
    private Vector3 smartPivotOffset = Vector3.zero;
    private Vector3 lastPlayerPosition;
    private Vector3 playerVelocity;
    
    // Estado de colisión
    private bool isColliding = false;
    private float originalDistance;
    
    void Start()
    {
        StartCoroutine(FindPlayer());
        SetupCamera();
    }
    
    void SetupCamera()
    {
        distanciaActual = offset.magnitude;
        targetDistance = distanciaActual;
        originalDistance = distanciaActual;
        
        // Validaciones de seguridad
        if (smoothSpeed <= 0) smoothSpeed = 8f;
        if (targetDistance <= 0) targetDistance = 10f;
        if (zoomSmoothness <= 0) zoomSmoothness = 10f;
        
        // Configurar cursor
        SetCursorState(true, false);
        
        // Inicializar variables
        lastPlayerPosition = player != null ? player.position : Vector3.zero;
        currentVelocity = Vector3.zero;
        targetPosition = transform.position;
        
        Debug.Log("📹 Cámara profesional inicializada");
    }
    
    IEnumerator FindPlayer()
    {
        while (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                lastPlayerPosition = player.position;
                Debug.Log("✅ Jugador encontrado y cámara asignada.");
                break;
            }
            else
            {
                Debug.LogWarning("⏳ Buscando jugador...");
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
    
    void Update()
    {
        HandleInput();
        CalculatePlayerVelocity();
        UpdateCameraShake();
    }
    
    void LateUpdate()
    {
        if (player == null) return;
        
        switch (currentMode)
        {
            case CameraMode.ThirdPerson:
                UpdateThirdPersonCamera();
                break;
            case CameraMode.FirstPerson:
                UpdateFirstPersonCamera();
                break;
            case CameraMode.OverShoulder:
                UpdateOverShoulderCamera();
                break;
            case CameraMode.Free:
                UpdateFreeCamera();
                break;
            case CameraMode.Cinematic:
                UpdateCinematicCamera();
                break;
        }
        
        if (enableCollisionDetection)
        {
            HandleCameraCollision();
        }
        
        if (showDebugInfo)
        {
            ShowDebugInfo();
        }
    }
    
    void HandleInput()
    {
        // Toggle cursor
        if (Input.GetKeyDown(toggleCursorKey))
        {
            ToggleCursor();
        }
        
        // Reset camera
        if (Input.GetKeyDown(resetCameraKey))
        {
            ResetCamera();
        }
        
        // Switch camera mode
        if (Input.GetKeyDown(switchModeKey))
        {
            SwitchCameraMode();
        }
        
        // Escape para liberar cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorState(false, true);
        }
    }
    
    void UpdateThirdPersonCamera()
    {
        if (!cursorLocked) return;
        
        // Validación de jugador
        if (player == null) return;
        
        // Input del mouse
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadY * Time.deltaTime;
        
        if (invertYAxis) mouseY = -mouseY;
        
        // Validar inputs
        if (float.IsNaN(mouseX)) mouseX = 0f;
        if (float.IsNaN(mouseY)) mouseY = 0f;
        
        rotacionActualX += mouseX;
        rotacionActualY -= mouseY;
        rotacionActualY = Mathf.Clamp(rotacionActualY, limiteRotacionMin, limiteRotacionMax);
        
        // Zoom
        HandleZoom();
        
        // Pivot inteligente con validación
        Vector3 pivot = player.position + Vector3.up * lookAtHeight;
        if (enableSmartPivot)
        {
            Vector3 smartPivot = CalculateSmartPivot();
            if (IsValidVector3(smartPivot))
            {
                pivot += smartPivot;
            }
        }
        
        // Calcular posición de la cámara con validaciones
        Quaternion rotacionFinal = Quaternion.Euler(rotacionActualY, rotacionActualX, 0);
        Vector3 newTargetPosition = pivot - (rotacionFinal * Vector3.forward * distanciaActual);
        
        // Validar nueva posición
        if (IsValidVector3(newTargetPosition))
        {
            targetPosition = newTargetPosition;
        }
        else
        {
            Debug.LogWarning("⚠️ Posición de cámara inválida detectada, usando posición anterior");
            targetPosition = transform.position; // Usar posición actual como fallback
        }
        
        // Validar shake offset
        if (!IsValidVector3(shakeOffset))
        {
            shakeOffset = Vector3.zero;
        }
        
        // Aplicar movimiento suave con validaciones
        Vector3 finalTargetPosition = targetPosition + shakeOffset;
        if (IsValidVector3(finalTargetPosition) && smoothSpeed > 0)
        {
            Vector3 newPosition = Vector3.SmoothDamp(transform.position, finalTargetPosition, ref currentVelocity, 1f / smoothSpeed);
            
            if (IsValidVector3(newPosition))
            {
                transform.position = newPosition;
            }
            else
            {
                Debug.LogWarning("⚠️ Nueva posición inválida, manteniendo posición actual");
            }
        }
        
        // Mirar al jugador con validación
        Vector3 lookTarget = pivot;
        if (enableAutoFocus)
        {
            Vector3 autoFocus = CalculateAutoFocus();
            if (IsValidVector3(autoFocus))
            {
                lookTarget += autoFocus;
            }
        }
        
        if (IsValidVector3(lookTarget))
        {
            transform.LookAt(lookTarget);
        }
    }
    
    void UpdateFirstPersonCamera()
    {
        if (!cursorLocked || player == null) return;
        
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadY * Time.deltaTime;
        
        // Validar inputs
        if (float.IsNaN(mouseX)) mouseX = 0f;
        if (float.IsNaN(mouseY)) mouseY = 0f;
        
        if (invertYAxis) mouseY = -mouseY;
        
        rotacionActualX += mouseX;
        rotacionActualY -= mouseY;
        rotacionActualY = Mathf.Clamp(rotacionActualY, -90f, 90f);
        
        // Posicionar cámara en la cabeza del jugador
        Vector3 headPosition = player.position + Vector3.up * firstPersonHeight;
        Vector3 finalPosition = headPosition + shakeOffset;
        
        if (IsValidVector3(finalPosition) && smoothSpeed > 0)
        {
            transform.position = Vector3.Lerp(transform.position, finalPosition, smoothSpeed * Time.deltaTime);
        }
        
        // Rotar cámara
        Quaternion targetRotation = Quaternion.Euler(rotacionActualY, rotacionActualX, 0);
        if (smoothSpeed > 0)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
        }
    }
    
    void UpdateOverShoulderCamera()
    {
        if (!cursorLocked || player == null) return;
        
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadY * Time.deltaTime;
        
        // Validar inputs
        if (float.IsNaN(mouseX)) mouseX = 0f;
        if (float.IsNaN(mouseY)) mouseY = 0f;
        
        if (invertYAxis) mouseY = -mouseY;
        
        rotacionActualX += mouseX;
        rotacionActualY -= mouseY;
        rotacionActualY = Mathf.Clamp(rotacionActualY, limiteRotacionMin, limiteRotacionMax);
        
        HandleZoom();
        
        // Posición sobre el hombro
        Vector3 shoulderOffset = new Vector3(shoulderOffsetX, lookAtHeight, -distanciaActual * 0.3f);
        Quaternion rotacionFinal = Quaternion.Euler(rotacionActualY, rotacionActualX, 0);
        
        Vector3 pivot = player.position + Vector3.up * lookAtHeight;
        Vector3 newTargetPosition = pivot + (rotacionFinal * shoulderOffset);
        
        if (IsValidVector3(newTargetPosition))
        {
            targetPosition = newTargetPosition;
        }
        
        Vector3 finalPosition = targetPosition + shakeOffset;
        if (IsValidVector3(finalPosition) && smoothSpeed > 0)
        {
            transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref currentVelocity, 1f / smoothSpeed);
        }
        
        if (IsValidVector3(pivot))
        {
            transform.LookAt(pivot);
        }
    }
    
    void UpdateFreeCamera()
    {
        if (!cursorLocked) return;
        
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadY * Time.deltaTime;
        
        // Validar inputs
        if (float.IsNaN(mouseX)) mouseX = 0f;
        if (float.IsNaN(mouseY)) mouseY = 0f;
        
        if (invertYAxis) mouseY = -mouseY;
        
        rotacionActualX += mouseX;
        rotacionActualY -= mouseY;
        rotacionActualY = Mathf.Clamp(rotacionActualY, -90f, 90f);
        
        // Movimiento libre con WASD
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;
        if (Input.GetKey(KeyCode.Q)) moveDirection += Vector3.down;
        if (Input.GetKey(KeyCode.E)) moveDirection += Vector3.up;
        
        Vector3 newPosition = transform.position + moveDirection * 10f * Time.deltaTime;
        if (IsValidVector3(newPosition))
        {
            transform.position = newPosition;
        }
        
        Quaternion targetRotation = Quaternion.Euler(rotacionActualY, rotacionActualX, 0);
        if (smoothSpeed > 0)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
        }
    }
    
    void UpdateCinematicCamera()
    {
        if (player == null) return;
        
        // Cámara cinemática que sigue al jugador con movimientos suaves
        Vector3 cinematicPosition = player.position + offset + Vector3.up * 3f;
        Vector3 finalPosition = cinematicPosition + shakeOffset;
        
        if (IsValidVector3(finalPosition))
        {
            transform.position = Vector3.Slerp(transform.position, finalPosition, Time.deltaTime * 0.5f);
        }
        
        Vector3 lookDirection = (player.position - transform.position).normalized;
        if (IsValidVector3(lookDirection) && lookDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
        }
    }
    
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        
        // Validar scroll
        if (float.IsNaN(scroll)) scroll = 0f;
        
        targetDistance = Mathf.Clamp(targetDistance - scroll, minZoom, maxZoom);
        
        // Validar targetDistance
        if (float.IsNaN(targetDistance) || targetDistance <= 0)
        {
            targetDistance = originalDistance;
        }
        
        if (zoomSmoothness > 0)
        {
            float newDistance = Mathf.Lerp(distanciaActual, targetDistance, zoomSmoothness * Time.deltaTime);
            if (!float.IsNaN(newDistance) && newDistance > 0)
            {
                distanciaActual = newDistance;
            }
        }
    }
    
    void HandleCameraCollision()
    {
        if (currentMode == CameraMode.FirstPerson) return;
        
        Vector3 targetPos = player.position + Vector3.up * lookAtHeight;
        Vector3 directionToCamera = (targetPosition - targetPos).normalized;
        float desiredDistance = Vector3.Distance(targetPos, targetPosition);
        
        RaycastHit hit;
        if (Physics.Raycast(targetPos, directionToCamera, out hit, desiredDistance + collisionBuffer, collisionMask))
        {
            float adjustedDistance = hit.distance - collisionBuffer;
            Vector3 adjustedPosition = targetPos + directionToCamera * adjustedDistance;
            
            transform.position = Vector3.Lerp(transform.position, adjustedPosition + shakeOffset, collisionSmoothness * Time.deltaTime);
            isColliding = true;
        }
        else
        {
            isColliding = false;
        }
    }
    
    Vector3 CalculateSmartPivot()
    {
        // Validar playerVelocity
        if (!IsValidVector3(playerVelocity))
        {
            playerVelocity = Vector3.zero;
        }
        
        // Ajustar el pivot basado en la velocidad del jugador
        Vector3 velocityOffset = Vector3.zero;
        if (playerVelocity.magnitude > 0.1f)
        {
            velocityOffset = playerVelocity.normalized * Mathf.Clamp(playerVelocity.magnitude * 0.5f, 0f, 2f);
        }
        
        if (pivotSmoothness > 0)
        {
            Vector3 newOffset = Vector3.Lerp(smartPivotOffset, velocityOffset, pivotSmoothness * Time.deltaTime);
            if (IsValidVector3(newOffset))
            {
                smartPivotOffset = newOffset;
            }
        }
        
        return smartPivotOffset;
    }
    
    Vector3 CalculateAutoFocus()
    {
        // Validar playerVelocity
        if (!IsValidVector3(playerVelocity))
        {
            return Vector3.zero;
        }
        
        // Enfoque automático basado en la velocidad
        if (playerVelocity.magnitude > 0.1f)
        {
            return playerVelocity.normalized * Mathf.Clamp(playerVelocity.magnitude * 0.3f, 0f, 1f);
        }
        
        return Vector3.zero;
    }
    
    void CalculatePlayerVelocity()
    {
        if (player != null)
        {
            Vector3 newVelocity = (player.position - lastPlayerPosition) / Time.deltaTime;
            
            // Validar velocidad calculada
            if (IsValidVector3(newVelocity) && Time.deltaTime > 0)
            {
                playerVelocity = newVelocity;
            }
            else
            {
                playerVelocity = Vector3.zero;
            }
            
            lastPlayerPosition = player.position;
        }
    }
    
    void UpdateCameraShake()
    {
        if (!enableCameraShake) return;
        
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            float shakeAmount = currentShakeIntensity * (shakeTimer / shakeDuration);
            
            // Validar shakeAmount
            if (float.IsNaN(shakeAmount) || shakeDuration <= 0)
            {
                shakeAmount = 0f;
            }
            
            shakeOffset = new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount)
            );
            
            // Validar shakeOffset
            if (!IsValidVector3(shakeOffset))
            {
                shakeOffset = Vector3.zero;
            }
        }
        else
        {
            shakeOffset = Vector3.Lerp(shakeOffset, Vector3.zero, 10f * Time.deltaTime);
            
            // Validar shakeOffset después del lerp
            if (!IsValidVector3(shakeOffset))
            {
                shakeOffset = Vector3.zero;
            }
        }
    }
    
    #region Public Methods
    
    /// <summary>
    /// 💥 Activar shake de cámara
    /// </summary>
    public void ShakeCamera(float duration = 0.5f, float intensity = 1f)
    {
        if (!enableCameraShake) return;
        
        shakeDuration = duration;
        shakeTimer = duration;
        currentShakeIntensity = intensity * shakeIntensity;
        
        Debug.Log($"📹 Camera shake: {intensity} por {duration}s");
    }
    
    /// <summary>
    /// 🔄 Resetear cámara a posición inicial
    /// </summary>
    public void ResetCamera()
    {
        rotacionActualX = 0f;
        rotacionActualY = 15f;
        targetDistance = originalDistance;
        currentMode = CameraMode.ThirdPerson;
        
        Debug.Log("🔄 Cámara reseteada");
    }
    
    /// <summary>
    /// 🎮 Cambiar modo de cámara
    /// </summary>
    public void SwitchCameraMode()
    {
        int nextMode = ((int)currentMode + 1) % System.Enum.GetValues(typeof(CameraMode)).Length;
        currentMode = (CameraMode)nextMode;
        
        Debug.Log($"📹 Modo de cámara: {currentMode}");
    }
    
    /// <summary>
    /// 🖱️ Toggle cursor lock/unlock
    /// </summary>
    public void ToggleCursor()
    {
        SetCursorState(!cursorLocked, !cursorVisible);
    }
    
    /// <summary>
    /// 🔧 Configurar estado del cursor
    /// </summary>
    public void SetCursorState(bool locked, bool visible)
    {
        cursorLocked = locked;
        cursorVisible = visible;
        
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = visible;
        
        Debug.Log($"🖱️ Cursor - Locked: {locked}, Visible: {visible}");
    }
    
    /// <summary>
    /// 🎯 Cambiar objetivo de la cámara
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        player = newTarget;
        if (player != null)
        {
            lastPlayerPosition = player.position;
            Debug.Log($"🎯 Nuevo objetivo: {player.name}");
        }
    }
    
    /// <summary>
    /// 🛡️ Validar si un Vector3 es válido (no contiene NaN o infinito)
    /// </summary>
    bool IsValidVector3(Vector3 vector)
    {
        return !float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z) &&
               !float.IsInfinity(vector.x) && !float.IsInfinity(vector.y) && !float.IsInfinity(vector.z);
    }
    
    /// <summary>
    /// 🔧 Resetear todos los valores de la cámara a valores seguros
    /// </summary>
    public void ResetToSafeValues()
    {
        rotacionActualX = 0f;
        rotacionActualY = 15f;
        distanciaActual = originalDistance > 0 ? originalDistance : 10f;
        targetDistance = distanciaActual;
        currentVelocity = Vector3.zero;
        shakeOffset = Vector3.zero;
        smartPivotOffset = Vector3.zero;
        playerVelocity = Vector3.zero;
        
        if (player != null)
        {
            lastPlayerPosition = player.position;
            targetPosition = player.position + offset;
        }
        
        Debug.Log("🛡️ Cámara reseteada a valores seguros");
    }
    
    #endregion
    
    #region Debug
    
    void ShowDebugInfo()
    {
        if (!showDebugInfo) return;
        
        Debug.Log($"📹 Cámara - Modo: {currentMode} | Distancia: {distanciaActual:F1} | Colisión: {isColliding} | Velocidad Player: {playerVelocity.magnitude:F1}");
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugInfo || player == null) return;
        
        // Mostrar línea de visión
        Gizmos.color = isColliding ? Color.red : Color.green;
        Vector3 pivot = player.position + Vector3.up * lookAtHeight;
        Gizmos.DrawLine(pivot, transform.position);
        
        // Mostrar pivot
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pivot, 0.2f);
        
        // Mostrar rango de zoom
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pivot, minZoom);
        Gizmos.DrawWireSphere(pivot, maxZoom);
    }
    
    #endregion
}
