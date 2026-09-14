using UnityEngine;

public class RecorridoPersonaje : MonoBehaviour
{
    public enum Etapa { Parque, Tumba }
    public Etapa etapaActual = Etapa.Parque;
    int estoy = 0;

    public float distanciaDeteccion = 1f;

    public Transform[] posicionesParque; // 26 elementos, en orden
    public Transform[] posicionesTumba;  // 16 elementos, en orden

    int[][] puedoIrParque = new int[][] {
        new int[]{1,2,3},
        new int[]{4,5,6},
        new int[]{7,8,9},
        new int[]{10,11,12},
        new int[]{13,14}, new int[]{13,14}, new int[]{13,14},
        new int[]{15,16}, new int[]{15,16}, new int[]{15,16},
        new int[]{17,18}, new int[]{17,18}, new int[]{17,18},
        new int[]{19,20}, new int[]{19,20}, new int[]{19,20},
        new int[]{21,22}, new int[]{21,22}, new int[]{21,22},
        new int[]{23}, new int[]{23},
        new int[]{24}, new int[]{24},
        new int[]{25}, new int[]{25},
        new int[]{}
    };

    int[][] puedoIrTumba = new int[][] {
        new int[]{1,2},
        new int[]{3,4,5},
        new int[]{6,7,8},
        new int[]{9,10}, new int[]{9,10}, new int[]{9,10},
        new int[]{11,12}, new int[]{11,12}, new int[]{11,12},
        new int[]{13}, new int[]{13},
        new int[]{14}, new int[]{14},
        new int[]{15}, new int[]{15},
        new int[]{}
    };

    void Start()
    {
        GetComponent<SphereCollider>().radius = distanciaDeteccion;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            AvanzarSiguiente();
    }

    void AvanzarSiguiente()
    {
        int[][] tabla = etapaActual == Etapa.Parque ? puedoIrParque : puedoIrTumba;
        int[] opciones = tabla[estoy];

        if (opciones.Length == 0)
        {
            if (etapaActual == Etapa.Parque)
            {
                etapaActual = Etapa.Tumba;
                estoy = 0;
            }
            else
            {
                return;
            }
        }
        else
        {
            estoy = opciones[Random.Range(0, opciones.Length)];
        }

        Transform[] posiciones = etapaActual == Etapa.Parque ? posicionesParque : posicionesTumba;
        transform.position = posiciones[estoy].position;
    }
}
