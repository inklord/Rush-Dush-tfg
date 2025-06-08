using UnityEngine;

/// <summary>
/// 🔨 Script auxiliar para manejar triggers de SpinningHammer
/// Usado cuando el collider principal no puede ser trigger (ej: MeshCollider cóncavo)
/// </summary>
public class SpinningHammerTrigger : MonoBehaviour
{
    [HideInInspector]
    public SpinningHammer parentHammer;
    
    void OnTriggerEnter(Collider other)
    {
        if (parentHammer != null)
        {
            // Delegar el manejo del trigger al martillo principal
            parentHammer.HandleTriggerEnter(other);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (parentHammer != null)
        {
            // Delegar el manejo del trigger al martillo principal
            parentHammer.HandleTriggerExit(other);
        }
    }
} 