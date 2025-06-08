@echo off
echo 🔧 === FALL GUYS BUILD HELPER ===
echo.

:: Cerrar procesos que puedan bloquear el build
echo 🔄 Cerrando procesos Fall Guys...
taskkill /f /im "Fall Guys_Final.exe" 2>nul
taskkill /f /im "FallGuys.exe" 2>nul
taskkill /f /im "Unity.exe" 2>nul
echo.

:: Limpiar directorios problemáticos
echo 🧹 Limpiando directorios build...
if exist "C:\unity\builds\" (
    rmdir /s /q "C:\unity\builds\" 2>nul
    echo ✅ Directorio C:\unity\builds\ limpio
)

if exist "C:\UnityBuilds\" (
    rmdir /s /q "C:\UnityBuilds\" 2>nul
    echo ✅ Directorio C:\UnityBuilds\ limpio
)

:: Crear directorios limpios
echo 📁 Creando directorios limpios...
mkdir "C:\FallGuysBuilds" 2>nul
mkdir "C:\UnityBuilds" 2>nul
echo ✅ Directorios preparados

:: Dar permisos completos
echo 🔐 Configurando permisos...
icacls "C:\FallGuysBuilds" /grant Everyone:F /t 2>nul
icacls "C:\UnityBuilds" /grant Everyone:F /t 2>nul
echo ✅ Permisos configurados

echo.
echo 🎯 === DIRECTRICES PARA UNITY ===
echo 📁 Usar ruta: C:\FallGuysBuilds\FallGuys.exe
echo 📁 O alternativamente: C:\UnityBuilds\Game.exe
echo ⚠️ EVITAR espacios y caracteres especiales
echo 🔧 Ejecutar Unity como administrador si persiste
echo.

echo ✅ === LISTO PARA BUILD ===
pause 