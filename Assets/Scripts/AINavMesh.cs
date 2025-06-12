using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AINavMesh : MonoBehaviour
{
    GameObject destPos;
    NavMeshAgent agent;
    Rigidbody rigid;
    
    [Header("🔧 Debug")]
    public bool enableDebugLogs = true;
    
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        
        // Buscar DestinationPos en lugar de RealDestPos
        destPos = GameObject.Find("DestinationPos");
        
        if (destPos == null)
        {
            Debug.LogError("❌ AINavMesh: No se encontró el objeto DestinationPos");
        }
        else if (enableDebugLogs)
        {
            Debug.Log($"🎯 AINavMesh: Destino configurado a {destPos.name}");
        }
    }

    void FixedUpdate()
    {
        if (destPos != null)
        {
            agent.SetDestination(destPos.transform.position);
            if (enableDebugLogs && Vector3.Distance(transform.position, destPos.transform.position) < 1f)
            {
                Debug.Log($"🏃 AINavMesh: {gameObject.name} llegó al destino");
            }
        }
        FreezeRotation();
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }
}
