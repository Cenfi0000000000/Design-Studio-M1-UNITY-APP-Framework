using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;
using Meta.XR.Samples;

public class HandSpawnManager : MonoBehaviour
{
    [Header("要生成的物件 Prefab")]
    public GameObject objectToSpawn;

    [Header("生成在手部的 Transform")]
    public Transform handTransform;

    [Header("監聽的手部 OVRHand (Left or Right)")]
    public OVRHand ovrHand;

    [Header("複製物件的父物件（Poke 時生成）")]
    public Transform spawnParent;

    [HideInInspector] public GameObject spawnedMesh;
    private bool isHandsReady = false;

    private List<GameObject> spawnedCopies = new List<GameObject>();

    private bool wasPinching = false;
    private bool wasPoking = false;
    private bool wasGrabbing = false;

    private string currentHandState = "Default";

    private void Start()
    {
        StartCoroutine(CheckHandsInitializedCoroutine());
    }

    private IEnumerator CheckHandsInitializedCoroutine()
    {
        while (HandsManager.Instance == null || !HandsManager.Instance.IsInitialized())
            yield return null;

        while (ovrHand == null)
        {
            ovrHand = handTransform.GetComponentInParent<OVRHand>();
            yield return null;
        }

        Debug.Log("✅ XR HandsManager initialized and OVRHand found!");
        isHandsReady = true;
    }

    private void Update()
    {
        if (!isHandsReady || ovrHand == null)
            return;

        // ------------------ 手指彎曲度 ------------------
        float thumb = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb);
        float index = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        float middle = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);



        // 真實手勢偵測
        bool isPinching = ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Index) &&
                          ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Thumb);

        bool isPoking = ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Middle) &&
                          ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Thumb);

        bool isGrabbing = ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Ring) &&
                          ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Thumb);

        // 🔹 鍵盤模擬（只在 Editor 有效）
#if UNITY_EDITOR
        // P鍵模擬 Poke
        if (Input.GetKey(KeyCode.P)) isPoking = true;
        else isPoking = false;

        // G鍵模擬 Grab
        if (Input.GetKey(KeyCode.G)) isGrabbing = true;
        else isGrabbing = false;

        // 可選：H鍵模擬 Pinch
        if (Input.GetKey(KeyCode.H)) isPinching = true;
#endif

        // 之後就可以沿用原本的 Pinch / Poke / Grab 邏輯
        if (isPinching && !wasPinching) SpawnObjectOnHand();
        if (isPoking && !wasPoking) CopyObjectToParent();
        if (isGrabbing && !wasGrabbing) ClearAllCopies();

        wasPinching = isPinching;
        wasPoking = isPoking;
        wasGrabbing = isGrabbing;
    }


    private void SpawnObjectOnHand()
    {
        if (!isHandsReady || objectToSpawn == null || handTransform == null)
            return;

        if (spawnedMesh != null)
        {
            Debug.Log("⚠️ 手上已有物件，不重複生成");
            return;
        }

        GameObject obj = Instantiate(objectToSpawn, handTransform.position, handTransform.rotation);
        obj.transform.SetParent(handTransform);
        spawnedMesh = obj;

        foreach (var col in obj.GetComponentsInChildren<Collider>())
            col.enabled = false;

        StartCoroutine(EnableCollidersAfterDelay(obj, 0.1f));
        Debug.Log("🟢 Spawned object on hand!");
    }

    private void CopyObjectToParent()
    {
        if (spawnedMesh == null || spawnParent == null)
        {
            Debug.Log("⚠️ 沒有可複製的物件或 spawnParent 未設定！");
            return;
        }

        GameObject copy = Instantiate(spawnedMesh, spawnedMesh.transform.position, spawnedMesh.transform.rotation, spawnParent);
        copy.name = spawnedMesh.name + "_Copy";

        foreach (var rb in copy.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = true;

        spawnedCopies.Add(copy);
        Debug.Log($"🟡 Copied object to spawnParent: {copy.name}");
    }

    private void ClearAllCopies()
    {
        foreach (var obj in spawnedCopies)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedCopies.Clear();
        Debug.Log("🔴 Cleared all copied objects!");
    }

    public void _ResetSpawn()
    {
        if (spawnedMesh != null)
        {
            Destroy(spawnedMesh);
            spawnedMesh = null;
        }
    }

    private IEnumerator EnableCollidersAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var col in obj.GetComponentsInChildren<Collider>())
            col.enabled = true;
    }
}