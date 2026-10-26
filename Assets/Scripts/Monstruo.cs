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

    [Header("Contacto con el jugador")]
    [SerializeField] private float distanciaContacto = 1.5f; // distancia a la que se considera que lo tocó

    [Header("Frenado al ser iluminado")]
    [SerializeField] private float desaceleracionFrenado = 40f; // unidades/s² (más alto = frena más rápido)

    private NavMeshAgent agent;
    private bool congelado = false;
    private bool frenando = false;
    private Transform jugador;
    private MuerteJugador muerteJugador;
    private bool jugadorMuerto = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = velocidad;

        GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");
        if (objetoJugador != null)
        {
            jugador = objetoJugador.transform;
            muerteJugador = objetoJugador.GetComponent<MuerteJugador>();
            if (muerteJugador == null)
                Debug.LogWarning("El jugador no tiene el componente MuerteJugador.");
        }

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
        if (jugadorMuerto)
            return;

        // Si toca al jugador, el jugador muere (aunque esté iluminado o congelado)
        if (jugador != null && TocandoJugador())
        {
            jugadorMuerto = true;
            Congelar();
            if (muerteJugador != null)
                muerteJugador.Morir();
            return;
        }

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
        if (jugadorMuerto || congelado)
            return;

        frenando = true;
        agent.isStopped = true;
    }

    bool TocandoJugador()
    {
        Vector3 a = transform.position;
        Vector3 b = jugador.position;
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b) <= distanciaContacto;
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