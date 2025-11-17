using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandGestureDisplay : MonoBehaviour
{
    [Header("要顯示的 HandSpawnManager")]
    public HandSpawnManager handSpawnManager;

    [Header("UI Canvas 與文字")]
    public Canvas worldCanvas;
    public Text gestureText;

    [Header("距離手部的偏移")]
    public Vector3 offset = new Vector3(0, 0.1f, 0.2f);

    private void Start()
    {
        if (handSpawnManager == null)
        {
            Debug.LogError("HandSpawnManager 尚未指定！");
            enabled = false;
            return;
        }

        if (worldCanvas == null)
        {
            // 建立一個簡單 Canvas
            GameObject canvasGO = new GameObject("HandGestureCanvas");
            canvasGO.transform.SetParent(handSpawnManager.handTransform);
            canvasGO.transform.localPosition = offset;
            canvasGO.transform.localRotation = Quaternion.identity;

            worldCanvas = canvasGO.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.worldCamera = Camera.main;
            worldCanvas.scaleFactor = 0.01f;

            // 加一個 Text
            GameObject textGO = new GameObject("GestureText");
            textGO.transform.SetParent(canvasGO.transform);
            gestureText = textGO.AddComponent<Text>();
            gestureText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            gestureText.fontSize = 50;
            gestureText.alignment = TextAnchor.MiddleCenter;
            gestureText.color = Color.white;

            // 設定 RectTransform
            RectTransform rt = gestureText.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 200);
            rt.localPosition = Vector3.zero;
        }
    }

    private void Update()
    {
        if (handSpawnManager == null || gestureText == null)
            return;

        // 將 Console 輸出的資訊顯示到畫面上
        string thumb = handSpawnManager.ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb).ToString("F2");
        string index = handSpawnManager.ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index).ToString("F2");
        string middle = handSpawnManager.ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle).ToString("F2");

        string gesture = "Default";
        if (handSpawnManager.ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            gesture = "Pinch";
        else if (handSpawnManager.ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb) > 0.25f &&
                 handSpawnManager.ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index) < 0.1f &&
                 handSpawnManager.ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle) > 0.25f)
            gesture = "Poke";
        else if (handSpawnManager.ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb) > 0.25f &&
                 handSpawnManager.ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index) > 0.25f &&
                 handSpawnManager.ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle) > 0.25f)
            gesture = "Grab";

        gestureText.text = $"手勢: {gesture}\nThumb: {thumb}\nIndex: {index}\nMiddle: {middle}";

        // 讓 Canvas 面向玩家
        worldCanvas.transform.rotation = Quaternion.LookRotation(worldCanvas.transform.position - Camera.main.transform.position);
    }
}
