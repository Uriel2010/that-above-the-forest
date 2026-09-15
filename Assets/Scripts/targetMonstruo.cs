using UnityEngine;

public class targetMonstruo : MonoBehaviour
{
   public Transform playerPosition;

    private bool posicionFijada = false;

    void Start()
    {
        
    }

    void Update()
    {
        if (!posicionFijada)
        {
            transform.position = playerPosition.position;
        }
    }

    // Llamado desde el raycast del jugador cuando detecta al monstruo.
    public void FijarPosicion(Vector3 posicion)
    {
        posicionFijada = true;
        transform.position = posicion;
    }

    public void LiberarPosicion()
    {
        posicionFijada = false;
    }
}