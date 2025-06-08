# 🚀 CONFIGURACIÓN PHOTON SIMPLE - Tutorial Style

## 🎯 **OBJETIVO**
Implementar el sistema simple de Photon que viste en el tutorial:
- ✅ Jugador como prefab en Resources  
- ✅ Launcher que spawns y reemplaza AIs
- ✅ LHS_MainPlayer adaptado para red
- ✅ Integración con UI login existente

## 📋 **PASOS DE CONFIGURACIÓN**

### 1. **CREAR PREFAB SIMPLE PLAYER**

1. **En la escena InGame**, selecciona un objeto con `LHS_MainPlayer`
2. **Añadir componentes de red**:
   ```
   - PhotonView
   - PhotonTransformView  
   - PhotonAnimatorView (si tiene Animator)
   ```

3. **Configurar PhotonView**:
   ```
   View ID: Auto
   Synchronization: Unreliable On Change
   Ownership Transfer: Takeover
   Observables:
   - LHS_MainPlayer (OnPhotonSerializeView)
   - PhotonTransformView (Position, Rotation)
   - PhotonAnimatorView (si tiene animaciones)
   ```

4. **Asegurar configuración LHS_MainPlayer**:
   ```
   ✅ Tag: "Player"
   ✅ Layer: Default
   ✅ Rigidbody configurado
   ✅ Collider configurado
   ✅ MovimientoCamaraSimple disponible en cámara
   ```

5. **Crear prefab en Resources**:
   - Arrastrar objeto a `Assets/Resources/`
   - Nombrar: **`SimplePlayer`**
   - Eliminar de escena

### 2. **CONFIGURAR LAUNCHER EN ESCENA INGAME**

1. **Crear GameObject vacío**: `PhotonLauncher`
2. **Añadir script**: `PhotonLauncher.cs`
3. **Configurar**:
   ```
   Spawn Point: [Arrastrar punto donde aparece AI]
   Show Debug Info: ✓
   ```

### 3. **CONFIGURAR PUNTO DE SPAWN**

1. **Buscar IA en la escena** que quieres reemplazar
2. **Crear punto de spawn**:
   - GameObject vacío en posición de la IA
   - Nombrar: `PlayerSpawnPoint`
   - Tag: `Respawn` (opcional)

3. **Asignar en PhotonLauncher**:
   - Arrastrar `PlayerSpawnPoint` a `Spawn Point`

### 4. **INTEGRACIÓN CON LOGIN UI**

En lugar de conectar directamente, el `PhotonLauncher` debe esperar a que el sistema de login haga la conexión.

**Modificación requerida en script de login**:
```csharp
// Después de conectar exitosamente
PhotonNetwork.JoinRandomOrCreateRoom(); // Esto trigger OnJoinedRoom en launcher
```

### 5. **SETUP PHOTON SETTINGS**

1. **Verificar PhotonServerSettings**:
   ```
   Fixed Region: tu región preferida
   Enable UDP: ✓
   ```

2. **Configurar en script de login**:
   ```csharp
   PhotonNetwork.ConnectUsingSettings();
   ```

## 🔧 **CONFIGURACIÓN ACTUAL**

### ✅ **Scripts Listos**
- `PhotonLauncher.cs` - Maneja spawn y conexión
- `LHS_MainPlayer.cs` - Adaptado para red con Photon
- `MovimientoCamaraSimple.cs` - Cámara compatible

### 🎮 **Flujo de Trabajo**
1. **Login UI** → conecta a Photon
2. **PhotonLauncher** → detecta conexión  
3. **Spawning** → reemplaza AI con jugador real
4. **Cámara** → sigue automáticamente al nuevo jugador

## 🚨 **TROUBLESHOOTING**

### ❌ "No spawnea jugador"
- **Verificar**: Prefab `SimplePlayer` en `/Resources/`
- **Verificar**: PhotonView configurado
- **Verificar**: Conexión exitosa a Photon

### ❌ "Prefab not found"
```csharp
// En PhotonLauncher, cambiar línea:
GameObject player = PhotonNetwork.Instantiate("SimplePlayer", ...);
// Por el nombre exacto de tu prefab
```

### ❌ "Multiple players"
- **Verificar**: Solo un PhotonLauncher en escena
- **Verificar**: Script no duplicando spawns

### ❌ "No reemplaza AI"
- **Verificar**: IA tiene tag "AI" o nombre contiene "ai"
- **Verificar**: Spawn point cerca de IA (radio 2 unidades)

## 🎉 **RESULTADO ESPERADO**

1. **Login exitoso** → conecta automáticamente
2. **Entra a sala** → spawns jugador en punto específico  
3. **IA desaparece** → reemplazada por jugador real
4. **Cámara sigue** → movimiento suave automático
5. **Multijugador funcional** → otros ven tu movimiento

¡Sistema simple y funcional como en el tutorial! 