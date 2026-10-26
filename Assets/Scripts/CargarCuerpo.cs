using System.Reflection;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

// Se agrega al GameObject del jugador (junto a FirstPersonController y DetectorMonstruo).
// No toca la linterna: se puede cargar el cuerpo y usar la luz (clic derecho) a la vez.
public class CargarCuerpo : MonoBehaviour
{
    [Header("Referencias")]
    public Camera cam;
    public FirstPersonController controlador;
    [Tooltip("Dónde queda el cuerpo mientras se carga. Si queda vacío se crea uno delante de la cámara.")]
    public Transform puntoCarga;

    [Header("Configuración")]
    public KeyCode teclaCargar = KeyCode.E;
    public float distanciaAgarre = 2.5f;
    [Tooltip("Qué tan cerca de la mira tiene que estar el cuerpo (grados desde el centro de la pantalla)")]
    public float anguloAgarre = 45f;
    [Range(0.1f, 1f)] public float multiplicadorCorrer = 0.5f; // al correr con el cuerpo, velocidad = normal x esto
    public float distanciaSoltar = 1.2f;                        // a qué distancia delante del jugador se suelta

    private CuerpoCargable cuerpoActual;

    // Se accede a las velocidades del FirstPersonController por reflexión, así este script
    // funciona con cualquier versión del controlador sin tener que modificarlo.
    private FieldInfo campoRun;
    private FieldInfo campoWalk;
    private float velocidadCorrerOriginal;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        if (controlador == null)
            controlador = GetComponent<FirstPersonController>();

        if (controlador != null)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            campoRun = typeof(FirstPersonController).GetField("m_RunSpeed", flags);
            campoWalk = typeof(FirstPersonController).GetField("m_WalkSpeed", flags);

            if (campoRun == null || campoWalk == null)
                Debug.LogWarning("CargarCuerpo: no se encontraron m_RunSpeed / m_WalkSpeed en FirstPersonController.");
        }

        if (puntoCarga == null && cam != null)
        {
            GameObject punto = new GameObject("PuntoCarga");
            punto.transform.SetParent(cam.transform);
            punto.transform.localPosition = new Vector3(0f, -0.6f, 1.1f);
            punto.transform.localRotation = Quaternion.identity;
            puntoCarga = punto.transform;
        }
    }

    void Update()
    {
        if (!Input.GetKeyDown(teclaCargar))
            return;

        if (cuerpoActual == null)
            IntentarCargar();
        else
            Soltar();
    }

    void IntentarCargar()
    {
        if (cam == null || puntoCarga == null)
            return;

        Vector3 origen = cam.transform.position;
        Collider[] cercanos = Physics.OverlapSphere(origen, distanciaAgarre, ~0, QueryTriggerInteraction.Ignore);

        CuerpoCargable mejor = null;
        float mejorAngulo = anguloAgarre;

        foreach (Collider c in cercanos)
        {
            CuerpoCargable cuerpo = c.GetComponentInParent<CuerpoCargable>();
            if (cuerpo == null || cuerpo.EstaCargado())
                continue;

            // Se mide hacia el punto más cercano del collider, así no depende de qué cara se mire
            Vector3 punto = c.ClosestPoint(origen);
            Vector3 haciaCuerpo = punto - origen;
            float distancia = haciaCuerpo.magnitude;
            if (distancia > distanciaAgarre)
                continue;

            // Debe estar dentro del cono de visión (si ya está pegado, siempre cuenta)
            float angulo = distancia < 0.05f ? 0f : Vector3.Angle(cam.transform.forward, haciaCuerpo);
            if (angulo > mejorAngulo)
                continue;

            // Que no haya una pared u otro objeto en el medio
            RaycastHit hit;
            if (distancia > 0.05f &&
                Physics.Raycast(origen, haciaCuerpo.normalized, out hit, distancia + 0.05f, ~0, QueryTriggerInteraction.Ignore) &&
                hit.collider.GetComponentInParent<CuerpoCargable>() != cuerpo)
                continue;

            mejor = cuerpo;
            mejorAngulo = angulo;
        }

        if (mejor != null)
        {
            mejor.Cargar(puntoCarga);
            cuerpoActual = mejor;
            AplicarVelocidadCarga();
        }
    }

    void Soltar()
    {
        // Delante del jugador (horizontal), a la altura actual del cuerpo
        Vector3 adelante = cam.transform.forward;
        adelante.y = 0f;
        adelante = adelante.sqrMagnitude > 0.001f ? adelante.normalized : transform.forward;

        Vector3 posicion = cuerpoActual.transform.position;
        posicion = new Vector3(transform.position.x, posicion.y, transform.position.z) + adelante * distanciaSoltar;

        cuerpoActual.Soltar(posicion);
        cuerpoActual = null;

        RestaurarVelocidad();
    }

    // Reduce la velocidad de correr (nunca por debajo de la de caminar)
    void AplicarVelocidadCarga()
    {
        if (controlador == null || campoRun == null || campoWalk == null)
            return;

        velocidadCorrerOriginal = (float)campoRun.GetValue(controlador);
        float caminar = (float)campoWalk.GetValue(controlador);
        campoRun.SetValue(controlador, Mathf.Max(caminar, velocidadCorrerOriginal * multiplicadorCorrer));
    }

    void RestaurarVelocidad()
    {
        if (controlador == null || campoRun == null)
            return;

        campoRun.SetValue(controlador, velocidadCorrerOriginal);
    }

    void OnDisable()
    {
        if (cuerpoActual != null)
            RestaurarVelocidad();
    }

    public bool EstaCargando() { return cuerpoActual != null; }
}