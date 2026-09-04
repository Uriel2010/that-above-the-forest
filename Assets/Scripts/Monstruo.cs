using UnityEngine;
using UnityEngine.AI;

public class Monstruo : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 3.5f;

    [Header("Jugador")]
    [SerializeField] private Transform jugador;

    [Header("Linterna")]
    [SerializeField] private LanternController linterna;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = velocidad;
        if (jugador == null)
        {
            GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");

            if (objetoJugador != null)
            {
                jugador = objetoJugador.transform;
            }
            else
            {
                Debug.LogWarning("No se encontró un objeto con la etiqueta Player.");
            }
        }
    }

    void Update()
    {
        if (jugador == null)
            return;

        if (linterna == null && jugador != null)
        {
            linterna = jugador.GetComponentInChildren<LanternController>();
        }
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(jugador.position);
    }
}
