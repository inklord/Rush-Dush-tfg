# 🚩 **Guía de Implementación - Escena FinalFracaso**

> **Sistema de derrota dinámico para Fall Guys TFG**

---

## 🎯 **Qué se ha implementado**

### **✅ Scripts Modificados/Creados:**
- `SceneChange.cs` - ➕ Métodos `GoToEndingFailure()` y `FinalFracasoSceneChange()`
- `UIManager.cs` - ➕ Transición automática a FinalFracaso en caso de fallo
- `FinalFracasoManager.cs` - 🆕 Gestor completo para la escena de derrota
- `GlobalOptionsManager.cs` - ➕ Reconocimiento de escena FinalFracaso
- `UniversalOptionsHandler.cs` - ✅ Compatible con todas las escenas

### **🎬 Lógica Implementada:**
1. **Detección de Fallo**: `UIManager` verifica posición del jugador (≤560 = fallo)
2. **Navegación Dinámica**: Carga automáticamente `FinalFracaso` en lugar de `Ending`
3. **Animación de Fracaso**: Usa `RoundFailure.anim` existente
4. **Sistema de Opciones**: ESC funciona en escena FinalFracaso

---

## 🔧 **Configuración de la Escena FinalFracaso**

### **Paso 1: Crear la Escena**
```
1. Duplicar Assets/Scenes/Ending.unity
2. Renombrar a FinalFracaso.unity
3. Agregar a Build Settings
```

### **Paso 2: Configurar GameObjects**
```
FinalFracaso Scene:
├── 📱 Canvas
│   ├── 💀 FracasoPanel (GameObject)
│   │   ├── 🎬 Animator (RoundFailure.controller)
│   │   ├── 📝 FracasoText (Text UI)
│   │   ├── 📝 SubtitleText (Text UI)
│   │   ├── 🔄 RetryButton (Button)
│   │   └── 🚪 ExitButton (Button)
│   └── 🎵 AudioSource (Fracaso)
├── 🎮 FinalFracasoManager (GameObject vacío)
│   └── FinalFracasoManager.cs
└── ⚙️ OptionsHandler (GameObject vacío)
    └── UniversalOptionsHandler.cs
```

### **Paso 3: Asignar Referencias**
En `FinalFracasoManager`:
```
🎬 Elementos de Fracaso:
- fracasoPanel → FracasoPanel GameObject
- fracasoAnimator → Animator con RoundFailure.controller
- fracasoAudio → AudioSource component
- fracasoSound → Audio clip de fracaso

🎨 UI Elements:
- fracasoText → "¡Has sido eliminado!"
- subtitleText → "No llegaste a la meta a tiempo"
- retryButton → Botón Reintentar
- exitButton → Botón Salir al Lobby
```

---

## 🎮 **Flujo de Juego Actualizado**

### **🏃‍♂️ Durante el Juego:**
```
Timer se agota → UIManager verifica posición jugador
├── 🏆 Posición Z > 560 → SceneChange.GoToEndingSuccess() → Ending
└── 💀 Posición Z ≤ 560 → SceneChange.GoToEndingFailure() → FinalFracaso
```

### **💀 En FinalFracaso:**
```
1. ⏱️ Delay inicial (1s)
2. 📱 Mostrar panel de fracaso
3. 🔊 Reproducir sonido de fracaso
4. 🎬 Animación RoundFailure (3s)
5. 🎮 Habilitar botones (Reintentar/Salir)
6. ⏰ Auto-return al lobby (8s)
```

---

## 🎨 **Elementos Visuales Requeridos**

### **🎬 Animaciones:**
- ✅ `RoundFailure.anim` - **YA EXISTE**
- ✅ `RoundFailure.controller` - **YA EXISTE**

### **🔊 Audio:**
- 📢 `fracasoSound` - Sonido de derrota/eliminación
- 🎵 `backgroundMusic` - Música sombría para la escena

### **🎨 UI Sprites:**
- 💀 Imagen de fracaso/derrota (para el panel)
- 🔄 Icono de reintentar
- 🚪 Icono de salir

---

## ⚙️ **Configuración de Opciones**

### **En FinalFracaso Scene:**
```
GameObject: OptionsHandler
└── UniversalOptionsHandler.cs
    ├── enableInScene = ✅ true
    ├── excludeScenes = [] (vacío)
    ├── menuOpenSound = (opcional)
    └── menuCloseSound = (opcional)
```

**ESC abrirá/cerrará opciones normalmente**

---

## 🧪 **Testing**

### **🎯 Casos de Prueba:**
1. **Fallo por Tiempo**: No llegar a meta antes de que se acabe el timer
2. **Navegación**: Verificar que carga FinalFracaso (no Ending)
3. **Animación**: Confirmar que usa RoundFailure.anim
4. **Opciones**: ESC debe abrir menú de opciones
5. **Botones**: Reintentar y Salir funcionan correctamente
6. **Audio**: Sonido de fracaso se reproduce

### **🔍 Debug Points:**
```
UIManager.cs línea ~230: "RESULTADO: FRACASO"
UIManager.cs línea ~235: "Transicionando a escena de FRACASO"
FinalFracasoManager.cs línea ~40: "Inicializando escena FinalFracaso"
```

---

## 🚀 **Resultado Final**

### **✅ Funcionalidades Completas:**
- 🏆 **Victoria** → Ending (celebración)
- 💀 **Derrota** → FinalFracaso (fracaso)
- ⚙️ **Opciones** funcionan en ambas escenas
- 🎬 **Animaciones** apropiadas para cada resultado
- 🔄 **Navegación** fluida entre escenas
- 🎮 **Controles** consistentes (ESC/SPACE)

### **🎯 Comportamiento como Fall Guys:**
- **Detección automática** de éxito/fracaso
- **Escenas finales dinámicas** según resultado
- **Animaciones contextuales** (celebración vs fracaso)
- **Sistema de opciones universal**

---

## 🐛 **Troubleshooting**

### **❗ Problemas Comunes:**
1. **No carga FinalFracaso**: Verificar que esté en Build Settings
2. **Animación no funciona**: Asignar RoundFailure.controller al Animator
3. **ESC no abre opciones**: Añadir UniversalOptionsHandler a la escena
4. **Audio no suena**: Asignar AudioClip al fracasoAudio

### **🔧 Soluciones:**
- Revisar Console logs con prefijo "🏴‍☠️", "💀", "🎬"
- Verificar referencias en FinalFracasoManager
- Confirmar que UIManager detecta correctamente la posición del jugador

---

**¡Sistema de FinalFracaso implementado y listo para usar! 🎮** 