using System.Collections;
using UnityEngine;

/// <summary>
/// 🤖 Controlador básico de IA para jugadores automáticos
/// Proporciona comportamiento automático cuando no hay suficientes jugadores reales
/// </summary>
public class AIPlayerController : MonoBehaviour
{
    [Header("🤖 AI Settings")]
    public float moveSpeed = 8f;                  // Velocidad de movimiento de la IA
    public float reactionTime = 0.2f;             // Tiempo de reacción a obstáculos
    public float jumpChance = 0.7f;               // Probabilidad de saltar al encontrar obstáculos
    public float randomMovementInterval = 2f;     // Intervalo para cambiar dirección aleatoriamente
    
    [Header("🎯 Pathfinding")]
    public float targetSearchRadius = 15f;        // Radio para buscar objetivos
    public LayerMask obstacleLayerMask = -1;      // Capas de obstáculos
    public float avoidanceDistance = 3f;          // Distancia para evitar obstáculos
    
    [Header("🏃‍♂️ Behavior")]
    public bool followWaypoints = true;           // Seguir waypoints si existen
    public bool avoidOtherPlayers = true;         // Evitar otros jugadores
    public float playerAvoidanceDistance = 2f;    // Distancia para evitar otros jugadores
    
    // Referencias
    private LHS_MainPlayer playerController;
    private Rigidbody rb;
    private Transform target;
    private Vector3 currentDirection;
    private bool isActive = true;
    
    // Variables de estado
    private float lastDirectionChangeTime;
    private float lastJumpTime;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private const float maxStuckTime = 3f;
    
    // Waypoints y objetivos
    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private GameObject[] goalObjects;
    
    #region Unity Lifecycle
    
    void Start()
    {
        Initialize();
    }
    
    void Update()
    {
        if (!isActive) return;
        
        UpdateAI();
    }
    
    #endregion
    
    #region Initialization
    
    void Initialize()
    {
        // Obtener referencias
        playerController = GetComponent<LHS_MainPlayer>();
        rb = GetComponent<Rigidbody>();
        
        // Configurar como IA
        gameObject.tag = "IA";
        
        // Desactivar control manual
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Configurar dirección inicial aleatoria
        currentDirection = GetRandomDirection();
        lastDirectionChangeTime = Time.time;
        lastPosition = transform.position;
        
        // Buscar waypoints y objetivos
        FindWaypoints();
        FindGoalObjects();
        
        
    }
    
    void FindWaypoints()
    {
        // Buscar waypoints en la escena
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");
        
        if (waypointObjects.Length > 0)
        {
            waypoints = new Transform[waypointObjects.Length];
            for (int i = 0; i < waypointObjects.Length; i++)
            {
                waypoints[i] = waypointObjects[i].transform;
            }
            
            // Encontrar waypoint más cercano como inicio
            currentWaypointIndex = GetClosestWaypointIndex();
            
        }
    }
    
    void FindGoalObjects()
    {
        // Buscar objetos objetivo (meta, finish line, etc.)
        goalObjects = GameObject.FindGameObjectsWithTag("Goal");
        
        if (goalObjects.Length == 0)
        {
            goalObjects = GameObject.FindGameObjectsWithTag("Finish");
        }
        
        if (goalObjects.Length > 0)
        {
            
        }
    }
    
    #endregion
    
    #region AI Behavior
    
    void UpdateAI()
    {
        // Verificar si está atascado
        CheckIfStuck();
        
        // Determinar objetivo
        Vector3 targetPosition = DetermineTarget();
        
        // Calcular dirección de movimiento
        Vector3 moveDirection = CalculateMoveDirection(targetPosition);
        
        // Evitar obstáculos
        moveDirection = AvoidObstacles(moveDirection);
        
        // Evitar otros jugadores
        if (avoidOtherPlayers)
        {
            moveDirection = AvoidOtherPlayers(moveDirection);
        }
        
        // Aplicar movimiento
        ApplyMovement(moveDirection);
        
        // Decidir sobre saltar
        HandleJumping();
        
        // Cambiar dirección ocasionalmente
        if (Time.time - lastDirectionChangeTime > randomMovementInterval)
        {
            RandomizeDirection();
        }
    }
    
