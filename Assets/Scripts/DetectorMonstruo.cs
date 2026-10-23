using UnityEngine;

// Script independiente: se agrega como componente aparte en el mismo
// GameObject del jugador (junto a FirstPersonController), sin modificarlo.
public class DetectorMonstruo : MonoBehaviour
{
    [Header("Referencias")]
    public Camera cam;
    public targetMonstruo targetMonstruoRef;
    public LanternController linterna;
    public Monstruo monstruo; // si queda vacío se busca solo en la escena

    [Header("Configuracion")]
    [Tooltip("Rango de respaldo si no hay linterna asignada")]
    public float distanciaRaycast = 20f;

    private Collider monstruoCollider;
    private bool iluminando = false;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        if (monstruo == null)
            monstruo = FindObjectOfType<Monstruo>();

        if (monstruo != null)
            monstruoCollider = monstruo.GetComponentInChildren<Collider>();
    }

    void Update()
    {
        DetectarMonstruo();
    }

    void DetectarMonstruo()
    {
        bool ilumina = EstaIluminandoMonstruo();

        if (ilumina)
        {
            if (targetMonstruoRef != null)
                targetMonstruoRef.FijarPosicion(monstruo.transform.position);
            iluminando = true;
        }
        else if (iluminando)
        {
            if (targetMonstruoRef != null)
                targetMonstruoRef.LiberarPosicion();
            iluminando = false;
        }
    }

    // El monstruo está iluminado si está dentro del CONO de la linterna (no solo en el centro de la
    // mira) y ningún objeto (árbol, pared) se interpone entre la cámara y él.
    bool EstaIluminandoMonstruo()
    {
        if (monstruo == null || cam == null)
            return false;

        if (linterna == null || !linterna.EstaEnModoAlto())
            return false;

        Vector3 origen = cam.transform.position;
        Vector3 objetivo = monstruoCollider != null ? monstruoCollider.bounds.center : monstruo.transform.position;
        Vector3 haciaMonstruo = objetivo - origen;
        float distancia = haciaMonstruo.magnitude;

        float rango = linterna.RangoActual() > 0f ? linterna.RangoActual() : distanciaRaycast;
        if (distancia > rango)
            return false;

        // Dentro del ángulo del cono (spotAngle es el ángulo total, se usa la mitad)
        float mitadAngulo = linterna.AnguloActual() * 0.5f;
        if (Vector3.Angle(cam.transform.forward, haciaMonstruo) > mitadAngulo)
            return false;

        // Línea de visión: lo primero que golpea el rayo debe ser el monstruo
        RaycastHit hit;
        if (Physics.Raycast(origen, haciaMonstruo.normalized, out hit, distancia + 0.5f, ~0, QueryTriggerInteraction.Ignore))
            return hit.collider.GetComponentInParent<Monstruo>() == monstruo;

        return false;
    }
}