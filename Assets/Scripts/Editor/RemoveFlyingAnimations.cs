using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// 🗑️ Eliminador de Animaciones de Volar/Caer
/// Script de editor para limpiar todos los parámetros de animación no deseados
/// </summary>
public class RemoveFlyingAnimations : EditorWindow
{
    [MenuItem("Tools/Remove Flying Animations")]
    public static void ShowWindow()
    {
        GetWindow<RemoveFlyingAnimations>("Remove Flying Animations");
    }

    void OnGUI()
    {
        GUILayout.Label("🗑️ Eliminar Animaciones de Volar/Caer", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("Este tool eliminará los siguientes parámetros de animación:");
        GUILayout.Label("• isFalling (animación de caída)");
        GUILayout.Label("• IsFlying (animación de volar)");
        GUILayout.Space(10);
        
        GUILayout.Label("De todos los Animator Controllers en el proyecto:");
        GUILayout.Label("• Player.controller");
        GUILayout.Label("• Waiting.controller");
        GUILayout.Label("• Cualquier otro controller encontrado");
        GUILayout.Space(20);
        
        if (GUILayout.Button("🗑️ ELIMINAR ANIMACIONES DE VOLAR/CAER", GUILayout.Height(40)))
        {
            RemoveAllFlyingAnimationParameters();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("🔧 LIMPIAR TRANSICIONES ROTAS", GUILayout.Height(30)))
        {
            CleanBrokenTransitions();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("🔍 Solo mostrar controllers encontrados", GUILayout.Height(25)))
        {
            ShowFoundControllers();
        }
    }

    void RemoveAllFlyingAnimationParameters()
    {
        string[] parameterNames = { "isFalling", "IsFlying" };
        int totalRemoved = 0;
        int controllersProcessed = 0;

        // Buscar todos los Animator Controllers en el proyecto
        string[] controllerGUIDs = AssetDatabase.FindAssets("t:AnimatorController");
        
        foreach (string guid in controllerGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            
            if (controller != null)
            {
                controllersProcessed++;
                Debug.Log($"🔍 Procesando: {path}");
                
                bool modified = false;
                
                foreach (string paramName in parameterNames)
                {
                    // Buscar el parámetro
                    for (int i = controller.parameters.Length - 1; i >= 0; i--)
                    {
                        if (controller.parameters[i].name == paramName)
                        {
                            controller.RemoveParameter(i);
                            totalRemoved++;
                            modified = true;
                            Debug.Log($"✅ Eliminado parámetro '{paramName}' de {controller.name}");
                        }
                    }
                }
                
                if (modified)
                {
                    EditorUtility.SetDirty(controller);
                    AssetDatabase.SaveAssets();
                }
            }
        }
        
        Debug.Log($"🎯 PROCESO COMPLETADO:");
        Debug.Log($"   Controllers procesados: {controllersProcessed}");
        Debug.Log($"   Parámetros eliminados: {totalRemoved}");
        
        EditorUtility.DisplayDialog("Proceso Completado", 
            $"✅ Animaciones eliminadas exitosamente!\n\n" +
            $"Controllers procesados: {controllersProcessed}\n" +
            $"Parámetros eliminados: {totalRemoved}\n\n" +
            $"Revisa la consola para más detalles.", "OK");
            
        AssetDatabase.Refresh();
    }

    void ShowFoundControllers()
    {
        string[] controllerGUIDs = AssetDatabase.FindAssets("t:AnimatorController");
        
        Debug.Log($"🔍 ANIMATOR CONTROLLERS ENCONTRADOS ({controllerGUIDs.Length}):");
        
        foreach (string guid in controllerGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            
            if (controller != null)
            {
                Debug.Log($"   📁 {path}");
                
                // Mostrar parámetros actuales
                foreach (var param in controller.parameters)
                {
                    if (param.name == "isFalling" || param.name == "IsFlying")
                    {
                        Debug.Log($"      ❌ Parámetro a eliminar: {param.name} ({param.type})");
                    }
                    else
                    {
                        Debug.Log($"      ✅ Parámetro normal: {param.name} ({param.type})");
                    }
                }
            }
        }
    }

    void CleanBrokenTransitions()
    {
        string[] brokenParameters = { "isFalling", "IsFlying" };
        int transitionsFixed = 0;
        int controllersProcessed = 0;

        // Buscar todos los Animator Controllers en el proyecto
        string[] controllerGUIDs = AssetDatabase.FindAssets("t:AnimatorController");
        
        foreach (string guid in controllerGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            
            if (controller != null)
            {
                controllersProcessed++;
                Debug.Log($"🔧 Limpiando transiciones en: {path}");
                
                bool modified = false;
                
                // Revisar todas las capas
                for (int layerIndex = 0; layerIndex < controller.layers.Length; layerIndex++)
                {
                    var layer = controller.layers[layerIndex];
                    var stateMachine = layer.stateMachine;
                    
                    // Limpiar transiciones en todos los estados
                    foreach (var state in stateMachine.states)
                    {
                        var transitions = state.state.transitions;
                        for (int i = transitions.Length - 1; i >= 0; i--)
                        {
                            var transition = transitions[i];
                            bool hasInvalidCondition = false;
                            
                            foreach (var condition in transition.conditions)
                            {
                                foreach (string brokenParam in brokenParameters)
                                {
                                    if (condition.parameter == brokenParam)
                                    {
                                        hasInvalidCondition = true;
                                        Debug.Log($"❌ Transición rota encontrada en {state.state.name} usando parámetro '{brokenParam}'");
                                        break;
                                    }
                                }
                                if (hasInvalidCondition) break;
                            }
                            
                            if (hasInvalidCondition)
                            {
                                // Eliminar la transición problemática
                                state.state.RemoveTransition(transition);
                                transitionsFixed++;
                                modified = true;
                                Debug.Log($"✅ Transición eliminada de {state.state.name}");
                            }
                        }
                    }
                    
                    // Limpiar transiciones desde AnyState
                    var anyStateTransitions = stateMachine.anyStateTransitions;
                    for (int i = anyStateTransitions.Length - 1; i >= 0; i--)
                    {
                        var transition = anyStateTransitions[i];
                        bool hasInvalidCondition = false;
                        
                        foreach (var condition in transition.conditions)
                        {
                            foreach (string brokenParam in brokenParameters)
                            {
                                if (condition.parameter == brokenParam)
                                {
                                    hasInvalidCondition = true;
                                    Debug.Log($"❌ Transición AnyState rota encontrada usando parámetro '{brokenParam}'");
                                    break;
                                }
                            }
                            if (hasInvalidCondition) break;
                        }
                        
                        if (hasInvalidCondition)
                        {
                            stateMachine.RemoveAnyStateTransition(transition);
                            transitionsFixed++;
                            modified = true;
                            Debug.Log($"✅ Transición AnyState eliminada");
                        }
                    }
                }
                
                if (modified)
                {
                    EditorUtility.SetDirty(controller);
                    AssetDatabase.SaveAssets();
                }
            }
        }
        
        Debug.Log($"🎯 LIMPIEZA DE TRANSICIONES COMPLETADA:");
        Debug.Log($"   Controllers procesados: {controllersProcessed}");
        Debug.Log($"   Transiciones eliminadas: {transitionsFixed}");
        
        EditorUtility.DisplayDialog("Transiciones Limpiadas", 
            $"✅ Transiciones rotas eliminadas!\n\n" +
            $"Controllers procesados: {controllersProcessed}\n" +
            $"Transiciones eliminadas: {transitionsFixed}\n\n" +
            $"Los errores del Animator deberían estar resueltos.", "OK");
            
        AssetDatabase.Refresh();
    }
} 