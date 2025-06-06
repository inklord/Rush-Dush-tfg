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

### **🚪 WaitingUser/Lobby**
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
| **Login** | ❌ | ❌ | ❌ |

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
✅ **Setup en 30 segundos** por escena  

**¡Comportamiento idéntico a Fall Guys! 🎯** 