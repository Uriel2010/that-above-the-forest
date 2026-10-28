using UnityEngine;

public class TreeTerrainTagger : MonoBehaviour
{
    public Terrain terrain;

    void Start()
    {
        var data = terrain.terrainData;
        var parent = new GameObject("TreeProxies").transform;

        foreach (var t in data.treeInstances)
        {
            Vector3 pos = Vector3.Scale(t.position, data.size) + terrain.transform.position;

            var go = new GameObject("TreeProxy");
            go.tag = "arbol";
            go.transform.position = pos;
            go.transform.parent = parent;

            var col = go.AddComponent<CapsuleCollider>();
            col.radius = 0.5f;
            col.height = 4f;
            col.center = Vector3.up * 2f;
        }
    }
}
