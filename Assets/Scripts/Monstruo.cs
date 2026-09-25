using UnityEngine;
using UnityEngine.AI;

public class Monstruo : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 5f;

    [Header("targetMonstruo")]
    [SerializeField] private Transform targetMonstruo;

    [Header("Linterna")]
    [SerializeField] private LanternController linterna;

    private NavMeshAgent agent;
    private bool congelado = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = velocidad;
        if (targetMonstruo == null)
        {
            GameObject objetotargetMonstruo = GameObject.FindGameObjectWithTag("Player");

            if (objetotargetMonstruo != null)
            {
                targetMonstruo = objetotargetMonstruo.transform;
            }
            else
            {
                Debug.LogWarning("No se encontró un objeto con la etiqueta Player.");
            }
        }
    }

    void Update()
    {
        if (congelado)
            return;

        if (targetMonstruo == null)
            return;

        agent.destination = targetMonstruo.position;
    }

    // Llamado por LanternController cuando la linterna en modo alto lo atrapa
    public void Congelar()
    {
        congelado = true;
        agent.isStopped = true;
    }

    public void Descongelar()
    {
        congelado = false;
        agent.isStopped = false;
    }
}
