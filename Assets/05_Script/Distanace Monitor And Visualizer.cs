using System.Collections.Generic;
using UnityEngine;

public class ChildMonitor : MonoBehaviour
{
    [Header("要監聽的父物件")]
    public Transform parentObject;

    [Header("Collider 擴張倍率")]
    public float offsetScale = 1.11f;

    [Header("碰撞 Sphere 半徑")]
    public float sphereRadius = 0.05f;

    [Header("材質設定")]
    public Material originMaterial;
    public Material offsetMaterial;

    [Header("Child Collider Layers")]
    public string originLayerName = "ChildOrigin";
    public string offsetLayerName = "ChildOffset";

    private List<GameObject> trackedChildren = new List<GameObject>();
    private bool isCheckingCollisions = false;
    private int originLayer;
    private int offsetLayer;

    void Start()
    {
        originLayer = LayerMask.NameToLayer(originLayerName);
        offsetLayer = LayerMask.NameToLayer(offsetLayerName);

        if (originLayer == -1) Debug.LogWarning($"Layer '{originLayerName}' 不存在");
        if (offsetLayer == -1) Debug.LogWarning($"Layer '{offsetLayerName}' 不存在");
    }

    void Update()
    {
        if (parentObject == null) return;

        bool hasNewChild = false;

        foreach (Transform child in parentObject)
        {
            if (!trackedChildren.Contains(child.gameObject))
            {
                trackedChildren.Add(child.gameObject);
                AddOrUpdateColliders(child.gameObject);
                hasNewChild = true;

                Debug.Log($"🟡 New Child Added: {child.name}");
            }
        }

        if (hasNewChild && trackedChildren.Count > 1 && !isCheckingCollisions)
        {
            StartCoroutine(DelayedCollisionCheck());
        }
    }

    private void AddOrUpdateColliders(GameObject obj)
    {
        Vector3 originSize = new Vector3(0.02967f, 0.01267f, 0.5f);
        Vector3 localOffset = new Vector3(-0.01483f, 0.00633f, 0.25f);
        Transform t = obj.transform;

        // origin Collider
        BoxCollider origin = obj.transform.Find("originalCollider")?.GetComponent<BoxCollider>();
        if (origin == null)
        {
            GameObject go = new GameObject("originalCollider");
            go.transform.parent = obj.transform;
            go.transform.position = t.position;
            go.transform.rotation = t.rotation;
            origin = go.AddComponent<BoxCollider>();
            origin.size = originSize;
            origin.center = localOffset;

            if (originLayer >= 0 && originLayer <= 31)
                go.layer = originLayer;

            Debug.Log($"🟢 Added Origin Collider to: {obj.name}");
        }

        // offset Collider
        BoxCollider offset = obj.transform.Find("offsetCollider")?.GetComponent<BoxCollider>();
        if (offset == null)
        {
            GameObject go = new GameObject("offsetCollider");
            go.transform.parent = obj.transform;
            go.transform.position = t.position;
            go.transform.rotation = t.rotation;
            offset = go.AddComponent<BoxCollider>();
            offset.size = originSize * offsetScale;
            offset.center = localOffset;

            if (offsetLayer >= 0 && offsetLayer <= 31)
                go.layer = offsetLayer;

            Debug.Log($"🔵 Added Offset Collider to: {obj.name}");
        }
    }

    private System.Collections.IEnumerator DelayedCollisionCheck()
    {
        isCheckingCollisions = true;
        yield return null;
        Debug.Log("🟠 Begin Collision Check");
        CheckCollisions();
        Debug.Log("🟣 End Collision Check");
        isCheckingCollisions = false;
    }

    private void CheckCollisions()
    {
        var processedPairs = new HashSet<(GameObject, GameObject)>();

        for (int i = 0; i < trackedChildren.Count; i++)
        {
            var objA = trackedChildren[i];
            BoxCollider originA = objA.transform.Find("originalCollider")?.GetComponent<BoxCollider>();
            BoxCollider offsetA = objA.transform.Find("offsetCollider")?.GetComponent<BoxCollider>();
            if (originA == null || offsetA == null) continue;

            Bounds originABounds = originA.bounds;
            Bounds offsetABounds = offsetA.bounds;

            for (int j = i + 1; j < trackedChildren.Count; j++)
            {
                var objB = trackedChildren[j];
                BoxCollider originB = objB.transform.Find("originalCollider")?.GetComponent<BoxCollider>();
                BoxCollider offsetB = objB.transform.Find("offsetCollider")?.GetComponent<BoxCollider>();
                if (originB == null || offsetB == null) continue;

                Bounds originBBounds = originB.bounds;
                Bounds offsetBBounds = offsetB.bounds;

                // ORIGIN 對 ORIGIN
                if (originABounds.Intersects(originBBounds))
                {
                    var pair = (originA.gameObject, originB.gameObject);
                    var pairRev = (originB.gameObject, originA.gameObject);

                    if (!processedPairs.Contains(pair) && !processedPairs.Contains(pairRev))
                    {
                        processedPairs.Add(pair);

                        Vector3 contactPos = (originABounds.center + originBBounds.center) / 2f;
                        Debug.Log($"🟥 Origin Collision: {objA.name} ↔ {objB.name} @ {contactPos}");

                        SpawnSphere(contactPos, originMaterial);
                    }
                }

                // OFFSET 對 OFFSET
                if (offsetABounds.Intersects(offsetBBounds))
                {
                    var pair = (offsetA.gameObject, offsetB.gameObject);
                    var pairRev = (offsetB.gameObject, offsetA.gameObject);

                    if (!processedPairs.Contains(pair) && !processedPairs.Contains(pairRev))
                    {
                        processedPairs.Add(pair);

                        Vector3 contactPos = (offsetABounds.center + offsetBBounds.center) / 2f;
                        Debug.Log($"🟧 Offset Collision: {objA.name} ↔ {objB.name} @ {contactPos}");

                        SpawnSphere(contactPos, offsetMaterial);
                    }
                }
            }
        }
    }

    public void SpawnSphere(Vector3 position, Material mat)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * sphereRadius * 2f;

        if (mat != null)
            sphere.GetComponent<Renderer>().material = mat;

        Debug.Log($"🟢 Spawned Sphere @ {position}");
    }
}
