using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Se agrega al GameObject del jugador (junto a CargarCuerpo).
// Con el click izquierdo, si lleva en la mano un objeto con el tag "pala",
// baja la altura del Terrain en el punto al que mira.
public class CavarTerreno : MonoBehaviour
{
    [Header("Referencias")]
    public Camera cam;
    public CargarCuerpo cargarCuerpo;

    [Header("Configuración")]
    public string tagPala = "pala";
    public float profundidad = 0.6f;     // cuánto baja el suelo en cada cavada (unidades del mundo)
    public float radio = 1.5f;              // radio del pozo (unidades del mundo)
    public float distanciaCavar = 3f;     // a qué distancia puede cavar mirando
    public float distanciaAdelante = 1.5f; // si no mira al suelo, cava a esta distancia delante del jugador
    [Tooltip("Muestra en la consola por qué no se pudo cavar")]
    public bool mostrarDebug = true;

    [Header("Rellenar pozo")]
    public float radioRelleno = 2f;       // radio del área que se rellena (debe abarcar todo el pozo)
    public float duracionRelleno = 1.5f;  // segundos que tarda en rellenarse

    [Tooltip("Tecla para devolver el terreno a su forma original sin salir del juego (None = desactivada)")]
    public KeyCode teclaRestaurar = KeyCode.None;

    // Terrain -> TerrainData original del proyecto (que nunca se modifica)
    private Dictionary<Terrain, TerrainData> originales = new Dictionary<Terrain, TerrainData>();

