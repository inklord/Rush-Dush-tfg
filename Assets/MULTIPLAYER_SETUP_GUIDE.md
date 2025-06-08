# 🌐 GUÍA DE CONFIGURACIÓN - MODO MULTIJUGADOR
## Fall Guys Unity Clone - Sistema Multijugador Completo

---

## 📋 **RESUMEN DEL SISTEMA**

### **¿Qué hace el sistema multijugador?**
- ✅ Permite jugar con hasta **20 jugadores reales** en red
- 🤖 **Rellena automáticamente** con IA si hay menos de 20 jugadores
- 🔄 **Reemplaza** la IA por jugadores reales cuando se unen
- 🌐 Utiliza **Photon PUN2** para networking
- 🎮 **Mantiene toda la funcionalidad** del modo single player

---

## 🎯 **PASO 1: CONFIGURAR EL LOBBY**

### **A. Añadir LobbyManager a la escena Lobby**

1. **Abrir escena**: `Assets/Scenes/Lobby/Lobby.unity`
2. **Crear GameObject vacío**: `GameObject → Create Empty`
3. **Nombre**: "LobbyManager"
4. **Añadir script**: `LobbyManager.cs` (ubicado en `Assets/Scripts/`)

### **B. Configurar UI del Lobby**

#### **Estructura de UI recomendada:**
```
Canvas/
├── MainMenuPanel/
│   ├── Title (Text: "🎮 FALL GUYS CLONE")
│   ├── SinglePlayerButton (Button: "🎮 MODO UN JUGADOR")
│   └── MultiplayerButton (Button: "🌐 MODO MULTIJUGADOR") [NUEVO]
├── MultiplayerPanel/ [NUEVO]
│   ├── ConnectionPanel/
│   │   ├── StatusText (Text: "Estado: Conectando...")
│   │   └── PlayerCountText (Text: "Jugadores: 0")
│   ├── InputPanel/
│   │   ├── PlayerNameInput (InputField: "Nombre del jugador")
│   │   └── RoomNameInput (InputField: "Nombre de la sala")
│   ├── ButtonsPanel/
│   │   ├── CreateRoomButton (Button: "🏗️ CREAR SALA")
│   │   ├── JoinRandomButton (Button: "🎲 UNIRSE A SALA")
│   │   └── BackToMenuButton (Button: "🏠 VOLVER")
│   └── RoomInfoPanel/
│       ├── RoomInfoText (Text: "Sala: No conectado")
│       ├── PlayerListText (Text: "Jugadores: Ninguno")
│       └── StartGameButton (Button: "🚀 INICIAR JUEGO")
```

#### **C. Configurar referencias en LobbyManager:**

En el Inspector del LobbyManager:

**🎮 UI Elements:**
- `singlePlayerButton` → Botón modo un jugador (existente)
- `multiplayerButton` → Botón modo multijugador (nuevo)
- `multiplayerPanel` → Panel de opciones multijugador
- `connectionPanel` → Panel de estado de conexión

**🌐 Multiplayer UI:**
- `createRoomButton` → Botón crear sala
- `joinRandomButton` → Botón unirse aleatoriamente
- `backToMenuButton` → Botón volver al menú
- `roomNameInput` → Campo nombre de sala
- `playerNameInput` → Campo nombre de jugador

**📊 Status Display:**
- `statusText` → Texto de estado de conexión
- `roomInfoText` → Información de la sala actual
- `playerListText` → Lista de jugadores en la sala
- `startGameButton` → Botón iniciar juego

---

## 🎮 **PASO 2: CREAR PREFABS DE JUGADORES**

### **A. Prefab de Jugador Multijugador**

1. **Duplicar** el prefab de jugador existente
2. **Nombre**: "NetworkPlayer"
3. **Añadir componente**: `PhotonView`
4. **Añadir componente**: `NetworkPlayerController` (ubicado en `Assets/Scripts/`)
5. **Configurar PhotonView**:
   - View ID: Asignar automáticamente
   - Ownership: Fixed
   - Synchronization: UnreliableOnChange
   - Observed Components: NetworkPlayerController

### **B. Configurar NetworkPlayerController**

En el Inspector del NetworkPlayerController:

**🌐 Network Settings:**
- ✅ `synchronizePosition` = true
- ✅ `synchronizeRotation` = true
- ✅ `synchronizeAnimations` = true
- ✅ `synchronizeVelocity` = true

