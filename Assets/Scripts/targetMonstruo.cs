using UnityEngine;

public class targetMonstruo : MonoBehaviour
{
   public Transform playerPosition; 

    void Start()
    {
        
    }

    void Update()
    {
        transform.position = playerPosition.position;
    }
}
