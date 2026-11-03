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

    [Header("Cuerpo escondido (pie del arbusto)")]
    [Tooltip("El cuerpo que el jugador recibe al apretar la tecla de cargar mirando el 'pie'. Puede estar desactivado en la escena.")]
    public CuerpoCargable cuerpoDelPie;
    [Tooltip("Tag del arbusto (o de alguno de sus padres) que hay que mirar")]
    public string tagPie = "pie";
    [Tooltip("También se acepta el nombre del objeto (o de alguno de sus padres)")]
    public string nombrePie = "pie";
    [Tooltip("Si está activo, el arbusto desaparece al sacar el cuerpo")]
    public bool ocultarPieAlSacar = true;

    [Header("Configuración")]
    public KeyCode teclaCargar = KeyCode.E;
    [Tooltip("Distancia horizontal máxima desde el jugador hasta el borde del cuerpo")]
    public float distanciaAgarre = 2.5f;
    [Tooltip("Qué tan cerca de la mira tiene que estar el cuerpo (grados desde el centro de la pantalla)")]
    public float anguloAgarre = 60f;
    [Tooltip("Muestra en la consola por qué no se pudo agarrar el cuerpo")]
    public bool mostrarDebug = true;
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

        // El cuerpo del arbusto empieza oculto: aparece recién al tocar E sobre el arbusto
        if (cuerpoDelPie != null)
            cuerpoDelPie.gameObject.SetActive(false);

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

        // Mirando el pie del arbusto: lo que se tiene en las manos pasa a ser el cuerpo
        if (IntentarSacarDelPie())
            return;

        if (cuerpoActual == null)
            IntentarCargar();
        else
            Soltar();
    }

    private bool pieUsado = false;

    bool IntentarSacarDelPie()
    {
        if (pieUsado || cuerpoDelPie == null || cam == null || puntoCarga == null)
            return false;

        // Si el cuerpo ya está en las manos o ya fue enterrado, no hay nada que sacar
        if (cuerpoDelPie.EstaCargado() || cuerpoDelPie.EstaEnterrado())
            return false;

        RaycastHit hit;
        if (!Physics.SphereCast(cam.transform.position, 0.3f, cam.transform.forward, out hit, distanciaAgarre, ~0, QueryTriggerInteraction.Ignore))
            return false;

        // Se busca el "pie" (por tag o por nombre) en el objeto golpeado y en sus padres
        Transform pie = null;
        for (Transform t = hit.collider.transform; t != null; t = t.parent)
        {
            if (t.CompareTag(tagPie) || t.name == nombrePie)
            {
                pie = t;
                break;
            }
        }
        if (pie == null)
            return false;

        // Si ya tiene algo en las manos (por ejemplo la pala), lo suelta primero
        if (cuerpoActual != null)
            Soltar();

        cuerpoDelPie.gameObject.SetActive(true);
        cuerpoDelPie.Cargar(puntoCarga);
        cuerpoActual = cuerpoDelPie;
        AplicarVelocidadCarga();

        pieUsado = true;
        if (ocultarPieAlSacar)
            pie.gameObject.SetActive(false);

        Log("se sacó el cuerpo del pie");
        return true;
    }

    void IntentarCargar()
    {
        if (cam == null || puntoCarga == null)
            return;

        Vector3 origen = cam.transform.position;

        // Se buscan candidatos con margen vertical (el collider puede estar en el piso y la cámara arriba)
        Collider[] cercanos = Physics.OverlapSphere(origen, distanciaAgarre + 2f, ~0, QueryTriggerInteraction.Ignore);

        CuerpoCargable mejor = null;
        float mejorAngulo = float.MaxValue;
        bool habiaCandidato = false;

        foreach (Collider c in cercanos)
        {
            CuerpoCargable cuerpo = c.GetComponentInParent<CuerpoCargable>();
            if (cuerpo == null || cuerpo.EstaCargado() || cuerpo.EstaEnterrado())
                continue;

            habiaCandidato = true;

            // 1) Distancia HORIZONTAL desde el jugador hasta el borde más cercano del cuerpo
            Vector3 puntoCercano = c.ClosestPoint(transform.position);
            Vector3 d = puntoCercano - transform.position;
            d.y = 0f;
            if (d.magnitude > distanciaAgarre)
            {
                Log("fuera de alcance (" + d.magnitude.ToString("F1") + " > " + distanciaAgarre + ")");
                continue;
            }

            // 2) Cono de visión, medido hacia el centro del cuerpo
            Vector3 centro = c.bounds.center;
            Vector3 haciaCentro = centro - origen;
            float angulo = Vector3.Angle(cam.transform.forward, haciaCentro);
            if (angulo > anguloAgarre)
            {
                Log("fuera de la mira (" + angulo.ToString("F0") + "° > " + anguloAgarre + "°)");
                continue;
            }

            // 3) Que no haya algo (pared, árbol) en el medio. Se ignoran el propio jugador y el propio cuerpo.
            if (HayObstaculo(origen, centro, cuerpo))
                continue;

            if (angulo < mejorAngulo)
            {
                mejor = cuerpo;
                mejorAngulo = angulo;
            }
        }

        if (mejor != null)
        {
            mejor.Cargar(puntoCarga);
            cuerpoActual = mejor;
            AplicarVelocidadCarga();
        }
        else if (!habiaCandidato)
        {
            Log("no hay ningún CuerpoCargable cerca");
        }
    }

    bool HayObstaculo(Vector3 origen, Vector3 destino, CuerpoCargable cuerpo)
    {
        Vector3 dir = destino - origen;
        float dist = dir.magnitude;
        if (dist < 0.01f)
            return false;

        RaycastHit[] hits = Physics.RaycastAll(origen, dir / dist, dist, ~0, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit h in hits)
        {
            if (h.collider.transform.IsChildOf(transform)) continue;                 // el propio jugador
            if (h.collider.GetComponentInParent<CuerpoCargable>() == cuerpo) continue; // el propio cuerpo

            Log("bloqueado por " + h.collider.name);
            return true;
        }
        return false;
    }

    void Log(string mensaje)
    {
        if (mostrarDebug)
            Debug.Log("CargarCuerpo: " + mensaje);
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

    // Devuelve true si el objeto que se tiene en la mano tiene ese tag (ej: "pala")
    public bool CargaObjetoConTag(string tag)
    {
        return cuerpoActual != null && cuerpoActual.CompareTag(tag);
    }
}