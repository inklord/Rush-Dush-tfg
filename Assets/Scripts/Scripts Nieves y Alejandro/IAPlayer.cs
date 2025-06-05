using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAPlayerSimple : MonoBehaviour
{
    [Header("Configuración de IA")]
    public float moveSpeed = 3f;
    public float detectionRadius = 8f;
    public float safeDistance = 3f;
    public float wanderRadius = 5f;
    public float updateFrequency = 0.5f;
    
    [Header("Configuración de Errores")]
    [Range(0f, 1f)]
    public float errorProbability = 0.15f;
    public float confusionTime = 2f;
    
    [Header("Configuración de Caída Rápida")]
    public float fastFallMultiplier = 3f;   // Multiplicador de gravedad para caída rápida
    public float fastFallForce = 15f;       // Fuerza adicional hacia abajo
    public float hexagonCheckRadius = 4f;   // Radio para buscar hexágonos seguros
    
    [Header("Configuración de Físicas")]
    public float groundCheckDistance = 1.5f;
    public LayerMask groundLayerMask = -1;       
    public float maxSlopeAngle = 45f;
    public float groundStickForce = 10f;
    
    [Header("Configuración de Límites")]
    public float mapBoundaryCheck = 2f;     // Distancia para verificar límites del mapa
    public float edgeAvoidanceForce = 5f;   // Fuerza para evitar bordes
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    public bool enableDebugLogs = true;
    
    // Componentes
    private Rigidbody rb;
    private Collider col;
    
    // Estado de la IA
    private Vector3 targetPosition;
    private Vector3 wanderTarget;
    private bool isConfused = false;
    private bool isInDanger = false;
    private bool isGrounded = true;
    private bool isFastFalling = false;     // Nuevo: estado de caída rápida
    private bool shouldStickToGround = true; // Nuevo: controlar adherencia al suelo
    private float lastUpdateTime;
    private float lastErrorTime;
    private float lastWanderTime;
    private Vector3 lastValidPosition;
    
    // Listas para tracking
    private List<DestroyPlayer> dangerousHexagons = new List<DestroyPlayer>();
    
    // Estados de comportamiento
    private enum IAState
    {
        Exploring,
        Fleeing,
        Confused
    }
    
    private IAState currentState = IAState.Exploring;

    private void Start()
    {
        InitializeComponents();
        
        // Configurar tag para que funcione con el sistema de hexágonos
        if (!gameObject.CompareTag("IA"))
        {
            gameObject.tag = "IA";
        }
        
        // Inicializar posición objetivo
        targetPosition = transform.position;
        wanderTarget = transform.position;
        lastValidPosition = transform.position;
        
        StartCoroutine(IAUpdateLoop());
        
        if (enableDebugLogs)
            Debug.Log($"IA Simple {gameObject.name} iniciada en posición {transform.position}");
    }

    private void InitializeComponents()
    {
        // Obtener componentes
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        
        // Configurar Rigidbody para mejor control pero menos restrictivo
        if (rb != null)
        {
            rb.freezeRotation = true;      // Evitar rotaciones indeseadas
            rb.drag = 3f;                  // Reducido de 5f a 3f - menos resistencia
            rb.angularDrag = 8f;           // Reducido de 10f a 8f
            rb.mass = 1f;                  // Masa estándar
            rb.useGravity = true;          // ✅ SIEMPRE activar gravedad para caída rápida
            rb.interpolation = RigidbodyInterpolation.Interpolate; // Movimiento suave
            
            if (enableDebugLogs)
                Debug.Log($"IA {gameObject.name}: Rigidbody configurado - Mass: {rb.mass}, Drag: {rb.drag}, Gravity: {rb.useGravity}");
        }
        else
        {
            Debug.LogError($"IA {gameObject.name}: ¡NO TIENE RIGIDBODY! Añadiendo uno...");
            rb = gameObject.AddComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.drag = 3f;
            rb.angularDrag = 8f;
            rb.mass = 1f;
            rb.useGravity = true;  // ✅ SIEMPRE activar gravedad
        }
        
        // Asegurar que el collider no sea trigger
        if (col != null && col.isTrigger)
        {
            col.isTrigger = false;
            if (enableDebugLogs)
                Debug.Log($"IA {gameObject.name}: Collider configurado como sólido para activar hexágonos");
        }
        else if (col == null)
        {
            Debug.LogWarning($"IA {gameObject.name}: ¡NO TIENE COLLIDER! Añadiendo CapsuleCollider...");
            col = gameObject.AddComponent<CapsuleCollider>();
        }
    }

    private void Update()
    {
        CheckGroundStatus();
        
        // ✅ VERIFICAR SI CAYÓ DEMASIADO BAJO (ELIMINACIÓN)
        if (transform.position.y < -30f && GameManager.Instance != null)
        {
            // Notificar al GameManager que este jugador debe ser eliminado
            GameManager.Instance.ForceEliminatePlayer(gameObject);
            return; // No continuar con el update si está siendo eliminado
        }
        
        // ✅ SIEMPRE APLICAR CAÍDA RÁPIDA cuando no esté en el suelo
        if (!isGrounded && rb != null)
        {
            ApplyFastFall();
            isFastFalling = true;
            shouldStickToGround = false;
        }
        else if (isGrounded)
        {
            isFastFalling = false;
            shouldStickToGround = true;
            StickToGround();
        }
        
        // Mover hacia el objetivo
        MoveTowardsTarget();
        
        // Verificar límites del mapa (menos restrictivo)
        CheckMapBoundaries();
        
        // ✅ DEBUG: Información de movimiento cada 2 segundos
        if (enableDebugLogs && Time.time % 2f < Time.deltaTime)
        {
            string debugInfo = $"IA {gameObject.name}: ";
            debugInfo += $"Grounded={isGrounded}, ";
            debugInfo += $"FastFall={isFastFalling}, ";
            debugInfo += $"Y={transform.position.y:F1}, ";
            debugInfo += $"Target={targetPosition}, ";
            debugInfo += $"Distance={Vector3.Distance(transform.position, targetPosition):F1}m, ";
            debugInfo += $"RB_Vel={rb?.velocity.magnitude:F2}, ";
            debugInfo += $"State={currentState}";
            
            Debug.Log(debugInfo);
        }
        
        // ✅ FORZAR MOVIMIENTO si está completamente parado
        if (rb != null && rb.velocity.magnitude < 0.1f && Time.time - lastWanderTime > 5f && isGrounded)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"IA {gameObject.name}: ¡Forzando nuevo movimiento - IA parada!");
            
            ForceNewMovement();
        }
        
        // ✅ TEST: Forzar caída rápida con tecla R (ahora solo para debug)
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log($"IA {gameObject.name}: Caída rápida está SIEMPRE activa. Estado actual: {isFastFalling}");
        }
        
        // ✅ TEST: Debug de sistema de caída rápida con tecla T
        if (Input.GetKeyDown(KeyCode.T))
        {
            bool onDestroying = IsOnDestroyingHexagon();
            bool hasSafe = HasSafeHexagonsNearby();
            
            Debug.Log($"IA {gameObject.name}: DEBUG CAÍDA RÁPIDA (SIEMPRE ACTIVA)");
            Debug.Log($"- En hexágono destruyéndose: {onDestroying}");
            Debug.Log($"- Hay hexágonos seguros cerca: {hasSafe}");
            Debug.Log($"- Estado caída rápida: {isFastFalling}");
            Debug.Log($"- En el suelo: {isGrounded}");
            Debug.Log($"- Altura Y: {transform.position.y}");
        }
        
        // ✅ TEST: Forzar eliminación con tecla E
        if (Input.GetKeyDown(KeyCode.E) && GameManager.Instance != null)
        {
            Debug.Log($"IA {gameObject.name}: Forzando eliminación manual");
            GameManager.Instance.ForceEliminatePlayer(gameObject);
        }
    }
    
    private void CheckGroundStatus()
    {
        // Verificar si está tocando el suelo con múltiples raycast
        RaycastHit hit;
        Vector3[] checkPositions = {
            transform.position,
            transform.position + Vector3.forward * 0.3f,
            transform.position + Vector3.back * 0.3f,
            transform.position + Vector3.left * 0.3f,
            transform.position + Vector3.right * 0.3f
        };
        
        bool wasGrounded = isGrounded;
        isGrounded = false;
        
        foreach (Vector3 checkPos in checkPositions)
        {
            if (Physics.Raycast(checkPos, Vector3.down, out hit, groundCheckDistance, groundLayerMask))
            {
                // Verificar que la superficie no sea demasiado inclinada
                float angle = Vector3.Angle(hit.normal, Vector3.up);
                if (angle <= maxSlopeAngle)
                {
                    isGrounded = true;
                    break;
                }
            }
        }
        
        // Si perdió contacto con el suelo, intentar volver a la última posición válida
        if (wasGrounded && !isGrounded)
                    {
                        if (enableDebugLogs)
                Debug.LogWarning($"IA {gameObject.name}: Perdió contacto con el suelo, regresando a posición segura");
            
            StartCoroutine(ReturnToSafePosition());
        }
        else if (isGrounded)
        {
            // Actualizar última posición válida solo si está en el suelo
            lastValidPosition = transform.position;
        }
    }
    
    private void StickToGround()
    {
        // No adherirse al suelo si debe caer rápido
        if (!isGrounded || rb == null || !shouldStickToGround) return;
        
        // Aplicar fuerza hacia abajo para mantener contacto con el suelo
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance + 0.5f, groundLayerMask))
        {
            float distanceToGround = hit.distance;
            if (distanceToGround > 0.1f) // Si está un poco elevado
            {
                // Aplicar fuerza hacia abajo suavemente
                Vector3 stickForce = Vector3.down * groundStickForce * (distanceToGround * 2f);
                rb.AddForce(stickForce, ForceMode.Force);
            }
        }
    }
    
    private void CheckMapBoundaries()
    {
        if (!isGrounded) return;
        
        // Verificar si está cerca del borde del mapa
        Vector3 currentPos = transform.position;
        
        // Verificar en las 4 direcciones principales
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        
        foreach (Vector3 direction in directions)
        {
            RaycastHit hit;
            Vector3 checkPosition = currentPos + direction * mapBoundaryCheck;
            
            // Raycast hacia abajo desde la posición de verificación
            if (!Physics.Raycast(checkPosition + Vector3.up * 2f, Vector3.down, out hit, 5f, groundLayerMask))
            {
                // No hay suelo en esa dirección - está cerca del borde
                if (enableDebugLogs)
                    Debug.LogWarning($"IA {gameObject.name}: Borde detectado en dirección {direction}");
                
                // Aplicar fuerza en dirección opuesta
                Vector3 avoidanceForce = -direction * edgeAvoidanceForce;
        if (rb != null)
        {
                    rb.AddForce(avoidanceForce, ForceMode.Force);
                }
                
                // Cambiar objetivo para alejarse del borde
                Vector3 safeDirection = -direction * wanderRadius;
                targetPosition = currentPos + safeDirection;
                wanderTarget = targetPosition;
                
                break; // Solo procesar una dirección por frame
            }
        }
    }

    private void MoveTowardsTarget()
    {
        // Remover la restricción de solo moverse cuando está grounded - ahora puede intentar moverse
        if (rb == null) return;
        
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        
        if (distanceToTarget > 0.5f)
        {
            // Calcular dirección solo en el plano horizontal
            Vector3 direction = (targetPosition - transform.position);
            direction.y = 0;
            direction = direction.normalized;
            
            // Verificar dirección pero ser menos restrictivo
            if (!IsDirectionSafe(direction))
            {
                // Buscar dirección alternativa pero no cancelar el movimiento
                Vector3 safeDir = FindSafeDirection();
                if (safeDir != Vector3.zero)
                {
                    direction = safeDir;
                    if (enableDebugLogs)
                        Debug.Log($"IA {gameObject.name}: Usando dirección alternativa segura");
                }
                else
                {
                    // Seguir con la dirección original pero con menos fuerza
                    direction = direction * 0.5f;
                    if (enableDebugLogs)
                        Debug.Log($"IA {gameObject.name}: Dirección no completamente segura, reduciendo fuerza");
                }
            }
            
            if (direction.magnitude > 0.1f)
            {
                // Velocidad ajustada según el estado
                float currentSpeed = isConfused ? moveSpeed * 0.5f : moveSpeed;
                if (currentState == IAState.Fleeing) currentSpeed *= 1.5f;
                
                // Mover usando AddForce para mejor control físico
                Vector3 moveForce = direction * currentSpeed * 15f; // Aumentado de 10f a 15f para más fuerza
                rb.AddForce(moveForce, ForceMode.Force);
                
                // Limitar velocidad máxima de forma más permisiva
                Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                float maxSpeed = currentSpeed * 1.2f; // 20% más permisivo
                
                if (horizontalVelocity.magnitude > maxSpeed)
                {
                    Vector3 limitedVelocity = horizontalVelocity.normalized * maxSpeed;
                    rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
                }
                
                // Rotación suave hacia la dirección de movimiento
                RotateTowardsDirection(direction);
                
                if (enableDebugLogs && Time.time % 1f < Time.deltaTime)
                {
                    Debug.Log($"IA {gameObject.name}: Moviéndose hacia {targetPosition} con fuerza {moveForce.magnitude:F1}");
                }
            }
        }
    }
    
    private bool IsDirectionSafe(Vector3 direction)
    {
        // Verificación más permisiva
        Vector3 checkPosition = transform.position + direction * 1.5f; // Reducido de 2f a 1.5f
        
        // Solo verificar que haya suelo básico
        RaycastHit hit;
        return Physics.Raycast(checkPosition + Vector3.up * 1f, Vector3.down, out hit, 6f); // Aumentado de 3f a 6f
    }
    
    private Vector3 FindSafeDirection()
    {
        // Encontrar una dirección segura para moverse
        Vector3[] testDirections = {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            (Vector3.forward + Vector3.right).normalized,
            (Vector3.forward + Vector3.left).normalized,
            (Vector3.back + Vector3.right).normalized,
            (Vector3.back + Vector3.left).normalized
        };
        
        foreach (Vector3 dir in testDirections)
        {
            if (IsDirectionSafe(dir))
            {
                return dir;
            }
        }
        
        // Si no encuentra dirección segura, quedarse en su lugar
        return Vector3.zero;
    }
    
    private void RotateTowardsDirection(Vector3 direction)
    {
        if (direction.magnitude < 0.1f) return;
        
        // Rotación suave solo en el eje Y
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        
        // Mantener la rotación X y Z en 0 para evitar inclinaciones
        Vector3 eulerAngles = targetRotation.eulerAngles;
        eulerAngles.x = 0;
        eulerAngles.z = 0;
        targetRotation = Quaternion.Euler(eulerAngles);
        
        // Aplicar rotación suave
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
    }
    
    private IEnumerator ReturnToSafePosition()
    {
        // Pausar el movimiento normal
        yield return new WaitForSeconds(0.1f);
        
        if (!isGrounded && lastValidPosition != Vector3.zero)
        {
            if (enableDebugLogs)
                Debug.Log($"IA {gameObject.name}: Regresando a posición segura: {lastValidPosition}");
            
            // Mover gradualmente hacia la última posición válida
            float returnSpeed = moveSpeed * 2f;
            while (Vector3.Distance(transform.position, lastValidPosition) > 0.5f && !isGrounded)
            {
                Vector3 direction = (lastValidPosition - transform.position).normalized;
                
                if (rb != null)
                {
                    rb.velocity = direction * returnSpeed;
                }
                
                yield return new WaitForFixedUpdate();
            }
            
            // Asegurar posición final
            if (!isGrounded)
            {
                transform.position = lastValidPosition;
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                }
            }
        }
    }

    private IEnumerator IAUpdateLoop()
    {
        while (gameObject != null)
        {
            if (!isConfused && Time.time - lastUpdateTime > updateFrequency)
            {
                UpdateIADecision();
                lastUpdateTime = Time.time;
            }
            
            yield return new WaitForSeconds(updateFrequency);
        }
    }

    private void UpdateIADecision()
    {
        // Solo tomar decisiones si está en el suelo
        if (!isGrounded) return;
        
        // Detectar hexágonos peligrosos
        DetectDangerousHexagons();
        
        // Decidir próxima acción basada en el estado
        switch (currentState)
        {
            case IAState.Exploring:
                HandleExploringState();
                break;
            case IAState.Fleeing:
                HandleFleeingState();
                break;
            case IAState.Confused:
                HandleConfusedState();
                break;
        }
        
        // Posibilidad de cometer errores
        if (ShouldMakeError())
        {
            MakeRandomError();
        }
    }

    private void DetectDangerousHexagons()
    {
        dangerousHexagons.Clear();
        
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        
        foreach (Collider col in nearbyColliders)
        {
            DestroyPlayer hexagon = col.GetComponent<DestroyPlayer>();
            if (hexagon != null)
            {
                // Considerar peligroso si está muy cerca Y tiene color peligroso
                bool isDangerous = Vector3.Distance(transform.position, col.transform.position) < safeDistance;
                
                // Verificar color del hexágono para mayor precisión
                if (isDangerous)
                {
                    Renderer hexRenderer = col.GetComponent<Renderer>();
                    if (hexRenderer != null && hexRenderer.material != null)
                    {
                        Color currentColor = hexRenderer.material.color;
                        
                        // Detectar colores peligrosos (rojo, amarillo)
                        bool isRedOrYellow = (currentColor.r > 0.6f && currentColor.g < 0.5f) || 
                                           (currentColor.r > 0.6f && currentColor.g > 0.5f && currentColor.b < 0.5f);
                        
                        if (isRedOrYellow)
                        {
                            dangerousHexagons.Add(hexagon);
                        }
                    }
                    else
                    {
                        // Si no puede verificar color, asumir peligroso por distancia
                        dangerousHexagons.Add(hexagon);
                    }
                }
            }
        }
        
        // Actualizar estado de peligro
        bool wasInDanger = isInDanger;
        isInDanger = dangerousHexagons.Count > 0;
        
        if (!wasInDanger && isInDanger)
        {
            currentState = IAState.Fleeing;
            if (enableDebugLogs)
                Debug.Log($"IA Simple {gameObject.name} entrando en modo huida!");
        }
    }

    private void HandleExploringState()
    {
        if (isInDanger)
        {
            currentState = IAState.Fleeing;
            return;
        }
        
        // Movimiento de wandering mejorado
        HandleWandering();
    }

    private void HandleWandering()
    {
        // Cambiar objetivo de wandering cada cierto tiempo
        if (Time.time - lastWanderTime > 3f || Vector3.Distance(transform.position, wanderTarget) < 1f)
        {
            Vector3 newTarget = FindSafeWanderPosition();
            
            if (newTarget != Vector3.zero)
            {
                wanderTarget = newTarget;
                targetPosition = wanderTarget;
                lastWanderTime = Time.time;
                
                if (enableDebugLogs)
                    Debug.Log($"IA Simple {gameObject.name} nuevo objetivo: {targetPosition}");
            }
        }
    }
    
    private Vector3 FindSafeWanderPosition()
    {
        // Intentar varias posiciones aleatorias - versión simplificada
        for (int attempts = 0; attempts < 12; attempts++) // Más intentos
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection.y = 0;
            Vector3 candidatePosition = transform.position + randomDirection;
            
            // Primera verificación: posición segura completa
            if (IsPositionValidAndSafe(candidatePosition))
            {
                return candidatePosition;
            }
            
            // Segunda verificación: solo verificar que haya suelo (más permisivo)
            if (attempts > 6 && IsPositionBasicallySafe(candidatePosition))
            {
                if (enableDebugLogs)
                    Debug.Log($"IA {gameObject.name}: Usando posición básicamente segura en intento {attempts}");
                return candidatePosition;
            }
        }
        
        // Si no encuentra nada válido, usar posiciones simples en las 4 direcciones
        Vector3[] fallbackDirections = {
            transform.position + Vector3.forward * 2f,
            transform.position + Vector3.back * 2f,
            transform.position + Vector3.left * 2f,
            transform.position + Vector3.right * 2f
        };
        
        foreach (Vector3 fallbackPos in fallbackDirections)
        {
            if (IsPositionBasicallySafe(fallbackPos))
            {
                if (enableDebugLogs)
                    Debug.Log($"IA {gameObject.name}: Usando posición de fallback");
                return fallbackPos;
            }
        }
        
        // Último recurso: mover un poco en cualquier dirección
        Vector3 emergencyPos = transform.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        
        if (enableDebugLogs)
            Debug.LogWarning($"IA {gameObject.name}: Usando posición de emergencia - puede no ser segura");
        
        return emergencyPos;
    }
    
    private float CalculatePositionSafety(Vector3 position)
    {
        // Calcular qué tan segura es una posición
        float safety = 10f; // Puntuación base
        
        // Penalizar por proximidad a hexágonos peligrosos
        Collider[] nearbyDangers = Physics.OverlapSphere(position, safeDistance * 2f);
        foreach (Collider col in nearbyDangers)
        {
            if (col.GetComponent<DestroyPlayer>() != null)
            {
                float distance = Vector3.Distance(position, col.transform.position);
                safety -= (safeDistance * 2f - distance); // Más cerca = menos seguro
            }
        }
        
        // Bonificar por distancia al borde del mapa
        if (IsPositionValidAndSafe(position))
        {
            safety += 2f;
        }
        
        return safety;
    }

    private void HandleFleeingState()
    {
        if (!isInDanger)
        {
            currentState = IAState.Exploring;
            return;
        }
        
        // Encontrar la posición más segura para huir
        Vector3 fleePosition = FindSafeFleePosition();
        if (fleePosition != Vector3.zero)
        {
            targetPosition = fleePosition;
        }
    }
    
    private Vector3 FindSafeFleePosition()
    {
        Vector3 fleeDirection = Vector3.zero;
        
        // Calcular dirección opuesta a todos los hexágonos peligrosos
        foreach (DestroyPlayer dangerous in dangerousHexagons)
        {
            if (dangerous != null)
            {
                Vector3 directionAway = transform.position - dangerous.transform.position;
                directionAway.y = 0;
                fleeDirection += directionAway.normalized;
            }
        }
        
        if (fleeDirection.magnitude > 0.1f)
        {
            fleeDirection = fleeDirection.normalized;
            
            // Buscar múltiples distancias para encontrar la mejor
            float[] testDistances = { wanderRadius * 0.5f, wanderRadius, wanderRadius * 1.5f };
            
            foreach (float distance in testDistances)
            {
                Vector3 fleePosition = transform.position + fleeDirection * distance;
                
                if (IsPositionValidAndSafe(fleePosition))
                {
                    return fleePosition;
                }
            }
        }
        
        // Si no puede huir en la dirección calculada, buscar cualquier posición segura
        return FindSafeWanderPosition();
    }

    private void HandleConfusedState()
    {
        // Durante la confusión, la IA se mueve aleatoriamente pero de forma más controlada
        if (Random.value > 0.7f) // Menos frecuente
        {
            Vector3 randomPos = FindSafeWanderPosition();
            
            if (randomPos != Vector3.zero)
            {
                targetPosition = randomPos;
            }
        }
    }

    private Vector3 GetFleePosition()
    {
        return FindSafeFleePosition();
    }

    private bool IsPositionValid(Vector3 position)
    {
        return IsPositionValidAndSafe(position);
    }
    
    private bool IsPositionValidAndSafe(Vector3 position)
    {
        // Verificación MÁS PERMISIVA para que la IA se pueda mover
        
        // 1. Verificar que haya suelo básico
        RaycastHit hit;
        if (!Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out hit, 8f)) // Aumentado de 5f a 8f
        {
            return false; // No hay suelo
        }
        
        // 2. Verificar pendiente (más permisivo)
        float angle = Vector3.Angle(hit.normal, Vector3.up);
        if (angle > 60f) // Aumentado de 45f a 60f - más permisivo
        {
            return false; // Demasiado inclinado
        }
        
        // 3. Verificación de bordes más permisiva - solo verificar si está MUY cerca del borde
        Vector3[] edgeCheckDirections = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        int edgesWithoutGround = 0;
        
        foreach (Vector3 direction in edgeCheckDirections)
        {
            Vector3 edgeCheckPos = position + direction * (mapBoundaryCheck * 0.5f); // Reducido para ser menos restrictivo
            if (!Physics.Raycast(edgeCheckPos + Vector3.up * 2f, Vector3.down, out hit, 8f))
            {
                edgesWithoutGround++;
            }
        }
        
        // Solo rechazar si 3 o más direcciones no tienen suelo (muy cerca del borde)
        if (edgesWithoutGround >= 3)
        {
            return false; // Muy cerca del borde
        }
        
        return true; // Posición válida
    }
    
    // Versión aún más simple para cuando la IA está completamente atascada
    private bool IsPositionBasicallySafe(Vector3 position)
    {
        // Solo verificar que haya suelo - nada más
        RaycastHit hit;
        return Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out hit, 10f);
    }

    private bool ShouldMakeError()
    {
        return Random.value < errorProbability && Time.time - lastErrorTime > 3f && isGrounded;
    }

    private void MakeRandomError()
    {
        lastErrorTime = Time.time;
        
        // Tipos de errores que puede cometer la IA
        int errorType = Random.Range(0, 3);
        
        switch (errorType)
        {
            case 0: // Moverse hacia un hexágono peligroso (pero de forma segura)
                if (dangerousHexagons.Count > 0)
                {
                    DestroyPlayer target = dangerousHexagons[Random.Range(0, dangerousHexagons.Count)];
                    if (target != null)
                    {
                        // Ir hacia el hexágono pero no directamente encima
                        Vector3 dangerousDirection = (target.transform.position - transform.position).normalized;
                        Vector3 errorTarget = transform.position + dangerousDirection * (safeDistance * 0.8f);
                        
                        if (IsPositionValidAndSafe(errorTarget))
                        {
                            targetPosition = errorTarget;
                            if (enableDebugLogs)
                                Debug.Log($"IA Simple {gameObject.name} cometió error: acercándose a hexágono peligroso");
                        }
                    }
                }
                break;
                
            case 1: // Quedarse confundido por un tiempo
                StartCoroutine(BeConfused());
                break;
                
            case 2: // Movimiento en dirección aleatoria pero segura
                Vector3 randomTarget = FindSafeWanderPosition();
                if (randomTarget != Vector3.zero)
                {
                    targetPosition = randomTarget;
                    if (enableDebugLogs)
                        Debug.Log($"IA Simple {gameObject.name} cometió error: movimiento aleatorio");
                }
                break;
        }
    }

    private IEnumerator BeConfused()
    {
        isConfused = true;
        currentState = IAState.Confused;
        
        if (enableDebugLogs)
            Debug.Log($"IA Simple {gameObject.name} está confundida por {confusionTime}s");
        
        yield return new WaitForSeconds(confusionTime);
        
        isConfused = false;
        currentState = IAState.Exploring;
    }

    private void ForceNewMovement()
    {
        // Buscar una posición simple para moverse
        Vector3[] simpleDirections = {
            Vector3.forward * 3f,
            Vector3.back * 3f,
            Vector3.left * 3f,
            Vector3.right * 3f
        };
        
        foreach (Vector3 direction in simpleDirections)
        {
            Vector3 testPos = transform.position + direction;
            
            // Verificación simple: solo que haya suelo
            RaycastHit hit;
            if (Physics.Raycast(testPos + Vector3.up * 2f, Vector3.down, out hit, 5f))
            {
                targetPosition = testPos;
                wanderTarget = testPos;
                lastWanderTime = Time.time;
                
                if (enableDebugLogs)
                    Debug.Log($"IA {gameObject.name}: Movimiento forzado hacia {testPos}");
                return;
            }
        }
        
        // Si no encuentra nada, mover en cualquier dirección
        Vector3 randomDir = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
        targetPosition = transform.position + randomDir;
        lastWanderTime = Time.time;
        
        if (enableDebugLogs)
            Debug.Log($"IA {gameObject.name}: Movimiento de emergencia hacia {targetPosition}");
    }

    private void ApplyFastFall()
    {
        if (rb == null) return;
        
        // Aplicar fuerza adicional hacia abajo continuamente
        Vector3 extraGravity = Vector3.down * (fastFallMultiplier * Physics.gravity.magnitude);
        rb.AddForce(extraGravity, ForceMode.Acceleration);
        
        // Verificar si ha aterrizado en una nueva superficie
        if (isGrounded)
        {
        RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
            {
                // Si aterrizó en una superficie que no se está destruyendo, parar caída rápida
                if (!hit.collider.CompareTag("Hexagono") || !IsOnDestroyingHexagon())
                {
                    isFastFalling = false;
                    shouldStickToGround = true;
                }
            }
        }
    }

    private bool IsOnDestroyingHexagon()
    {
        // Verificar hexágono directamente debajo
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f, groundLayerMask))
        {
            // Verificar si es un hexágono
            if (hit.collider.CompareTag("Hexagono"))
            {
            DestroyPlayer hexagon = hit.collider.GetComponent<DestroyPlayer>();
            if (hexagon != null)
            {
                    // Verificar si es peligroso por color
                Renderer hexRenderer = hit.collider.GetComponent<Renderer>();
                if (hexRenderer != null && hexRenderer.material != null)
                {
                    Color currentColor = hexRenderer.material.color;
                    
                        // Detectar colores peligrosos (rojo = destruyéndose)
                    bool isRed = (currentColor.r > 0.7f && currentColor.g < 0.4f && currentColor.b < 0.4f);
                        
                        return isRed;
                    }
                }
            }
        }
        
        return false;
    }
    
    private bool HasSafeHexagonsNearby()
    {
        // Buscar hexágonos seguros en el radio especificado
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, hexagonCheckRadius);
        
        foreach (Collider col in nearbyColliders)
        {
            if (col.CompareTag("Hexagono"))
            {
                DestroyPlayer hexagon = col.GetComponent<DestroyPlayer>();
        if (hexagon != null)
        {
                    // Verificar si es seguro por color
                    Renderer hexRenderer = col.GetComponent<Renderer>();
                    if (hexRenderer != null && hexRenderer.material != null)
                    {
                        Color currentColor = hexRenderer.material.color;
                        
                        // Detectar colores seguros (verde, azul, blanco)
                        bool isGreen = (currentColor.g > 0.6f && currentColor.r < 0.5f && currentColor.b < 0.5f);
                        bool isBlue = (currentColor.b > 0.6f && currentColor.r < 0.5f && currentColor.g < 0.5f);
                        bool isWhite = (currentColor.r > 0.7f && currentColor.g > 0.7f && currentColor.b > 0.7f);
                        
                        if (isGreen || isBlue || isWhite)
                        {
                            // Verificar que esté a una distancia alcanzable
                            float distance = Vector3.Distance(transform.position, col.transform.position);
                            if (distance <= hexagonCheckRadius && distance >= 1f) // No muy cerca ni muy lejos
                            {
                                return true; // Hay al menos un hexágono seguro cerca
                            }
                        }
                    }
                }
            }
        }
        
        return false; // No hay hexágonos seguros cerca
    }

    // Gizmos para debug mejorados
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        // Radio de detección
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Radio de verificación de hexágonos para caída rápida
        Gizmos.color = isFastFalling ? Color.red : new Color(1f, 0.5f, 0f); // Color naranja
        Gizmos.DrawWireSphere(transform.position, hexagonCheckRadius);
        
        // Distancia segura
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, safeDistance);
        
        // Radio de wandering
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
        
        // Verificación de límites del mapa
            Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mapBoundaryCheck);
        
        // Raycast de verificación de suelo
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3[] checkPositions = {
            transform.position,
            transform.position + Vector3.forward * 0.3f,
            transform.position + Vector3.back * 0.3f,
            transform.position + Vector3.left * 0.3f,
            transform.position + Vector3.right * 0.3f
        };
        
        foreach (Vector3 pos in checkPositions)
        {
            Gizmos.DrawRay(pos, Vector3.down * groundCheckDistance);
        }
        
        // Raycast especial para detección de hexágono actual (caída rápida)
        if (isFastFalling)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Vector3.down * 2f);
        }
        
        // Hexágonos peligrosos
        Gizmos.color = Color.red;
        foreach (DestroyPlayer dangerous in dangerousHexagons)
        {
            if (dangerous != null)
            {
                Gizmos.DrawLine(transform.position, dangerous.transform.position);
                Gizmos.DrawWireSphere(dangerous.transform.position, 0.5f);
            }
        }
        
        // Objetivo actual
        if (targetPosition != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(targetPosition, Vector3.one);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
        
        // Objetivo de wandering
        if (wanderTarget != Vector3.zero && wanderTarget != targetPosition)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(wanderTarget, 0.5f);
        }
        
        // Última posición válida
        if (lastValidPosition != Vector3.zero)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(lastValidPosition, Vector3.one * 0.5f);
        }
        
        // Estado actual (color del objeto)
        switch (currentState)
        {
            case IAState.Exploring:
                Gizmos.color = isFastFalling ? Color.red : Color.white;
                break;
            case IAState.Fleeing:
                Gizmos.color = Color.red;
                break;
            case IAState.Confused:
                Gizmos.color = Color.magenta;
                break;
        }
        
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.3f);
        
        // Indicador de estado de suelo
        Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 3f, Vector3.one * 0.2f);
        
        // Indicador de caída rápida
        if (isFastFalling)
            {
                Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 4f, Vector3.one * 0.4f);
            
            // Flecha hacia abajo para mostrar dirección de caída
            Gizmos.DrawRay(transform.position + Vector3.up * 4f, Vector3.down * 2f);
        }
    }
} 