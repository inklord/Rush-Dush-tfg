@echo off
echo 🔧 === ARREGLANDO ERRORES DE COMPILACION DE PHOTON ===
echo.

REM Eliminar carpetas problemáticas
if exist "Assets\Photon\PhotonUnityNetworking\UtilityScripts\UI" (
    echo 📁 Eliminando carpeta UI...
    rmdir /s /q "Assets\Photon\PhotonUnityNetworking\UtilityScripts\UI"
)

if exist "Assets\Photon\PhotonUnityNetworking\UtilityScripts\Prototyping" (
    echo 📁 Eliminando carpeta Prototyping...
    rmdir /s /q "Assets\Photon\PhotonUnityNetworking\UtilityScripts\Prototyping"
)

REM Eliminar archivos problemáticos específicos
if exist "Assets\Photon\PhotonUnityNetworking\UtilityScripts\Debugging\PointedAtGameObjectInfo.cs" (
    echo 📄 Eliminando PointedAtGameObjectInfo.cs...
    del "Assets\Photon\PhotonUnityNetworking\UtilityScripts\Debugging\PointedAtGameObjectInfo.cs"
)

if exist "Assets\Photon\PhotonUnityNetworking\UtilityScripts\Room\CountdownTimer.cs" (
    echo 📄 Eliminando CountdownTimer.cs...
    del "Assets\Photon\PhotonUnityNetworking\UtilityScripts\Room\CountdownTimer.cs"
)

REM Eliminar archivos .meta huérfanos
if exist "Assets\Photon\PhotonUnityNetworking\UtilityScripts\UI.meta" (
    echo 🗑️ Eliminando UI.meta huérfano...
    del "Assets\Photon\PhotonUnityNetworking\UtilityScripts\UI.meta"
)

if exist "Assets\Photon\PhotonUnityNetworking\UtilityScripts\Prototyping.meta" (
    echo 🗑️ Eliminando Prototyping.meta huérfano...
    del "Assets\Photon\PhotonUnityNetworking\UtilityScripts\Prototyping.meta"
)

echo.
echo ✅ === PROCESO COMPLETADO ===
echo 🔄 Refresca Unity (Ctrl+R) para que los cambios tomen efecto
echo.
pause 