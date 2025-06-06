# 🎮 **Guía de Configuración: Sistema de Opciones Universal**

> **Sistema completo de opciones que funciona en todas las escenas del Fall Guys Clone**

---

## 📋 **Resumen del Sistema**

El sistema consta de **4 scripts principales** que trabajan juntos:

1. **`GlobalOptionsManager.cs`** - Gestor global que persiste entre escenas
2. **`OptionsMenu.cs`** - Menú de opciones con controles UI
3. **`AudioManager.cs`** - Gestión de audio y volumen
4. **`UniversalOptionsButton.cs`** - Botón que aparece automáticamente

---

## 🚀 **Setup Rápido (Método Automático)**

### **Opción A: Solo añadir UniversalOptionsButton**

**Para cada escena donde quieras opciones:**

1. **Crear GameObject vacío**
2. **Añadir script** `UniversalOptionsButton.cs`
3. **¡Listo!** - El sistema se configura automáticamente

```csharp
// El botón se creará automáticamente y funcionará con ESC
// Las configuraciones se sincronizarán entre todas las escenas
```

---

## 🛠️ **Setup Completo (Método Manual)**

### **Paso 1: Configurar GlobalOptionsManager**

#### **A) Crear el Objeto Global**
1. **Escena principal** (ej: WaitingUser o Login)
2. **GameObject → Create Empty** → Nombrar: "GlobalOptionsManager"
3. **Añadir script**: `GlobalOptionsManager.cs`

#### **B) Configurar AudioMixer**
1. **Assets → Create → Audio Mixer** → Nombrar: "MasterAudioMixer"
2. **En el Mixer crear grupos**:
   - Master
     - Music
     - SFX  
     - UI
3. **Exponer parámetros** (click derecho en Volume):
   - "MasterVolume"
   - "MusicVolume" 
   - "SFXVolume"
4. **Asignar** el AudioMixer al GlobalOptionsManager

#### **C) Configurar AudioManager**
1. **Mismo GameObject** o **nuevo GameObject**: "AudioManager"
2. **Añadir script**: `AudioManager.cs`
3. **Asignar** el mismo AudioMixer
4. **Añadir clips de música** en backgroundTracks (opcional)

### **Paso 2: Crear Prefab de OptionsMenu**

#### **A) Crear UI del Menú**
1. **Canvas → Panel** → Nombrar: "OptionsPanel"
2. **Dentro del Panel, añadir**:

```
OptionsPanel/
├── Background (Image - semitransparente)
├── Title (Text: "⚙️ OPCIONES")
├── VolumeSection/
│   ├── VolumeLabel (Text: "🔊 Volumen")
│   ├── VolumeSlider (Slider: 0-1)
│   └── VolumeText (Text: "Volumen: 75%")
├── DisplaySection/
│   ├── ResolutionLabel (Text: "🖥️ Resolución")
│   ├── ResolutionDropdown (Dropdown)
│   └── FullscreenToggle (Toggle: "Pantalla Completa")
└── ButtonsSection/
    ├── BackButton (Button: "🎮 Volver al Juego")
    ├── ApplyButton (Button: "💾 Aplicar")
    └── QuitButton (Button: "🚪 Salir del Juego")
```

#### **B) Configurar OptionsMenu Script**
1. **En OptionsPanel** añadir script: `OptionsMenu.cs`
2. **Asignar referencias**:
   - audioMixer → MasterAudioMixer
   - volumeSlider → VolumeSlider
   - volumeText → VolumeText
   - resolutionDropdown → ResolutionDropdown
   - fullscreenToggle → FullscreenToggle
   - backToGameButton → BackButton
   - quitGameButton → QuitButton
   - applyButton → ApplyButton
   - optionsPanel → OptionsPanel
   - gameUI → Canvas principal de la escena

#### **C) Crear Prefab**
1. **Drag OptionsPanel** a la carpeta Assets/Prefabs/
2. **Nombrar**: "OptionsMenuPrefab"
3. **Asignar** este prefab al GlobalOptionsManager

### **Paso 3: Configurar cada Escena**

#### **Método 1: Automático (Recomendado)**
1. **GameObject → Create Empty** → "OptionsButton"
2. **Añadir script**: `UniversalOptionsButton.cs`
3. **Configurar en Inspector**:
   - showOnlyInGame: ✅ (si solo quieres en escenas de juego)
   - excludeScenes: ["Login", "Intro"] (escenas donde NO mostrar)

#### **Método 2: Manual**
1. **Instantiate** el OptionsMenuPrefab en cada escena
2. **Conectar** referencias específicas de la escena
3. **Añadir botón** que llame a `OptionsMenu.ToggleOptionsMenu()`

---

## 🎯 **Configuración por Escena**

### **🌟 InGame (Escena Principal)**
```csharp
// Setup recomendado:
showOnlyInGame = false;  // Mostrar siempre
excludeScenes = [];      // No excluir nada
buttonPosition = (-120, -50);  // Esquina superior derecha
```

