# 💾 Guía: Sistema de Configuraciones PERMANENTES

## 🎯 **Objetivo**
Hacer que **TODAS** las configuraciones del menú de opciones se mantengan **PERMANENTEMENTE** durante todo el juego y entre sesiones.

---

## ✅ **QUÉ INCLUYE EL SISTEMA**

### **🔊 Audio Settings**
- ✅ Volumen maestro, música, SFX, UI
- ✅ Mute/unmute audio
- ✅ AudioMixer automático

### **🖥️ Display Settings**
- ✅ Resolución (incluyendo RefreshRate)
- ✅ Pantalla completa/ventana
- ✅ Calidad gráfica
- ✅ VSync on/off

### **🎮 Gameplay Settings**
- ✅ Sensibilidad del mouse
- ✅ Invertir mouse
- ✅ Campo de visión (FOV)

### **⌨️ Controls Settings**
- ✅ Teclas personalizables (Pausa, Opciones, Screenshot)

### **📱 UI Settings**
- ✅ Mostrar FPS
- ✅ Mostrar información debug
- ✅ Escala de UI

### **🌐 Game Preferences**
- ✅ Nombre del jugador
- ✅ Saltar intros
- ✅ Auto-guardado
- ✅ Intervalo de auto-guardado

---

## 🚀 **IMPLEMENTACIÓN PASO A PASO**

### **Paso 1: 📁 Crear PersistentSettingsManager**

#### **A) Escena Principal (Login o WaitingUser)**
1. **GameObject** → **Create Empty** → Nombrar: "PersistentSettingsManager"
2. **Add Component** → **PersistentSettingsManager**

#### **B) Configurar AudioMixer**
1. **Assets** → **Create** → **Audio Mixer** → Nombrar: "MasterAudioMixer"
2. **Crear grupos**:
   ```
   Master
   ├── Music
   ├── SFX
   └── UI
   ```
3. **Exponer parámetros** (click derecho en Volume de cada grupo):
   - MasterVolume
   - MusicVolume
   - SFXVolume
   - UIVolume
4. **Asignar** AudioMixer al PersistentSettingsManager

#### **C) Configuración Inicial**
```csharp
🎵 Audio Configuration:
- masterAudioMixer: MasterAudioMixer (arrastra desde Assets)

📊 Debug Settings:
- enableDebugLogs: ✅ true (para testing)
- showDebugUI: ✅ true (solo en Editor)
```

### **Paso 2: 🔗 Integrar con OptionsMenu Existente**

#### **A) OptionsMenu se integra automáticamente**
- ✅ Ya modificado para usar PersistentSettingsManager
- ✅ Fallback a sistema anterior si no encuentra PersistentSettingsManager
- ✅ Sincronización automática con GlobalOptionsManager

#### **B) Verificar Referencias**
En cada escena con OptionsMenu:
1. **Verificar que audioMixer** esté asignado al **MasterAudioMixer**
2. **Ejecutar** `AutoSetupAudioMixer()` si es necesario

### **Paso 3: 🎮 Configurar Sistemas Existentes**

#### **A) ResolutionManager + EarlyResolutionFix**
- ✅ Se integran automáticamente
- ✅ PersistentSettingsManager toma prioridad
- ✅ Sincronización bidireccional

#### **B) GlobalOptionsManager**
- ✅ Continúa funcionando como backup
- ✅ Se sincroniza automáticamente
- ✅ No hay conflictos

---

## 🔧 **CONFIGURACIÓN POR ESCENA**

### **🌟 Login/WaitingUser (Escena Inicial)**
```csharp
Crear objetos:
1. PersistentSettingsManager (principal)
2. EarlyResolutionFix (resolución inmediata)
3. ResolutionManager (backup resolución)

Configurar:
- AudioMixer asignado
- Debug logs enabled
- Auto-save enabled
```

### **🎮 InGame/Carrera/Hexagonia (Escenas de Juego)**
```csharp
OptionsMenu ya configurado:
- Se conecta automáticamente a PersistentSettingsManager
- Pausa/resume funcionando
- Configuraciones se mantienen

No requiere configuración adicional
```

### **🎬 Ending/FinalFracaso (Escenas Finales)**
```csharp
Configuraciones se mantienen:
- Audio persiste
- Resolución persiste
- Auto-save antes de cambiar escena
```

---

## 💾 **CÓMO FUNCIONA LA PERSISTENCIA**

### **🔄 Auto-Save Inteligente**
```csharp
- Cada cambio → MarkDirty()
- Auto-save cada 5 minutos (configurable)
- Guardado inmediato al aplicar configuraciones
- Guardado antes de cambiar escenas
- Guardado antes de cerrar juego
```

### **📂 PlayerPrefs Optimizado**
```csharp
Estructura de guardado:
Settings_MasterVolume: 0.75
Settings_ResolutionWidth: 1920
Settings_ResolutionHeight: 1080
Settings_Fullscreen: 0
Settings_QualityLevel: 3
Settings_PlayerName: "Player"
... (30+ configuraciones)
```

