using System.Collections.Generic;
using UnityEngine;

public class CollsionVisualizer : MonoBehaviour
{
    [Header("要監聽的父物件")]
    public Transform parentObject;

    [Header("Offset Collider 擴張倍率")]
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
    private GameObject lastAddedChild = null;
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

        foreach (Transform child in parentObject)
        {
            if (!trackedChildren.Contains(child.gameObject))
            {
                trackedChildren.Add(child.gameObject);
                AddOrUpdateColliders(child.gameObject);

                lastAddedChild = child.gameObject;

                Debug.Log($"🟡 New Child Added: {child.name}");

                if (trackedChildren.Count > 1)
                {
                    StartCoroutine(DelayedCollisionCheck());
                }
            }
        }
    }

    private void AddOrUpdateColliders(GameObject obj)
    {
        Vector3 originSize = new Vector3(0.02967f, 0.01267f, 0.5f);
        Vector3 localOffset = new Vector3(-0.01483f, 0.00633f, 0.25f);
        Transform t = obj.transform;

        // origin collider
        BoxCollider origin = obj.transform.Find("originalCollider")?.GetComponent<BoxCollider>();
        if (origin == null)
        {
            GameObject go = new GameObject("originalCollider");
            go.transform.SetParent(obj.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            origin = go.AddComponent<BoxCollider>();
            origin.size = originSize;
            origin.center = localOffset;

            if (originLayer >= 0) go.layer = originLayer;

            Debug.Log($"🟢 Added Origin Collider to: {obj.name}");
        }

        // offset collider
        BoxCollider offset = obj.transform.Find("offsetCollider")?.GetComponent<BoxCollider>();
        if (offset == null)
        {
            GameObject go = new GameObject("offsetCollider");
            go.transform.SetParent(obj.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            offset = go.AddComponent<BoxCollider>();
            offset.size = originSize * offsetScale;
            offset.center = localOffset;

            if (offsetLayer >= 0) go.layer = offsetLayer;

            Debug.Log($"🔵 Added Offset Collider to: {obj.name}");
        }
    }

    private System.Collections.IEnumerator DelayedCollisionCheck()
    {
        isCheckingCollisions = true;
        yield return null;

        Debug.Log("🟠 Begin Collision Check (only newest child)");
        CheckCollisions();
        Debug.Log("🟣 End Collision Check");

        isCheckingCollisions = false;
    }

    private void CheckCollisions()
    {
        if (lastAddedChild == null) return;

        var objA = lastAddedChild;
        var originA = objA.transform.Find("originalCollider")?.GetComponent<BoxCollider>();
        var offsetA = objA.transform.Find("offsetCollider")?.GetComponent<BoxCollider>();

        if (originA == null || offsetA == null) return;

        Bounds originABounds = originA.bounds;
        Bounds offsetABounds = offsetA.bounds;

        // 💡 舊的 child（除了 lastAddedChild）
        foreach (var objB in trackedChildren)
        {
            if (objB == objA) continue;

            var originB = objB.transform.Find("originalCollider")?.GetComponent<BoxCollider>();
            var offsetB = objB.transform.Find("offsetCollider")?.GetComponent<BoxCollider>();

            if (originB == null || offsetB == null) continue;

            Bounds originBBounds = originB.bounds;
            Bounds offsetBBounds = offsetB.bounds;

            bool originHit = originABounds.Intersects(originBBounds);
            bool offsetHit = offsetABounds.Intersects(offsetBBounds);

            // 情況 1：origin + offset 都撞 → 只產生 origin
            if (originHit && offsetHit)
            {
                Vector3 pA = originA.ClosestPoint(originBBounds.center);
                Vector3 pB = originB.ClosestPoint(originABounds.center);
                Vector3 pos = (pA + pB) / 2f;

                Debug.Log($"🟥(Origin Only) Both Hit: {objA.name} ↔ {objB.name}");
                SpawnSphere(pos, originMaterial);
                continue;
            }

            // 情況 2：只有 origin
            if (originHit)
            {
                Vector3 pA = originA.ClosestPoint(originBBounds.center);
                Vector3 pB = originB.ClosestPoint(originABounds.center);
                Vector3 pos = (pA + pB) / 2f;

                Debug.Log($"🟥 Origin Hit: {objA.name} ↔ {objB.name}");
                SpawnSphere(pos, originMaterial);
                continue;
            }

            // 情況 3：只有 offset
            if (offsetHit)
            {
                Vector3 pA = offsetA.ClosestPoint(offsetBBounds.center);
                Vector3 pB = offsetB.ClosestPoint(offsetABounds.center);
                Vector3 pos = (pA + pB) / 2f;

                Debug.Log($"🟧 Offset Hit: {objA.name} ↔ {objB.name}");
                SpawnSphere(pos, offsetMaterial);
                continue;
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