    // Antes de empezar, cada Terrain pasa a usar una COPIA de su TerrainData.
    // Así lo que se cava vive solo en la copia: el asset del proyecto no cambia
    // y el terreno empieza siempre en su estado inicial cada vez que se da Play.
    void Awake()
    {
        foreach (Terrain t in Terrain.activeTerrains)
        {
            TerrainData original = t.terrainData;
            TerrainData copia = Instantiate(original);

            t.terrainData = copia;
            TerrainCollider colisionador = t.GetComponent<TerrainCollider>();
            if (colisionador != null)
                colisionador.terrainData = copia;

            originales[t] = original;
        }
    }

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        if (cargarCuerpo == null)
            cargarCuerpo = GetComponent<CargarCuerpo>();
    }

    void Update()
    {
        if (teclaRestaurar != KeyCode.None && Input.GetKeyDown(teclaRestaurar))
            RestaurarTerreno();

        if (!Input.GetMouseButtonDown(0))
            return;

        if (cargarCuerpo == null)
        {
            Log("falta el componente CargarCuerpo en el jugador");
            return;
        }

        if (!cargarCuerpo.CargaObjetoConTag(tagPala))
        {
            Log("no hay en la mano un objeto con el tag '" + tagPala + "' (¿lo agarraste con E? ¿el tag está en el mismo objeto que tiene CuerpoCargable?)");
            return;
        }

        // Si se hace click sobre un cuerpo enterrado, se rellena el pozo. Si no, se cava.
        if (IntentarRellenar())
            return;

        Cavar();
    }

    void Log(string mensaje)
    {
        if (mostrarDebug)
            Debug.Log("CavarTerreno: " + mensaje);
    }

    void Cavar()
    {
        // 1) Hacia donde mira. Solo cuenta si lo primero que golpea es un Terrain.
        RaycastHit hit;
        bool encontrado = Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, distanciaCavar, ~0, QueryTriggerInteraction.Ignore)
                          && hit.collider.GetComponent<Terrain>() != null;

        // 2) Si no mira al suelo, cava en el piso delante del jugador
        if (!encontrado)
        {
            Vector3 adelante = cam.transform.forward;
            adelante.y = 0f;
            adelante = adelante.sqrMagnitude > 0.001f ? adelante.normalized : transform.forward;

            Vector3 origen = transform.position + adelante * distanciaAdelante + Vector3.up * 2f;
            encontrado = Physics.Raycast(origen, Vector3.down, out hit, 8f, ~0, QueryTriggerInteraction.Ignore);
        }

        if (!encontrado)
        {
            Log("no se encontró suelo para cavar");
            return;
        }

        Terrain terreno = hit.collider.GetComponent<Terrain>();
        if (terreno == null)
        {
            Log("el suelo no es un Terrain, es '" + hit.collider.name + "' (este script solo cava Terrains)");
            return;
        }

        TerrainData td = terreno.terrainData;

        // Un Terrain no puede bajar de su altura mínima (0)
        float alturaSobreBase = hit.point.y - terreno.transform.position.y;
        if (alturaSobreBase < 0.01f)
            Log("el terreno ya está en su altura mínima en este punto, no puede bajar más. Subí la altura base del terreno (ver instrucciones).");

        int res = td.heightmapResolution;
        Vector3 local = hit.point - terreno.transform.position;

        // Posición del impacto en el mapa de alturas
        int cx = Mathf.RoundToInt(local.x / td.size.x * (res - 1));
        int cz = Mathf.RoundToInt(local.z / td.size.z * (res - 1));

        // Cuántas muestras del mapa de alturas abarca el radio en cada eje
        int rx = Mathf.CeilToInt(radio / td.size.x * (res - 1));
        int rz = Mathf.CeilToInt(radio / td.size.z * (res - 1));

        int x0 = Mathf.Clamp(cx - rx, 0, res - 1);
        int z0 = Mathf.Clamp(cz - rz, 0, res - 1);
        int x1 = Mathf.Clamp(cx + rx, 0, res - 1);
        int z1 = Mathf.Clamp(cz + rz, 0, res - 1);

        int ancho = x1 - x0 + 1;
        int alto = z1 - z0 + 1;
        float[,] alturas = td.GetHeights(x0, z0, ancho, alto);

        // Las alturas del Terrain van de 0 a 1 respecto a su altura máxima
        float bajada = profundidad / td.size.y;

        for (int i = 0; i < alto; i++)          // i = eje Z
        {
            for (int j = 0; j < ancho; j++)     // j = eje X
            {
                float dx = (x0 + j - cx) / (float)(res - 1) * td.size.x;
                float dz = (z0 + i - cz) / (float)(res - 1) * td.size.z;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);
                if (dist > radio) continue;

                // 1 en el centro, 0 en el borde (bordes suaves)
                float suavizado = 0.5f * (1f + Mathf.Cos(Mathf.PI * dist / radio));
                alturas[i, j] = Mathf.Max(0f, alturas[i, j] - bajada * suavizado);
            }
        }

        td.SetHeights(x0, z0, alturas);
    }

    // ---------- Rellenar ----------

    bool IntentarRellenar()
    {
        RaycastHit hit;
        if (!Physics.SphereCast(cam.transform.position, 0.25f, cam.transform.forward, out hit, distanciaCavar, ~0, QueryTriggerInteraction.Ignore))
            return false;

        CuerpoCargable cuerpo = hit.collider.GetComponentInParent<CuerpoCargable>();
        if (cuerpo == null || !cuerpo.EstaEnterrado() || cuerpo.YaRellenado())
            return false;

        cuerpo.MarcarRellenado();
        StartCoroutine(RellenarPozo(cuerpo.transform.position));
        Log("rellenando el pozo");
        return true;
    }

    // Devuelve el terreno alrededor de "centro" a su altura original, de forma gradual
    IEnumerator RellenarPozo(Vector3 centro)
    {
        Terrain terreno = null;
        TerrainData original = null;
        foreach (KeyValuePair<Terrain, TerrainData> par in originales)
        {
            Vector3 l = centro - par.Key.transform.position;
            if (l.x >= 0f && l.z >= 0f && l.x <= par.Value.size.x && l.z <= par.Value.size.z)
            {
                terreno = par.Key;
                original = par.Value;
                break;
            }
        }
        if (terreno == null)
            yield break;

        TerrainData td = terreno.terrainData;
        int res = td.heightmapResolution;
        Vector3 local = centro - terreno.transform.position;

        int cx = Mathf.RoundToInt(local.x / td.size.x * (res - 1));
        int cz = Mathf.RoundToInt(local.z / td.size.z * (res - 1));
        int rx = Mathf.CeilToInt(radioRelleno / td.size.x * (res - 1));
        int rz = Mathf.CeilToInt(radioRelleno / td.size.z * (res - 1));

        int x0 = Mathf.Clamp(cx - rx, 0, res - 1);
        int z0 = Mathf.Clamp(cz - rz, 0, res - 1);
        int x1 = Mathf.Clamp(cx + rx, 0, res - 1);
        int z1 = Mathf.Clamp(cz + rz, 0, res - 1);
        int ancho = x1 - x0 + 1;
        int alto = z1 - z0 + 1;

        float[,] inicio = td.GetHeights(x0, z0, ancho, alto);
        float[,] objetivo = original.GetHeights(x0, z0, ancho, alto);
        float[,] actual = (float[,])inicio.Clone();

        float tiempo = 0f;
        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime / Mathf.Max(0.01f, duracionRelleno);
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo));

            for (int i = 0; i < alto; i++)
            {
                for (int j = 0; j < ancho; j++)
                {
                    float dx = (x0 + j - cx) / (float)(res - 1) * td.size.x;
                    float dz = (z0 + i - cz) / (float)(res - 1) * td.size.z;
                    if (dx * dx + dz * dz > radioRelleno * radioRelleno) continue;

                    actual[i, j] = Mathf.Lerp(inicio[i, j], objetivo[i, j], k);
                }
            }

            td.SetHeights(x0, z0, actual);
            yield return null;
        }
    }

    // Cuánto más bajo está el terreno en ese punto respecto de su forma original (en unidades del mundo)
    public float ProfundidadEn(Vector3 posicion)
    {
        foreach (KeyValuePair<Terrain, TerrainData> par in originales)
        {
            Vector3 l = posicion - par.Key.transform.position;
            TerrainData orig = par.Value;
            if (l.x < 0f || l.z < 0f || l.x > orig.size.x || l.z > orig.size.z)
                continue;

            float u = l.x / orig.size.x;
            float v = l.z / orig.size.z;
            return orig.GetInterpolatedHeight(u, v) - par.Key.terrainData.GetInterpolatedHeight(u, v);
        }
        return 0f;
    }

    // Devuelve todos los terrenos a su forma original (la del proyecto)
    [ContextMenu("Restaurar terreno")]
    public void RestaurarTerreno()
    {
        foreach (KeyValuePair<Terrain, TerrainData> par in originales)
        {
            TerrainData original = par.Value;
            TerrainData actual = par.Key.terrainData;
            int r = original.heightmapResolution;
            actual.SetHeights(0, 0, original.GetHeights(0, 0, r, r));
        }
    }
}