**⚙️ Smoothing Settings:**
- `positionSmoothRate` = 20f
- `rotationSmoothRate` = 20f
- `velocitySmoothRate` = 10f

### **C. Prefab de IA (opcional)**

1. **Duplicar** el prefab de jugador
2. **Nombre**: "AIPlayer"
3. **Tag**: "IA"
4. **Añadir script**: `AIPlayerController` (ubicado en `Assets/Scripts/`)

---

## 🎯 **PASO 3: CONFIGURAR ESCENAS DE JUEGO**

### **A. Añadir MultiplayerPlayerManager a cada escena**

Para cada escena de juego (`WaitingUser`, `InGame`, `Carrera`, `Hexagonia`):

1. **Crear GameObject vacío**: "MultiplayerManager"
2. **Añadir script**: `MultiplayerPlayerManager` (ubicado en `Assets/Scripts/`)
3. **Añadir componente**: `PhotonView`

### **B. Configurar MultiplayerPlayerManager**

**🎮 Player Prefabs:**
- `playerPrefab` → Arrastra el prefab NetworkPlayer
- `aiPlayerPrefab` → Arrastra el prefab AIPlayer

**📍 Spawn Settings:**
- `spawnPoints` → Arrastra todos los puntos de spawn
- `spawnRadius` = 2f
- ✅ `randomizeSpawnOrder` = true

**👥 Player Management:**
- `maxPlayers` = 20
- `minRealPlayers` = 2
- ✅ `fillWithAI` = true

**🎯 Scene Management:**
- `nextSceneName` = "" (se auto-detecta)
- `gameStartDelay` = 3f

---

## 🔧 **PASO 4: CONFIGURAR PHOTON**

### **A. Configuración de Photon PUN2**

1. **Window → Photon Unity Networking → Wizard**
2. **Crear cuenta** en Photon o usar existente
3. **Copiar App ID** al proyecto
4. **Configurar regiones** (Europe, US, Asia, etc.)

### **B. Crear Resources folder**

1. **Assets → Create → Folder** → "Resources"
2. **Mover los prefabs** NetworkPlayer y AIPlayer a Resources/
3. **Los prefabs deben estar** en Resources/ para PhotonNetwork.Instantiate

### **C. Configurar PhotonView en prefabs**

Para cada prefab en Resources/:

1. **Seleccionar prefab** NetworkPlayer
2. **PhotonView component**:
   - Observed Components: añadir NetworkPlayerController
   - View ID: debe ser 0 (se asigna automáticamente)

---

## 🎮 **PASO 5: ADAPTAR SCRIPTS EXISTENTES**

### **A. Integración con sistemas existentes**

Los scripts multijugador se integran automáticamente con:

- **GameManager**: Detecta automáticamente el modo multijugador
- **LavaGameManager**: Compatible con eliminación de jugadores
- **UIManager**: Funciona con `ForceSuccess()` y `ForceFailure()`
- **MovimientoCamaraNuevo**: Se configura automáticamente para jugador local

### **B. Modificar sistemas de eliminación (opcional)**

En scripts como `MuerteLava.cs`, puedes añadir soporte explícito:

```csharp
void EliminatePlayer(GameObject player)
{
    // Verificar si está en modo multijugador
    if (MultiplayerPlayerManager.Instance != null && 
        MultiplayerPlayerManager.Instance.IsMultiplayerMode())
    {
        // Usar sistema multijugador
        MultiplayerPlayerManager.Instance.EliminatePlayer(player);
    }
    else
    {
        // Usar sistema single player existente
        // ... lógica original
    }
}
```

---

## 🎯 **PASO 6: CONFIGURAR BUILD SETTINGS**

### **A. Añadir escenas al Build**

1. **File → Build Settings**
2. **Añadir todas las escenas** en orden:
   - Lobby
   - WaitingUser
   - InGame
   - Carrera
   - Hexagonia
   - Ending
   - FinalFracaso

### **B. Configurar Photon Settings**

1. **Photon → PhotonServerSettings**
2. **Configurar**:
   - Auto-Join Lobby: ✅
   - Enable Close Connection: ✅
   - Enable Protocol Fallback: ✅

---

## 🔄 **FLUJO DE FUNCIONAMIENTO**

