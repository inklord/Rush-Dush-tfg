using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class IAWaypoints : MonoBehaviour
{
    public List<Transform> waypoints;
    private int currentIndex = 0;

    private NavMeshAgent agent;
    private Rigidbody rigid;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rigid = GetComponent<Rigidbody>();

        if (waypoints.Count > 0)
            agent.SetDestination(waypoints[currentIndex].position);
    }

    void Update()
    {
        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            currentIndex++;
            if (currentIndex < waypoints.Count)
                agent.SetDestination(waypoints[currentIndex].position);
        }

        rigid.angularVelocity = Vector3.zero;
    }
}
