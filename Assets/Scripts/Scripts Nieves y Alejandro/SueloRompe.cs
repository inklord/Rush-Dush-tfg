using System.Collections;
using UnityEngine;

public class DestroyPlayer : MonoBehaviour
{
    [Header("Configuración de Ruptura")]
    public float baseDestroyDelay = 1.0f;  // Tiempo base más rápido para mayor dificultad
    public float delayReduction = 0.15f;   // Reducción más agresiva entre plantas
    public float minDestroyDelay = 0.05f;  // Tiempo mínimo más rápido
    public Color destroyColor = Color.red; // Color al tocar
    
    private float currentDestroyDelay;
    private Transform plantaPadre;
    private int plantaLevel;
    private bool isBeingDestroyed = false;

    private void Start()
    {
        // Buscar la planta padre (que tiene tag "Superficie")
        plantaPadre = FindPlantaPadre();
        
        if (plantaPadre != null)
        {
            // Calcular el nivel de la planta
            plantaLevel = CalculatePlantLevel();
            
            // Calcular el delay de destrucción para esta planta
            currentDestroyDelay = CalculateDestroyDelay(plantaLevel);
            
            Debug.Log($"Hexágono en {plantaPadre.name} (Nivel {plantaLevel}), delay de ruptura: {currentDestroyDelay:F2}s");
        }
        else
        {
            Debug.LogWarning($"No se encontró planta padre para hexágono: {gameObject.name}");
            currentDestroyDelay = baseDestroyDelay;
        }
    }

    private Transform FindPlantaPadre()
    {
        // Buscar hacia arriba en la jerarquía hasta encontrar un objeto con tag "Superficie"
        Transform current = transform;
        
        while (current != null)
        {
            if (current.CompareTag("Superficie"))
            {
                return current;
            }
            current = current.parent;
        }
        
        return null;
    }

    private int CalculatePlantLevel()
    {
        if (plantaPadre == null) return 0;
        
        // Extraer el número de la planta del nombre (ej: "Planta 1" -> 1)
        string plantaName = plantaPadre.name;
        
        // Buscar por patrón "Planta X" donde X es el número
        if (plantaName.Contains("Planta"))
        {
            string[] parts = plantaName.Split(' ');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int plantNumber))
            {
                // Planta 1 = nivel 0 (más alta, más lenta)
                // Planta 2 = nivel 1 (más rápida)
                // Planta 3 = nivel 2 (aún más rápida), etc.
                return plantNumber - 1;
            }
        }
        
        // Si no se puede extraer el número, usar la posición Y como respaldo
        return Mathf.FloorToInt(Mathf.Abs(plantaPadre.position.y) / 5f);
    }

    private float CalculateDestroyDelay(int level)
    {
        // LÓGICA INVERTIDA: nivel más alto (plantas con números más altos) = más lento
        // nivel más bajo (plantas con números más bajos) = más rápido
        float calculatedDelay = baseDestroyDelay + (level * delayReduction);
        
        return Mathf.Max(minDestroyDelay, calculatedDelay);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Verificar si colisiona con jugador humano o IA
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("IA"))
        {
            if (isBeingDestroyed) return; // Evitar múltiples activaciones
            
            string playerType = collision.gameObject.CompareTag("IA") ? "IA" : "Jugador";
            Debug.Log($"¡{playerType} pisó hexágono en {plantaPadre?.name}!");
            
            // Encontrar el hexágono completo (padre que contiene todas las partes)
            GameObject hexagonoCompleto = FindHexagonoCompleto();
            
            if (hexagonoCompleto != null)
            {
                StartCoroutine(DestroyHexagonAfterDelay(hexagonoCompleto));
            }
            else
            {
                // Si no encuentra el hexágono completo, destruir solo esta parte
                StartCoroutine(DestroyHexagonAfterDelay(gameObject));
            }
        }
    }

    private GameObject FindHexagonoCompleto()
    {
        // Buscar hacia arriba en la jerarquía para encontrar el objeto padre del hexágono
        Transform current = transform;
        
        // Si este objeto ya tiene tag "Hexagono", es el hexágono completo
        if (current.CompareTag("Hexagono"))
        {
            return current.gameObject;
        }
        
        // Buscar hacia arriba hasta encontrar un objeto con tag "Hexagono"
        while (current.parent != null)
        {
            current = current.parent;
            if (current.CompareTag("Hexagono"))
            {
                return current.gameObject;
            }
            
            // Si llegamos a la planta, parar la búsqueda
            if (current.CompareTag("Superficie"))
            {
                break;
            }
        }
        
        // Si no encontramos un padre con tag "Hexagono", 
        // buscar entre los hermanos si alguno tiene el tag
        Transform parent = transform.parent;
        if (parent != null)
        {
            foreach (Transform sibling in parent)
            {
                if (sibling.CompareTag("Hexagono"))
                {
                    return sibling.gameObject;
                }
            }
            
            // Si el padre no tiene tag "Superficie", usar el padre como hexágono completo
            if (!parent.CompareTag("Superficie"))
            {
                return parent.gameObject;
            }
        }
        
        return null;
    }

    private IEnumerator DestroyHexagonAfterDelay(GameObject hexagon)
    {
        isBeingDestroyed = true;
        
        // Obtener TODOS los renderers del hexágono y sus hijos
        Renderer[] renderers = hexagon.GetComponentsInChildren<Renderer>();
        
        // Guardar los colores originales
        Color[] originalColors = new Color[renderers.Length];
        Material[] originalMaterials = new Material[renderers.Length];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                originalMaterials[i] = renderers[i].material;
                if (renderers[i].material.HasProperty("_Color"))
                {
                    originalColors[i] = renderers[i].material.color;
                }
            }
        }

        // Cambiar el color de TODAS las partes del hexágono a rojo (efecto visual de ruptura)
        foreach (Renderer rend in renderers)
        {
            if (rend != null && rend.material != null && rend.material.HasProperty("_Color"))
            {
                rend.material.color = destroyColor;
            }
        }

        // Esperar el tiempo calculado antes de la destrucción
        yield return new WaitForSeconds(currentDestroyDelay);

        // Destruir el hexágono completo
        Destroy(hexagon);
        
        Debug.Log($"Hexágono destruido en {plantaPadre?.name} con delay de {currentDestroyDelay:F2}s");
    }

    // Método para obtener información del nivel desde otros scripts
    public int GetPlantaLevel()
    {
        return plantaLevel;
    }
    
    public float GetDestroyDelay()
    {
        return currentDestroyDelay;
    }
    
    public bool IsWeakened()
    {
        // Siempre retornar false ya que no hay sistema de debilitamiento
        return false;
    }

    // Método para debug - mostrar información del nivel en el editor
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && plantaPadre != null)
        {
            // Cambiar color del gizmo según la velocidad
            float normalizedSpeed = 1f - (currentDestroyDelay / baseDestroyDelay);
            Gizmos.color = Color.Lerp(Color.green, Color.red, normalizedSpeed);
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
            
            // Mostrar información de la planta
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, plantaPadre.position);
        }
    }
}
