using UnityEngine;

/// <summary>
/// 🔍 Script de debug para trigger de respawn
/// Colócalo en el objeto que tiene el trigger de respawn para verificar que funciona
/// </summary>
public class RespawnTriggerDebug : MonoBehaviour
{
    [Header("🔍 Debug Info")]
    public bool showDebugLogs = true;
    
    void Start()
    {
        // Verificar configuración del trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"❌ {gameObject.name}: No tiene componente Collider");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError($"❌ {gameObject.name}: Collider no está marcado como Trigger");
        }
        else
        {
            Debug.Log($"✅ {gameObject.name}: Trigger configurado correctamente");
        }
        
        // Verificar si tiene LHS_Respawn2
        LHS_Respawn2 respawnScript = GetComponent<LHS_Respawn2>();
        if (respawnScript == null)
        {
            Debug.LogWarning($"⚠️ {gameObject.name}: No tiene componente LHS_Respawn2");
        }
        else
        {
            Debug.Log($"✅ {gameObject.name}: LHS_Respawn2 encontrado");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"🔍 RespawnTriggerDebug: {other.name} ENTRÓ al trigger");
            Debug.Log($"    Tag: {other.tag}");
            Debug.Log($"    Layer: {other.gameObject.layer}");
            Debug.Log($"    ¿Es Player?: {other.CompareTag("Player")}");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"🔍 RespawnTriggerDebug: {other.name} SALIÓ del trigger");
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (showDebugLogs)
        {
            // Solo mostrar cada segundo para no spam
            if (Time.fixedTime % 1f < 0.1f)
            {
                Debug.Log($"🔍 RespawnTriggerDebug: {other.name} está DENTRO del trigger");
            }
        }
    }
    
    void OnDrawGizmos()
    {
        // Visualizar el trigger en la escena
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
            else if (col is CapsuleCollider capsule)
            {
                Gizmos.DrawWireCube(capsule.center, new Vector3(capsule.radius * 2, capsule.height, capsule.radius * 2));
            }
        }
    }
} 