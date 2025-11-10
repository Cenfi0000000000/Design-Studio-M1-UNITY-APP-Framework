using System.Collections;
using UnityEngine;
using Meta.XR.Samples;
using OculusSampleFramework;
using Oculus;

public class SpawnOnHandSafe : MonoBehaviour
{
    [Header("要生成的物件 Prefab")]
    public GameObject objectToSpawn;

    [Header("生成在手部的 Transform")]
    public Transform handTransform;

    [Header("目標手 (Left 或 Right)")]
    public OVRHand ovrHand; // ✅ 指定手部（例如 LeftHandAnchor 或 RightHandAnchor 上的 OVRHand）

    [HideInInspector] public GameObject spawnedMesh;
    private bool hasSpawned = false;
    private bool isHandsReady = false;

    private void Start()
    {
        StartCoroutine(CheckHandsInitializedCoroutine());
    }

    private IEnumerator CheckHandsInitializedCoroutine()
    {
        // 等待 HandsManager 初始化完成
        while (HandsManager.Instance == null || !HandsManager.Instance.IsInitialized())
        {
            yield return null;
        }

        // 等待 OVRHand 可用
        while (ovrHand == null)
        {
            ovrHand = handTransform.GetComponentInParent<OVRHand>();
            yield return null;
        }

        Debug.Log("XR HandsManager initialized and OVRHand found!");
        isHandsReady = true;
    }

    private void Update()
    {
        if (!isHandsReady || ovrHand == null)
            return;

        // ✅ 偵測 Pinch 狀態
        bool isPinching = ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        // ✅ 只有在剛開始 Pinch 時觸發生成
        if (isPinching && !hasSpawned)
        {
            _SpawnObject();
        }

        // ✅ 若放開 pinch，可重置生成狀態
        if (!isPinching && hasSpawned)
        {
            _ResetSpawn();
        }
    }

    public void _SpawnObject()
    {
        if (!isHandsReady)
        {
            StartCoroutine(WaitAndSpawn());
            return;
        }

        if (!hasSpawned && objectToSpawn != null && handTransform != null)
        {
            spawnedMesh = Instantiate(objectToSpawn, handTransform.position, handTransform.rotation);
            spawnedMesh.transform.SetParent(handTransform);
            hasSpawned = true;

            // 暫時關閉所有 collider
            foreach (var col in spawnedMesh.GetComponentsInChildren<Collider>())
                col.enabled = false;

            StartCoroutine(EnableCollidersAfterDelay(spawnedMesh, 0.1f));

            Debug.Log("✅ Spawned object safely!");
        }
    }

    private IEnumerator EnableCollidersAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var col in obj.GetComponentsInChildren<Collider>())
            col.enabled = true;
    }


    private IEnumerator WaitAndSpawn()
    {
        while (!isHandsReady)
            yield return null;
        _SpawnObject();
    }

    public void _ResetSpawn()
    {
        if (spawnedMesh != null)
        {
            Destroy(spawnedMesh);
        }

        hasSpawned = false;
        spawnedMesh = null;
    }
}
