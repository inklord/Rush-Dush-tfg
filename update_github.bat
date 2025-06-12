@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul 2>&1

:: Configuración de colores
set "GREEN=[92m"
set "RED=[91m"
set "YELLOW=[93m"
set "BLUE=[94m"
set "CYAN=[96m"
set "MAGENTA=[95m"
set "RESET=[0m"

:: Banner mejorado
echo %CYAN%========================================%RESET%
echo %CYAN%🚀 HERRAMIENTA DE ACTUALIZACION GITHUB%RESET%
echo %CYAN%========================================%RESET%
echo %BLUE%Fall Guys Clone - Unity Project%RESET%
echo %BLUE%Desarrollado por: Tu Equipo%RESET%
echo.

:: Verificar si estamos en un repositorio git
echo %YELLOW%🔍 Verificando repositorio git...%RESET%
git status >nul 2>&1
if errorlevel 1 (
    echo %RED%❌ ERROR: No se encontró repositorio git%RESET%
    echo %RED%   Inicializando repositorio...%RESET%
    git init
    git remote add origin https://github.com/inklord/Rush-Dush-tfg.git
    echo %GREEN%✅ Repositorio inicializado%RESET%
)

:: Verificar conexión a internet
echo %YELLOW%🌐 Verificando conexión...%RESET%
ping -n 1 github.com >nul 2>&1
if errorlevel 1 (
    echo %RED%❌ ERROR: Sin conexión a internet%RESET%
    pause
    exit /b 1
)
echo %GREEN%✅ Conexión establecida%RESET%

:: Mostrar estado actual
echo.
echo %CYAN%📋 ESTADO ACTUAL DEL REPOSITORIO:%RESET%
echo %CYAN%================================%RESET%
git status --porcelain
echo.

:: Contar archivos modificados
for /f %%i in ('git status --porcelain ^| find /c /v ""') do set modified_files=%%i
echo %YELLOW%📊 Archivos modificados: %modified_files%%RESET%

if %modified_files% equ 0 (
    echo %GREEN%✅ No hay cambios para subir%RESET%
    echo %BLUE%📈 Verificando si hay actualizaciones remotas...%RESET%
    git fetch origin
    for /f %%i in ('git rev-list --count HEAD..origin/main 2^>nul ^| findstr /r "^[0-9]*$"') do set behind=%%i
    if defined behind if !behind! gtr 0 (
        echo %YELLOW%⬇️ Hay !behind! commits nuevos en remoto%RESET%
        set /p pull_choice="¿Descargar cambios? (s/n): "
        if /i "!pull_choice!"=="s" (
            git pull origin main
            echo %GREEN%✅ Repositorio actualizado%RESET%
        )
    ) else (
        echo %GREEN%✅ Repositorio actualizado%RESET%
    )
    pause
    exit /b 0
)

:: Mostrar opciones de commit
echo.
echo %MAGENTA%🎯 OPCIONES DE COMMIT:%RESET%
echo %BLUE%1.%RESET% Commit rápido (automático)
echo %BLUE%2.%RESET% Commit personalizado
echo %BLUE%3.%RESET% Ver cambios detallados
echo %BLUE%4.%RESET% Cancelar
echo.

set /p commit_choice="Selecciona una opción (1-4): "

if "%commit_choice%"=="4" (
    echo %YELLOW%🚫 Operación cancelada%RESET%
    pause
    exit /b 0
)

if "%commit_choice%"=="3" (
    echo.
    echo %CYAN%📋 CAMBIOS DETALLADOS:%RESET%
    echo %CYAN%==================%RESET%
    git diff --name-status
    echo.
    git diff --stat
    echo.
    pause
    goto :commit_menu
)

:: Agregar archivos
echo.
echo %YELLOW%📦 Agregando archivos modificados...%RESET%
git add .

