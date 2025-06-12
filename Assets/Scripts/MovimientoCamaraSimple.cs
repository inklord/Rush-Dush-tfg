using UnityEngine;
using Photon.Pun;
using System.Collections;

/// <summary>
/// 📷 Cámara simple estilo Fall Guys
/// La cámara sigue automáticamente al jugador
/// El JUGADOR controla su rotación con el ratón (no la cámara)
/// </summary>
public class MovimientoCamaraSimple : MonoBehaviour
{
    [Header("🎯 Target & Referencias")]
    public Transform player;
    
    [Header("📐 Posicionamiento")]
    public float distance = 8f; // Distancia de la cámara al jugador
    public float height = 5f; // Altura de la cámara sobre el jugador
    public float smoothSpeed = 8f; // Velocidad de seguimiento
    public float lookAtHeight = 1.5f; // Altura a la que mira la cámara en el jugador
    
    [Header("🎯 Seguimiento Automático")]
    public float autoFollowSpeed = 6f; // Velocidad con que sigue la dirección del jugador
    public float followOffset = 180f; // Offset angular detrás del jugador (180° = detrás)
    
    [Header("🔒 Límites de Distancia")]
    public float minDistance = 3f;
    public float maxDistance = 15f;
    public float zoomSpeed = 2f;
    
    [Header("💥 Camera Shake")]
    public bool enableShake = true;
    public float shakeIntensity = 1f;
    
    [Header("🔧 Debug")]
    public bool showDebugInfo = false;
    
    // Variables privadas
    private float currentYaw = 0f; // Rotación actual de la cámara
    private Vector3 currentVelocity;
    private bool isFollowingLocalPlayer = false;
    
    // Sistema de shake
    private Vector3 shakeOffset = Vector3.zero;
    private float shakeTimer = 0f;
    private float shakeDuration = 0f;
    
    void Start()
    {
        // Buscar jugador local si no está asignado
        if (player == null)
        {
            StartCoroutine(FindLocalPlayer());
        }
        else
        {
            InitializeCamera();
        }
    }
    
    IEnumerator FindLocalPlayer()
    {
        if (showDebugInfo) Debug.Log("🔍 Buscando jugador local...");
        
        // Intentar varias veces
        for (int i = 0; i < 20; i++)
        {
            // Buscar primero en modo singleplayer
            BasicPlayerMovement basicPlayer = FindObjectOfType<BasicPlayerMovement>();
            if (basicPlayer != null)
            {
                SetPlayer(basicPlayer.transform);
                if (showDebugInfo) Debug.Log("✅ Jugador singleplayer encontrado");
                break;
            }
            
            // Buscar en modo multiplayer
            LHS_MainPlayer[] allPlayers = FindObjectsOfType<LHS_MainPlayer>();
            foreach (LHS_MainPlayer playerObj in allPlayers)
            {
                PhotonView pv = playerObj.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    SetPlayer(playerObj.transform);
                    if (showDebugInfo) Debug.Log("✅ Jugador local multiplayer encontrado");
                    break;
                }
            }
            
            if (player != null) break;
            yield return new WaitForSeconds(0.5f);
        }
        
        if (player == null)
        {
            if (showDebugInfo) Debug.LogWarning("⚠️ No se pudo encontrar jugador local");
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Zoom con scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
        }
        
        // Reset con R
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCamera();
        }
        
        UpdateCameraPosition();
        UpdateShake();
    }
    
    void UpdateCameraPosition()
    {
        // Método más simple: calcular directamente la posición detrás del jugador
        Vector3 playerForward = player.transform.forward;
        Vector3 playerPosition = player.position;
        
        // Posición detrás del jugador: ir hacia atrás desde el jugador
        Vector3 targetPosition = playerPosition - (playerForward * distance) + (Vector3.up * height);
        
        // Suavizar movimiento de la cámara
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition + shakeOffset, ref currentVelocity, 1f / smoothSpeed);
        
        // Mirar hacia el jugador
        Vector3 lookTarget = playerPosition + Vector3.up * lookAtHeight;
        transform.LookAt(lookTarget);
        
        // Actualizar currentYaw para el debug (opcional)
        currentYaw = transform.eulerAngles.y;
    }
    
    void UpdateShake()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            
            float shakeAmount = shakeIntensity * (shakeTimer / shakeDuration);
            shakeOffset = Random.insideUnitSphere * shakeAmount;
            
            if (shakeTimer <= 0)
            {
                shakeOffset = Vector3.zero;
            }
        }
    }
    
    /// <summary>
    /// 🎯 Asignar jugador a seguir
    /// </summary>
    public void SetPlayer(Transform newPlayer)
    {
        if (isFollowingLocalPlayer && player != null)
        {
            Debug.LogWarning("⚠️ La cámara ya está siguiendo a un jugador");
            return;
        }
        
        // Verificar si es el jugador local (permitir singleplayer)
        PhotonView pv = newPlayer.GetComponent<PhotonView>();
        if (pv == null || pv.IsMine)
        {
            player = newPlayer;
            isFollowingLocalPlayer = true;
            InitializeCamera();
            if (showDebugInfo) Debug.Log($"📹 Cámara Fall Guys asignada a: {newPlayer.name}");
        }
        else
        {
            if (showDebugInfo) Debug.LogWarning("❌ No se puede asignar un jugador remoto");
        }
    }
    
    /// <summary>
    /// 🔧 Inicializar cámara cuando se asigna un jugador
    /// </summary>
    void InitializeCamera()
    {
        if (player != null)
        {
            // Inicializar ángulos basados en la rotación del jugador
            currentYaw = player.eulerAngles.y;
        }
    }
    
    /// <summary>
    /// 🔄 Resetear cámara
    /// </summary>
    public void ResetCamera()
    {
        if (player != null)
        {
            currentYaw = player.eulerAngles.y;
            distance = 8f;
            shakeOffset = Vector3.zero;
            shakeTimer = 0f;
            if (showDebugInfo) Debug.Log("🔄 Cámara Fall Guys reseteada");
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
    
    void OnGUI()
    {
        if (!showDebugInfo || !Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 150, 300, 200));
        GUILayout.Label("=== CÁMARA FALL GUYS ===");
        GUILayout.Label($"Jugador: {(player ? player.name : "NINGUNO")}");
        GUILayout.Label($"Cámara Yaw: {currentYaw:F1}°");
        GUILayout.Label($"Jugador Yaw: {(player ? player.eulerAngles.y.ToString("F1") : "N/A")}°");
        GUILayout.Label($"Distancia: {distance:F1}m");
        GUILayout.Label($"Siguiendo: {(isFollowingLocalPlayer ? "SÍ" : "NO")}");
        GUILayout.Label("Scroll = Zoom | R = Reset");
        
        if (player != null)
        {
            LHS_MainPlayer playerScript = player.GetComponent<LHS_MainPlayer>();
            if (playerScript != null)
            {
                GUILayout.Label("--- JUGADOR ---");
                GUILayout.Label($"Velocidad: {playerScript.CurrentSpeed:F1}");
                GUILayout.Label($"En suelo: {(playerScript.IsGrounded ? "SÍ" : "NO")}");
                GUILayout.Label($"Saltando: {(playerScript.IsJumping ? "SÍ" : "NO")}");
            }
        }
        
        GUILayout.EndArea();
    }
} 