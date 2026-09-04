```csharp
using UnityEngine;
using System.Collections;

public class Ghost_Spawner : MonoBehaviour
{
    [Header("Fantasma")]
    public GameObject ghostPrefab;

    [Header("Zona de aparición")]
    public Transform spawnArea;

    [Header("Fantasma actual")]
    public Transform ghostTransform;

    [Header("Tiempo de aparición")]
    public float minAppearTime = 5f;
    public float maxAppearTime = 10f;

    [Header("Tiempo que permanece")]
    public float minLifeTime = 10f;
    public float maxLifeTime = 20f;

    [Header("Movimiento")]
    public float moveSpeed = 1.5f;
    public float waitTime = 2f;

    private GameObject currentGhost;

    void Start()
    {
        Debug.Log("👻 Ghost Spawner iniciado");
        StartCoroutine(SpawnGhost());
    }

    IEnumerator SpawnGhost()
    {
        while (true)
        {
            // Esperar antes de aparecer
            float waitBeforeSpawn = Random.Range(
                minAppearTime,
                maxAppearTime
            );

            yield return new WaitForSeconds(waitBeforeSpawn);

            // Buscar posición aleatoria
            Vector3 spawnPosition;

            if (GetRandomPosition(out spawnPosition))
            {
                // Crear fantasma
                currentGhost = Instantiate(
                    ghostPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

                // Guardar el Transform del fantasma
                ghostTransform = currentGhost.transform;

                Debug.Log(
                    "👻 ¡FANTASMA APARECIÓ! Posición: " +
                    ghostTransform.position
                );

                // Configurar movimiento
                GhostAI movement =
                    currentGhost.GetComponent<GhostAI>();

                if (movement != null)
                {
                    movement.spawnArea = spawnArea;
                    movement.moveSpeed = moveSpeed;
                    movement.waitTime = waitTime;
                }

                // Esperar mientras el fantasma está activo
                float lifeTime = Random.Range(
                    minLifeTime,
                    maxLifeTime
                );

                yield return new WaitForSeconds(lifeTime);

                // Destruir fantasma
                if (currentGhost != null)
                {
                    Destroy(currentGhost);

                    currentGhost = null;

                    // Borrar la referencia cuando desaparece
                    ghostTransform = null;

                    Debug.Log("👻 Fantasma desapareció");
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    bool GetRandomPosition(out Vector3 result)
    {
        result = Vector3.zero;

        if (spawnArea == null)
        {
            Debug.LogWarning(
                "Ghost Spawner: falta Spawn Area."
            );

            return false;
        }

        BoxCollider area =
            spawnArea.GetComponent<BoxCollider>();

        if (area == null)
        {
            Debug.LogWarning(
                "GhostSpawnArea necesita un Box Collider."
            );

            return false;
        }

        Bounds bounds = area.bounds;

        // Intentar 20 posiciones diferentes
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

            UnityEngine.AI.NavMeshHit hit;

            if (UnityEngine.AI.NavMesh.SamplePosition(
                randomPoint,
                out hit,
                5f,
                UnityEngine.AI.NavMesh.AllAreas))
            {
                if (bounds.Contains(hit.position))
                {
                    Debug.Log(
                        "👻 Posición encontrada: " +
                        hit.position
                    );

                    result = hit.position;

                    return true;
                }
            }
        }

        Debug.LogWarning(
            "No encontré una posición válida para el fantasma."
        );

        return false;
    }
}
```
