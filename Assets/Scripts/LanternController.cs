using UnityEngine;

public class LanternController : MonoBehaviour
{
    [SerializeField] private Light lantern;

    [SerializeField] private float intensidadAlta = 15f;
    [SerializeField] private float intensidadBaja = 3f;
    [SerializeField] private float rangoAlto = 20f;
    [SerializeField] private float rangoBajo = 10f;
    [SerializeField] private float anguloAlto = 100f;
    [SerializeField] private float anguloBajo = 90f;

    private bool modoAlto = true;

    void Start()
    {
        lantern.intensity = intensidadBaja;
        lantern.range = rangoBajo;
        lantern.spotAngle = anguloBajo;
    }

    void Update()
    {
        if (lantern == null)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            modoAlto = !modoAlto;

            if (modoAlto)
            {
                lantern.intensity = intensidadAlta;
                lantern.range = rangoAlto;
                lantern.spotAngle = anguloAlto;
            }
            else
            {
                lantern.intensity = intensidadBaja;
                lantern.range = rangoBajo;
                lantern.spotAngle = anguloBajo;
            }

            Debug.Log("Intensidad de linterna: " + lantern.intensity);
        }
    }
}