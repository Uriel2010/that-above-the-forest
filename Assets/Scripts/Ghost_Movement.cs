using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    [Header("Zona de movimiento")]
    public Transform spawnArea;

    [Header("Movimiento")]
    public float moveSpeed = 1.5f;
    public float waitTime = 2f;

    private NavMeshAgent agent;
    private float waitTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.speed = moveSpeed;

        ChooseNewDestination();
    }

    void Update()
    {
        if (agent == null || spawnArea == null)
            return;

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                waitTimer = 0f;
                ChooseNewDestination();
            }
        }
    }

    void ChooseNewDestination()
    {
        BoxCollider area = spawnArea.GetComponent<BoxCollider>();

        if (area == null)
        {
            Debug.LogWarning(
                "GhostSpawnArea necesita un Box Collider."
            );
            return;
        }

        Bounds bounds = area.bounds;

        for (int i = 0; i < 20; i++)
        {
            float randomX = Random.Range(
                bounds.min.x,
                bounds.max.x
            );

            float randomZ = Random.Range(
                bounds.min.z,
                bounds.max.z
            );

            Vector3 randomPoint = new Vector3(
                randomX,
                bounds.center.y,
                randomZ
            );

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                randomPoint,
                out hit,
                5f,
                NavMesh.AllAreas))
            {
                if (bounds.Contains(hit.position))
                {
                    agent.SetDestination(hit.position);
                    return;
                }
            }
        }

        Debug.LogWarning(
            "No encontré un destino dentro de GhostSpawnArea."
        );
    }
}