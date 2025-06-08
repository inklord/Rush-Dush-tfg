# 🚨 SOLUCIÓN PROBLEMAS MULTIPLAYER

## 📋 **PROBLEMAS DETECTADOS Y SOLUCIONES**

### 1. **🎮 PROBLEMA: Cursor bloqueado en opciones**
**CAUSA:** La cámara bloquea el cursor y no lo libera para menús  
**SOLUCIÓN:** Implementado `CursorManager.cs`

**🔧 CONFIGURACIÓN:**
1. Crear un GameObject vacío llamado "CursorManager"
2. Añadir el script `CursorManager.cs`
3. En tu menú de opciones, llamar: `CursorManager.Instance.SetOptionsCursor()`
4. Al volver al juego, llamar: `CursorManager.Instance.SetGameCursor()`

### 2. **👥 PROBLEMA: Múltiples jugadores spawning**
**CAUSA:** Cada cliente spawns jugadores en lugar de solo uno  
**SOLUCIÓN:** Arreglado en `PhotonLauncher.cs`

**✅ CAMBIOS IMPLEMENTADOS:**
- Solo spawning si no hay jugador ya
- Verificación de `PhotonView.IsMine`
- Posiciones únicas por `ActorNumber`

### 3. **🕹️ PROBLEMA: Todos los jugadores se mueven**
**CAUSA:** Falta de verificación de ownership en `LHS_MainPlayer.cs`  
**SOLUCIÓN:** Ownership estricto implementado

**✅ CAMBIOS IMPLEMENTADOS:**
- Solo owner puede mover su jugador
- Jugadores remotos solo interpolan
- Input bloqueado para jugadores remotos

### 4. **🌐 PROBLEMA: Sincronización fallida**
**CAUSA:** Problemas de red o configuración PhotonView  
**SOLUCIÓN:** Debug UI para diagnosticar

---

## 🛠️ **CONFIGURACIÓN PASO A PASO**

### **PASO 1: Añadir CursorManager**
```csharp
// En tu escena principal
1. Crear GameObject vacío -> "CursorManager"
2. Añadir script CursorManager.cs
3. Configurar:
   - Cursor Locked: ✅ true
   - Show Debug: ✅ true
```

### **PASO 2: Añadir Debug UI**
```csharp
// En tu escena de juego
1. Crear GameObject vacío -> "MultiplayerDebug"
2. Añadir script MultiplayerDebugUI.cs
3. Presionar F5 en juego para ver debug
```

### **PASO 3: Configurar PhotonView en NetworkPlayer**
```
NetworkPlayer prefab debe tener:
1. PhotonView component
2. Observed Components: LHS_MainPlayer
3. Synchronization: Send Rate 30, Receive Rate 30
```

### **PASO 4: Verificar Configuración**
```
1. Build el juego
2. Ejecutar Editor + Build
3. Presionar F5 para debug UI
4. Verificar:
   - Solo 1 jugador "MÍO" por cliente
   - Otros jugadores aparecen como "REMOTO"
   - Total jugadores = número de clientes
```

---

## 🔧 **CONTROLES DE DEBUG**

| Tecla | Función |
|-------|---------|
| `F5` | Toggle Debug UI |
| `F6` | Info Avanzada |
| `F3` | Log ownership del jugador |
| `F1` | Debug cursor |
| `ESC` | Toggle cursor en juego |

---

## 🚨 **SOLUCIÓN PROBLEMAS ESPECÍFICOS**

### **"Desde build veo 3, desde editor veo 2"**
**CAUSA:** Spawning duplicado  
**SOLUCIÓN:** 
- Usar `PhotonLauncher.cs` corregido
- Verificar que no haya múltiples launchers activos

### **"Movimiento no se refleja entre build/editor"**
**CAUSA:** Sincronización fallida  
**SOLUCIÓN:**
- Verificar PhotonView configurado correctamente
- Usar Debug UI (F5) para verificar ownership
- Comprobar que `LHS_MainPlayer` esté en "Observed Components"

### **"Me puedo mover en ambas instancias"**
**CAUSA:** Falta control de ownership  
**SOLUCIÓN:**
- Usar `LHS_MainPlayer.cs` corregido
- Solo owner puede mover su jugador
- Verificar logs con F3

---

## 🎯 **VERIFICACIÓN FINAL**

### **✅ CHECKLIST DE FUNCIONAMIENTO CORRECTO:**

1. **En Debug UI (F5):**
   - [ ] Cada cliente tiene exactamente 1 jugador "MÍO"
   - [ ] Otros jugadores aparecen como "REMOTO"
   - [ ] Total jugadores = número de clientes conectados

2. **En Juego:**
   - [ ] Solo puedo mover MI jugador
   - [ ] Veo a otros jugadores moverse
   - [ ] Cursor funciona en menús (ESC)
   - [ ] Cursor se bloquea en juego

3. **En Red:**
   - [ ] Movimiento se sincroniza entre clientes
   - [ ] Saltos y animaciones se sincronizan
   - [ ] No hay duplicación de jugadores

---

## 🆘 **SI SIGUES TENIENDO PROBLEMAS**

### **RESET COMPLETO:**
1. Cerrar Unity
2. Borrar `Library/` folder
3. Abrir Unity
4. Verificar que `NetworkPlayer` prefab esté en `Resources/`
5. Verificar que PhotonView esté configurado correctamente
6. Hacer build limpio

### **DIAGNÓSTICO AVANZADO:**
```csharp
// Añadir esto a LHS_MainPlayer Start() para debug
Debug.Log($"Player {gameObject.name}: PhotonView = {photonView != null}, IsMine = {photonView?.IsMine}, ActorNumber = {PhotonNetwork.LocalPlayer.ActorNumber}");
```

---

## 📝 **NOTAS IMPORTANTES**

- **NetworkPlayer prefab** debe estar en `Resources/` folder
- **PhotonView** debe observar `LHS_MainPlayer` component
- **Solo UN** `PhotonLauncher` por escena
- **Solo UN** `CursorManager` por escena (se mantiene entre escenas)
- **Debug UI** activo durante testing para diagnosticar problemas

---

**🎉 Con estas correcciones, el multiplayer debería funcionar correctamente:**
- ✅ Solo 1 jugador por cliente
- ✅ Sincronización perfecta
- ✅ Cursor funcional en menús
- ✅ Control de ownership estricto 