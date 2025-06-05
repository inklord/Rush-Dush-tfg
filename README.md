# 🎮 Fall Guys Clone - Trabajo de Fin de Grado

> **Clón del popular juego Fall Guys desarrollado en Unity con sistema de IA avanzado y multijugador**

---

## 📖 Descripción del Proyecto

Este proyecto es un **Trabajo de Fin de Grado** que recrea la mecánica principal del juego Fall Guys, enfocándose especialmente en el desarrollo de múltiples niveles con énfasis particular en **InGame** como nivel principal, complementado con **Hexagonia** y **Carreras**.

### 🎯 Objetivos Principales

- **Desarrollo de IA inteligente** para jugadores no humanos con comportamiento realista
- **Sistema multijugador** usando Photon Unity Networking (PUN2)
- **Mecánicas de juego** fieles al original con física y colisiones
- **Interfaz de usuario** intuitiva y responsive
- **Sistema de navegación** entre múltiples escenas de juego

---

## 🏗️ Estructura del Proyecto

### 🎮 **Niveles de Desarrollo (Por Orden de Prioridad)**

#### 1. 🌟 **InGame** - *Nivel Principal (Mayor Enfoque)*
**`Assets/Scenes/InGame/`**
- **Sistema de IA más avanzado** con `IAPlayer.cs`
  - Detección inteligente de peligros con raycast múltiple
  - Navegación NavMesh optimizada
  - Comportamiento adaptativo (caminar/correr según situación)
  - Sistema de caída rápida y aterrizaje
- **Mecánicas de juego complejas**
  - Eliminación por caída (Y < -50)
  - Sistema de supervivencia con múltiples amenazas
  - Física realista y detección de colisiones
- **Scripts principales:**
  - `GameManager.cs` - Gestión principal del juego
  - `IAPlayer.cs` - Comportamiento IA avanzado
  - `UISetup.cs` - Interfaz dinámica

#### 2. 🔥 **Hexagonia** - *Nivel Secundario*
**`Assets/Scenes/Hexagonia/`**
- **Mecánica de hexágonos destructibles**
  - Sistema de cambio de colores (Verde→Amarillo→Rojo)
  - Destrucción progresiva de plataformas
  - Detección de hexágonos seguros/peligrosos
- **IA especializada para Hexagonia**
  - Detección de colores de hexágonos
  - Búsqueda de posiciones seguras
  - Evitar hexágonos rojos/amarillos
- **Sistema de lava**
  - `LavaGameManager.cs` - Gestión de muerte por lava
  - `MuerteLava.cs` - Detección de contacto con lava
  - Diferenciación entre jugadores reales e IA

#### 3. 🏁 **Carreras** - *Nivel Terciario*
**`Assets/Scenes/Carrera/`**
- **Mecánicas de carrera**
  - Sistema de obstáculos dinámicos
  - Checkpoints y meta
  - Competencia entre jugadores e IAs
- **IA de carrera**
  - Navegación optimizada para velocidad
  - Evitar obstáculos móviles
  - Comportamiento competitivo

### 🛠️ **Sistemas de Soporte**

#### **Sistema de Navegación de Escenas**
**`Assets/Scripts/SceneChange.cs`**
```
Login → WaitingUser → Intro → InGame → Carrera → Hexagonia → Ending
```

#### **Sistema Multijugador (Photon PUN2)**
- **`Assets/Photon/`** - Integración completa de PUN2
- Scripts de red: `PlayerName.cs`, `MuerteLava.cs`
- Soporte para jugadores reales e IA simultáneamente

#### **Sistema de UI Avanzado**
- **Canvas dinámico** con contador de jugadores
- **Sistema de debug** con teclas H/K/M/F/G
- **Interfaz responsive** para diferentes resoluciones

---

## 📁 Arquitectura de Scripts

### **🧠 Scripts de IA (Principal)**
```
Assets/Scripts/
├── IAPlayer.cs                    # ⭐ Sistema IA principal
├── GameManager.cs                 # ⭐ Gestor de juego principal
└── LavaGameManager.cs             # ⭐ Gestor específico Hexagonia
```

### **🎮 Scripts de Juego por Nivel**
```
Assets/Scripts/
├── Scripts Nieves y Alejandro/    # 🔥 Scripts Hexagonia
│   ├── MuerteLava.cs             # Sistema muerte por lava
│   └── Reglas.cs                 # Reglas específicas nivel
├── ScriptsMarioEnrique/          # 🏁 Scripts Carreras
│   ├── Player.cs                 # Comportamiento jugador
│   └── DestruirAlContacto.cs     # Obstáculos destructivos
└── ScriptsLeandroYKevin/         # 🌟 Scripts InGame adicionales
```

### **🎨 Recursos por Nivel**
```
Assets/
├── Scenes/
│   ├── InGame/                   # 🌟 Escena principal
│   ├── Hexagonia/                # 🔥 Nivel hexágonos
│   └── Carrera/                  # 🏁 Nivel carreras
├── Prefabs/                      # Prefabs compartidos
├── Materials/                    # Materiales por equipos
└── Animation/                    # Animaciones UI y personajes
```

