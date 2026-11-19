using UnityEngine;

public class targetMonstruo : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerPosition;
    public Monstruo monstruo; // si queda vacío se busca solo en la escena

    [Header("Configuración")]
    public float distanciaDetras = 1f;        // cuánto atraviesa la línea al árbol
    public float radioOcultamiento = 35f;       // radio alrededor del jugador donde empieza a esconderse
    public float distanciaPersecucion = 10f;    // a esta distancia o menos persigue directo
    public float distanciaLlegada = 0.2f;         // distancia para considerar que el monstruo "llegó"
    public float tiempoGracia = 1.5f;             // segundos que sigue escondiéndose después de perder la luz
    public float distanciaMinimaEntreArboles = 5f; // el árbol siguiente debe estar al menos a esta distancia del anterior; si no, persigue directo
    public float distanciaAtaque = 10f;          // a esta distancia o menos (y sin estar iluminado) pasa a la animación de ataque
    public float distanciaHuida = 25f;
    public float distanciaLlegadaHuida = 1.5f;  // la huida termina al llegar a esta distancia del escondite del árbol de huida
    public float tiempoMaxHuida = 15f;          // seguridad: la huida termina sí o sí después de estos segundos
    [Header("Atascos (si ocurre, persigue directo)")]
    public float tiempoMaxQuieto = 10f;         // segundos sin moverse
    public float radioQuieto = 0.5f;            // cuánto tiene que alejarse de su posición para considerar que se movió
    public float tiempoMaxHaciaArbol = 20f;     // segundos yendo hacia el mismo arbolSiguiente          // al ser iluminado, se aleja hasta estar al menos a esta distancia del jugador

    [Header("Árboles (solo lectura, para debug)")]
    public Transform arbolCercaMonstruo;
    public Transform arbolSiguiente;
    public Transform arbolLejosPlayer;
    public Transform arbolHuida;

    private enum Fase { Lejos, Siguiente }
    private Fase fase = Fase.Lejos;
    private bool rutaActiva = false;
    private float tiempoHuyendo = 0f;
    private bool huyendo = false;               // true mientras se aleja del jugador tras ser iluminado

    private bool posicionFijada = false;        // true = monstruo iluminado
    private Vector3 ultimaPosMonstruo;
    private float tiempoGraciaRestante = 0f;

    private Transform[] arboles;
    private Transform ultimoArbolLog;
    private bool avisoPersecucionDirecta = false;

    // Detección de atascos
    private bool persecucionDirecta = false;    // true = ignora los árboles y persigue al jugador hasta que lo iluminen
    private Vector3 posicionReferencia;
    private float tiempoQuieto = 0f;
    private Transform arbolContado;
    private float tiempoHaciaArbol = 0f;

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

        // Animación: ataca si está cerca y NO está iluminado, escondiéndose ni huyendo.
        // Si lo iluminan a tiempo, vuelve a la animación de caminar.
        bool escondido = posicionFijada || huyendo || tiempoGraciaRestante > 0f;
        bool cercaParaAtacar = DistHoriz(monstruo.transform.position, playerPosition.position) <= distanciaAtaque;
        monstruo.Atacar(!escondido && cercaParaAtacar);

        // 1) Huida: tras ser iluminado, se aleja hasta estar a "distanciaHuida" del jugador.
        //    Sigue huyendo aunque la luz ya no lo toque.
        if (huyendo)
        {
            tiempoHuyendo += Time.deltaTime;

            bool lejosSuficiente = DistHoriz(monstruo.transform.position, playerPosition.position) >= distanciaHuida;
            bool llegoAlEscondite = arbolHuida != null &&
                DistHoriz(monstruo.transform.position, PosicionDetras(arbolHuida)) <= distanciaLlegadaHuida;
            bool demasiadoTiempo = tiempoHuyendo >= tiempoMaxHuida;

            // La huida termina al estar lo bastante lejos, o al llegar al árbol de huida (aunque el
            // jugador se haya acercado), o si tarda demasiado. Después sigue el comportamiento normal.
            if (lejosSuficiente || llegoAlEscondite || demasiadoTiempo)
            {
                huyendo = false;
                arbolHuida = null;
            }
            else
            {
                IrA(arbolHuida);
                rutaActiva = false;
                persecucionDirecta = false;
                return;
            }
        }

        // 2) Iluminado ya lejos (o dentro del tiempo de gracia): se esconde detrás del árbol más cercano a él
        if (posicionFijada || tiempoGraciaRestante > 0f)
        {
            ActualizarIluminado();
            rutaActiva = false;
            persecucionDirecta = false;
            return;
        }

        // 3) No iluminado y cerca del jugador: persecución directa
        if (DistHoriz(monstruo.transform.position, playerPosition.position) <= distanciaPersecucion)
        {
            transform.position = playerPosition.position;
            rutaActiva = false;
            return;
        }

        // 4) No iluminado: avanza de árbol en árbol hacia el jugador
        ActualizarOculto();
    }

    // Llamado desde DetectorMonstruo cuando la linterna ilumina al monstruo.
    public void FijarPosicion(Vector3 posicion)
    {
        // Al pasar de "no iluminado" a "iluminado" (y sin estar ya escondiéndose o huyendo),
        // el monstruo frena por completo antes de cambiar de dirección.
        if (!posicionFijada && !huyendo && tiempoGraciaRestante <= 0f && monstruo != null)
            monstruo.Frenar();

        // Al ser iluminado estando a menos de "distanciaHuida", empieza la huida
        if (!posicionFijada && !huyendo && monstruo != null && playerPosition != null &&
            DistHoriz(monstruo.transform.position, playerPosition.position) < distanciaHuida)
        {
            arbolHuida = ElegirArbolHuida();
            huyendo = arbolHuida != null;
            tiempoHuyendo = 0f;

            if (!huyendo)
                Debug.LogWarning("targetMonstruo: no hay árboles a " + distanciaHuida + " unidades o más del jugador para huir.");
        }

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
        // Atascado: se olvida de los árboles y va directo al jugador hasta que lo iluminen de nuevo
        if (persecucionDirecta)
        {
            transform.position = playerPosition.position;
            return;
        }

        if (!rutaActiva)
            IniciarRuta();

        if (HayAtasco())
        {
            persecucionDirecta = true;
            transform.position = playerPosition.position;
            return;
        }

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

        posicionReferencia = monstruo.transform.position;
        tiempoQuieto = 0f;
        arbolContado = null;
        tiempoHaciaArbol = 0f;
    }

    // true si el monstruo lleva demasiado tiempo en el mismo lugar o yendo hacia el mismo arbolSiguiente
    bool HayAtasco()
    {
        // 1) Sin moverse: se considera que se movió si se alejó "radioQuieto" de su posición de referencia
        Vector3 pos = monstruo.transform.position;
        if (DistHoriz(pos, posicionReferencia) > radioQuieto)
        {
            posicionReferencia = pos;
            tiempoQuieto = 0f;
        }
        else
        {
            tiempoQuieto += Time.deltaTime;
            if (tiempoQuieto >= tiempoMaxQuieto)
            {
                Debug.Log("targetMonstruo: el monstruo estuvo " + tiempoMaxQuieto + " s sin moverse, persigue directo.");
                return true;
            }
        }

        // 2) Demasiado tiempo yendo hacia el mismo arbolSiguiente (el contador se reinicia con cada árbol nuevo)
        if (fase == Fase.Siguiente && arbolSiguiente != null)
        {
            if (arbolSiguiente != arbolContado)
            {
                arbolContado = arbolSiguiente;
                tiempoHaciaArbol = 0f;
            }
            else
            {
                tiempoHaciaArbol += Time.deltaTime;
                if (tiempoHaciaArbol >= tiempoMaxHaciaArbol)
                {
                    Debug.Log("targetMonstruo: " + tiempoMaxHaciaArbol + " s yendo hacia '" + arbolSiguiente.name + "', persigue directo.");
                    return true;
                }
            }
        }
        else
        {
            arbolContado = null;
            tiempoHaciaArbol = 0f;
        }

        return false;
    }

    // Si no hay árbol siguiente, va directo al jugador.
    void IrA(Transform arbol)
    {
        if (arbol == null)
        {
            // Se avisa una sola vez (no en cada frame)
            if (!avisoPersecucionDirecta)
            {
                avisoPersecucionDirecta = true;
                Debug.Log("targetMonstruo: no hay un árbol siguiente a " + distanciaMinimaEntreArboles + " unidades o más del anterior (y más cerca del jugador), el monstruo persigue directo.");
            }
            transform.position = playerPosition.position;
            return;
        }
        avisoPersecucionDirecta = false;

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

    // Árbol más cercano al monstruo que esté a "distanciaHuida" o más del jugador, preferentemente
    // del mismo lado que el monstruo (para que no tenga que pasar cerca del jugador al huir).
    Transform ElegirArbolHuida()
    {
        Vector3 posMonstruo = monstruo.transform.position;
        Vector3 dirMonstruo = posMonstruo - playerPosition.position;
        dirMonstruo.y = 0f;
        dirMonstruo.Normalize();

        Transform mejorMismoLado = null, mejorCualquiera = null;
        float distMismoLado = float.MaxValue, distCualquiera = float.MaxValue;

        foreach (Transform a in arboles)
        {
            if (a == null) continue;
            if (DistHoriz(a.position, playerPosition.position) < distanciaHuida) continue;

            float d = DistHoriz(a.position, posMonstruo);

            if (d < distCualquiera)
            {
                distCualquiera = d;
                mejorCualquiera = a;
            }

            Vector3 dirArbol = a.position - playerPosition.position;
            dirArbol.y = 0f;
            if (Vector3.Dot(dirMonstruo, dirArbol.normalized) > 0f && d < distMismoLado)
            {
                distMismoLado = d;
                mejorMismoLado = a;
            }
        }

        return mejorMismoLado != null ? mejorMismoLado : mejorCualquiera;
    }

    // Árbol más cercano a "actual" que esté más cerca del jugador que "actual" y a al menos
    // "distanciaMinimaEntreArboles" de él. Si no hay ninguno, devuelve null (persecución directa).
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
            if (d < distanciaMinimaEntreArboles) continue; // demasiado cerca del árbol anterior: no vale la pena pasar por ahí
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