using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    [SerializeField] private Image Barra_Lintera;

    // Texto del contador
    [SerializeField] private TMP_Text textoContador;

    private float tiempoRestante;
    private bool modoAlto = false;

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

        if (Input.GetMouseButtonDown(1))
        {
            if (!modoAlto && tiempoRestante > 0)
            {
                modoAlto = true;

                lantern.intensity = intensidadAlta;
                lantern.range = rangoAlto;
                lantern.spotAngle = anguloAlto;
            }

            else if (modoAlto)
            {
                modoAlto = false;

                lantern.intensity = intensidadBaja;
                lantern.range = rangoBajo;
                lantern.spotAngle = anguloBajo;
            }
        }

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
    }

    void ActualizarContador()
    {
        {
    if (textoContador != null)
        textoContador.text = Mathf.CeilToInt(tiempoRestante).ToString();

    if (Barra_Lintera!= null)
        Barra_Lintera.fillAmount = tiempoRestante / tiempoMaximo;
}
    }
    public bool EstaEnModoAlto() { return modoAlto; }

    // Datos actuales de la luz, para que otros scripts puedan calcular el cono de iluminación
    public float RangoActual() { return lantern != null ? lantern.range : 0f; }
    public float AnguloActual() { return lantern != null ? lantern.spotAngle : 0f; }

}