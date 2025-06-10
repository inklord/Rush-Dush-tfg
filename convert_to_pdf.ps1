# Script para convertir HTML a PDF usando Microsoft Edge
param(
    [string]$HtmlFile = "DOCUMENTACION_FALL_GUYS_CLONE_STYLED.html",
    [string]$OutputPdf = "DOCUMENTACION_FALL_GUYS_CLONE.pdf"
)

Write-Host "🔄 Convirtiendo HTML a PDF..." -ForegroundColor Yellow
Write-Host "📄 Archivo origen: $HtmlFile" -ForegroundColor Green
Write-Host "📋 Archivo destino: $OutputPdf" -ForegroundColor Green

# Obtener la ruta completa del archivo HTML
$currentDir = Get-Location
$htmlFullPath = Join-Path $currentDir $HtmlFile
$pdfFullPath = Join-Path $currentDir $OutputPdf

# Verificar que el archivo HTML existe
if (-not (Test-Path $htmlFullPath)) {
    Write-Host "❌ Error: El archivo HTML no existe: $htmlFullPath" -ForegroundColor Red
    exit 1
}

# Convertir espacios a %20 para la URL file://
$fileUrl = "file:///" + $htmlFullPath.Replace('\', '/').Replace(' ', '%20')

Write-Host "🌐 URL del archivo: $fileUrl" -ForegroundColor Cyan

# Definir la ruta de Edge
$edgePath = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"

# Verificar que Edge existe
if (-not (Test-Path $edgePath)) {
    Write-Host "❌ Error: Microsoft Edge no encontrado en: $edgePath" -ForegroundColor Red
    exit 1
}

try {
    Write-Host "🚀 Ejecutando conversión con Microsoft Edge..." -ForegroundColor Yellow
    
    # Ejecutar Edge con parámetros de conversión a PDF
    & $edgePath --headless --disable-gpu --run-all-compositor-stages-before-draw --disable-extensions --disable-plugins --disable-software-rasterizer --print-to-pdf="$pdfFullPath" "$fileUrl"
    
    # Esperar un momento para que se complete la conversión
    Start-Sleep -Seconds 3
    
    # Verificar que el PDF se creó
    if (Test-Path $pdfFullPath) {
        $pdfSize = (Get-Item $pdfFullPath).Length
        Write-Host "✅ ¡PDF creado exitosamente!" -ForegroundColor Green
        Write-Host "📊 Tamaño del archivo: $([math]::Round($pdfSize/1024/1024, 2)) MB" -ForegroundColor Green
        Write-Host "📁 Ubicación: $pdfFullPath" -ForegroundColor Green
        
        # Abrir el PDF automáticamente
        Write-Host "🔍 Abriendo PDF..." -ForegroundColor Cyan
        Start-Process $pdfFullPath
        
    } else {
        Write-Host "❌ Error: El PDF no se pudo crear" -ForegroundColor Red
        Write-Host "💡 Sugerencia: Intenta ejecutar el script como administrador" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "❌ Error durante la conversión: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎮 Conversión completada - Fall Guys Clone Documentation" -ForegroundColor Magenta 