### **Modo Single Player (Existente)**
```
Lobby → [UN JUGADOR] → WaitingUser → InGame → Carrera → Hexagonia → Ending
```

### **Modo Multijugador (Nuevo)**
```
Lobby → [MULTIJUGADOR] → Conectar Photon → Crear/Unir Sala → 
Esperar Jugadores → [INICIAR] → WaitingUser → InGame → Carrera → 
Hexagonia → Ending
```

---

## 🎮 **CÓMO FUNCIONA EL REEMPLAZO DE IA**

1. **Al iniciar** una escena multijugador:
   - Se crean **jugadores reales** para cada jugador conectado
   - Se **rellenan espacios vacíos** con IA hasta llegar a 20 total

2. **Durante el juego**:
   - Los **jugadores reales** son controlados por usuarios
   - La **IA** actúa automáticamente
   - Ambos participan en el mismo juego

3. **Eliminación**:
   - Los jugadores eliminados **se sincronizan** en red
   - El **último jugador real** en pie gana
   - Transición automática a siguiente escena

---

## 🎯 **TESTING Y DEBUG**

### **A. Testing Local**

1. **Build del juego** con networking habilitado
2. **Ejecutar múltiples instancias**:
   - Una en Editor
   - Una o más builds
3. **Crear sala** en una instancia
4. **Unirse** desde las otras

### **B. Debug Info**

Los scripts incluyen debug visual:
- **LobbyManager**: Estado de conexión y sala
- **MultiplayerPlayerManager**: Contadores de jugadores
- **NetworkPlayerController**: Interpolación de red

### **C. Logs importantes**

```
🌐 Conectando a Photon...
✅ Conectado (EU)
🏗️ Creando sala: FallGuysRoom123
🏠 Unido a sala: FallGuysRoom123
👤 Jugador se unió: Player1234
🚀 Iniciando juego multijugador...
👥 Configurando jugadores...
🌐 Jugadores conectados: 3
🤖 IA creada: AI_0
✅ Setup completo - Reales: 3, IA: 17, Total: 20
```

---

## ⚠️ **TROUBLESHOOTING**

### **Problema: Errores de compilación**
- ✅ Asegurar que todos los scripts están en `Assets/Scripts/`
- ✅ Verificar que PhotonView usa `.Owner` (con mayúscula)
- ✅ MovimientoCamaraNuevo usa `.player` no `.target`

### **Problema: No conecta a Photon**
- ✅ Verificar App ID en PhotonServerSettings
- ✅ Verificar conexión a internet
- ✅ Comprobar región seleccionada

### **Problema: Prefabs no se crean**
- ✅ Prefabs deben estar en carpeta Resources/
- ✅ Verificar nombre exacto del prefab
- ✅ PhotonView debe estar configurado

### **Problema: Jugadores no se sincronizan**
- ✅ NetworkPlayerController en PhotonView observados
- ✅ PhotonView tiene View ID asignado
- ✅ Script NetworkPlayerController activado

### **Problema: Cámara no sigue al jugador local**
- ✅ MovimientoCamaraNuevo existe en la escena
- ✅ SetupLocalCamera se ejecuta correctamente
- ✅ Solo el jugador local tiene cámara asignada

---

## 📁 **UBICACIÓN DE ARCHIVOS**

### **Scripts principales** (en `Assets/Scripts/`):
- `LobbyManager.cs` - Gestión del lobby multijugador
- `MultiplayerPlayerManager.cs` - Gestión de jugadores en red
- `NetworkPlayerController.cs` - Sincronización de jugadores
- `AIPlayerController.cs` - Comportamiento de IA

### **Scripts existentes modificados**:
- `LHS_MainPlayer.cs` - Métodos de compatibilidad añadidos
- Scripts de eliminación - Integración automática

---

## 🎉 **¡SISTEMA COMPLETO!**

Una vez configurado todo:

1. **Modo un jugador** funciona igual que antes
2. **Modo multijugador** permite hasta 20 jugadores
3. **IA rellena** espacios vacíos automáticamente
4. **Sincronización perfecta** entre todos los clientes
5. **Transiciones automáticas** entre escenas
6. **Sistema de eliminación** multijugador completo
7. **API corregida** para PhotonView.Owner y MovimientoCamaraNuevo.player

¡El proyecto ahora soporta completamente modo multijugador manteniendo toda la funcionalidad original! 🎮🌐 