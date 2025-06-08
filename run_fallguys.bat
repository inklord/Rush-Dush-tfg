@echo off
echo 🎮 Fall Guys - Preservador de .exe
echo ================================

:: Crear backup del exe
if exist "FallGuys.exe" (
    copy "FallGuys.exe" "FallGuys_backup.exe" >nul
    echo ✅ Backup creado
) else (
    echo ❌ No se encontró FallGuys.exe
    pause
    exit
)

:: Ejecutar el juego
echo 🚀 Iniciando Fall Guys...
start "Fall Guys" "FallGuys.exe"

:: Esperar a que el juego termine
echo ⏳ Esperando a que cierre el juego...
:wait
tasklist /FI "IMAGENAME eq FallGuys.exe" 2>NUL | find /I /N "FallGuys.exe">NUL
if "%ERRORLEVEL%"=="0" (
    timeout /t 2 >nul
    goto wait
)

:: Restaurar el exe si desapareció
if not exist "FallGuys.exe" (
    if exist "FallGuys_backup.exe" (
        copy "FallGuys_backup.exe" "FallGuys.exe" >nul
        echo ✅ .exe restaurado automáticamente!
    ) else (
        echo ❌ No se pudo restaurar el .exe
    )
) else (
    echo ✅ El .exe no desapareció esta vez!
)

echo 🎉 Terminado. Presiona cualquier tecla para salir.
pause >nul 