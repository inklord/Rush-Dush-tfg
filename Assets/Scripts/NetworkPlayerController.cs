using UnityEngine;
using Photon.Pun;

/// <summary>
/// 🌐 Controlador de jugador con soporte de red
/// Extiende LHS_MainPlayer para añadir sincronización multijugador
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class NetworkPlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("🌐 Network Settings")]
    public bool synchronizePosition = true;
    public bool synchronizeRotation = true;
    public bool synchronizeAnimations = true;
    public bool synchronizeVelocity = true;
    
    [Header("⚙️ Smoothing Settings")]
    public float positionSmoothRate = 15f;
    public float rotationSmoothRate = 15f;
    public float velocitySmoothRate = 10f;
    public float maxPositionError = 3f;          // Máximo error de posición antes de corregir bruscamente
    
    [Header("🎮 Control Settings")]
    public bool enableInputForOwner = true;      // Solo el owner puede controlar
    public bool enablePhysicsForOwner = true;    // Solo el owner tiene física activa
    public bool showNetworkDebug = false;        // Mostrar información de debug
    
    // Referencias locales
    private LHS_MainPlayer playerController;
    private Rigidbody playerRigidbody;
    private Animator playerAnimator;
    private new PhotonView photonView;
    
    // Variables de red para interpolación
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Vector3 networkVelocity;
    private bool networkGrounded;
    private float networkSpeed;
    private bool networkJumping;
    private bool networkMoving;
    
    // Estado local
    private bool isLocalPlayer;
    private bool isInitialized = false;
    private float lastSendTime;
    private float sendRate = 20f; // Envíos por segundo
    
    // Para correccion de posición
    private Vector3 positionError = Vector3.zero;
    private const float maxCorrectionTime = 1f;
    
    #region Unity Lifecycle
    
    void Awake()
    {
        // 🔍 VERIFICAR SI ESTAMOS EN MODO MULTIJUGADOR
        if (!PhotonNetwork.IsConnected && !PhotonNetwork.OfflineMode)
        {
            Debug.Log($"🎮 Modo SINGLE PLAYER detectado - Desactivando NetworkPlayerController en {gameObject.name}");
            this.enabled = false;
            return;
        }
        
        // Obtener referencias
        photonView = GetComponent<PhotonView>();
        playerController = GetComponent<LHS_MainPlayer>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        
        // Verificar si es jugador local
        isLocalPlayer = photonView.IsMine;
        
        Debug.Log($"🌐 NetworkPlayerController Awake - PhotonView.IsMine: {photonView.IsMine}, Owner: {photonView.Owner?.NickName}");
    }
    
    void Start()
    {
        // Pequeño delay para asegurar que PhotonNetwork esté listo
        Invoke("Initialize", 0.1f);
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        if (isLocalPlayer)
        {
            // El jugador local envía datos
            HandleLocalPlayerUpdate();
        }
        else
        {
            // Jugadores remotos reciben e interpolan datos
            HandleRemotePlayerUpdate();
        }
        
        // Debug información si está habilitado
        if (showNetworkDebug)
        {
            UpdateDebugInfo();
        }
    }
    
    void FixedUpdate()
    {
        if (!isInitialized) return;
        
        // Solo aplicar correcciones de física para jugadores remotos
        if (!isLocalPlayer)
        {
            ApplyNetworkCorrections();
        }
    }
    
    #endregion
    
    #region Initialization
    
    void Initialize()
    {
        // Re-verificar ownership después del delay
        isLocalPlayer = photonView.IsMine;
        
        Debug.Log($"🌐 NetworkPlayerController inicializado - Local: {isLocalPlayer}, Name: {gameObject.name}, PhotonView ID: {photonView.ViewID}");
        
        // Configurar según el ownership
        ConfigureForOwnership();
        
        // Configurar valores iniciales de red
        if (transform != null)
        {
            networkPosition = transform.position;
            networkRotation = transform.rotation;
        }
        
        if (playerRigidbody != null)
        {
            networkVelocity = playerRigidbody.velocity;
        }
        
        isInitialized = true;
        
        // Configurar nombre del jugador
        if (isLocalPlayer)
        {
            SetupPlayerName();
        }
    }
    
    void ConfigureForOwnership()
    {
        if (isLocalPlayer)
        {
            // Configuración para jugador local
            if (playerController != null)
            {
                playerController.enabled = true; // SIEMPRE habilitado para jugador local
                Debug.Log($"🎮 Control LOCAL habilitado para: {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ LHS_MainPlayer no encontrado en {gameObject.name}");
            }
            
            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = false;
                playerRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                Debug.Log($"🏃 Física LOCAL configurada para: {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Rigidbody no encontrado en {gameObject.name}");
            }
            
            // Configurar tag específico para jugador local
            gameObject.tag = "Player";
            
            Debug.Log($"🎮 Configurado como jugador LOCAL: {gameObject.name}");
        }
        else
        {
            // Configuración para jugador remoto
            if (playerController != null)
            {
                playerController.enabled = false; // Deshabilitado para remotos
                Debug.Log($"🌐 Control REMOTO deshabilitado para: {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ LHS_MainPlayer no encontrado en {gameObject.name}");
            }
            
            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = true; // Controlado por red
                playerRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                Debug.Log($"🌐 Física REMOTA configurada para: {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Rigidbody no encontrado en {gameObject.name}");
            }
            
            // Tag específico para jugadores remotos
            gameObject.tag = "RemotePlayer";
            
            Debug.Log($"🌐 Configurado como jugador REMOTO: {gameObject.name}");
        }
    }
    
    void SetupPlayerName()
    {
        // Configurar nombre del jugador
        PlayerName nameComponent = GetComponent<PlayerName>();
        if (nameComponent != null && PhotonNetwork.LocalPlayer != null)
        {
            string playerName = PhotonNetwork.LocalPlayer.NickName ?? "Player";
            nameComponent.photonView.RPC("SetNameText", RpcTarget.All, playerName);
            gameObject.name = $"{playerName}_Local";
        }
    }
    
    #endregion
    
    #region Local Player
    
    void HandleLocalPlayerUpdate()
    {
        // El jugador local actualiza y envía su estado automáticamente
        // Los datos se envían a través de OnPhotonSerializeView
        
        // Verificar que el control esté habilitado
        if (playerController != null && !playerController.enabled)
        {
            Debug.LogWarning($"⚠️ Reactivando control para jugador local: {gameObject.name}");
            playerController.enabled = true;
        }
        
        // Forzar envío si han pasado demasiados frames sin enviar
        if (Time.time - lastSendTime > 1f / sendRate)
        {
            lastSendTime = Time.time;
            // Los datos se envían automáticamente por PhotonNetwork
        }
    }
    
    #endregion
    
    #region Remote Player
    
    void HandleRemotePlayerUpdate()
    {
        // Interpolación suave hacia las posiciones de red
        if (synchronizePosition)
        {
            InterpolatePosition();
        }
        
        if (synchronizeRotation)
        {
            InterpolateRotation();
        }
        
        if (synchronizeAnimations)
        {
            UpdateRemoteAnimations();
        }
    }
    
    void InterpolatePosition()
    {
        if (networkPosition == Vector3.zero) return;
        
        float distance = Vector3.Distance(transform.position, networkPosition);
        
        // Si la distancia es muy grande, corregir inmediatamente
        if (distance > maxPositionError)
        {
            Debug.LogWarning($"⚠️ Corrección brusca de posición: {distance:F2}m para {gameObject.name}");
            transform.position = networkPosition;
            if (playerRigidbody != null && playerRigidbody.isKinematic)
            {
                playerRigidbody.MovePosition(networkPosition);
            }
        }
        else if (distance > 0.01f) // Solo interpolar si hay diferencia significativa
        {
            // Interpolación suave
            Vector3 targetPosition = Vector3.Lerp(transform.position, networkPosition, 
                positionSmoothRate * Time.deltaTime);
            
            transform.position = targetPosition;
            
            // Para objetos kinematic, usar MovePosition
            if (playerRigidbody != null && playerRigidbody.isKinematic)
            {
                playerRigidbody.MovePosition(targetPosition);
            }
        }
    }
    
    void InterpolateRotation()
    {
        if (networkRotation == Quaternion.identity) return;
        
        transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, 
            rotationSmoothRate * Time.deltaTime);
    }
    
    void UpdateRemoteAnimations()
    {
        if (playerAnimator == null) return;
        
        // Actualizar parámetros de animación basados en datos de red
        try
        {
            playerAnimator.SetBool("isMove", networkMoving);
            playerAnimator.SetBool("isJump", networkJumping);
            playerAnimator.SetFloat("Speed", networkSpeed);
            
            // Opcional: sincronizar estado de grounded
            if (playerAnimator.parameters.Length > 0)
            {
                foreach (var param in playerAnimator.parameters)
                {
                    if (param.name == "isGrounded" && param.type == AnimatorControllerParameterType.Bool)
                    {
                        playerAnimator.SetBool("isGrounded", networkGrounded);
                        break;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Error actualizando animaciones remotas: {e.Message}");
        }
    }
    
    void ApplyNetworkCorrections()
    {
        // No aplicar correcciones de velocidad para objetos kinematic
        if (playerRigidbody == null || playerRigidbody.isKinematic) return;
        
        // Aplicar velocidad de red si es significativamente diferente
        if (synchronizeVelocity && networkVelocity != Vector3.zero)
        {
            Vector3 velocityDifference = networkVelocity - playerRigidbody.velocity;
            
            if (velocityDifference.magnitude > 1f) // Solo corregir diferencias significativas
            {
                Vector3 correctionVelocity = Vector3.Lerp(playerRigidbody.velocity, networkVelocity, 
                    velocitySmoothRate * Time.fixedDeltaTime);
                
                playerRigidbody.velocity = correctionVelocity;
            }
        }
    }
    
    #endregion
    
    #region IPunObservable Implementation
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Enviando datos (jugador local)
            WriteNetworkData(stream);
        }
        else
        {
            // Recibiendo datos (jugador remoto)
            ReadNetworkData(stream, info);
        }
    }
    
    void WriteNetworkData(PhotonStream stream)
    {
        // Enviar posición
        if (synchronizePosition)
        {
            stream.SendNext(transform.position);
        }
        
        // Enviar rotación
        if (synchronizeRotation)
        {
            stream.SendNext(transform.rotation);
        }
        
        // Enviar velocidad
        if (synchronizeVelocity && playerRigidbody != null)
        {
            stream.SendNext(playerRigidbody.velocity);
        }
        
        // Enviar datos de animación
        if (synchronizeAnimations)
        {
            bool isMoving = false;
            bool isJumping = false;
            bool isGrounded = true;
            float speed = 0f;
            
            if (playerRigidbody != null)
            {
                Vector3 horizontalVelocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
                speed = horizontalVelocity.magnitude;
                isMoving = speed > 0.1f;
                isJumping = playerRigidbody.velocity.y > 0.1f;
            }
            
            // Detectar si está en el suelo
            if (Physics.Raycast(transform.position, Vector3.down, 1.5f))
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
            
            stream.SendNext(isMoving);
            stream.SendNext(isJumping);
            stream.SendNext(isGrounded);
            stream.SendNext(speed);
        }
    }
    
    void ReadNetworkData(PhotonStream stream, PhotonMessageInfo info)
    {
        // Recibir posición
        if (synchronizePosition)
        {
            networkPosition = (Vector3)stream.ReceiveNext();
        }
        
        // Recibir rotación
        if (synchronizeRotation)
        {
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
        
        // Recibir velocidad
        if (synchronizeVelocity)
        {
            networkVelocity = (Vector3)stream.ReceiveNext();
        }
        
        // Recibir datos de animación
        if (synchronizeAnimations)
        {
            networkMoving = (bool)stream.ReceiveNext();
            networkJumping = (bool)stream.ReceiveNext();
            networkGrounded = (bool)stream.ReceiveNext();
            networkSpeed = (float)stream.ReceiveNext();
        }
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// 🎯 Forzar eliminación del jugador desde la red
    /// </summary>
    public void EliminateFromNetwork()
    {
        if (photonView.IsMine)
        {
            // Notificar eliminación a todos los clientes
            photonView.RPC("OnPlayerEliminated", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
            
            // Destruir el objeto de red
            PhotonNetwork.Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 📊 Obtener información de latencia del jugador
    /// </summary>
    public int GetPlayerPing()
    {
        if (photonView.Owner != null)
        {
            return PhotonNetwork.GetPing();
        }
        return 0;
    }
    
    /// <summary>
    /// 🎮 Verificar si este es el jugador local
    /// </summary>
    public bool IsLocalPlayer()
    {
        return isLocalPlayer;
    }
    
    /// <summary>
    /// ⚙️ Forzar sincronización de posición
    /// </summary>
    public void ForceSyncPosition(Vector3 position)
    {
        if (photonView.IsMine)
        {
            transform.position = position;
            if (playerRigidbody != null)
            {
                playerRigidbody.position = position;
            }
        }
    }
    
    /// <summary>
    /// 🔧 Forzar reconfiguración de ownership
    /// </summary>
    public void ForceReconfigure()
    {
        // Verificar referencias antes de usar
        if (photonView == null)
        {
            photonView = GetComponent<PhotonView>();
        }
        
        if (photonView == null)
        {
            Debug.LogError($"❌ No se puede reconfigurar {gameObject.name} - PhotonView es null");
            return;
        }
        
        isLocalPlayer = photonView.IsMine;
        
        // Re-obtener referencias si son null
        if (playerController == null)
        {
            playerController = GetComponent<LHS_MainPlayer>();
        }
        
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }
        
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
        }
        
        ConfigureForOwnership();
        Debug.Log($"🔧 Reconfigurado: {gameObject.name} - Local: {isLocalPlayer}");
    }
    
    #endregion
    
    #region RPC Methods
    
    [PunRPC]
    void OnPlayerEliminated(string playerName)
    {
        Debug.Log($"💀 Jugador eliminado por red: {playerName}");
        
        // Efectos de eliminación si los hay
        if (!isLocalPlayer)
        {
            // Solo reproducir efectos para jugadores remotos
            // (el local ya está siendo destruido)
            PlayEliminationEffect();
        }
    }
    
    void PlayEliminationEffect()
    {
        // Aquí puedes añadir efectos de partículas, sonidos, etc.
        Debug.Log($"🎆 Efectos de eliminación para {gameObject.name}");
    }
    
    #endregion
    
    #region Debug
    
    void UpdateDebugInfo()
    {
        if (Time.time % 2f < Time.deltaTime) // Cada 2 segundos
        {
            string debugInfo = $"[{gameObject.name}] ";
            debugInfo += isLocalPlayer ? "LOCAL" : "REMOTE";
            debugInfo += $" | Pos: {transform.position:F1}";
            debugInfo += $" | Vel: {(playerRigidbody?.velocity.magnitude ?? 0):F1}";
            debugInfo += $" | Controller: {(playerController?.enabled ?? false)}";
            
            if (!isLocalPlayer)
            {
                debugInfo += $" | NetPos: {networkPosition:F1}";
                debugInfo += $" | Dist: {Vector3.Distance(transform.position, networkPosition):F2}m";
            }
            
            Debug.Log(debugInfo);
        }
    }
    
    void OnGUI()
    {
        if (!showNetworkDebug || !isInitialized) return;
        
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);
        
        if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && 
            screenPos.y > 0 && screenPos.y < Screen.height)
        {
            string info = $"{gameObject.name}\n";
            info += isLocalPlayer ? "LOCAL" : "REMOTE";
            info += $"\nController: {(playerController?.enabled ?? false)}";
            
            if (!isLocalPlayer)
            {
                float distance = Vector3.Distance(transform.position, networkPosition);
                info += $"\nDist: {distance:F2}m";
            }
            
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = isLocalPlayer ? Color.green : Color.red;
            
            GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 50, 100, 60), info, style);
        }
    }
    
    #endregion
    
    #region Photon Callbacks
    
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        // Verificar si este objeto pertenecía al jugador que se fue
        if (photonView.Owner == otherPlayer)
        {
            Debug.Log($"👋 Destruyendo objeto del jugador que se fue: {otherPlayer.NickName}");
            Destroy(gameObject);
        }
    }
    
    #endregion
} 