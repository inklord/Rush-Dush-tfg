# 🔧 Guía: SOLUCIONAR "Acceso Denegado" en Unity Builds

## 🚨 **Problema Común**
```
Building C:\unity\builds\1\Fall Guys_Final.exe failed with output:
Copying the file failed: Acceso denegado.
```

## ✅ **SOLUCIONES INMEDIATAS**

### **1. 🎯 USAR RUTAS CORRECTAS**
```
❌ PROBLEMÁTICAS:
- C:\unity\builds\1\Fall Guys_Final.exe (espacios + números)
- C:\Program Files\MiJuego\juego.exe (espacios + Program Files)
- D:\Mi Carpeta\juego.exe (espacios)

✅ RECOMENDADAS:
- C:\FallGuysBuilds\FallGuys.exe
- C:\UnityBuilds\Game.exe
- D:\Builds\FallGuys.exe
- C:\Games\FG.exe
```

### **2. 🔄 EJECUTAR BuildHelper.bat**
Antes de cada build, ejecutar:
```bash
.\BuildHelper.bat
```

Este script automáticamente:
- ✅ Cierra procesos bloqueantes
- ✅ Limpia directorios problemáticos
- ✅ Crea directorios limpios
- ✅ Configura permisos necesarios

### **3. 🛠️ CONFIGURACIÓN UNITY**
En **Build Settings**:
```
🎯 Target Platform: Windows x86_64
📁 Build Path: C:\FallGuysBuilds\FallGuys.exe
🔧 Configuration: Release
🚫 Development Build: OFF (opcional)
⚡ Compression Method: LZ4HC (recomendado)
```

---

## 🛡️ **PREVENCIÓN PERMANENTE**

### **📋 Checklist Pre-Build:**
- [ ] ❌ Cerrar juego si está ejecutándose
- [ ] 🔄 Ejecutar BuildHelper.bat
- [ ] 📁 Verificar ruta sin espacios
- [ ] 🔐 Unity ejecutado como administrador (si es necesario)

### **🚀 Unity como Administrador:**
1. **Click derecho** en icono de Unity
2. **"Ejecutar como administrador"**
3. Abrir proyecto normalmente

### **🧹 Limpiar Periódicamente:**
Ejecutar mensualmente:
```bash
# Limpiar cachés Unity
C:\Users\%USERNAME%\AppData\Local\Unity\cache\
C:\Users\%USERNAME%\AppData\LocalLow\Unity\

# Limpiar builds antiguos
C:\FallGuysBuilds\
C:\UnityBuilds\
```

---

## 🔍 **DIAGNÓSTICO AVANZADO**

### **Verificar Procesos Activos:**
```bash
tasklist | findstr "Fall"
tasklist | findstr "Unity"
```

### **Verificar Permisos Directorio:**
```bash
icacls "C:\FallGuysBuilds\"
```

### **Liberar Archivos Bloqueados:**
```bash
# Opción 1: Handle.exe (Sysinternals)
handle.exe "Fall Guys_Final.exe"

# Opción 2: Restart Explorer
taskkill /f /im explorer.exe && start explorer.exe
```

---

## 🚨 **SOLUCIONES ESCALADAS**

### **Nivel 1: Básico**
```bash
.\BuildHelper.bat
```

### **Nivel 2: Antivirus**
```
1. Excluir en Antivirus:
   - C:\FallGuysBuilds\
   - Proyecto Unity completo
   - Unity Editor installation

2. Desactivar Windows Defender temporalmente
3. Desactivar "Real-time protection"
```

### **Nivel 3: Reinicio de Servicios**
```bash
# Reiniciar Windows Explorer
taskkill /f /im explorer.exe
start explorer.exe

# Reiniciar servicio de archivos
net stop "Windows Search"
net start "Windows Search"
```

### **Nivel 4: Último Recurso**
```
1. Reiniciar Windows completamente
2. Cambiar directorio a otro disco (D:\, E:\)
3. Usar Unity Hub en lugar de Unity directo
4. Reinstalar Unity si es necesario
```

---

## 📊 **RUTAS RECOMENDADAS por TIPO**

### **🎮 Para Desarrollo:**
```
C:\FallGuysBuilds\Development\FallGuys.exe
```

### **🚀 Para Release:**
```
C:\FallGuysBuilds\Release\FallGuys.exe
```

### **🧪 Para Testing:**
```
C:\FallGuysBuilds\Test\FallGuys.exe
```

### **📦 Para Distribución:**
```
C:\FallGuysBuilds\Final\FallGuys.exe
```

---

## ⚡ **TIPS EXTRA**

### **🔧 Optimización Build:**
```
Player Settings > Publishing Settings:
- Scripting Backend: IL2CPP
- Api Compatibility Level: .NET Standard 2.1
- Target Architectures: x86_64 only
```

### **💾 Backup Automático:**
```bash
# Añadir al BuildHelper.bat para backup automático
if exist "C:\FallGuysBuilds\FallGuys.exe" (
    copy "C:\FallGuysBuilds\FallGuys.exe" "C:\FallGuysBuilds\Backup\FallGuys_%date%.exe"
)
```

### **🕒 Build Nocturno:**
```bash
# Para builds automáticos nocturnos
schtasks /create /sc daily /st 02:00 /tn "FallGuysBuild" /tr "C:\ruta\BuildHelper.bat"
```

---

## ✅ **RESULTADO FINAL**

Con estas configuraciones:
- **🚫 Nunca más "Acceso denegado"**
- **⚡ Builds más rápidos y estables**
- **🔄 Proceso automatizado**
- **🛡️ Prevención proactiva**

**¡Build sin problemas garantizado!** 🎯🚀 