using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GhostSpawner : MonoBehaviour
{
    [Header("Fantasma")]
    public GameObject ghostPrefab;

    [Header("Zona de aparición")]
    public Transform spawnArea;

    [Header("Tiempo")]
    public float minAppearTime = 5f;
    public float maxAppearTime = 15f;

    [Header("Duración")]
    public float minLifeTime = 10f;
    public float maxLifeTime = 25f;

    [Header("Movimiento")]
    public float moveRadius = 15f;
    public float waitAtPoint = 2f;

    private GameObject currentGhost;

    void Start()
    {
        StartCoroutine(GhostRoutine());
    }

    IEnumerator GhostRoutine()
    {
        while (true)
        {
            // Esperar antes de aparecer
            float appearTime = Random.Range(
                minAppearTime,
                maxAppearTime
            );

            yield return new WaitForSeconds(appearTime);

            // Buscar posición aleatoria
            Vector3 spawnPosition;

            if (GetRandomSpawnPosition(out spawnPosition))
            {
                // Crear fantasma
                currentGhost = Instantiate(
                    ghostPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

                // Agregar IA
                GhostAI ghostAI = currentGhost.AddComponent<GhostAI>();

                ghostAI.moveRadius = moveRadius;
                ghostAI.waitTime = waitAtPoint;

                // Tiempo que permanece
                float lifeTime = Random.Range(
                    minLifeTime,
                    maxLifeTime
                );

                yield return new WaitForSeconds(lifeTime);

                // Desaparecer
                if (currentGhost != null)
                {
                    Destroy(currentGhost);
                    currentGhost = null;
                }
            }

            // Pequeña espera antes de volver a intentar
            yield return new WaitForSeconds(1f);
        }
    }

    bool GetRandomSpawnPosition(out Vector3 result)
    {
        result = Vector3.zero;

        if (spawnArea == null)
            return false;

        Collider areaCollider = spawnArea.GetComponent<Collider>();

        if (areaCollider == null)
        {
            Debug.LogWarning(
                "GhostSpawnArea necesita un Collider."
            );

            return false;
        }

        Bounds bounds = areaCollider.bounds;

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
                result = hit.position;
                return true;
            }
        }

        return false;
    }
}