### **⚡ Aplicación Automática**
```csharp
Cuándo se aplican las configuraciones:
✅ Al iniciar cada escena
✅ Al abrir menú de opciones
✅ Al cambiar cualquier configuración
✅ Al hacer click en "Aplicar"
✅ Al cerrar el juego
```

---

## 🧪 **TESTING DEL SISTEMA**

### **Checklist de Verificación:**
- [ ] **Cambiar volumen** → Cerrar juego → Reabrir → Verificar que se mantiene
- [ ] **Cambiar resolución** → Cambiar escena → Verificar que se mantiene
- [ ] **Activar pantalla completa** → Reiniciar → Verificar que se mantiene
- [ ] **Configurar calidad gráfica** → Cambiar escena → Verificar que se mantiene
- [ ] **Cambiar nombre jugador** → Cerrar/abrir → Verificar que se mantiene

### **Debug en Editor:**
```csharp
En Play Mode:
- Ver logs en Console: "[PersistentSettings] ..."
- Ver debug UI en esquina superior izquierda
- Botones para recargar/guardar/aplicar configuraciones
```

### **Testing en Build:**
```csharp
1. Build del juego
2. Configurar opciones
3. Cerrar juego
4. Reabrir → Verificar que TODO se mantiene
```

---

## 🔄 **INTEGRACIÓN CON SISTEMAS EXISTENTES**

### **ResolutionManager + EarlyResolutionFix**
```csharp
Orden de prioridad:
1. PersistentSettingsManager (principal)
2. EarlyResolutionFix (fuerza inicial)
3. ResolutionManager (backup)

Sincronización automática:
- PersistentSettingsManager.SyncWithResolutionManager()
```

### **GlobalOptionsManager**
```csharp
Mantiene compatibilidad:
- Funciona como backup si falla PersistentSettingsManager
- Se sincroniza automáticamente
- No hay conflictos

Sincronización:
- PersistentSettingsManager.SyncWithGlobalOptionsManager()
```

### **OptionsMenu**
```csharp
Funcionalidad mejorada:
- Detecta automáticamente PersistentSettingsManager
- Fallback a sistema anterior
- UI se actualiza automáticamente
- Métodos adicionales: RefreshFromPersistentSettings()
```

---

## 🎯 **API PÚBLICA DEL SISTEMA**

### **Métodos Principales:**
```csharp
// Configurar audio
PersistentSettingsManager.Instance.SetMasterVolume(0.8f);
PersistentSettingsManager.Instance.SetMusicVolume(0.7f);
PersistentSettingsManager.Instance.SetMuteAudio(true);

// Configurar display
PersistentSettingsManager.Instance.SetResolution(1920, 1080, false);
PersistentSettingsManager.Instance.SetQualityLevel(2);
PersistentSettingsManager.Instance.SetVSync(true);

// Configurar gameplay
PersistentSettingsManager.Instance.SetMouseSensitivity(1.5f);
PersistentSettingsManager.Instance.SetFieldOfView(75f);

// Configurar UI
PersistentSettingsManager.Instance.SetShowFPS(true);
PersistentSettingsManager.Instance.SetPlayerName("MiNombre");

// Gestión manual
PersistentSettingsManager.Instance.SaveAllSettings();
PersistentSettingsManager.Instance.LoadAllSettings();
PersistentSettingsManager.Instance.ApplyAllSettings();
```

### **Integración desde OptionsMenu:**
```csharp
// Actualizar UI con configuraciones actuales
optionsMenu.RefreshFromPersistentSettings();

// Verificar si menú está abierto
bool isOpen = optionsMenu.IsMenuOpen();

// Auto-configurar AudioMixer
optionsMenu.AutoSetupAudioMixer();
```

---

## ✅ **RESULTADO FINAL GARANTIZADO**

Con este sistema:

### **🔒 Persistencia Total**
- **Todas las configuraciones** se guardan automáticamente
- **Se mantienen entre escenas** y sesiones de juego
- **Auto-save inteligente** para no perder cambios

### **⚡ Rendimiento Optimizado**
- **No duplica** aplicación de configuraciones
- **Caché interno** para evitar operaciones innecesarias
- **Sincronización eficiente** entre sistemas

### **🛡️ Compatibilidad Total**
- **Funciona con sistemas existentes** (ResolutionManager, GlobalOptionsManager)
- **Fallback automático** si algo falla
- **Sin conflictos** entre diferentes gestores

### **🎮 Experiencia de Usuario**
- **Configuraciones se recuerdan** siempre
- **Cambios inmediatos** en la UI
- **Feedback visual** cuando se guardan cambios
- **Debugging fácil** para desarrolladores

**¡CONFIGURACIONES PERMANENTES GARANTIZADAS!** 🎯✨ 