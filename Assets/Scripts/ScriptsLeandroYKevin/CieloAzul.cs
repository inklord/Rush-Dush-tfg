using UnityEngine;

public class CieloAzul : MonoBehaviour
{
    void Start()
    {
        // Color azul para todo
        Color colorAzul = new Color(0.4f, 0.7f, 1f);

        // Configura el cielo
        RenderSettings.skybox = new Material(Shader.Find("Skybox/Procedural"));
        Camera.main.clearFlags = CameraClearFlags.Skybox;
        RenderSettings.ambientSkyColor = colorAzul;

        // Crea un plano infinito azul
        GameObject plano = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plano.transform.position = new Vector3(0, -5, 0); // Lo ponemos más abajo
        plano.transform.localScale = new Vector3(100, 1, 100); // Lo hacemos muy grande
        
        // Material azul para el plano
        Material materialAzul = new Material(Shader.Find("Standard"));
        materialAzul.color = colorAzul;
        plano.GetComponent<Renderer>().material = materialAzul;
    }
}