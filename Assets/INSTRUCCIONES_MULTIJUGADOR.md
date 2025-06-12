# 🎯 GUÍA COMPLETA: CONFIGURACIÓN MULTIJUGADOR PARA TODAS LAS ESCENAS

## 📋 Resumen

Este documento explica cómo aplicar la misma configuración multijugador que funciona en **InGame** a las escenas **Carrera** y **Hexagonia**, **con lógica de victoria diferencial** para cada tipo de escena.

## 🏁 LÓGICA DE VICTORIA POR TIPO DE ESCENA

### 🔵 Hexagonia (Nivel Final)
- **Condición**: Último jugador en pie O supervivencia de 3 minutos
- **Timer**: Cuenta regresiva visible de 180 segundos  
- **Victoria**: Automática por GameManager
- **Logs**: `🔵 GameManager [HEXAGONIA]: ¡{jugador} es el último en pie!`

### 🏁 Carreras (InGame/Carrera)
- **Condición**: Llegada a la línea de meta (trigger)
- **Timer**: Para tiempo límite, NO para victoria automática
- **Victoria**: Por `FinishLineTrigger` (NO por GameManager)
- **Logs**: `🏁 GameManager [CARRERA]: NO declarando victoria automática`
- **⚠️ Importante**: GameManager **NO declara victoria automáticamente** en carreras

### 🎮 Otros Niveles
- **Condición**: Último jugador en pie
- **Victoria**: Automática por GameManager
- **Logs**: `🎮 GameManager [ESTÁNDAR]: ¡{jugador} es el último en pie!`

## 🚀 MÉTODOS DE CONFIGURACIÓN

### 🔍 DETECCIÓN AUTOMÁTICA DE MODO

El sistema ahora **detecta automáticamente** si estás en:
- **🎮 SINGLEPLAYER:** Configuración mínima (solo cámara y GameManager)
- **🌐 MULTIJUGADOR:** Configuración completa (spawners, red, etc.)

### Método 1: 🛠️ Herramienta de Editor (RECOMENDADO)

La forma más fácil y rápida:

1. **Abrir la herramienta:**
   - En Unity, ir a `Tools > Configure Multiplayer Scenes`

2. **Configurar escenas:**
   - Clic en "🏁 Configurar Escena CARRERA"
   - Clic en "⬡ Configurar Escena HEXAGONIA" 
   - O clic en "🌐 Configurar AMBAS Escenas"

3. **¡Listo!** Las escenas quedarán configuradas automáticamente.

### Método 2: 📝 Manual - Agregar Script a las Escenas

Si prefieres control manual:

1. **Abrir la escena Carrera:**
   - File > Open Scene > `Assets/Scenes/Carrera.unity`

2. **Crear GameObject vacío:**
   - Click derecho en Hierarchy > Create Empty
   - Nombrar: "MultiplayerSetup"

3. **Agregar script:**
   - Con el GameObject seleccionado
   - Add Component > `UniversalMultiplayerSetup`

4. **Configurar script:**
   - ✅ Auto Setup On Start
   - ✅ Setup Camera Automatically  
   - ✅ Show Debug Info
   - Player Prefab Name: "NetworkPlayer"

5. **Repetir para Hexagonia:**
   - Abrir `Assets/Scenes/Hexagonia.unity`
   - Repetir pasos 2-4
   - **Importante:** En Hexagonia, marcar ✅ "Is Hexagonia Level"

6. **Guardar escenas:** Ctrl+S

### Método 3: 🔧 Automático - Sistema Runtime

El sistema también funciona automáticamente con **detección inteligente**:

1. **Script AutoSceneMultiplayerFixer:**
   - Se ejecuta automáticamente al cargar cualquier escena
   - **🔍 Detecta automáticamente el modo:** Singleplayer vs Multijugador
   - **🎮 Singleplayer:** Solo configura cámara y GameManager
   - **🌐 Multijugador:** Configuración completa de red

2. **No requiere intervención manual**
   - Simplemente juega el juego
   - El sistema se configura solo según el modo detectado

## 📊 COMPONENTES QUE SE CONFIGURAN AUTOMÁTICAMENTE

### 🎯 MasterSpawnController
- **Función:** Coordina el spawn de jugadores
- **Previene:** Duplicación de jugadores
- **Configuración:** PlayerPrefab = "NetworkPlayer"

### 📷 Sistema de Cámara
- **MovimientoCamaraSimple:** Seguimiento suave del jugador
- **LHS_Camera:** Sistema de respaldo
- **Configuración automática** al detectar jugador local

### 🎮 GameManager
- **Carrera:** Configuración estándar de juego
- **Hexagonia:** Timer de 3 minutos, modo eliminación
- **Detecta automáticamente** el tipo de escena

### 🧹 Limpieza de Spawners
- **Desactiva spawners problemáticos:**
  - SimplePlayerSpawner
  - PhotonLauncher
  - WaitingUserSpawner
  - GuaranteedPlayerSpawner

### 🎯 Spawner Principal
- **PhotonSpawnController o SimpleFallGuysMultiplayer**
- **Configuración automática** de spawn points
- **Integración con MasterSpawnController**

