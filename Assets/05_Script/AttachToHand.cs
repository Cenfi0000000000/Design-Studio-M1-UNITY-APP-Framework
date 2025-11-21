using UnityEngine;

public class AttachToHand : MonoBehaviour
{
    [Header("要跟隨的手")]
    public Transform handTransform;

    [Header("是否跟隨旋轉")]
    public bool followRotation = true;

    [Header("位置偏移")]
    public Vector3 positionOffset = Vector3.zero;

    [Header("旋轉偏移 (Euler)")]
    public Vector3 rotationOffsetEuler = Vector3.zero;

    // 將 Euler 轉 Quaternion，方便計算旋轉偏移
    private Quaternion rotationOffset => Quaternion.Euler(rotationOffsetEuler);

    void LateUpdate()
    {
        if (handTransform == null) return;

        // 套用位置偏移
        transform.position = handTransform.position + handTransform.rotation * positionOffset;

        if (followRotation)
        {
            // 套用旋轉偏移
            transform.rotation = handTransform.rotation * rotationOffset;
        }
    }
}
