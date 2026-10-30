using System.Collections;
using UnityEngine;


public class Puerta : MonoBehaviour
{
    public Transform jugador;
    public GameObject cartel;
    public float distancia = 2f;
    public KeyCode tecla = KeyCode.O;
    public float anguloApertura = 90f;
    public float velocidad = 3f;

    Quaternion cerrada, abierta;
    bool estaAbierta, moviendo;

    void Start()
    {
        cerrada = transform.localRotation;
        abierta = cerrada * Quaternion.Euler(0, anguloApertura, 0);
        cartel.SetActive(false);
    }

    void Update()
    {
        bool cerca = Vector3.Distance(jugador.position, transform.position) <= distancia;
        cartel.SetActive(cerca && !moviendo);

        if (cerca && Input.GetKeyDown(tecla)) Alternar();
        float d = Vector3.Distance(jugador.position, transform.position);
Debug.Log("Distancia: " + d);
    }

    void Alternar()
    {
        if (moviendo) return;
        StartCoroutine(Rotar(estaAbierta ? cerrada : abierta));
        estaAbierta = !estaAbierta;
    }

    IEnumerator Rotar(Quaternion destino)
    {
        moviendo = true;
        while (Quaternion.Angle(transform.localRotation, destino) > 0.1f)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, destino, Time.deltaTime * velocidad);
            yield return null;
        }
        transform.localRotation = destino;
        moviendo = false;
    }
}
