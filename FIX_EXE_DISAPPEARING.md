# 🚨 SOLUCIÓN: .EXE DESAPARECE AL CERRAR

## 🔍 **DIAGNÓSTICO RÁPIDO**

### **Problema:** El archivo .exe desaparece de la carpeta cuando cierras la build
### **Causa más probable:** Windows Defender o antivirus eliminando el archivo

---

## 🛡️ **SOLUCIÓN 1: WINDOWS DEFENDER (MÁS COMÚN)**

### **Paso 1: Añadir excepción en Windows Defender**
```
1. Abrir Windows Security (Buscar "Windows Security")
2. Ir a "Virus & threat protection"
3. Click en "Manage settings" bajo "Virus & threat protection settings"
4. Scroll down y click "Add or remove exclusions"
5. Click "Add an exclusion" → "Folder"
6. Seleccionar la carpeta donde generas las builds
```

### **Paso 2: Excepción por tipo de archivo**
```
1. En la misma pantalla de exclusiones
2. Click "Add an exclusion" → "File extension"
3. Escribir: .exe
4. Click "Add"
```

### **Paso 3: Verificar historial de amenazas**
```
1. En Windows Security
2. Ir a "Virus & threat protection"
3. Click en "Protection history"
4. Buscar tu .exe en la lista de "amenazas" eliminadas
5. Si está ahí, restaurarlo y añadir excepción
```

---

## 📁 **SOLUCIÓN 2: CAMBIAR UBICACIÓN DE BUILD**

### **Crear carpeta dedicada para builds:**
```
1. Crear carpeta: C:\UnityBuilds\FallGuys\
2. En Unity: File → Build Settings
3. Click "Build" (no "Build and Run")
4. Seleccionar la nueva carpeta
5. Nombrar el .exe: FallGuys.exe
```

### **Configurar carpeta con permisos completos:**
```
1. Click derecho en C:\UnityBuilds\
2. Properties → Security → Advanced
3. Click "Change" junto al owner
4. Escribir tu usuario y click "Check Names"
5. Marcar "Full control"
```

---

## 🔧 **SOLUCIÓN 3: CONFIGURACIÓN DE UNITY**

### **Build Settings correctos:**
```
1. File → Build Settings
2. Platform: PC, Mac & Linux Standalone
3. Target Platform: Windows
4. Architecture: x86_64
5. ✅ Development Build: DESMARCAR (puede causar problemas con antivirus)
6. ✅ Script Debugging: DESMARCAR
7. ✅ Autoconnect Profiler: DESMARCAR
```

### **Player Settings seguros:**
```
1. Edit → Project Settings → Player
2. Publishing Settings:
   - ✅ Scripting Backend: IL2CPP (más estable)
   - ✅ Api Compatibility Level: .NET Framework
   - ✅ Target Device Family: Desktop
```

---

## 🔍 **SOLUCIÓN 4: OTROS ANTIVIRUS**

### **Para Avast, AVG, Norton, etc.:**
```
1. Abrir tu antivirus
2. Buscar "Exclusions" o "Exceptions"
3. Añadir:
   - Carpeta: C:\UnityBuilds\
   - Archivo: FallGuys.exe
   - Proceso: FallGuys.exe
```

### **Para McAfee:**
```
1. Abrir McAfee Security Center
2. Real-Time Scanning → Excluded Files
3. Add → Browse
4. Seleccionar carpeta de builds
```

---

## 🧪 **TESTING Y VERIFICACIÓN**

### **Test 1: Build simple**
```
1. Crear build en C:\UnityBuilds\FallGuys\
2. Ejecutar FallGuys.exe
3. Cerrar normalmente
4. ✅ Verificar que el .exe sigue ahí
```

### **Test 2: Monitorear en tiempo real**
```
1. Abrir Task Manager
2. Ir a "Details" tab
3. Buscar "FallGuys.exe" mientras está corriendo
4. Al cerrar, verificar que no aparecen procesos eliminándolo
```

### **Test 3: Event Viewer (Avanzado)**
```
1. Win+R → eventvwr.msc
2. Windows Logs → System
3. Buscar eventos de "File deletion" o "Windows Defender"
4. Verificar si hay logs de tu .exe siendo eliminado
```

---

## 🚨 **SOLUCIÓN DE EMERGENCIA**

### **Si nada funciona:**
```
1. Crear .bat file para preservar el .exe:

Crear archivo: "run_fallguys.bat"
Contenido:
@echo off
copy "FallGuys.exe" "FallGuys_backup.exe"
start "Fall Guys" "FallGuys.exe"
echo Esperando a que cierre el juego...
pause
copy "FallGuys_backup.exe" "FallGuys.exe"
echo .exe restaurado!
pause

2. Usar este .bat en lugar del .exe directamente
```

---

## ✅ **CONFIGURACIÓN FINAL RECOMENDADA**

### **Estructura de carpetas:**
```
C:\UnityBuilds\
  └── FallGuys\
      ├── FallGuys.exe          ← Tu juego
      ├── FallGuys_Data\        ← Datos de Unity
      ├── UnityCrashHandler64.exe
      └── UnityPlayer.dll
```

### **Exclusiones en antivirus:**
```
✅ Carpeta: C:\UnityBuilds\
✅ Archivo: *.exe en esa carpeta
✅ Proceso: FallGuys.exe
```

### **Build settings seguros:**
```
✅ Development Build: OFF
✅ Script Debugging: OFF
✅ Autoconnect Profiler: OFF
✅ IL2CPP backend
✅ .NET Framework compatibility
```

---

## 📞 **SI SIGUES TENIENDO PROBLEMAS**

### **Información adicional necesaria:**
```
1. ¿Qué antivirus usas?
2. ¿En qué carpeta generas el build?
3. ¿El .exe desaparece inmediatamente o después de un tiempo?
4. ¿Aparece algún mensaje de Windows/antivirus?
```

### **Diagnóstico avanzado:**
```
1. Process Monitor (ProcMon) de Microsoft
2. Monitorear en tiempo real qué proceso elimina el archivo
3. Buscar en Event Viewer logs de eliminación
```

---

**🎯 El 90% de las veces es Windows Defender. Añade la excepción y debería solucionarse inmediatamente.** 🛡️ 