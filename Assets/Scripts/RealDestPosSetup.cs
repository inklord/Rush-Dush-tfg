using UnityEngine;

public class RealDestPosSetup : MonoBehaviour
{
    void Start()
    {
        // Buscar todos los objetos con el tag RealDestPos
        GameObject[] realDestPosObjects = GameObject.FindGameObjectsWithTag("RealDestPos");
        
        foreach (GameObject obj in realDestPosObjects)
        {
            // Verificar si ya tiene el script RealDestPosTrigger
            RealDestPosTrigger trigger = obj.GetComponent<RealDestPosTrigger>();
            if (trigger == null)
            {
                // Añadir el script si no lo tiene
                trigger = obj.AddComponent<RealDestPosTrigger>();
                Debug.Log($"🎯 Añadido RealDestPosTrigger a: {obj.name}");
            }
            
            // Asegurarse de que el collider sea trigger
            Collider col = obj.GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.Log($"🎯 Activado trigger en collider de: {obj.name}");
            }
            
            // Eliminar otros scripts que puedan interferir
            MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != null && script.GetType() != typeof(RealDestPosTrigger))
                {
                    Destroy(script);
                    Debug.Log($"🎯 Eliminado script {script.GetType().Name} de: {obj.name}");
                }
            }
        }
    }
} 