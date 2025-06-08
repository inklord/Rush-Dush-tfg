using UnityEngine;
using System.IO;

/// <summary>
/// 🔧 SOLUCIONADOR DE ERRORES DE COMPILACIÓN DE PHOTON
/// Este script ayuda a deshabilitar automáticamente archivos problemáticos de Photon
/// que requieren módulos de UI que no están disponibles en este proyecto.
/// </summary>
public class FixPhotonCompilationErrors : MonoBehaviour
{
    [Header("🔧 Configuración")]
    public bool autoFixOnStart = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixPhotonErrors();
        }
    }
    
    /// <summary>
    /// 🔧 Arreglar errores de compilación de Photon
    /// </summary>
    [ContextMenu("Fix Photon Compilation Errors")]
    public void FixPhotonErrors()
    {
        Debug.Log("🔧 === ARREGLANDO ERRORES DE COMPILACIÓN DE PHOTON ===");
        
        string photonPath = Path.Combine(Application.dataPath, "Photon", "PhotonUnityNetworking", "UtilityScripts");
        string disabledPath = Path.Combine(photonPath, "_DISABLED");
        
        // Crear carpeta _DISABLED si no existe
        if (!Directory.Exists(disabledPath))
        {
            Directory.CreateDirectory(disabledPath);
            Debug.Log("✅ Carpeta _DISABLED creada");
        }
        
        // Lista de archivos y carpetas problemáticos
        string[] problematicItems = {
            "UI",
            "Prototyping", 
            "Room/CountdownTimer.cs",
            "Debugging/PointedAtGameObjectInfo.cs"
        };
        
        int movedItems = 0;
        
        foreach (string item in problematicItems)
        {
            string sourcePath = Path.Combine(photonPath, item);
            string fileName = Path.GetFileName(item);
            string destPath = Path.Combine(disabledPath, fileName);
            
            try
            {
                if (Directory.Exists(sourcePath))
                {
                    if (!Directory.Exists(destPath))
                    {
                        Directory.Move(sourcePath, destPath);
                        Debug.Log($"📁 Movida carpeta: {item}");
                        movedItems++;
                    }
                }
                else if (File.Exists(sourcePath))
                {
                    if (!File.Exists(destPath))
                    {
                        File.Move(sourcePath, destPath);
                        Debug.Log($"📄 Movido archivo: {item}");
                        movedItems++;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ No se pudo mover {item}: {e.Message}");
            }
        }
        
        Debug.Log($"✅ === PROCESO COMPLETADO: {movedItems} elementos movidos ===");
        
        if (movedItems > 0)
        {
            Debug.Log("🔄 Refresca el proyecto (Ctrl+R) para que los cambios tomen efecto");
        }
        else
        {
            Debug.Log("✅ No se encontraron archivos problemáticos - Sistema limpio");
        }
    }
    
    /// <summary>
    /// 🔄 Restaurar archivos de Photon (si los necesitas)
    /// </summary>
    [ContextMenu("Restore Photon Files")]
    public void RestorePhotonFiles()
    {
        Debug.Log("🔄 === RESTAURANDO ARCHIVOS DE PHOTON ===");
        
        string photonPath = Path.Combine(Application.dataPath, "Photon", "PhotonUnityNetworking", "UtilityScripts");
        string disabledPath = Path.Combine(photonPath, "_DISABLED");
        
        if (!Directory.Exists(disabledPath))
        {
            Debug.LogWarning("⚠️ No hay archivos para restaurar");
            return;
        }
        
        string[] disabledItems = Directory.GetFileSystemEntries(disabledPath);
        int restoredItems = 0;
        
        foreach (string item in disabledItems)
        {
            string fileName = Path.GetFileName(item);
            string destPath = Path.Combine(photonPath, fileName);
            
            try
            {
                if (Directory.Exists(item))
                {
                    if (!Directory.Exists(destPath))
                    {
                        Directory.Move(item, destPath);
                        Debug.Log($"📁 Restaurada carpeta: {fileName}");
                        restoredItems++;
                    }
                }
                else if (File.Exists(item))
                {
                    if (!File.Exists(destPath))
                    {
                        File.Move(item, destPath);
                        Debug.Log($"📄 Restaurado archivo: {fileName}");
                        restoredItems++;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ No se pudo restaurar {fileName}: {e.Message}");
            }
        }
        
        Debug.Log($"✅ === RESTAURACIÓN COMPLETADA: {restoredItems} elementos restaurados ===");
        
        if (restoredItems > 0)
        {
            Debug.Log("⚠️ NOTA: Es posible que los errores de compilación regresen");
            Debug.Log("🔄 Refresca el proyecto (Ctrl+R) para que los cambios tomen efecto");
        }
    }
    
    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 200, 100), "🔧 PHOTON FIXER");
        
        if (GUI.Button(new Rect(20, 35, 180, 25), "Fix Compilation Errors"))
        {
            FixPhotonErrors();
        }
        
        if (GUI.Button(new Rect(20, 65, 180, 25), "Restore Files"))
        {
            RestorePhotonFiles();
        }
    }
} 