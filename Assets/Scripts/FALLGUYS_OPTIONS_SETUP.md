# 🎮 **Fall Guys Style - Setup de Opciones (Solo ESC)**

> **Sistema de opciones invisible como Fall Guys - Solo tecla ESC**

---

## 🚀 **Setup Súper Rápido**

### **🎯 Para cada escena (2 pasos):**

1. **Crear GameObject vacío** → Nombrar: "OptionsHandler"
2. **Añadir script**: `UniversalOptionsHandler.cs`

**¡Listo!** Ya funciona con **ESC** en esa escena.

---

## ⚙️ **Configuración por Escena**

### **🌟 InGame (Principal)**
```
enableInScene = ✅ true
excludeScenes = [] (vacío)
```

### **🔥 Hexagonia**
```
enableInScene = ✅ true  
excludeScenes = [] (vacío)
```

### **🏁 Carreras**
```
enableInScene = ✅ true
excludeScenes = [] (vacío)
```

### **🌊 WaitingUser/Lobby**
```
enableInScene = ✅ true
excludeScenes = [] (vacío)
```

### **🏆 Ending (Victoria)**
```
enableInScene = ✅ true
excludeScenes = [] (vacío)
```

### **💀 FinalFracaso (Derrota)**
```
enableInScene = ✅ true
excludeScenes = [] (vacío)
```

### **📺 Login/Intro (Sin opciones)**
```
enableInScene = ❌ false
O bien:
excludeScenes = ["Login", "Intro"]
```

---

## 🎮 **Comportamiento**

| Escena | ESC | Menú | Pausa |
|--------|-----|------|-------|
| **InGame** | ✅ | ✅ | ✅ |
| **Hexagonia** | ✅ | ✅ | ✅ |
| **Carreras** | ✅ | ✅ | ✅ |
| **WaitingUser** | ✅ | ✅ | ❌ |
| **Ending** | ✅ | ✅ | ❌ |
| **FinalFracaso** | ✅ | ✅ | ❌ |
| **Login** | ❌ | ❌ | ❌ |

---

## 🎬 **Sistema de Escenas Finales**

### **🏆 Victoria (Ending)**
- Se carga cuando el jugador **completa** la ronda exitosamente
- Muestra animación de **celebración**
- Opciones disponibles con **ESC**

### **💀 Derrota (FinalFracaso)**
- Se carga cuando el jugador **falla** la ronda
- Muestra animación de **fracaso** (`RoundFailure.anim`)
- Opciones disponibles con **ESC**
- Botones: **Reintentar** y **Salir al Lobby**

---

## 🔊 **Audio (Opcional)**

En `UniversalOptionsHandler` puedes asignar:
- **menuOpenSound** - Sonido al abrir opciones
- **menuCloseSound** - Sonido al cerrar opciones

---

## 🏆 **Resultado**

✅ **ESC abre/cierra opciones** en todas las escenas  
✅ **Sin botones visibles** (como Fall Guys original)  
✅ **Configuraciones persisten** entre escenas  
✅ **Sistema de pausa** automático en juego  
✅ **Escenas finales dinámicas** (éxito/fracaso)  
✅ **Setup en 30 segundos** por escena  

**¡Comportamiento idéntico a Fall Guys! 🎯** 