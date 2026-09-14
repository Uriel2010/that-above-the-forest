using UnityEngine;

public class targetMonstruo : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerPosition;
    public Transform monstruo;   

    private bool congelado = false;

    void Start()
    {
        transform.position = playerPosition.position;
    }

    void Update()
    {
        monstruo = GetComponent<UnityEngine.AI.NavMeshAgent>();
        transform.position = congelado ? monstruo.position : playerPosition.position;
    }

    public void SetCongelado(bool valor)
    {
        congelado = valor;
    }
}
