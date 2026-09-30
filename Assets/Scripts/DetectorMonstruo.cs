using UnityEngine;

// Script independiente: se agrega como componente aparte en el mismo
// GameObject del jugador (junto a FirstPersonController), sin modificarlo.
public class DetectorMonstruo : MonoBehaviour
{
    [Header("Referencias")]
    public Camera cam;
    public targetMonstruo targetMonstruoRef;
    public LanternController linterna;

    [Header("Configuracion")]
    public float distanciaRaycast = 20f;

    // Guardamos a qué monstruo le estamos "fijando" el target,
    // para poder liberarlo cuando dejemos de mirarlo.
    private Monstruo monstruoActual;

    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    void Update()
    {
        DetectarMonstruo();
    }

    void DetectarMonstruo()
    {
        bool linternaEnModoAlto = linterna != null && linterna.EstaEnModoAlto();

        RaycastHit hit;
        bool golpeoMonstruo = false;

        if (linternaEnModoAlto && cam != null &&
            Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, distanciaRaycast))
        {
            Monstruo monstruo = hit.collider.GetComponentInParent<Monstruo>();

            if (monstruo != null)
            {
                golpeoMonstruo = true;

                if (targetMonstruoRef != null)
                {
                    targetMonstruoRef.FijarPosicion(monstruo.transform.position);
                }

                monstruoActual = monstruo;
            }
        }

        if (!golpeoMonstruo && monstruoActual != null)
        {
            if (targetMonstruoRef != null)
            {
                targetMonstruoRef.LiberarPosicion();
            }

            monstruoActual = null;
        }
    }
}
