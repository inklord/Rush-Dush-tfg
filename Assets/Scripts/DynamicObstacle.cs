using UnityEngine;

/// <summary>
/// 🚧 Tipos de efectos que puede aplicar un obstáculo dinámico
/// </summary>
public enum ObstacleEffectType
{
    Knockback,    // Empujar hacia atrás
    Push,         // Empujar suavemente
    Stun,         // Aturdir temporalmente
    Bounce        // Rebotar hacia arriba
}

/// <summary>
/// 🎯 Obstáculo dinámico simplificado
/// Proporciona efectos configurables para colisiones con el jugador
/// </summary>
public class DynamicObstacle : MonoBehaviour
{
    [Header("🚧 Obstacle Settings")]
    public ObstacleEffectType effectType = ObstacleEffectType.Knockback;
    public float forceAmount = 10f;
    public float stunDuration = 1f;
    public bool enableSoundEffect = true;
    public bool enableParticleEffect = true;
    
    [Header("🎯 Force Direction")]
    public bool useCustomDirection = false;
    public Vector3 customDirection = Vector3.back;
    
    /// <summary>
    /// 📊 Obtener tipo de efecto
    /// </summary>
    public ObstacleEffectType GetEffectType()
    {
        return effectType;
    }
    
    /// <summary>
    /// 💪 Obtener cantidad de fuerza
    /// </summary>
    public float GetForceAmount()
    {
        return forceAmount;
    }
    
    /// <summary>
    /// 😵 Obtener duración del aturdimiento
    /// </summary>
    public float GetStunDuration()
    {
        return stunDuration;
    }
    
    /// <summary>
    /// 🎵 Verificar si tiene efecto de sonido
    /// </summary>
    public bool HasSoundEffect()
    {
        return enableSoundEffect;
    }
    
    /// <summary>
    /// ✨ Verificar si tiene efecto de partículas
    /// </summary>
    public bool HasParticleEffect()
    {
        return enableParticleEffect;
    }
    
    /// <summary>
    /// 🧭 Obtener dirección de la fuerza
    /// </summary>
    public Vector3 GetForceDirection(Vector3 playerPosition)
    {
        if (useCustomDirection)
        {
            return customDirection.normalized;
        }
        else
        {
            // Dirección desde el obstáculo hacia el jugador
            Vector3 direction = (playerPosition - transform.position).normalized;
            return direction;
        }
    }
    
    /// <summary>
    /// 🎨 Visualización en el editor
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Mostrar dirección de fuerza
        Gizmos.color = Color.yellow;
        Vector3 direction = useCustomDirection ? customDirection : Vector3.back;
        Gizmos.DrawRay(transform.position, direction.normalized * 2f);
        
        // Mostrar área de efecto
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
} 