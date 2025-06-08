using UnityEngine;

/// <summary>
/// 🧪 TEST SCRIPT - Para verificar compilación
/// </summary>
public class TestScript : MonoBehaviour
{
    [Header("🧪 Test")]
    public bool testMode = true;
    
    void Start()
    {
        if (testMode)
        {
            Debug.Log("🧪 TestScript funcionando correctamente!");
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("🧪 Test - Tecla T presionada!");
        }
    }
} 