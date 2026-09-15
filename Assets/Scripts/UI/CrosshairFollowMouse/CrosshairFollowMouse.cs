/****************************************************
    文件：CrosshairFollowMouse.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-28 16:13:29
	功能：准心预制体跟随鼠标
    说明：准心直接贴在鼠标像素位置；玩家/子弹方向由
        PlayerInputHandler 的鼠标射线→玩家高度平面交点决定，
        两者共用同一个鼠标输入，因此准心所指即子弹所向。
*****************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

public class CrosshairFollowMouse : MonoBehaviour
{
    [Header("目标面板")]
    [SerializeField] private RectTransform targetPanel;

    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private bool _paused; // 面板打开时暂停跟随（准心微动也随之冻结）

    void OnEnable()
    {
        PlayerEvents.OnGameplayBlocked += OnGameplayBlocked;
    }

    void OnDisable()
    {
        PlayerEvents.OnGameplayBlocked -= OnGameplayBlocked;
    }

    private void OnGameplayBlocked(bool blocked)
    {
        _paused = blocked;
    }

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
        if (_paused || Mouse.current == null || targetPanel == null) return;

        //  获取鼠标射线与地面平面的交点
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (!groundPlane.Raycast(ray, out float enter)) return;

        //  获取鼠标在屏幕上的像素坐标
        Vector3 worldAimPoint = ray.GetPoint(enter) + Vector3.up * 2f;

        // 将3D世界点转换为屏幕坐标
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldAimPoint);

        // 获取渲染相机（Overlay模式传null，Camera模式传Canvas的摄像机）
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

        // 核心换算：将屏幕坐标转换为 targetPanel 下的局部坐标
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
