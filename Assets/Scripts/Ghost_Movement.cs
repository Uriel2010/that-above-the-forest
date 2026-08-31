using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveRadius = 15f;
    public float waitTime = 2f;

    [Header("Altura")]
    public float floatHeight = 1.5f;

    private NavMeshAgent agent;
    private float waitTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        ChooseNewDestination();
    }

    void Update()
    {
        if (agent == null)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                waitTimer = 0f;
                ChooseNewDestination();
            }
        }

        // Mantener al fantasma flotando
        Vector3 position = transform.position;
        position.y = floatHeight;
        transform.position = position;
    }

    void ChooseNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * moveRadius;
        randomDirection.y = 0;

        Vector3 randomPosition = transform.position + randomDirection;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            randomPosition,
            out hit,
            moveRadius,
            NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}