### **🔥 Hexagonia**
```csharp
// El sistema detecta automáticamente que es escena de juego
// Las configuraciones se mantienen entre InGame → Hexagonia
```

### **🏁 Carreras**
```csharp
// Mismo comportamiento que Hexagonia
// Volumen y resolución se mantienen consistentes
```

### **🚪 WaitingUser / Lobby**
```csharp
// Aquí se puede crear el GlobalOptionsManager
// El botón aparece para configurar antes del juego
```

### **📺 Login / Intro**
```csharp
// Usar excludeScenes para ocultar botón si no es necesario
excludeScenes = ["Login", "Intro"];
```

---

## 🎮 **Funcionalidades por Escena**

| Escena | Botón Visible | Pausa Juego | Configuraciones |
|--------|---------------|-------------|-----------------|
| **Login** | ❌ | ❌ | Solo audio/resolución |
| **WaitingUser** | ✅ | ❌ | Todas |
| **InGame** | ✅ | ✅ | Todas + Pausa |
| **Hexagonia** | ✅ | ✅ | Todas + Pausa |
| **Carreras** | ✅ | ✅ | Todas + Pausa |
| **Ending** | ✅ | ❌ | Solo audio/resolución |

---

## ⚙️ **Controles Universales**

### **🎹 Teclas**
- **ESC** - Abrir/cerrar opciones (en cualquier escena)
- **Botón ⚙️** - Mismo efecto que ESC

### **🎵 Audio**
- **Configuraciones persisten** entre escenas
- **AudioMixer global** controla todo el audio
- **PlayerPrefs** guarda configuraciones

### **🖥️ Display**
- **Resolución se aplica** inmediatamente
- **Pantalla completa** funciona en cualquier momento
- **Lista filtrada** de resoluciones (sin duplicados)

---

## 🔧 **Personalización Avanzada**

### **Cambiar Posición del Botón**
```csharp
// En UniversalOptionsButton:
buttonPosition = new Vector2(-200, -100);  // Más abajo y a la izquierda
buttonSize = new Vector2(150, 50);         // Botón más grande
buttonText = "🎛️ CONFIGURACIÓN";           // Texto personalizado
```

### **Filtrar Escenas**
```csharp
// Mostrar solo en escenas específicas:
showOnlyInGame = true;
string[] gameScenes = { "InGame", "Hexagonia", "Carrera", "Boss" };

// Excluir escenas específicas:
excludeScenes = { "Login", "Intro", "Credits", "MainMenu" };
```

### **Audio Personalizado**
```csharp
// En AudioManager añadir más canales:
public AudioMixerGroup voiceGroup;
public AudioMixerGroup ambientGroup;

// Exponer más parámetros en el mixer:
// "VoiceVolume", "AmbientVolume", "MasterPitch", etc.
```

---

## 🐛 **Troubleshooting**

### **❓ "El botón no aparece"**
- ✅ Verificar que `UniversalOptionsButton` está en la escena
- ✅ Revisar `excludeScenes` y `showOnlyInGame`
- ✅ Comprobar que hay un Canvas en la escena

### **❓ "Las configuraciones no se guardan"**
- ✅ Verificar que `GlobalOptionsManager` persiste (DontDestroyOnLoad)
- ✅ Comprobar PlayerPrefs en Registry/Local Storage
- ✅ Llamar a `PlayerPrefs.Save()` después de cambios

### **❓ "El audio no funciona"**
- ✅ Asignar AudioMixer al GlobalOptionsManager
- ✅ Verificar que AudioManager tiene el mismo AudioMixer
- ✅ Comprobar que los grupos "Music", "SFX", "UI" existen

### **❓ "ESC no abre el menú"**
- ✅ Verificar que GlobalOptionsManager está activo
- ✅ Comprobar que OptionsMenu se crea correctamente
- ✅ Revisar logs en Console para errores

---

## 📊 **Testing**

### **✅ Checklist de Pruebas**

**Por cada escena:**
- [ ] Botón aparece en posición correcta
- [ ] ESC abre/cierra menú
- [ ] Configuraciones se mantienen al cambiar escena
- [ ] Audio funciona correctamente
- [ ] Resolución se aplica inmediatamente
- [ ] "Volver al juego" funciona
- [ ] "Salir del juego" cierra aplicación

**Entre escenas:**
- [ ] Volumen se mantiene
- [ ] Resolución se mantiene
- [ ] Pantalla completa se mantiene
- [ ] GlobalOptionsManager persiste

---

## 🏆 **Resultado Final**

Con este setup tendrás:

✅ **Menú de opciones** en todas las escenas  
✅ **Configuraciones persistentes** entre escenas  
✅ **Controles de volumen** y resolución universales  
✅ **Botón automático** que se adapta a cada escena  
✅ **Sistema de pausa** en escenas de juego  
✅ **Configuración una sola vez** - funciona en todo el proyecto  

**¡Tu Fall Guys Clone tendrá un sistema de opciones profesional! 🎮** 