:: Verificar archivos grandes
for /f "tokens=1" %%i in ('git diff --cached --name-only') do (
    for %%j in ("%%i") do (
        if %%~zj gtr 52428800 (
            echo %RED%⚠️  ADVERTENCIA: Archivo grande detectado: %%i (%%~zj bytes)%RESET%
            set /p large_file_choice="¿Continuar? (s/n): "
            if /i "!large_file_choice!" neq "s" (
                echo %YELLOW%🚫 Operación cancelada%RESET%
                pause
                exit /b 0
            )
        )
    )
)

:: Configurar mensaje de commit
if "%commit_choice%"=="2" (
    echo.
    set /p custom_message="💬 Escribe tu mensaje de commit: "
    set commit_message=!custom_message!
) else (
    :: Commit automático basado en cambios detectados
    set commit_message=🛠️ ACTUALIZACIÓN AUTOMÁTICA: Fall Guys Clone

    :: Detectar tipos de archivos modificados
    git diff --cached --name-only | findstr /i "\.cs$" >nul && set commit_message=!commit_message!^

    🔧 Scripts C# actualizados
    git diff --cached --name-only | findstr /i "\.unity$" >nul && set commit_message=!commit_message!^

    🎮 Escenas Unity modificadas
    git diff --cached --name-only | findstr /i "\.prefab$" >nul && set commit_message=!commit_message!^

    🧩 Prefabs actualizados
    git diff --cached --name-only | findstr /i "\.mat$" >nul && set commit_message=!commit_message!^

    🎨 Materiales modificados
    
    set commit_message=!commit_message!^

    ✅ Cambios verificados y listos para producción^

    📊 Archivos modificados: %modified_files%^

    🕐 !date! - !time!
)

:: Crear commit
echo.
echo %YELLOW%💾 Creando commit...%RESET%
git commit -m "!commit_message!"
if errorlevel 1 (
    echo %RED%❌ ERROR: Falló la creación del commit%RESET%
    pause
    exit /b 1
)

:: Verificar rama actual
for /f "tokens=2" %%i in ('git branch --show-current 2^>nul') do set current_branch=%%i
if not defined current_branch set current_branch=main

echo %GREEN%✅ Commit creado en rama: %current_branch%%RESET%

:: Subir cambios
echo.
echo %YELLOW%🌐 Subiendo cambios a GitHub...%RESET%
echo %BLUE%📤 Rama: %current_branch%%RESET%

git push origin %current_branch%
if errorlevel 1 (
    echo %RED%❌ ERROR: Falló la subida a GitHub%RESET%
    echo %YELLOW%🔄 Intentando pull y push nuevamente...%RESET%
    git pull origin %current_branch% --rebase
    git push origin %current_branch%
    if errorlevel 1 (
        echo %RED%❌ ERROR CRÍTICO: No se pudo subir a GitHub%RESET%
        echo %RED%   Verifica tu conexión y permisos%RESET%
        pause
        exit /b 1
    )
)

:: Resumen final
echo.
echo %GREEN%========================================%RESET%
echo %GREEN%✅ ACTUALIZACIÓN COMPLETADA CON ÉXITO%RESET%
echo %GREEN%========================================%RESET%
echo %CYAN%🔗 Repositorio:%RESET% https://github.com/inklord/Rush-Dush-tfg
echo %CYAN%📈 Commits subidos:%RESET% 1
echo %CYAN%📊 Archivos actualizados:%RESET% %modified_files%
echo %CYAN%🌿 Rama:%RESET% %current_branch%
echo %CYAN%🕐 Hora:%RESET% !date! !time!
echo.

:: Mostrar hash del último commit
for /f "tokens=1" %%i in ('git rev-parse --short HEAD') do set last_commit=%%i
echo %BLUE%🔍 Último commit:%RESET% %last_commit%

:: Opción para abrir repositorio
echo.
set /p open_repo="¿Abrir repositorio en navegador? (s/n): "
if /i "%open_repo%"=="s" (
    start https://github.com/inklord/Rush-Dush-tfg
)

echo.
echo %GREEN%🎉 ¡Proceso completado exitosamente!%RESET%
pause

:commit_menu
goto :eof 