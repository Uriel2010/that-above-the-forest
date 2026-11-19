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
    [Tooltip("Muestra en la consola cuándo cambia entre caminar y atacar")]
    [SerializeField] private bool mostrarDebug = true;

    [Header("Inclinación al atacar")]
    [Tooltip("Objeto que se inclina (el modelo). Si queda vacío se usa el del Animator. No puede ser el objeto raíz del monstruo.")]
    [SerializeField] private Transform modelo;
    [Tooltip("Grados que rota en el eje X mientras ataca (usá un valor negativo para inclinarlo hacia atrás)")]
    [SerializeField] private float anguloInclinacion = 40f;
    [SerializeField] private float duracionInclinacion = 0.75f; // segundos que tarda en llegar al ángulo completo

    private NavMeshAgent agent;
    private bool inclinacionLista = false;
    private Quaternion rotacionBase;
    private float inclinacionActual = 0f;
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

        // Si el Animator encontrado no tiene Controller (por ejemplo uno que sobró en el objeto raíz),
        // se busca entre los hijos (incluso inactivos) el que sí lo tenga: el del modelo.
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            foreach (Animator a in GetComponentsInChildren<Animator>(true))
            {
                if (a.runtimeAnimatorController != null)
                {
                    animator = a;
                    break;
                }
            }
        }

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
            Debug.Log("Monstruo: usando el Animator de '" + animator.name + "' con el controller '" + animator.runtimeAnimatorController.name + "'");

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

    // La inclinación se aplica en LateUpdate para que sea independiente de los "return" de Update
    // (frenado, congelado) y se aplique después de que el Animator actualice el modelo.
    void LateUpdate()
    {
        if (!inclinacionLista)
        {
            if (modelo == null && animator != null)
                modelo = animator.transform;

            if (modelo == transform)
            {
                Debug.LogWarning("Monstruo: el objeto a inclinar es la raíz del monstruo (la mueve el NavMeshAgent). Asigná el modelo hijo en el campo 'Modelo'.");
                modelo = null;
            }

            if (modelo == null)
                return;

            rotacionBase = modelo.localRotation;
            inclinacionLista = true;
        }

        // Llega al ángulo completo en "duracionInclinacion" segundos y vuelve a 0 a la misma velocidad
        float objetivo = atacando ? anguloInclinacion : 0f;
        float velocidadGiro = Mathf.Abs(anguloInclinacion) / Mathf.Max(0.01f, duracionInclinacion);
        inclinacionActual = Mathf.MoveTowards(inclinacionActual, objetivo, velocidadGiro * Time.deltaTime);

        // Rotación alrededor del eje X del padre (el eje lateral del monstruo)
        modelo.localRotation = Quaternion.Euler(inclinacionActual, 0f, 0f) * rotacionBase;
    }

    // Llamado por targetMonstruo cada frame: true = animación de ataque, false = animación de caminar
    public void Atacar(bool valor)
    {
        if (valor == atacando)
            return;

        atacando = valor;
        if (mostrarDebug)
            Debug.Log("Monstruo: " + (valor ? "pasa a la animación de ATAQUE" : "vuelve a la animación de CAMINAR"));

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