using UnityEngine;

public class Linterna : MonoBehaviour
{
    public Light luz;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            luz.enabled = !luz.enabled;
        }
    }
}