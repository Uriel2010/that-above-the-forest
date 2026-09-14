using UnityEngine;
using TMPro;

public class LanternController : MonoBehaviour
{
    [SerializeField] private Light lantern;

    [SerializeField] private float intensidadAlta = 6f;
    [SerializeField] private float intensidadBaja = 3f;
    [SerializeField] private float rangoAlto = 20f;
    [SerializeField] private float rangoBajo = 10f;
    [SerializeField] private float anguloAlto = 100f;
    [SerializeField] private float anguloBajo = 90f;

    [SerializeField] private float tiempoMaximo = 100f;

    // Texto del contador
    [SerializeField] private TMP_Text textoContador;

    [Header("Detección del monstruo")]
    [SerializeField] private Transform monstruo;             // Transform del monstruo
    [SerializeField] private Monstruo monstruoScript;         // Script del monstruo (para congelarlo)
    [SerializeField] private targetMonstruo target;           // Script del objetivo (para reposicionarlo)
    [Tooltip("Capas que bloquean la luz (paredes, etc). NO incluir la capa del monstruo.")]
    [SerializeField] private LayerMask capasObstaculo;

    private float tiempoRestante;
    private bool modoAlto = false;
    private bool monstruoDetectado = false;

    void Start()
    {
        tiempoRestante = tiempoMaximo;

        lantern.intensity = intensidadBaja;
        lantern.range = rangoBajo;
        lantern.spotAngle = anguloBajo;

        ActualizarContador();
    }

    void Update()
    {
        if (lantern == null)
            return;

        // Click derecho
        if (Input.GetMouseButtonDown(1))
        {
            // Pasar a intensidad alta
            if (!modoAlto && tiempoRestante > 0)
            {
                modoAlto = true;

                lantern.intensity = intensidadAlta;
                lantern.range = rangoAlto;
                lantern.spotAngle = anguloAlto;
            }
            // Pasar a intensidad baja
            else if (modoAlto)
            {
                modoAlto = false;

                lantern.intensity = intensidadBaja;
                lantern.range = rangoBajo;
                lantern.spotAngle = anguloBajo;
            }
        }

        // Consumir tiempo mientras está en intensidad alta
        if (modoAlto)
        {
            tiempoRestante -= Time.deltaTime;

            if (tiempoRestante <= 0)
            {
                tiempoRestante = 0;
                modoAlto = false;

                lantern.intensity = intensidadBaja;
                lantern.range = rangoBajo;
                lantern.spotAngle = anguloBajo;
            }

            ActualizarContador();
        }

        DetectarMonstruo();
    }

    void DetectarMonstruo()
    {
        if (monstruo == null)
            return;

        bool detectadoAhora = modoAlto && EstaMonstruoEnConoDeLuz();

        if (detectadoAhora && !monstruoDetectado)
        {
            monstruoDetectado = true;

            if (target != null)
                target.SetCongelado(true);

            if (monstruoScript != null)
                monstruoScript.Congelar();
        }
        else if (!detectadoAhora && monstruoDetectado)
        {
            monstruoDetectado = false;

            if (target != null)
                target.SetCongelado(false);

            if (monstruoScript != null)
                monstruoScript.Descongelar();
        }
    }

    bool EstaMonstruoEnConoDeLuz()
    {
        Vector3 origen = lantern.transform.position;
        Vector3 direccionHaciaMonstruo = monstruo.position - origen;
        float distancia = direccionHaciaMonstruo.magnitude;

        // Fuera del alcance
        if (distancia > lantern.range)
            return false;

        // Fuera del cono (ángulo del spot)
        float anguloHaciaMonstruo = Vector3.Angle(lantern.transform.forward, direccionHaciaMonstruo);
        if (anguloHaciaMonstruo > lantern.spotAngle / 2f)
            return false;

        // Línea de visión: algo bloquea el camino
        if (Physics.Raycast(origen, direccionHaciaMonstruo.normalized, out RaycastHit hit, distancia, capasObstaculo))
            return false;

        return true;
    }

    void ActualizarContador()
    {
        if (textoContador != null)
        {
            textoContador.text = Mathf.CeilToInt(tiempoRestante).ToString();
        }
    }
<<<<<<< Updated upstream
}
=======
    public bool EstaEnModoAlto() { return modoAlto; }
}
>>>>>>> Stashed changes
