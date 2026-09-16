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