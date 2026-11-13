using System.Collections.Generic;
using UnityEngine;

// Se agrega a un objeto vacío con un BoxCollider (marcado como Is Trigger) que cubre la zona
// donde se puede enterrar el cuerpo. Si un cuerpo queda quieto dentro de la zona y el suelo
// de ahí está cavado (pozo), el cuerpo se congela y ya no se puede agarrar.
[RequireComponent(typeof(BoxCollider))]
public class ZonaEntierro : MonoBehaviour
{
    [Header("Referencias")]
    public CavarTerreno cavarTerreno; // si queda vacío se busca solo

    [Header("Configuración")]
    [Tooltip("Profundidad mínima del pozo (unidades del mundo) para poder enterrar el cuerpo")]
    public float profundidadMinima = 2.5f;
    [Tooltip("Segundos que el cuerpo tiene que estar quieto dentro de la zona")]
    public float tiempoQuieto = 0.75f;
    [Tooltip("Los objetos con este tag nunca se entierran (la pala)")]
    public string tagIgnorado = "pala";
    public bool mostrarDebug = true;

    private class Estado
    {
        public Vector3 posicion;
        public float tiempo;
        public bool avisado;
    }

    private BoxCollider caja;
    private readonly Dictionary<CuerpoCargable, Estado> estados = new Dictionary<CuerpoCargable, Estado>();
    private readonly HashSet<CuerpoCargable> vistos = new HashSet<CuerpoCargable>();
    private readonly List<CuerpoCargable> aBorrar = new List<CuerpoCargable>();

    void Start()
    {
        caja = GetComponent<BoxCollider>();
        caja.isTrigger = true;

        if (cavarTerreno == null)
            cavarTerreno = FindObjectOfType<CavarTerreno>();

        if (cavarTerreno == null)
            Debug.LogWarning("ZonaEntierro: no se encontró CavarTerreno en la escena.");
    }

    void Update()
    {
        if (caja == null || cavarTerreno == null)
            return;

        // Se usa OverlapBox (y no OnTriggerStay) porque Unity deja de avisar de los
        // cuerpos que se duermen, y un cuerpo quieto es justo el caso que nos interesa.
        Vector3 centro = transform.TransformPoint(caja.center);
        Vector3 mitad = Vector3.Scale(caja.size, transform.lossyScale) * 0.5f;
        Collider[] dentro = Physics.OverlapBox(centro, mitad, transform.rotation, ~0, QueryTriggerInteraction.Ignore);

        vistos.Clear();
        foreach (Collider col in dentro)
        {
            CuerpoCargable cuerpo = col.GetComponentInParent<CuerpoCargable>();
            if (cuerpo == null || cuerpo.EstaCargado() || cuerpo.EstaEnterrado())
                continue;
            if (!string.IsNullOrEmpty(tagIgnorado) && cuerpo.CompareTag(tagIgnorado))
                continue;
            if (!vistos.Add(cuerpo))
                continue;

            Vector3 pos = cuerpo.transform.position;
            Estado e;
            if (!estados.TryGetValue(cuerpo, out e))
            {
                e = new Estado { posicion = pos };
                estados[cuerpo] = e;
            }

            bool quieto = (pos - e.posicion).sqrMagnitude < 0.0001f;
            e.posicion = pos;
            e.tiempo = quieto ? e.tiempo + Time.deltaTime : 0f;

            if (e.tiempo < tiempoQuieto)
                continue;

            float profundidad = cavarTerreno.ProfundidadEn(pos);
            if (profundidad >= profundidadMinima)
            {
                cuerpo.Enterrar();
                Log("cuerpo enterrado (pozo de " + profundidad.ToString("F2") + " de profundidad)");
            }
            else if (!e.avisado)
            {
                e.avisado = true;
                Log("el cuerpo está en la zona pero el pozo no es lo bastante hondo (" +
                    profundidad.ToString("F2") + " < " + profundidadMinima + ")");
            }
        }

        // Se olvidan los cuerpos que salieron de la zona
        aBorrar.Clear();
        foreach (CuerpoCargable c in estados.Keys)
            if (!vistos.Contains(c))
                aBorrar.Add(c);
        foreach (CuerpoCargable c in aBorrar)
            estados.Remove(c);
    }

    void Log(string mensaje)
    {
        if (mostrarDebug)
            Debug.Log("ZonaEntierro: " + mensaje);
    }
}
