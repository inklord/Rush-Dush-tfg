# 🎯 TUTORIAL SIMPLE - FALL GUYS MULTIPLAYER

## ✅ **LO QUE YA FUNCIONA**
- ✅ Crear sala
- ✅ Unirse a sala
- ✅ Conectar con Photon

## 🎯 **OBJETIVO**
Hacer que cada jugador tenga su personaje y se vea en pantalla.

---

## 📋 **PASOS SÚPER SIMPLES**

### **PASO 1: Limpiar scripts antiguos** 
1. Ve a tu escena de juego
2. **DESACTIVA** estos scripts si los tienes:
   - CompleteFix ❌
   - IAFixer ❌ 
   - PhotonLauncher ❌
   - DiagnosticoCompleto ❌
   - Cualquier script con "Multiplayer" en el nombre ❌

### **PASO 2: Añadir script simple**
1. Crear GameObject vacío llamado **"SimpleMultiplayer"**
2. Añadir el script **`SimpleFallGuysMultiplayer.cs`**
3. Configurar:
   - **Player Prefab Name**: "NetworkPlayer" (nombre del prefab en Resources)

### **PASO 3: Verificar prefab NetworkPlayer**
1. Ir a **Resources/NetworkPlayer**
2. Debe tener:
   - ✅ PhotonView component
   - ✅ LHS_MainPlayer script
   - ✅ Tag "Player"

### **PASO 4: Verificar cámara**
1. En la escena debe haber **MovimientoCamaraSimple**
2. Si no existe, añadir a la Main Camera

---

## 🎮 **CÓMO USAR**

### **Para probar:**
1. **Build** el juego
2. **Ejecutar build + editor** al mismo tiempo
3. En ambos:
   - Crear/unirse a sala ✅
   - Aparecerá GUI simple con estado
   - Si no spawnea automáticamente, clic en "🎮 SPAWN JUGADOR"

### **Resultado esperado:**
- ✅ Build: 1 jugador amarillo que puedo controlar
- ✅ Editor: 1 jugador amarillo que puedo controlar  
- ✅ Cada uno ve al otro moverse
- ✅ Cámara sigue a mi jugador
- ✅ IAs se mueven solas (las que ya están en el mapa)

---

## 🚨 **SI NO FUNCIONA**

### **Problema: No aparece mi jugador**
1. Verificar que NetworkPlayer está en Resources/
2. Clic manual en "🎮 SPAWN JUGADOR"
3. Verificar consola para errores

### **Problema: No veo al otro jugador**
1. Verificar que ambos están en la misma sala
2. Verificar que NetworkPlayer tiene PhotonView

### **Problema: Cámara no sigue**
1. Verificar que hay MovimientoCamaraSimple en la escena
2. Verificar que Main Camera tiene el script

---

## 🎯 **ESTO ES TODO**

**Solo 1 script**, **4 pasos**, **configuración mínima**.

Si esto no funciona, hay un problema más fundamental que necesitamos arreglar primero. 