using UnityEngine;

// Se agrega al objeto que representa el cuerpo (por ahora un cubo).
// Necesita un Collider y un Rigidbody.
[RequireComponent(typeof(Rigidbody))]
public class CuerpoCargable : MonoBehaviour
{
    [Tooltip("Posición del cuerpo respecto al punto de carga mientras se carga (X: derecha/izquierda, Y: arriba/abajo, Z: adelante/atrás).")]
    public Vector3 posicionAlCargar = Vector3.zero;

    [Tooltip("Rotación (en grados) con la que queda el cuerpo mientras se carga. Ej: (90, 0, 0) para llevarlo acostado.")]
    public Vector3 rotacionAlCargar = Vector3.zero;

    [Header("Balanceo al cargar")]
    [Tooltip("Hace que el cuerpo suba y baje suavemente mientras se carga")]
    public bool balanceo = true;
    [Tooltip("Cuánto sube y baja (en unidades) al caminar o correr")]
    public float amplitudAndando = 0.03f;
    [Tooltip("Subidas y bajadas por segundo al caminar o correr")]
    public float frecuenciaAndando = 2f;
    [Tooltip("Cuánto sube y baja cuando el jugador está quieto (como respirando)")]
    public float amplitudQuieto = 0.008f;
    [Tooltip("Subidas y bajadas por segundo cuando está quieto")]
    public float frecuenciaQuieto = 0.4f;
    [Tooltip("Velocidad del jugador a partir de la cual se aplica el balanceo completo")]
    public float velocidadReferencia = 5f;

    private Rigidbody rb;
    private Collider[] colliders;
    private bool cargado = false;

    private Transform jugador;
    private Vector3 ultimaPosJugador;
    private float fase;
    private float factorMovimiento;
    private Vector3 offsetBalanceo = Vector3.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
    }

    public bool EstaCargado() { return cargado; }

    // Se aplica cada frame mientras se carga, así se puede ajustar la posición y la rotación
    // en el Inspector con el juego corriendo y ver el resultado al instante.
    void LateUpdate()
    {
        if (cargado)
        {
            ActualizarBalanceo();
            AplicarPoseDeCarga();
        }
    }

    void AplicarPoseDeCarga()
    {
        transform.localPosition = posicionAlCargar + offsetBalanceo;
        transform.localRotation = Quaternion.Euler(rotacionAlCargar);
    }

    // Sube y baja más rápido y más fuerte cuanto más rápido se mueve el jugador
    void ActualizarBalanceo()
    {
        if (!balanceo || jugador == null || Time.deltaTime <= 0f)
        {
            offsetBalanceo = Vector3.zero;
            return;
        }

        Vector3 delta = jugador.position - ultimaPosJugador;
        delta.y = 0f;
        ultimaPosJugador = jugador.position;

        float velocidad = delta.magnitude / Time.deltaTime;
        float objetivo = Mathf.Clamp01(velocidad / Mathf.Max(0.01f, velocidadReferencia));
        factorMovimiento = Mathf.Lerp(factorMovimiento, objetivo, 10f * Time.deltaTime); // suaviza los cambios

        float frecuencia = Mathf.Lerp(frecuenciaQuieto, frecuenciaAndando, factorMovimiento);
        float amplitud = Mathf.Lerp(amplitudQuieto, amplitudAndando, factorMovimiento);

        fase += Time.deltaTime * frecuencia * 2f * Mathf.PI;
        offsetBalanceo = Vector3.up * (Mathf.Sin(fase) * amplitud);
    }

    // Lo llama CargarCuerpo: el cuerpo pasa a seguir el punto de carga del jugador.
    public void Cargar(Transform puntoCarga)
    {
        cargado = true;

        // Sin física ni colisiones mientras se carga, para que no empuje al jugador ni tape la luz
        rb.isKinematic = true;
        foreach (Collider c in colliders)
            c.enabled = false;

        transform.SetParent(puntoCarga);

        // El jugador es el objeto con el CharacterController (se usa para medir su velocidad)
        CharacterController cc = puntoCarga.GetComponentInParent<CharacterController>();
        jugador = cc != null ? cc.transform : puntoCarga.root;
        ultimaPosJugador = jugador.position;
        fase = 0f;
        factorMovimiento = 0f;
        offsetBalanceo = Vector3.zero;

        AplicarPoseDeCarga();
    }

    // Lo llama CargarCuerpo: el cuerpo vuelve a ser un objeto físico en el mundo.
    public void Soltar(Vector3 posicion)
    {
        cargado = false;

        transform.SetParent(null);
        transform.position = posicion;

        foreach (Collider c in colliders)
            c.enabled = true;

        rb.isKinematic = false;
    }
}