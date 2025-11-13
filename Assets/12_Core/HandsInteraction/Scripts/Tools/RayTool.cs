using UnityEngine;

namespace OculusSampleFramework
{
    /// <summary>
    /// 簡化的射線工具，無需 Interactable 支援。
    /// </summary>
    public class RayTool : MonoBehaviour
    {
        [Header("手部 Transform（射線起點）")]
        public Transform handTransform;

        [Header("Ray Tool View")]
        public RayToolView rayToolView;

        [Header("射線參數")]
        public float farFieldMaxDistance = 5f;

        private void Awake()
        {
            if (handTransform == null)
            {
                Debug.LogError("Hand Transform is required!");
            }

            if (rayToolView == null)
            {
                Debug.LogError("RayToolView is required!");
            }
        }

        private void Update()
        {
            if (handTransform == null || rayToolView == null)
                return;

            // 直接將 RayTool 位置與旋轉跟隨手
            transform.position = handTransform.position;
            transform.rotation = handTransform.rotation;

            // 設定射線目標為手前方一定距離
            Vector3 targetPosition = handTransform.position + handTransform.forward * farFieldMaxDistance;

            // 更新 RayToolView（不再依賴 Interactable）
            rayToolView.UpdateRayDirectly(handTransform, targetPosition);
        }
    }
}
