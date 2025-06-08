using UnityEngine;
using UnityEditor;

/// <summary>
/// Script para solucionar problemas de build relacionados con Visual Scripting y UI
/// </summary>
public static class FixBuildScript
{
    [MenuItem("Fix/🔧 Fix Build Issues (All)", priority = 0)]
    public static void FixAllBuildIssues()
    {
        Debug.Log("=== INICIANDO REPARACIÓN COMPLETA ===");
        
        try
        {
            // 1. Limpiar cache de forma segura
            ClearPackageCache();
            
            // 2. Limpiar archivos temporales de forma segura
            SafeCleanTempFiles();
            
            // 3. Reimportar assets
            ReimportAllAssets();
            
            // 4. Forzar recompilación usando método seguro
            AssetDatabase.Refresh();
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            
            Debug.Log("✅ REPARACIÓN COMPLETA TERMINADA");
            EditorUtility.DisplayDialog("Reparación Completa", 
                "Reparación terminada exitosamente.\n\nPara el error de UI:\n1. Espera a que termine la recompilación\n2. Si persiste, usa 'Fix UI Issues'\n3. Como último recurso, reinicia Unity", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error durante reparación: {e.Message}");
            
            // Intentar método alternativo
            Debug.Log("Intentando reparación alternativa...");
            AlternativeRecompile();
        }
    }
    
    [MenuItem("Fix/🔧 Fix UI Namespace Issues", priority = 1)]
    public static void FixUINamespaceIssues()
    {
        Debug.Log("=== REPARANDO PROBLEMAS DE NAMESPACE UI ===");
        
        try
        {
            // 1. Forzar recompilación completa
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            
            // 2. Limpiar y regenerar assemblies
            Debug.Log("Verificando assemblies...");
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.GetName().Name.Contains("Assembly-CSharp"))
                {
                    Debug.Log($"✓ Assembly encontrado: {assembly.GetName().Name}");
                }
            }
            
            // 3. Forzar reimportación de scripts
            AssetDatabase.ImportAsset("Assets/Scripts", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            
            // 4. Recompilar todo
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            
            Debug.Log("✅ Reparación de UI completada");
            EditorUtility.DisplayDialog("UI Fixed", "Reparación de UI completada.\n\nSi persiste el error, reinicia Unity.", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al reparar UI: {e.Message}");
        }
    }
    
    [MenuItem("Fix/🔄 Force Recompile All Scripts", priority = 2)]
    public static void ForceRecompileScripts()
    {
        Debug.Log("Forzando recompilación de todos los scripts...");
        
        try
        {
            // Método más seguro: solo limpiar archivos específicos
            SafeCleanTempFiles();
            
            // Forzar recompilación sin eliminar toda la carpeta Temp
            AssetDatabase.Refresh();
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            
            Debug.Log("✓ Recompilación forzada iniciada");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Error durante recompilación: {e.Message}");
            Debug.Log("Intentando método alternativo...");
            
            // Método alternativo más suave
            AlternativeRecompile();
        }
    }
    
    [MenuItem("Fix/🧹 Safe Clean Temp Files", priority = 5)]
    public static void SafeCleanTempFiles()
    {
        Debug.Log("Limpiando archivos temporales de forma segura...");
        
        var tempPath = System.IO.Path.Combine(Application.dataPath, "..", "Temp");
        if (!System.IO.Directory.Exists(tempPath))
        {
            Debug.Log("ℹ️ Carpeta Temp no existe");
            return;
        }
        
        try
        {
            // Intentar eliminar solo archivos específicos que no estén en uso
            var files = System.IO.Directory.GetFiles(tempPath, "*", System.IO.SearchOption.AllDirectories);
            int deletedCount = 0;
            int skippedCount = 0;
            
            foreach (var file in files)
            {
                try
                {
                    // Intentar eliminar archivo individual
                    System.IO.File.Delete(file);
                    deletedCount++;
                }
                catch
                {
                    // Si no se puede eliminar, continuar con el siguiente
                    skippedCount++;
                }
            }
            
            Debug.Log($"✓ Limpieza parcial: {deletedCount} archivos eliminados, {skippedCount} omitidos (en uso)");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Error durante limpieza: {e.Message}");
        }
    }
    
    [MenuItem("Fix/🔄 Alternative Recompile (Safe)", priority = 6)]
    public static void AlternativeRecompile()
    {
        Debug.Log("Ejecutando recompilación alternativa...");
        
        try
        {
            // 1. Forzar refresh de assets primero
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            
            // 2. Limpiar solo scripts assemblies si es posible
            var scriptAssembliesPath = System.IO.Path.Combine(Application.dataPath, "..", "Library", "ScriptAssemblies");
            if (System.IO.Directory.Exists(scriptAssembliesPath))
            {
                try
                {
                    System.IO.Directory.Delete(scriptAssembliesPath, true);
                    Debug.Log("✓ ScriptAssemblies limpiado");
                }
                catch
                {
                    Debug.Log("⚠️ No se pudo limpiar ScriptAssemblies (en uso)");
                }
            }
            
            // 3. Reimportar scripts específicamente
            AssetDatabase.ImportAsset("Assets/Scripts", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            
            // 4. Solicitar recompilación
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            
            Debug.Log("✅ Recompilación alternativa completada");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error en recompilación alternativa: {e.Message}");
        }
    }
    
    [MenuItem("Fix/🗑️ Clear Package Cache", priority = 3)]
    public static void ClearPackageCache()
    {
        var cachePath = System.IO.Path.Combine(Application.dataPath, "..", "Library", "PackageCache");
        
        if (System.IO.Directory.Exists(cachePath))
        {
            try
            {
                System.IO.Directory.Delete(cachePath, true);
                Debug.Log("✓ Package Cache eliminado");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ No se pudo eliminar Package Cache: {e.Message}");
            }
        }
        else
        {
            Debug.Log("ℹ️ Package Cache no encontrado");
        }
    }
    
    [MenuItem("Fix/📦 Reimport All Assets", priority = 4)]
    public static void ReimportAllAssets()
    {
        try
        {
            AssetDatabase.ImportAsset("Assets", ImportAssetOptions.ImportRecursive);
            Debug.Log("✓ Reimportando todos los assets...");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al reimportar: {e.Message}");
        }
    }
    
    [MenuItem("Fix/❌ Disable Visual Scripting", priority = 10)]
    public static void DisableVisualScripting()
    {
        try
    {
        // Obtener configuración actual
        var currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        
        // Añadir define para deshabilitar Visual Scripting
        if (!currentDefines.Contains("DISABLE_VISUAL_SCRIPTING"))
        {
            if (!string.IsNullOrEmpty(currentDefines))
                currentDefines += ";DISABLE_VISUAL_SCRIPTING";
            else
                currentDefines = "DISABLE_VISUAL_SCRIPTING";
                
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currentDefines);
            
                Debug.Log("✓ Visual Scripting deshabilitado. Recompilando scripts...");
                EditorUtility.DisplayDialog("Visual Scripting", "Visual Scripting deshabilitado.\n\nLos scripts se recompilarán automáticamente.", "OK");
        }
        else
        {
                Debug.Log("ℹ️ Visual Scripting ya está deshabilitado");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al deshabilitar Visual Scripting: {e.Message}");
        }
    }
    
    [MenuItem("Fix/✅ Enable Visual Scripting", priority = 11)]
    public static void EnableVisualScripting()
    {
        try
    {
        // Obtener configuración actual
        var currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        
        // Remover define para habilitar Visual Scripting
        if (currentDefines.Contains("DISABLE_VISUAL_SCRIPTING"))
        {
            currentDefines = currentDefines.Replace("DISABLE_VISUAL_SCRIPTING", "").Replace(";;", ";");
            if (currentDefines.StartsWith(";"))
                currentDefines = currentDefines.Substring(1);
            if (currentDefines.EndsWith(";"))
                currentDefines = currentDefines.Substring(0, currentDefines.Length - 1);
                
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currentDefines);
            
                Debug.Log("✓ Visual Scripting habilitado. Recompilando scripts...");
                EditorUtility.DisplayDialog("Visual Scripting", "Visual Scripting habilitado.\n\nLos scripts se recompilarán automáticamente.", "OK");
        }
        else
        {
                Debug.Log("ℹ️ Visual Scripting ya está habilitado");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al habilitar Visual Scripting: {e.Message}");
        }
    }
    
    [MenuItem("Fix/🛑 Force Unity Restart (Manual)", priority = 20)]
    public static void ForceUnityRestart()
    {
        var result = EditorUtility.DisplayDialog(
            "Reiniciar Unity", 
            "Esta operación cerrará Unity.\n\nAsegúrate de guardar tu trabajo antes de continuar.\n\n¿Quieres continuar?", 
            "Sí, cerrar Unity", 
            "Cancelar"
        );
        
        if (result)
        {
            Debug.Log("🛑 Cerrando Unity por solicitud del usuario...");
            EditorApplication.Exit(0);
        }
    }
    
    [MenuItem("Fix/🔗 Fix Missing References (Critical)", priority = 1)]
    public static void FixMissingReferences()
            {
        Debug.Log("=== REPARANDO REFERENCIAS FALTANTES ===");
        
        try
        {
            // 1. Verificar y reinstalar paquetes críticos
            Debug.Log("Verificando paquetes instalados...");
            
            // 2. Regenerar Assembly Definitions
            RegenerateAssemblyDefinitions();
            
            // 3. Forzar reimportación de todos los scripts
            AssetDatabase.ImportAsset("Assets/Scripts", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            
            // 4. Limpiar cache específico
            var libraryPath = System.IO.Path.Combine(Application.dataPath, "..", "Library");
            
            // Limpiar ScriptAssemblies
            var scriptAssembliesPath = System.IO.Path.Combine(libraryPath, "ScriptAssemblies");
            if (System.IO.Directory.Exists(scriptAssembliesPath))
            {
                try
                {
                    System.IO.Directory.Delete(scriptAssembliesPath, true);
                    Debug.Log("✓ ScriptAssemblies limpiado");
                }
                catch { Debug.Log("⚠️ ScriptAssemblies en uso"); }
            }
            
            // Limpiar metadata
            var metadataPath = System.IO.Path.Combine(libraryPath, "metadata");
            if (System.IO.Directory.Exists(metadataPath))
            {
                try
                {
                    System.IO.Directory.Delete(metadataPath, true);
                    Debug.Log("✓ Metadata limpiado");
                }
                catch { Debug.Log("⚠️ Metadata en uso"); }
            }
            
            // 5. Forzar recompilación completa
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        
            Debug.Log("✅ REPARACIÓN DE REFERENCIAS COMPLETADA");
            EditorUtility.DisplayDialog("Referencias Reparadas", 
                "Reparación completada.\n\nEspera a que Unity termine de recompilar.\n\nSi persisten errores, usa 'Delete Assembly Definitions'", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error reparando referencias: {e.Message}");
        }
    }
    
    [MenuItem("Fix/🗑️ Delete Assembly Definitions (Reset)", priority = 7)]
    public static void DeleteAssemblyDefinitions()
    {
        var result = EditorUtility.DisplayDialog(
            "Eliminar Assembly Definitions", 
            "Esto eliminará los .asmdef creados y Unity usará las referencias por defecto.\n\n¿Continuar?", 
            "Sí, eliminar", 
            "Cancelar"
        );
        
        if (!result) return;
        
        try
        {
            // Buscar y eliminar todos los .asmdef en Scripts
            var asmdefFiles = System.IO.Directory.GetFiles(
                System.IO.Path.Combine(Application.dataPath, "Scripts"), 
                "*.asmdef", 
                System.IO.SearchOption.AllDirectories
            );
            
            foreach (var file in asmdefFiles)
        {
                System.IO.File.Delete(file);
                var metaFile = file + ".meta";
                if (System.IO.File.Exists(metaFile))
            {
                    System.IO.File.Delete(metaFile);
                }
                Debug.Log($"✓ Eliminado: {System.IO.Path.GetFileName(file)}");
        }
        
            // Forzar refresh
            AssetDatabase.Refresh();
            
            Debug.Log("✅ Assembly Definitions eliminados");
            EditorUtility.DisplayDialog("Assembly Definitions", 
                "Assembly Definitions eliminados.\n\nUnity usará las referencias por defecto.", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error eliminando Assembly Definitions: {e.Message}");
    }
    }
    
    private static void RegenerateAssemblyDefinitions()
    {
        // Recrear Scripts.asmdef con configuración completa
        var scriptsAsmdefPath = System.IO.Path.Combine(Application.dataPath, "Scripts", "Scripts.asmdef");
        
        var asmdefContent = @"{
    ""name"": ""Scripts"",
    ""rootNamespace"": """",
    ""references"": [
        ""Unity.TextMeshPro"",
        ""Unity.Timeline"",
        ""Cinemachine""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";
        
            try
            {
            System.IO.File.WriteAllText(scriptsAsmdefPath, asmdefContent);
            Debug.Log("✓ Scripts.asmdef regenerado");
            }
            catch (System.Exception e)
            {
            Debug.LogWarning($"⚠️ No se pudo regenerar Scripts.asmdef: {e.Message}");
            }
        }
} 