---

## 🚀 Características Técnicas Destacadas

### **🤖 Sistema de IA Avanzado (InGame)**
- **Detección de peligros multi-punto** con 5 raycast simultáneos
- **Navegación inteligente** usando Unity NavMesh
- **Estados dinámicos**: Idle, Walking, Running, Falling, Landing
- **Velocidades adaptativas**: 3.5 (caminar) → 8.0 (correr)
- **Sistema de caída rápida** con gravedad 3x aumentada

### **🎮 Mecánicas de Juego Implementadas**
- **Eliminación por caída** (Y < -50) para IAs
- **Sistema de lava** para jugadores reales
- **Hexágonos destructibles** con cambio de colores
- **Física realista** con Rigidbody optimizado
- **Detección de superficies seguras** por tags

### **🌐 Sistema Multijugador**
- **Photon PUN2** completamente integrado
- **Híbrido jugadores reales + IA** en misma partida
- **Sincronización de estados** entre clientes
- **Sistema de rooms** y matchmaking

---

## 🎯 Niveles de Implementación

| Nivel | Estado | Enfoque | Características Principales |
|-------|--------|---------|----------------------------|
| **🌟 InGame** | ✅ **Completo** | **Principal** | IA avanzada, mecánicas complejas, sistema completo |
| **🔥 Hexagonia** | ✅ **Completo** | **Secundario** | Hexágonos destructibles, sistema lava, IA especializada |
| **🏁 Carreras** | ✅ **Funcional** | **Terciario** | Obstáculos, checkpoints, IA competitiva |
| Login/UI | ✅ **Completo** | **Soporte** | Navegación, multijugador, interfaces |

---

## 🛠️ Tecnologías Utilizadas

- **Unity 2022.3 LTS** - Motor de juego principal
- **C#** - Lenguaje de programación
- **Photon PUN2** - Sistema multijugador
- **NavMesh** - Sistema de navegación IA
- **Physics System** - Motor de física Unity
- **TextMeshPro** - Interfaz de usuario avanzada
- **Cinemachine** - Sistema de cámaras

## 🎮 Controles de Debug

| Tecla | Función | Nivel |
|-------|---------|-------|
| **L** | Eliminar IA aleatoria | InGame |
| **P** | Eliminar jugador | Global |
| **D** | Mostrar info debug | Global |
| **G** | Forzar fin de juego | Global |
| **V** | Simular muerte lava | Hexagonia |
| **I** | Estado sistemas | Global |

---

## 🏆 Logros Técnicos

### **⭐ Innovaciones en IA**
- **Comportamiento emergente** - Las IAs desarrollan estrategias no programadas
- **Detección predictiva** - Anticipan peligros antes de llegar
- **Navegación híbrida** - Combinan NavMesh con física para movimiento natural

### **🔥 Sistemas Complejos**
- **Gestión dual de jugadores** - Reales e IA con diferentes reglas
- **Sincronización de red** - Estados consistentes en multijugador
- **Optimización de rendimiento** - 50+ IAs simultáneas sin lag

### **🌟 Arquitectura Escalable**
- **Sistema modular** - Fácil añadir nuevos niveles
- **Scripts reutilizables** - Componentes compartidos entre niveles
- **Configuración flexible** - Parámetros ajustables sin recompilar

---

## 👥 Autor

<<<<<<< HEAD
**[Reemplaza con tu nombre completo]**  
*Estudiante de [Tu Carrera - ej: Ingeniería Informática]*  
*[Tu Universidad - ej: Universidad Complutense de Madrid]*  

📧 [tu-email@universidad.edu]  
🔗 [LinkedIn](https://linkedin.com/in/tu-perfil)  
💼 [GitHub](https://github.com/inklord)
=======
**Mario Cascado Nieto**  
*Estudiante de DAM*  
*Colegio Miralmonte*  

📧 mariocn881@gmail.com  
🔗 [LinkedIn]www.linkedin.com/in/mario-cascado-nieto-190a48283
>>>>>>> 4e4321653c5802f5b764099f3cb90d929e7bced2

---

## 📚 Documentación Adicional

- **Memoria del TFG**: [Enlace al documento PDF]
- **Video Demo**: [Enlace al video gameplay]
- **Presentación**: [Enlace a slides defensa]

---

## 🤝 Agradecimientos

<<<<<<< HEAD
Proyecto desarrollado como **Trabajo de Fin de Grado** con especial énfasis en el desarrollo del nivel **InGame** como demostración principal de las capacidades técnicas implementadas.

---

**⭐ Si este proyecto te ha resultado útil, no olvides darle una estrella ⭐** 
=======
- **Unity Technologies** - Por el motor de juego
- **Photon Engine** - Por el sistema de networking
- **Mediatonic** - Por la inspiración del juego original Fall Guys
- **Juan Antonio Arnau** - Por la supervisión académica
- **Comunidad Unity** - Por recursos y documentación

>>>>>>> 4e4321653c5802f5b764099f3cb90d929e7bced2
