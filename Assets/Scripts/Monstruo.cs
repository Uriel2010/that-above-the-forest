using UnityEngine;
using UnityEngine.AI;

public class Monstruo : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 12f;

    [Header("targetMonstruo")]
    [SerializeField] private Transform targetMonstruo;

    [Header("Linterna")]
    [SerializeField] private LanternController linterna;

    [Header("Frenado al ser iluminado")]
    [SerializeField] private float desaceleracionFrenado = 40f; // unidades/s² (más alto = frena más rápido)

    private NavMeshAgent agent;
    private bool congelado = false;
    private bool frenando = false;

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
        // Frenado: se detiene por completo antes de cambiar de dirección
        if (frenando)
        {
            agent.velocity = Vector3.MoveTowards(agent.velocity, Vector3.zero, desaceleracionFrenado * Time.deltaTime);

            if (agent.velocity.sqrMagnitude < 0.0025f)
            {
                agent.velocity = Vector3.zero;
                frenando = false;
                if (!congelado)
                    agent.isStopped = false;
            }
            return;
        }

        if (congelado)
            return;

        if (targetMonstruo == null)
            return;

        agent.destination = targetMonstruo.position;
    }

    // Llamado por targetMonstruo en el instante en que el monstruo es iluminado
    public void Frenar()
    {
        if (congelado)
            return;

        frenando = true;
        agent.isStopped = true;
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