## 🔍 VERIFICACIÓN QUE TODO FUNCIONA

### En Runtime:
1. **Debug GUI visible** en esquina superior derecha
2. **Modo detectado** claramente mostrado (🎮 SINGLEPLAYER o 🌐 MULTIJUGADOR)
3. **Estado "✅ Completado"** en el setup
4. **Jugador spawneado** correctamente
5. **Cámara siguiendo** al jugador local
6. **Sin duplicados** de jugadores

### En Logs - Modo Singleplayer:
```
🎮 Modo SINGLEPLAYER detectado en 'Carrera' - Configuración mínima
📷 Sistema de cámara configurado
🎮 GameManager creado (Hexagonia: False)
🧹 [SP] X spawners de red desactivados para singleplayer
✅ Configuración completada para 'Carrera' (Modo: Singleplayer)
```

### En Logs - Modo Multijugador:
```
🌐 Modo MULTIJUGADOR detectado en 'Carrera' - Configuración completa
🎯 MasterSpawnController creado automáticamente
📷 Sistema de cámara configurado
🧹 X spawners problemáticos desactivados
✅ Configuración completada para 'Carrera' (Modo: Multijugador)
```

## 🐛 SOLUCIÓN DE PROBLEMAS

### ❌ No aparece jugador:
1. **Verificar NetworkPlayer prefab** existe en Resources/
2. **Comprobar conexión** a Photon
3. **Verificar que estás en una sala**
4. **Usar botón "Reconfigurar"** en Debug GUI

### ❌ Múltiples jugadores:
1. **MasterSpawnController** automáticamente los limpia
2. **Esperar 10 segundos** para limpieza automática
3. **Usar botón "Clean Duplicates"** en Debug GUI

### ❌ Cámara no sigue:
1. **Verificar Camera.main** existe
2. **Comprobar MovimientoCamaraSimple** está agregado
3. **Usar botón "Fix Camera"** en Debug GUI

### ❌ Hexagonia sin timer:
1. **Verificar GameManager.isHexagoniaLevel = true**
2. **Reconfigurar con la herramienta de editor**
3. **Verificar nombre de escena** contiene "Hexagon"

## 🎮 ESCENAS COMPATIBLES

### ✅ Configuradas automáticamente:
- **InGame** (ya funciona)
- **Carrera** (configurar con herramienta)
- **Hexagonia** (configurar con herramienta)

### ❌ No requieren configuración:
- **Lobby** (solo menús)
- **Login** (solo menús)
- **Ending** (resultados)

## 🔧 CONFIGURACIÓN AVANZADA

### Spawn Points Personalizados:
1. **Crear objetos vacíos** en posiciones deseadas
2. **Asignar tag "Respawn"**
3. **O configurar manualmente** en UniversalMultiplayerSetup.manualSpawnPoints

### Debug Personalizado:
- **showDebugInfo:** Logs detallados
- **showDebugGUI:** Interfaz visual en runtime
- **Desactivar en build final**

### Configuración por Escena:
```csharp
// En Carrera
setup.isHexagoniaLevel = false;

// En Hexagonia  
setup.isHexagoniaLevel = true;
setup.hexagoniaTimerDuration = 180f;
```

## 📝 RESUMEN RÁPIDO

1. **Usar herramienta de editor** (Tools > Configure Multiplayer Scenes)
2. **Configurar ambas escenas** con un click
3. **Verificar que funciona** con Debug GUI
4. **¡Listo para multijugador!**

---

## 🏁 CONFIGURACIÓN DE LÍNEA DE META (Para Carreras)

Para que las carreras funcionen correctamente, necesitas un **trigger de línea de meta**:

### 📝 Pasos para agregar FinishLineTrigger:

1. **Crear GameObject** en la línea de meta:
   - Click derecho en Hierarchy > Create Empty
   - Nombrar: "FinishLine"
   - Posicionar al final del recorrido

2. **Agregar Collider:**
   - Add Component > Box Collider
   - ✅ Marcar "Is Trigger"
   - Ajustar size para cubrir toda la meta

3. **Agregar Script:**
   - Add Component > `FinishLineTrigger`
   - Next Scene: "Hexagonia" (o la siguiente escena)
   - Victory Delay: 3 segundos
   - ✅ Show Debug Logs

### ⚡ Funciones del FinishLineTrigger:
- **Detecta** cuando el jugador llega a la meta
- **Notifica** al GameManager que la carrera terminó
- **Maneja** la transición a la siguiente escena
- **Evita** que se declare victoria prematuramente

## 🎯 RESULTADO FINAL

Después de la configuración:

- ✅ **Carrera y Hexagonia** funcionan igual que InGame
- ✅ **Spawn correcto** de jugadores en multijugador  
- ✅ **Cámara automática** para cada jugador
- ✅ **Lógica de victoria diferencial** por tipo de escena
- ✅ **GameManager NO interfiere** con carreras prematuramente
- ✅ **Sin duplicados** de jugadores
- ✅ **GameManager apropiado** para cada escena
- ✅ **Sistema robusto** que funciona automáticamente

**¡Las tres escenas de juego ahora tienen la misma funcionalidad multijugador!** 🎉 