using UnityEngine;

public class targetMonstruo : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerPosition;
    public Monstruo monstruo; // si queda vacío se busca solo en la escena

    [Header("Configuración")]
    public float distanciaDetras = 0.5f;        // cuánto atraviesa la línea al árbol
    public float radioOcultamiento = 35f;       // radio alrededor del jugador donde empieza a esconderse
    public float distanciaPersecucion = 10f;    // a esta distancia o menos persigue directo
    public float distanciaLlegada = 1f;         // distancia para considerar que el monstruo "llegó"
    public float tiempoGracia = 5f;             // segundos que sigue escondiéndose después de perder la luz

    [Header("Árboles (solo lectura, para debug)")]
    public Transform arbolCercaMonstruo;
    public Transform arbolSiguiente;
    public Transform arbolLejosPlayer;

    private enum Fase { Lejos, Siguiente }
    private Fase fase = Fase.Lejos;
    private bool rutaActiva = false;

    private bool posicionFijada = false;        // true = monstruo iluminado
    private Vector3 ultimaPosMonstruo;
    private float tiempoGraciaRestante = 0f;

    private Transform[] arboles;
    private Transform ultimoArbolLog;

    void Start()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("arbol");
        arboles = new Transform[objs.Length];
        for (int i = 0; i < objs.Length; i++)
            arboles[i] = objs[i].transform;

        if (monstruo == null)
            monstruo = FindObjectOfType<Monstruo>();
    }

    void Update()
    {
        if (playerPosition == null)
            return;

        if (monstruo == null)
        {
            transform.position = playerPosition.position;
            return;
        }

        // Mientras está iluminado el contador se mantiene lleno; al perder la luz empieza a bajar
        if (posicionFijada)
            tiempoGraciaRestante = tiempoGracia;
        else if (tiempoGraciaRestante > 0f)
            tiempoGraciaRestante -= Time.deltaTime;

        // 1) Iluminado (o dentro del tiempo de gracia): se esconde detrás del árbol más cercano a él
        if (posicionFijada || tiempoGraciaRestante > 0f)
        {
            ActualizarIluminado();
            rutaActiva = false;
            return;
        }

        // 2) No iluminado y cerca del jugador: persecución directa
        if (DistHoriz(monstruo.transform.position, playerPosition.position) <= distanciaPersecucion)
        {
            transform.position = playerPosition.position;
            rutaActiva = false;
            return;
        }

        // 3) No iluminado: avanza de árbol en árbol hacia el jugador
        ActualizarOculto();
    }

    // Llamado desde DetectorMonstruo cuando la linterna ilumina al monstruo.
    public void FijarPosicion(Vector3 posicion)
    {
        posicionFijada = true;
        ultimaPosMonstruo = posicion;
    }

    public void LiberarPosicion()
    {
        posicionFijada = false;
    }

    // ---------- Monstruo iluminado ----------
    void ActualizarIluminado()
    {
        // Se usa la posición actual del monstruo para que el árbol se actualice también durante la gracia
        Vector3 posMonstruo = monstruo.transform.position;
        arbolCercaMonstruo = ArbolMasCercanoA(posMonstruo);

        if (arbolCercaMonstruo == null)
        {
            transform.position = posMonstruo; // sin árboles: se queda quieto
            return;
        }

        transform.position = PosicionDetras(arbolCercaMonstruo);
    }

    // ---------- Monstruo en la oscuridad ----------
    void ActualizarOculto()
    {
        if (!rutaActiva)
            IniciarRuta();

        if (arbolLejosPlayer == null)
        {
            transform.position = playerPosition.position;
            return;
        }

        if (fase == Fase.Lejos)
        {
            IrA(arbolLejosPlayer);

            if (Llego())
            {
                // Se recalcula ahora, con la posición actual del jugador (el que se calculó
                // al iniciar la ruta puede haber quedado obsoleto si el jugador se movió).
                fase = Fase.Siguiente;
                arbolSiguiente = SiguienteMasCercanoAlJugador(arbolLejosPlayer);
                IrA(arbolSiguiente);
            }
        }
        else
        {
            IrA(arbolSiguiente);

            if (Llego())
            {
                arbolSiguiente = SiguienteMasCercanoAlJugador(arbolSiguiente);
                IrA(arbolSiguiente);
            }
        }
    }

    void IniciarRuta()
    {
        fase = Fase.Lejos;
        arbolLejosPlayer = ElegirArbolLejosPlayer();
        arbolSiguiente = SiguienteMasCercanoAlJugador(arbolLejosPlayer);
        rutaActiva = true;
    }

    // Si no hay árbol siguiente, va directo al jugador.
    void IrA(Transform arbol)
    {
        if (arbol == null)
        {
            Debug.LogWarning("targetMonstruo: no quedan árboles más cerca del jugador, el monstruo va directo hacia él.");
            transform.position = playerPosition.position;
            return;
        }

        if (arbol != ultimoArbolLog)
        {
            ultimoArbolLog = arbol;
            Debug.Log($"targetMonstruo: nuevo destino -> {arbol.name} | dist. al jugador: {DistHoriz(arbol.position, playerPosition.position):F1} | dist. del monstruo al jugador: {DistHoriz(monstruo.transform.position, playerPosition.position):F1}");
        }
        transform.position = PosicionDetras(arbol);
    }

    bool Llego()
    {
        return DistHoriz(monstruo.transform.position, transform.position) <= distanciaLlegada;
    }

    // ---------- Búsqueda de árboles ----------

    // Árbol más cercano al monstruo y, a la vez, cercano al borde del radio de 35 u del jugador.
    // Si el monstruo ya está dentro del radio, simplemente el más cercano al monstruo.
    Transform ElegirArbolLejosPlayer()
    {
        Vector3 posMonstruo = monstruo.transform.position;
        bool fueraDelRadio = DistHoriz(posMonstruo, playerPosition.position) > radioOcultamiento;

        Transform mejor = null;
        float mejorPuntaje = float.MaxValue;

        foreach (Transform a in arboles)
        {
            if (a == null) continue;

            float puntaje = DistHoriz(a.position, posMonstruo);
            if (fueraDelRadio)
                puntaje += Mathf.Abs(DistHoriz(a.position, playerPosition.position) - radioOcultamiento);

            if (puntaje < mejorPuntaje)
            {
                mejorPuntaje = puntaje;
                mejor = a;
            }
        }
        return mejor;
    }

    // Árbol más cercano a "actual" que esté más cerca del jugador que "actual".
    Transform SiguienteMasCercanoAlJugador(Transform actual)
    {
        if (actual == null) return null;

        float distActualJugador = DistHoriz(actual.position, playerPosition.position);
        Transform mejor = null;
        float mejorDist = float.MaxValue;

        foreach (Transform a in arboles)
        {
            if (a == null || a == actual) continue;
            if (DistHoriz(a.position, playerPosition.position) >= distActualJugador) continue;

            float d = DistHoriz(a.position, actual.position);
            if (d < mejorDist)
            {
                mejorDist = d;
                mejor = a;
            }
        }
        return mejor;
    }

    Transform ArbolMasCercanoA(Vector3 pos)
    {
        Transform mejor = null;
        float mejorDist = float.MaxValue;

        foreach (Transform a in arboles)
        {
            if (a == null) continue;
            float d = DistHoriz(a.position, pos);
            if (d < mejorDist)
            {
                mejorDist = d;
                mejor = a;
            }
        }
        return mejor;
    }

    // ---------- Utilidades ----------

    // Punto de la línea jugador -> árbol, "distanciaDetras" después del árbol.
    Vector3 PosicionDetras(Transform arbol)
    {
        Vector3 dir = arbol.position - playerPosition.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = playerPosition.forward;

        Vector3 pos = arbol.position + dir.normalized * distanciaDetras;
        pos.y = monstruo.transform.position.y; // mantiene la altura del monstruo (NavMesh)

        Debug.DrawLine(playerPosition.position, pos, Color.red);
        return pos;
    }

    float DistHoriz(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}