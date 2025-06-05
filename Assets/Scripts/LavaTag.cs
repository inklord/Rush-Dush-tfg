using UnityEngine;

/// <summary>
/// Script simple para identificar objetos de lava.
/// Asegúrate de que el GameObject también tenga el tag "Lava" asignado.
/// </summary>
public class LavaTag : MonoBehaviour
{
    [Header("Configuración de Lava")]
    public bool isLethal = true;                    // Si esta lava mata al contacto
    public float damageAmount = 100f;               // Cantidad de daño (si se implementa)
    public Material lavaMaterial;                   // Material de lava (opcional)
    public ParticleSystem lavaEffect;              // Efecto de partículas (opcional)
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private void Start()
    {
        // Verificar que tenga el tag correcto
        if (!gameObject.CompareTag("Lava"))
        {
            Debug.LogWarning($"LavaTag: {gameObject.name} no tiene el tag 'Lava'. Asignándolo automáticamente.");
            gameObject.tag = "Lava";
        }
        
        // Asegurar que tenga un Collider
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"LavaTag: {gameObject.name} no tiene Collider. Añadiendo BoxCollider.");
            gameObject.AddComponent<BoxCollider>();
        }
        
        // Configurar material si está asignado
        if (lavaMaterial != null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = lavaMaterial;
            }
        }
        
        // Activar efecto de partículas si está asignado
        if (lavaEffect != null && !lavaEffect.isPlaying)
        {
            lavaEffect.Play();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"LavaTag: {gameObject.name} configurado como lava letal: {isLethal}");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!isLethal) return;
        
        // Si es trigger, también manejar la colisión
        HandleLavaContact(other.gameObject);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!isLethal) return;
        
        // Manejar colisión normal
        HandleLavaContact(collision.gameObject);
    }
    
    private void HandleLavaContact(GameObject contactObject)
    {
        bool isPlayer = contactObject.CompareTag("Player") || contactObject.CompareTag("IA");
        
        if (isPlayer)
        {
            if (showDebugInfo)
                Debug.Log($"LavaTag: {contactObject.name} tocó lava en {gameObject.name}");
            
            // El MuerteLava.cs ya maneja la lógica de eliminación
            // Este script solo confirma que es lava
        }
    }
    
    // Método para cambiar el estado letal (útil para testing)
    public void SetLethal(bool lethal)
    {
        isLethal = lethal;
        
        if (showDebugInfo)
            Debug.Log($"LavaTag: {gameObject.name} letal cambiado a: {isLethal}");
    }
    
    // Método para obtener información de la lava
    public bool IsLethal()
    {
        return isLethal;
    }
    
    public float GetDamageAmount()
    {
        return damageAmount;
    }
} 