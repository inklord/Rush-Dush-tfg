# 🎮 DOCUMENTACIÓN TÉCNICA - FALL GUYS UNITY CLONE
## "Aventuras Vertiginosas en Unity"

---

### 📋 **ÍNDICE**
1. [Información General del Proyecto](#información-general)
2. [Arquitectura del Sistema](#arquitectura-del-sistema)
3. [Sistema de Movimiento del Jugador](#sistema-de-movimiento)
4. [Sistema de Obstáculos](#sistema-de-obstáculos)
5. [Sistema de Cámara](#sistema-de-cámara)
6. [Mecánicas de Juego](#mecánicas-de-juego)
7. [Configuración y Setup](#configuración-y-setup)
8. [Guía de Implementación](#guía-de-implementación)
9. [Troubleshooting](#troubleshooting)
10. [Referencias y Recursos](#referencias-y-recursos)

---

## 📊 **INFORMACIÓN GENERAL**

### **Descripción del Proyecto**
Fall Guys Unity Clone es una implementación técnica del popular juego de Battle Royale multijugador, desarrollado en Unity Engine. El proyecto se centra en recrear las mecánicas principales de movimiento, obstáculos dinámicos, y sistemas de colisión que caracterizan a Fall Guys.

### **Especificaciones Técnicas**
- **Motor:** Unity 2022.3 LTS
- **Lenguaje:** C# (.NET Framework)
- **Plataforma Target:** PC (Windows)
- **Arquitectura:** Component-Based System
- **Networking:** Photon PUN2 (Multiplayer)
- **Physics:** Unity Physics 3D

### **Características Principales**
- ✅ Sistema de movimiento fluido con detección avanzada de suelo
- 🔨 Obstáculos dinámicos con física realista
- 📹 Sistema de cámara profesional con múltiples modos
- 🎯 Sistema de colisiones híbrido (Triggers + Colliders)
- 🎮 Mecánicas de salto mejoradas con Coyote Time
- 📊 Sistema de gestión de estados del jugador

---

## 🏗️ **ARQUITECTURA DEL SISTEMA**

### **Estructura de Componentes**

```
📁 Fall Guys Clone
├── 🎮 Core Systems
│   ├── LHS_MainPlayer.cs (Controlador principal)
│   ├── MovimientoCamaraNuevo.cs (Sistema de cámara)
│   └── GameManager.cs (Gestión de partida)
├── 🚧 Obstacle Systems
│   ├── SpinningHammer.cs (Martillos giratorios)
│   ├── DynamicObstacle.cs (Sistema universal)
│   └── BounceWall.cs (Paredes de rebote)
├── 🌐 Networking
│   ├── PhotonRealtime
│   └── PhotonUnityNetworking
└── 🎨 Assets & Resources
    ├── Materials/
    ├── Prefabs/
    └── Scenes/
```

### **Patrón de Diseño**
El proyecto utiliza principalmente el patrón **Component-Entity System** de Unity, complementado con:
- **Observer Pattern:** Para eventos de colisión
- **State Pattern:** Para estados del jugador
- **Factory Pattern:** Para generación de obstáculos

---

## 🏃‍♂️ **SISTEMA DE MOVIMIENTO DEL JUGADOR**

### **Clase Principal: `LHS_MainPlayer.cs`**

#### **Variables de Configuración**
```csharp
[Header("Movement Settings")]
public float speed = 10f;                    // Velocidad de movimiento
public float rotateSpeed = 5f;               // Velocidad de rotación
public float jumpPower = 5f;                 // Fuerza de salto

[Header("Ground Check")]
public float groundCheckDistance = 2.0f;     // Distancia de detección
public LayerMask groundLayerMask = -1;       // Capas de suelo
public bool showDebugInfo = true;            // Debug visual

[Header("Jump Settings")]
public float jumpCooldown = 0.1f;            // Cooldown entre saltos
public float coyoteTime = 0.1f;              // Tiempo extra para saltar
```

#### **Mecánicas Avanzadas**

##### **1. Detección Multi-Punto de Suelo**
```csharp
void CheckGrounded()
{
    // Raycast principal desde el centro
    Vector3 rayStart = transform.position + Vector3.up * 0.1f;
    bool centerHit = Physics.Raycast(rayStart, Vector3.down, out hit, 
                                    groundCheckDistance, groundLayerMask);
    
    // Verificación en 4 puntos adicionales para mayor precisión
    Vector3[] checkPoints = {
        rayStart + Vector3.forward * 0.3f,
        rayStart + Vector3.back * 0.3f,
        rayStart + Vector3.left * 0.3f,
        rayStart + Vector3.right * 0.3f
    };
    
    // Resultado final basado en cualquier punto de contacto
    isGrounded = centerHit || AnyCornerHit(checkPoints);
}
```

##### **2. Sistema de Salto con Coyote Time**
```csharp
void HandleJumpInput()
{
    if (Input.GetButtonDown("Jump"))
    {
        jumpRequested = true;
    }
    
    if (jumpRequested)
    {
        bool canPerformJump = isGrounded && canJump ||
                             (!isGrounded && canJump && 
                              (Time.time - lastGroundedTime) <= coyoteTime);
        
        if (canPerformJump && (Time.time - lastJumpTime) >= jumpCooldown)
        {
            PerformJump();
        }
        jumpRequested = false;
    }
}
```

##### **3. Control de Rotación Basado en Cámara**
```csharp
void Move()
{
    moveVec = new Vector3(hAxis, 0, vAxis).normalized;
    
    if (UseCameraRotation)
    {
        Quaternion cameraRotation = Quaternion.Euler(0f, currentCamera.transform.eulerAngles.y, 0f);
        moveVec = cameraRotation * moveVec;
    }
    
    transform.position += moveVec * speed * Time.deltaTime;
}
```

---

## 🚧 **SISTEMA DE OBSTÁCULOS**

### **Arquitectura Híbrida de Tres Capas**

#### **Capa 1: Detección de Colisiones**
```csharp
// Manejo unificado de Triggers y Colliders
private void OnTriggerEnter(Collider other) => HandleObstacleCollision(other.gameObject, other.transform.position);
private void OnCollisionEnter(Collision collision) => HandleObstacleCollision(collision.gameObject, collision.contacts[0].point);
```

#### **Capa 2: Sistema por Tags**
```csharp
void HandleObstacleCollision(GameObject collisionObject, Vector3 contactPoint)
{
    switch (collisionObject.tag)
    {
        case "SpinningHammer": HandleSpinningHammerCollision(collisionObject, contactPoint); break;
        case "MovingObstacle": HandleMovingObstacleCollision(collisionObject, contactPoint); break;
        case "Bouncer": HandleBouncerCollision(collisionObject, contactPoint); break;
        case "Wall": HandleWallCollision(collisionObject, contactPoint); break;
        default: HandleDynamicObstacleCollision(collisionObject, contactPoint); break;
    }
}
```

#### **Capa 3: Obstáculos Específicos**

### **SpinningHammer.cs - Martillos Giratorios**

#### **Configuración Principal**
```csharp
[Header("💥 Impact Settings")]
public float baseKnockbackForce = 35f;       // Fuerza base (incrementada)
public float maxKnockbackForce = 70f;        // Fuerza máxima
public float verticalForceMultiplier = 0.8f; // Multiplicador vertical
public float minVerticalForce = 15f;         // Fuerza vertical mínima

[Header("🔄 Rotation Settings")]
public float rotationSpeed = 180f;           // Grados por segundo
public Vector3 rotationAxis = Vector3.up;    // Eje de rotación
public bool randomStartRotation = true;      // Rotación inicial aleatoria
```

#### **Cálculo de Fuerzas Avanzado**
```csharp
public Vector3 GetLaunchForce(Vector3 playerPosition)
{
    // Dirección horizontal (alejar del martillo)
    Vector3 horizontalDirection = (playerPosition - transform.position).normalized;
    horizontalDirection.y = 0;
    
    // Combinar fuerza horizontal y vertical garantizada
    Vector3 horizontalForce = horizontalDirection * GetKnockbackForce();
    Vector3 verticalForce = Vector3.up * GetVerticalForce();
    
    return horizontalForce + verticalForce;
}

public float GetVerticalForce()
{
    float baseForce = GetKnockbackForce();
    return Mathf.Max(minVerticalForce, baseForce * verticalForceMultiplier);
}
```

### **DynamicObstacle.cs - Sistema Universal**

#### **Tipos de Efectos**
```csharp
public enum ObstacleEffectType
{
    Knockback,  // Empuje direccional fuerte
    Push,       // Empuje continuo
    Stun,       // Aturdimiento temporal
    Bounce,     // Rebote hacia arriba
    Slow,       // Ralentizar jugador
    Stop        // Parar completamente
}
```

#### **Configuración Modular**
```csharp
[Header("🎯 Obstacle Configuration")]
public ObstacleEffectType effectType = ObstacleEffectType.Knockback;
public float forceAmount = 15f;
public float stunDuration = 1f;

[Header("📐 Force Direction")]
public bool useCustomDirection = false;
public Vector3 customForceDirection = Vector3.forward;
public bool useObjectForward = true;
public bool addVerticalComponent = false;
public float verticalForceRatio = 0.3f;
```

---

## 📹 **SISTEMA DE CÁMARA**

### **MovimientoCamaraNuevo.cs - Cámara Profesional**

#### **5 Modos de Cámara**
```csharp
public enum CameraMode
{
    ThirdPerson,    // Tercera persona estándar
    FirstPerson,    // Primera persona
    OverShoulder,   // Sobre el hombro
    Free,          // Cámara libre
    Cinematic      // Modo cinemático
}
```

#### **Gestión de Cursor**
```csharp
void HandleCursorInput()
{
    if (Input.GetKeyDown(KeyCode.Alt))
    {
        cursorLocked = !cursorLocked;
        UpdateCursorState();
    }
    
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        cursorLocked = false;
        UpdateCursorState();
    }
}

void UpdateCursorState()
{
    if (cursorLocked)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    else
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
```

#### **Sistema Anti-NaN**
```csharp
void LateUpdate()
{
    if (!IsValidVector3(transform.position))
    {
        Debug.LogWarning("🚨 POSICIÓN NaN DETECTADA - Reseteo automático");
        ResetToSafeValues();
        return;
    }
    
    UpdateCameraPosition();
}

bool IsValidVector3(Vector3 vector)
{
    return !float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z) &&
           !float.IsInfinity(vector.x) && !float.IsInfinity(vector.y) && !float.IsInfinity(vector.z);
}
```

---

## 🎮 **MECÁNICAS DE JUEGO**

### **Estados del Jugador**
```csharp
// Control de estado de salto
bool isGrounded = false;
bool wasGrounded = false;
bool canJump = true;
bool jumpRequested = false;

// Timing crítico
float lastGroundedTime = 0f;
float lastJumpTime = 0f;
```

### **Efectos de Estado**
```csharp
// Aturdimiento temporal
IEnumerator StunEffect(float duration)
{
    float originalSpeed = speed;
    speed *= 0.3f; // Reducir velocidad al 30%
    
    yield return new WaitForSeconds(duration);
    
    speed = originalSpeed; // Restaurar velocidad
}
```

### **Sistema de Partículas y Efectos**
```csharp
void PlayImpactEffects(Vector3 position)
{
    // Sonido con variación de pitch
    if (audioSource != null)
    {
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(impactSound);
    }
    
    // Partículas en posición de impacto
    if (bounce != null)
    {
        bounce.Play();
        bounce.transform.position = position;
    }
    
    // Shake de cámara
    var camera = FindObjectOfType<MovimientoCamaraNuevo>();
    if (camera != null)
    {
        camera.ShakeCamera(1.0f, 2.5f);
    }
}
```

---

## ⚙️ **CONFIGURACIÓN Y SETUP**

### **Requerimientos del Sistema**
- Unity 2022.3 LTS o superior
- Visual Studio 2019/2022 o VS Code
- Git para control de versiones
- 8GB RAM mínimo, 16GB recomendado

### **Configuración Inicial**

#### **1. Configurar Tags**
```
Player
SpinningHammer
MovingObstacle
Bouncer
Pusher
Wall
Floor
Platform
```

#### **2. Configurar Layers**
```
0: Default
3: Ground
6: Player
7: Obstacles
8: Interactive
```

#### **3. Setup del Jugador**
```csharp
// Componentes requeridos en Player
- Rigidbody (Mass: 1, Use Gravity: true)
- CapsuleCollider (Height: 2, Radius: 0.5)
- LHS_MainPlayer script
- Animator (con estados de movimiento y salto)
```

#### **4. Setup de Obstáculos**
```csharp
// Para Martillos Giratorios
gameObject.tag = "SpinningHammer";
gameObject.AddComponent<SpinningHammer>();

// Configurar Collider
GetComponent<Collider>().isTrigger = true; // O false para colisión sólida
```

### **Configuración de Prefabs**

#### **Player Prefab**
```
Player
├── Model (3D Character)
├── Main Camera (Child)
└── UI Canvas (World Space)
```

#### **Spinning Hammer Prefab**
```
SpinningHammer
├── Hammer Model
├── HammerTrail (TrailRenderer)
├── Impact Particles
└── Audio Source
```

---

## 📚 **GUÍA DE IMPLEMENTACIÓN**

### **Paso 1: Configurar Movimiento Básico**
1. Crear GameObject "Player"
2. Agregar `LHS_MainPlayer` script
3. Configurar variables públicas en Inspector
4. Asignar referencias de cámara y efectos

### **Paso 2: Implementar Sistema de Obstáculos**
1. Crear obstáculo como GameObject
2. Agregar script específico (`SpinningHammer` o `DynamicObstacle`)
3. Configurar tag apropiado
4. Ajustar parámetros de fuerza

### **Paso 3: Setup de Cámara**
1. Agregar `MovimientoCamaraNuevo` a Main Camera
2. Asignar referencia del jugador
3. Configurar modo inicial y sensibilidad

### **Paso 4: Testing y Debug**
```csharp
// Activar debug visual
public bool showDebugInfo = true;

// Logs importantes
Debug.Log($"🚀 ¡LANZADO POR MARTILLO! Fuerza total: {launchForce.magnitude:F1}");
Debug.Log($"🌍 Tocó el suelo - Puede saltar de nuevo!");
```

---

## 🔧 **TROUBLESHOOTING**

### **Problemas Comunes**

#### **1. Jugador no salta**
```
❌ Problema: canJump = false
✅ Solución: Verificar isGrounded y lastGroundedTime
✅ Debug: Activar showDebugInfo para ver raycast
```

#### **2. Martillo no lanza al jugador**
```
❌ Problema: Tag incorrecto o ausente
✅ Solución: 
   1. Verificar tag "SpinningHammer"
   2. Comprobar que tiene script SpinningHammer
   3. Revisar configuración de Collider (Trigger vs Solid)
```

#### **3. Cámara con posición NaN**
```
❌ Problema: Cálculos matemáticos inválidos
✅ Solución: Sistema IsValidVector3() automático implementado
```

#### **4. Detección de suelo inconsistente**
```
❌ Problema: Raycast insuficiente
✅ Solución: Sistema multi-punto implementado
✅ Configurar: groundCheckDistance y groundLayerMask
```

### **Debug Tools**

#### **Gizmos Visuales**
```csharp
void OnDrawGizmosSelected()
{
    // Mostrar detección de suelo
    Gizmos.color = isGrounded ? Color.green : Color.red;
    Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    
    // Mostrar área de detección
    Gizmos.DrawWireCube(transform.position, new Vector3(0.6f, 0.1f, 0.6f));
}
```

#### **Console Logging**
```csharp
// En HandleObstacleCollision
Debug.Log($"🚧 Procesando colisión: {collisionObject.name} - Tag: '{obstacleTag}'");

// En SpinningHammer
Debug.Log($"🚀 Fuerza de lanzamiento - Horizontal: {horizontalForce:F1}, Vertical: {verticalForce:F1}");
```

---

## 📊 **MÉTRICAS Y VALORES RECOMENDADOS**

### **Configuración del Jugador**
```csharp
speed = 10f;                    // Velocidad óptima para Fall Guys
rotateSpeed = 5f;               // Rotación responsiva pero estable
jumpPower = 5f;                 // Salto satisfactorio
groundCheckDistance = 2.0f;     // Detección confiable
jumpCooldown = 0.1f;            // Previene spam
coyoteTime = 0.1f;              // Feel natural
```

### **Configuración de Martillos**
```csharp
baseKnockbackForce = 35f;       // Fuerza base visible
maxKnockbackForce = 70f;        // Máximo dramático
minVerticalForce = 15f;         // Garantiza elevación
rotationSpeed = 180f;           // Velocidad intimidante
```

### **Configuración de Cámara**
```csharp
mouseSensitivity = 2f;          // Sensibilidad cómoda
distance = 5f;                  // Distancia óptima
height = 2f;                    // Altura de seguimiento
smoothTime = 0.3f;              // Suavizado natural
```

---

## 🌐 **INTEGRACIÓN MULTIPLAYER**

### **Photon PUN2 Setup**
```csharp
// Sincronización de movimiento
public class NetworkPlayer : MonoBehaviourPunPV, IPunObservable
{
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(isGrounded);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
            isGrounded = (bool)stream.ReceiveNext();
        }
    }
}
```

---

## 📈 **OPTIMIZACIÓN Y PERFORMANCE**

### **Técnicas Implementadas**
1. **Object Pooling** para partículas y efectos
2. **LOD System** para modelos distantes
3. **Culling Optimization** en sistema de cámara
4. **Physics Optimization** con layers específicos

### **Monitoreo de Performance**
```csharp
// FPS Counter
void Update()
{
    if (showDebugInfo)
    {
        float fps = 1.0f / Time.deltaTime;
        Debug.Log($"FPS: {fps:F1}");
    }
}
```

---

## 🔗 **REFERENCIAS Y RECURSOS**

### **Documentación Unity**
- [Unity Physics Documentation](https://docs.unity3d.com/Manual/PhysicsSection.html)
- [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/index.html)
- [Unity Animator](https://docs.unity3d.com/Manual/class-AnimatorController.html)

### **Patrones de Diseño**
- Component System Architecture
- Observer Pattern para eventos
- State Machine para control de jugador

### **Fall Guys Mechanics Reference**
- Movement: Controlled but bouncy
- Physics: Exaggerated but predictable
- Obstacles: Clear telegraphing and fair reaction time

---

## 📝 **CHANGELOG**

### **Versión 2.0 - Sistema de Obstáculos Avanzado**
- ✅ Implementado SpinningHammer con fuerzas variables
- ✅ Sistema DynamicObstacle universal
- ✅ Detección híbrida Trigger + Collider
- ✅ Fuerzas verticales garantizadas

### **Versión 1.5 - Mejoras de Movimiento**
- ✅ Sistema de salto con Coyote Time
- ✅ Detección multi-punto de suelo
- ✅ Controles basados en cámara
- ✅ Sistema anti-NaN para cámara

### **Versión 1.0 - Base Implementation**
- ✅ Movimiento básico del jugador
- ✅ Sistema de cámara fundamental
- ✅ Colisiones básicas

---

## 🎯 **ROADMAP FUTURO**

### **Próximas Implementaciones**
- [ ] Sistema de checkpoints
- [ ] Multiplayer completo con sincronización
- [ ] Sistema de rankings y puntuaciones
- [ ] Más tipos de obstáculos (péndulos, plataformas móviles)
- [ ] Sistema de customización de personajes
- [ ] Modo espectador
- [ ] Replay system

---

**🔧 Desarrollado con Unity Engine**  
**📅 Última actualización:** Diciembre 2024  
**👨‍💻 Equipo de desarrollo:** [Tu nombre/equipo]  
**📧 Contacto:** [Tu email]

---

*Esta documentación es un documento vivo y se actualiza constantemente con nuevas implementaciones y mejoras del sistema.* 