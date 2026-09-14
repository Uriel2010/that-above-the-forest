using UnityEngine;

public class targetMonstruo : MonoBehaviour
{
   public Transform playerPosition;

    // Mientras esté en true, el objeto deja de seguir al jugador
    // y se queda fijo en la posición que le pase FijarPosicion().
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

    // Llamado cuando el jugador deja de mirar al monstruo,
    // para que vuelva a seguir al jugador normalmente.
    public void LiberarPosicion()
    {
        posicionFijada = false;
    }
}