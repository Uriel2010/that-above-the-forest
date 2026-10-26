using UnityEngine;

// Se agrega al objeto que representa el cuerpo (por ahora un cubo).
// Necesita un Collider y un Rigidbody.
[RequireComponent(typeof(Rigidbody))]
public class CuerpoCargable : MonoBehaviour
{
    private Rigidbody rb;
    private Collider[] colliders;
    private bool cargado = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
    }

    public bool EstaCargado() { return cargado; }

    // Lo llama CargarCuerpo: el cuerpo pasa a seguir el punto de carga del jugador.
    public void Cargar(Transform puntoCarga)
    {
        cargado = true;

        // Sin física ni colisiones mientras se carga, para que no empuje al jugador ni tape la luz
        rb.isKinematic = true;
        foreach (Collider c in colliders)
            c.enabled = false;

        transform.SetParent(puntoCarga);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
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
