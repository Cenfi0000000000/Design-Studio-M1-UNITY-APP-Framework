using UnityEngine;

/// <summary>
/// Quest 3 Passthrough 設定範例
/// 這個腳本會初始化 OVRPassthroughLayer，並控制顯示、透明度與邊緣描線
/// </summary>
[RequireComponent(typeof(OVRPassthroughLayer))]
public class Quest3PassthroughManager : MonoBehaviour
{
    private OVRPassthroughLayer passthroughLayer;

    [Header("Passthrough 設定")]
    [Tooltip("是否顯示 Passthrough")]
    public bool showPassthrough = true;

    [Tooltip("Passthrough 透明度 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float opacity = 1.0f;

    [Tooltip("是否開啟邊緣描線")]
    public bool enableEdgeRendering = true;

    void Awake()
    {
        // 取得或新增 OVRPassthroughLayer
        passthroughLayer = GetComponent<OVRPassthroughLayer>();
        if (passthroughLayer == null)
            passthroughLayer = gameObject.AddComponent<OVRPassthroughLayer>();

        InitializePassthrough();
    }

    /// <summary>
    /// 初始化 Passthrough 設定
    /// </summary>
    void InitializePassthrough()
    {
        // 顯示或隱藏 Passthrough
        passthroughLayer.hidden = !showPassthrough;

        // 設定透明度
        passthroughLayer.textureOpacity = opacity;

        // 設定邊緣描線
        passthroughLayer.edgeRenderingEnabled = enableEdgeRendering;

        Debug.Log($"Passthrough 初始化完成: show={showPassthrough}, opacity={opacity}, edge={enableEdgeRendering}");
    }

    /// <summary>
    /// 可在遊戲中動態切換顯示與透明度
    /// </summary>
    public void SetPassthroughVisibility(bool visible)
    {
        passthroughLayer.hidden = !visible;
    }

    public void SetPassthroughOpacity(float value)
    {
        passthroughLayer.textureOpacity = Mathf.Clamp01(value);
    }

    public void SetEdgeRendering(bool enable)
    {
        passthroughLayer.edgeRenderingEnabled = enable;
    }
}
