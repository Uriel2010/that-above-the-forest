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
    }

    void ActualizarContador()
    {
        if (textoContador != null)
        {
            textoContador.text = Mathf.CeilToInt(tiempoRestante).ToString();
        }
    }
}