    Vector3 DetermineTarget()
    {
        // Prioridad 1: Objetivo principal (meta)
        if (goalObjects != null && goalObjects.Length > 0)
        {
            Transform closestGoal = GetClosestTransform(goalObjects);
            if (closestGoal != null)
            {
                return closestGoal.position;
            }
        }
        
        // Prioridad 2: Siguiente waypoint
        if (followWaypoints && waypoints != null && waypoints.Length > 0)
        {
            Transform currentWaypoint = waypoints[currentWaypointIndex];
            
            // Si está cerca del waypoint actual, ir al siguiente
            if (Vector3.Distance(transform.position, currentWaypoint.position) < 3f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            
            return waypoints[currentWaypointIndex].position;
        }
        
        // Prioridad 3: Movimiento aleatorio hacia adelante
        return transform.position + currentDirection * 10f;
    }
    
    Vector3 CalculateMoveDirection(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Solo movimiento horizontal
        
        return direction;
    }
    
    Vector3 AvoidObstacles(Vector3 originalDirection)
    {
        // Raycast hacia adelante para detectar obstáculos
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        RaycastHit hit;
        
        if (Physics.Raycast(rayStart, originalDirection, out hit, avoidanceDistance, obstacleLayerMask))
        {
            // Hay un obstáculo, calcular dirección de evasión
            Vector3 avoidDirection = Vector3.Cross(hit.normal, Vector3.up).normalized;
            
            // Elegir la dirección que más se aleje del obstáculo
            if (Random.value > 0.5f)
            {
                avoidDirection = -avoidDirection;
            }
            
            return avoidDirection;
        }
        
        return originalDirection;
    }
    
    Vector3 AvoidOtherPlayers(Vector3 originalDirection)
    {
        // Buscar otros jugadores cercanos
        Collider[] nearbyPlayers = Physics.OverlapSphere(transform.position, playerAvoidanceDistance);
        
        Vector3 avoidanceVector = Vector3.zero;
        int playerCount = 0;
        
        foreach (Collider col in nearbyPlayers)
        {
            if (col.gameObject != gameObject && 
                (col.CompareTag("Player") || col.CompareTag("IA")))
            {
                Vector3 awayFromPlayer = transform.position - col.transform.position;
                awayFromPlayer.y = 0;
                avoidanceVector += awayFromPlayer.normalized;
                playerCount++;
            }
        }
        
        if (playerCount > 0)
        {
            avoidanceVector = avoidanceVector.normalized;
            return Vector3.Lerp(originalDirection, avoidanceVector, 0.7f).normalized;
        }
        
        return originalDirection;
    }
    
    void ApplyMovement(Vector3 moveDirection)
    {
        if (rb != null)
        {
            // Aplicar movimiento manteniendo la velocidad vertical
            Vector3 velocity = moveDirection * moveSpeed;
            velocity.y = rb.velocity.y;
            
            rb.velocity = velocity;
            
            // Rotar hacia la dirección de movimiento
            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
    }
    
    void HandleJumping()
    {
        // Solo saltar si ha pasado suficiente tiempo
        if (Time.time - lastJumpTime < 1f) return;
        
        // Verificar si debe saltar (obstáculo adelante o decisión aleatoria)
        bool shouldJump = false;
        
        // Raycast hacia adelante y arriba para detectar obstáculos que requieren salto
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        Vector3 forwardDirection = transform.forward;
        
        RaycastHit hit;
        if (Physics.Raycast(rayStart, forwardDirection, out hit, 2f))
        {
            // Si hay un obstáculo bajo que se puede saltar
            if (hit.collider.bounds.size.y < 2f)
            {
                shouldJump = Random.value < jumpChance;
            }
        }
        
        // Salto aleatorio ocasional
        if (!shouldJump && Random.value < 0.01f) // 1% de probabilidad por frame
        {
            shouldJump = true;
        }
        
        if (shouldJump)
        {
            PerformJump();
        }
    }
    
    void PerformJump()
    {
        if (rb != null && IsGrounded())
        {
            float jumpForce = playerController != null ? playerController.jumpPower : 5f;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time;
            
            
        }
    }
    
    void RandomizeDirection()
    {
        currentDirection = GetRandomDirection();
        lastDirectionChangeTime = Time.time;
    }
    
    void CheckIfStuck()
    {
        // Verificar si no se ha movido mucho
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        
        if (distanceMoved < 0.5f) // Se considera "atascado" si se mueve menos de 0.5 unidades
        {
            stuckTimer += Time.deltaTime;
            
            if (stuckTimer > maxStuckTime)
            {
                // Está atascado, cambiar dirección drásticamente
                currentDirection = GetRandomDirection();
                lastDirectionChangeTime = Time.time;
                stuckTimer = 0f;
                
                // Intentar saltar para desatascarse
                if (Random.value < 0.8f)
                {
                    PerformJump();
                }
                
                
            }
        }
        else
        {
            stuckTimer = 0f;
        }
        
        lastPosition = transform.position;
    }
    
    #endregion
    
    #region Helper Methods
    
    Vector3 GetRandomDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        return new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad));
    }
    
    bool IsGrounded()
    {
        // Verificar si está en el suelo
        return Physics.Raycast(transform.position, Vector3.down, 1.5f);
    }
    
    Transform GetClosestTransform(GameObject[] objects)
    {
        Transform closest = null;
        float minDistance = float.MaxValue;
        
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                float distance = Vector3.Distance(transform.position, obj.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = obj.transform;
                }
            }
        }
        
        return closest;
    }
    
    int GetClosestWaypointIndex()
    {
        if (waypoints == null || waypoints.Length == 0) return 0;
        
        int closestIndex = 0;
        float minDistance = float.MaxValue;
        
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                float distance = Vector3.Distance(transform.position, waypoints[i].position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }
        }
        
        return closestIndex;
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// 🎯 Establecer objetivo específico para la IA
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    /// <summary>
    /// ⚙️ Configurar velocidad de la IA
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    /// <summary>
    /// 🤖 Activar/desactivar IA
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (!active && rb != null)
        {
            rb.velocity = Vector3.zero;
        }
    }
    
    /// <summary>
    /// 📊 Obtener información de la IA
    /// </summary>
    public string GetAIInfo()
    {
        return $"AI: {gameObject.name}\n" +
               $"Active: {isActive}\n" +
               $"Speed: {moveSpeed}\n" +
               $"Target: {(target != null ? target.name : "None")}\n" +
               $"Waypoint: {currentWaypointIndex}/{(waypoints?.Length ?? 0)}";
    }
    
    #endregion
    
    #region Debug
    
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // Mostrar dirección actual
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, currentDirection * 2f);
        
        // Mostrar rango de detección de obstáculos
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, avoidanceDistance);
        
        // Mostrar rango de evitar jugadores
        if (avoidOtherPlayers)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, playerAvoidanceDistance);
        }
        
        // Mostrar objetivo actual
        Vector3 targetPos = DetermineTarget();
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, targetPos);
        Gizmos.DrawWireSphere(targetPos, 1f);
    }
    
    #endregion
} 
