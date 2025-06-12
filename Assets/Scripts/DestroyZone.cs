using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyZone : MonoBehaviour
{
    public ParticleSystem bounce;
    public bool enableDebugLogs = true;

    private void Start()
    {
        // Asegurarse de que el collider esté configurado como trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            if (enableDebugLogs) Debug.Log("🎯 DestroyZone: Configurando collider como trigger");
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enableDebugLogs) Debug.Log($"🎯 DestroyZone: Objeto entrando en zona: {other.gameObject.name} (Tag: {other.tag})");
        DestroyObject(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableDebugLogs) Debug.Log($"🎯 DestroyZone: Colisión con objeto: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
        DestroyObject(collision.gameObject);
    }

    private void DestroyObject(GameObject obj)
    {
        // Reproducir efecto si existe
        if (bounce != null)
        {
            bounce.transform.position = obj.transform.position;
            bounce.Play();
        }

        // Destruir el objeto
        Destroy(obj);

        if (enableDebugLogs) Debug.Log($"🎯 DestroyZone: Objeto destruido: {obj.name}");
    }
}
