/****************************************************
    文件：CrosshairFollowMouse.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-28 16:13:29
	功能：准心预制体跟随鼠标
*****************************************************/

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CrosshairFollowMouse : MonoBehaviour
{
    [Header("目标面板")]
    [SerializeField] private RectTransform targetPanel;

    private RectTransform rectTransform;
    private Canvas parentCanvas;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // 如果没有手动指定目标面板，则自动寻找父级 Canvas 作为默认
        if (targetPanel == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
                targetPanel = parentCanvas.GetComponent<RectTransform>();
        }

        // 如果连 Canvas 都找不到，给出提示
        if (targetPanel == null)
        {
            Debug.LogError("CrosshairFollowMouse: 未找到目标面板或Canvas，请手动指定！");
        }
    }

    void Update()
    {
        if (Mouse.current == null || targetPanel == null) return;

        // 1. 获取鼠标在屏幕上的像素坐标
        Vector2 screenPos = Mouse.current.position.ReadValue();

        // 2. 获取渲染相机（Overlay模式传null，Camera模式传Canvas的摄像机）
        Camera cam = null;
        if (parentCanvas != null)
        {
            // 如果 Canvas 是 Screen Space - Camera，需要传入对应摄像机进行坐标换算
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
                cam = parentCanvas.worldCamera;
            // 如果是 World Space，通常需要用主摄像机，但此处简化处理
            else if (parentCanvas.renderMode == RenderMode.WorldSpace)
                cam = Camera.main;
        }

        // 3. 核心换算：将屏幕坐标转换为 targetPanel 下的局部坐标
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetPanel,    // 以哪个面板为参考基准
            screenPos,      // 屏幕鼠标位置
            cam,            // 相机（Overlay 模式下传 null）
            out Vector2 localPoint))
        {
            // 4. 将换算后的局部坐标赋值给准心的 AnchoredPosition
            rectTransform.anchoredPosition = localPoint;
        }
    }
}