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

    [Header("Frenado al ser iluminado")]
    [SerializeField] private float desaceleracionFrenado = 40f; // unidades/s² (más alto = frena más rápido)

    [Header("Animación")]
    [SerializeField] private Animator animator; // si queda vacío se busca en el modelo (hijo)
    [Tooltip("Nombre EXACTO del parámetro Bool del Animator que activa la animación de ataque")]
    [SerializeField] private string parametroAtaque = "Atacando";

    private NavMeshAgent agent;
    private bool congelado = false;
    private bool frenando = false;
    private bool atacando = false;
    private int hashAtaque;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = velocidad;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogWarning("Monstruo: no se encontró un Animator en el monstruo ni en sus hijos.");
        }
        else if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("Monstruo: el Animator '" + animator.name + "' no tiene un Controller asignado.");
        }
        else
        {
            hashAtaque = Animator.StringToHash(parametroAtaque);

            // Avisa si el parámetro no existe en el Animator Controller
            bool existe = false;
            foreach (AnimatorControllerParameter p in animator.parameters)
                if (p.name == parametroAtaque && p.type == AnimatorControllerParameterType.Bool)
                    existe = true;

            if (!existe)
                Debug.LogWarning("Monstruo: el Animator no tiene un parámetro Bool llamado '" + parametroAtaque + "'.");
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

    // Llamado por targetMonstruo cada frame: true = animación de ataque, false = animación de caminar
    public void Atacar(bool valor)
    {
        if (valor == atacando)
            return;

        atacando = valor;
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetBool(hashAtaque, valor);
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