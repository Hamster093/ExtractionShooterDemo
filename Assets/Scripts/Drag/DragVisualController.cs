/****************************************************
    文件：DragVisualController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-04 15:25:11
	功能：拖拽视觉实现类，负责在拖拽过程中显示物品图标跟随鼠标移动
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

public class DragVisualController : MonoBehaviour 
{
    private GameObject dragClone;
    private RectTransform dragCloneRect;

    // <summary>
    /// 创建拖拽克隆体：在 Canvas 下生成一个跟随鼠标的半透明图标副本
    /// </summary>
    public void Show(Sprite icon, RectTransform originalRect, Vector2 screenPos)
    {
        //查找父物体
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            //按类型查找第一个对象
            canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("场景中没有 Canvas！");
            return;
        }

        // 创建克隆体 GameObject 并挂载到 Canvas 下
        dragClone = new GameObject("DragClone");
        dragClone.transform.SetParent(canvas.transform, false);// false 表示不继承父级缩放
        dragClone.transform.SetAsLastSibling();// 确保克隆体渲染在最上层

        // 添加 Image 组件并配置显示属性
        Image cloneImage = dragClone.AddComponent<Image>();
        cloneImage.sprite = icon;
        cloneImage.raycastTarget = false;// 关闭射线检测，防止克隆体阻挡下方槽位的 Drop/EndDrag 事件
        cloneImage.color = new Color(1, 1, 1, 0.8f);

        // 缓存 RectTransform 并同步原始槽位尺寸
        dragCloneRect = dragClone.GetComponent<RectTransform>();
        dragCloneRect.sizeDelta = originalRect.sizeDelta;
        Follow(screenPos);
    }
    /// <summary>
    /// 更新拖拽克隆体位置：将屏幕坐标转换为 Canvas 下的世界坐标
    /// </summary>
    /// <param name="screenPos">鼠标在屏幕上的坐标（来自 PointerEventData）</param>
    public void Follow(Vector2 screenPos) 
    {
        if (dragCloneRect == null) return;

        // 父级 RectTransform（即 Canvas）
        RectTransform parentRect = dragCloneRect.parent as RectTransform;
        if (parentRect == null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();

        // 依据画布渲染模式选择相机：
        // - Screen Space - Overlay：必须传 null 相机
        // - Screen Space - Camera / World Space：优先用画布指定相机，未指定再回退 Camera.main
        Camera cam;
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            cam = null;
        else if (canvas != null && canvas.worldCamera != null)
            cam = canvas.worldCamera;
        else
            cam = Camera.main;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            parentRect,
            screenPos,
            cam,
            out Vector3 worldPos);
        // 应用计算出的位置
        dragCloneRect.position = worldPos;
    }
    /// <summary>
    /// 销毁克隆体 GameObject，释放资源
    /// </summary>
    public void Hide() { /* 原 Destroy 逻辑 */
        if (dragClone != null)
        {
            Destroy(dragClone);
            dragClone = null;
        }
    }
}