/****************************************************
    文件：DragManager.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-01 17:18:34
	功能：全局拖拽管理器
*****************************************************/

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 拖拽管理器：负责处理物品在多个宝箱（ChestManager）之间的拖拽交互逻辑。
/// 通过 EventTrigger 动态绑定事件，实现物品的拾取、跟随鼠标移动及放置交换。
/// </summary>
public class DragManager : MonoBehaviour
{
    [Tooltip("场景中所有参与拖拽交互的宝箱管理器列表")]
    public List<ChestManager> chests;

    // --- 拖拽状态变量 ---
    private bool isDragging = false;          // 当前是否处于拖拽状态
    private ChestManager sourceChest;         // 拖拽起始的宝箱
    private int sourceIndex;                  // 拖拽起始的槽位索引
    private Image sourceSlotImage;            // 拖拽起始的槽位 UI 组件
    private GameObject dragClone;             // 拖拽时跟随鼠标的克隆体
    private RectTransform dragCloneRect;      // 克隆体的 RectTransform，用于位置更新

    private void Start()
    {
        // 如果 Inspector 中未手动指定宝箱列表，则自动查找场景中所有的 ChestManager
        if (chests == null || chests.Count == 0)
        {
            chests = new List<ChestManager>(FindObjectsByType<ChestManager>(FindObjectsSortMode.None));
        }

        // 遍历所有宝箱及其槽位，为每个槽位动态添加拖拽事件监听
        foreach (var chest in chests)
        {
            foreach (var slotImage in chest.slotImages)
            {
                AddEventTriggersToSlot(slotImage);
            }
        }
    }

    /// <summary>
    /// 为指定的槽位 Image 添加拖拽相关的 EventTrigger 事件
    /// </summary>
    /// <param name="slot">目标槽位的 Image 组件</param>
    private void AddEventTriggersToSlot(Image slot)
    {
        // 获取或创建 EventTrigger 组件
        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = slot.gameObject.AddComponent<EventTrigger>();

        // 注册 BeginDrag 事件：开始拖拽时触发
        EventTrigger.Entry beginEntry = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        beginEntry.callback.AddListener((data) => OnBeginDrag((PointerEventData)data, slot));
        trigger.triggers.Add(beginEntry);

        // 注册 Drag 事件：拖拽过程中持续触发
        EventTrigger.Entry dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        dragEntry.callback.AddListener((data) => OnDrag((PointerEventData)data, slot));
        trigger.triggers.Add(dragEntry);

        // 注册 EndDrag 事件：释放鼠标时触发（处理放置逻辑）
        EventTrigger.Entry endEntry = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
        endEntry.callback.AddListener((data) => OnEndDrag((PointerEventData)data, slot));
        trigger.triggers.Add(endEntry);

        // 注册 Drop 事件：作为被放置目标时触发
        EventTrigger.Entry dropEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drop };
        dropEntry.callback.AddListener((data) => OnDrop((PointerEventData)data, slot));
        trigger.triggers.Add(dropEntry);
    }

    /// <summary>
    /// 开始拖拽回调：验证物品有效性，创建拖拽克隆体，并将原槽位半透明化
    /// </summary>
    private void OnBeginDrag(PointerEventData eventData, Image slot)
    {
        // 查找当前槽位所属的宝箱及索引
        ChestManager ownerChest = null;
        int index = -1;
        foreach (var chest in chests)
        {
            // 获取 slot 在 chest.slotImages 列表中的索引，并赋值给整型变量 idx
            int idx = chest.slotImages.IndexOf(slot);
            if (idx != -1)
            {
                ownerChest = chest;
                index = idx;
                break;
            }
        }
        // 如果找不到归属宝箱或槽位无效，直接返回
        if (ownerChest == null || index == -1) return;

        // 检查该槽位是否有物品
        ItemData item = ownerChest.GetItem(index);
        if (item == null) return;

        // 根据物品的 iconKey 从 Resources 加载图标 Sprite
        Sprite icon = Resources.Load<Sprite>("UI/" + item.iconKey);
        if (icon == null)
        {
            Debug.LogWarning($"图标未找到: UI/{item.iconKey}");
            return;
        }

        // 记录拖拽源信息
        isDragging = true;
        sourceChest = ownerChest;
        sourceIndex = index;
        sourceSlotImage = slot;

        // 创建跟随鼠标的视觉克隆体
        CreateDragClone(icon, slot.rectTransform);
        // 将原始槽位设为半透明，提示用户该位置物品已被拿起
        slot.color = new Color(1, 1, 1, 0.3f);
    }

    /// <summary>
    /// 拖拽中回调：持续更新克隆体位置使其跟随鼠标
    /// </summary>
    private void OnDrag(PointerEventData eventData, Image slot)
    {
        if (!isDragging || dragClone == null) return;
        UpdateDragClonePosition(eventData.position);
    }

    /// <summary>
    /// 结束拖拽回调：恢复原槽位显示，销毁克隆体，检测放置目标并执行物品移动
    /// </summary>
    private void OnEndDrag(PointerEventData eventData, Image slot)
    {
        if (!isDragging) return;

        // 恢复原始槽位颜色为不透明
        if (sourceSlotImage != null)
            sourceSlotImage.color = Color.white;

        // 销毁拖拽克隆体
        if (dragClone != null)
        {
            Destroy(dragClone);
            dragClone = null;
        }

        // 修复问题 ：pointerCurrentRaycast 是结构体，不能直接判空
        // 必须先取出副本，再通过 isValid 属性判断射线检测结果是否有效
        RaycastResult raycast = eventData.pointerCurrentRaycast;
        GameObject targetObj = raycast.isValid ? raycast.gameObject : null;

        if (targetObj != null)
        {
            // 检查释放位置是否是另一个有效的槽位 Image
            Image targetSlot = targetObj.GetComponent<Image>();
            if (targetSlot != null && targetSlot != sourceSlotImage)
            {
                // 查找目标槽位所属的宝箱及索引
                ChestManager targetChest = null;
                int targetIndex = -1;
                foreach (var chest in chests)
                {
                    int idx = chest.slotImages.IndexOf(targetSlot);
                    if (idx != -1)
                    {
                        targetChest = chest;
                        targetIndex = idx;
                        break;
                    }
                }
                // 如果目标有效，执行物品从源位置到目标位置的移动/交换
                if (targetChest != null && targetIndex != -1)
                {
                    sourceChest.MoveItemFromTo(sourceIndex, targetChest, targetIndex);
                }
            }
        }
        // 重置拖拽状态
        isDragging = false;
        sourceChest = null;
        sourceSlotImage = null;
    }

    /// <summary>
    /// Drop 事件回调：当前实现中放置逻辑已在 OnEndDrag 中统一处理中处理，因此这里可以保留空实现
    /// </summary>
    private void OnDrop(PointerEventData eventData, Image slot)
    {
        // 可保留空实现
    }

    /// <summary>
    /// 创建拖拽克隆体：在 Canvas 下生成一个跟随鼠标的半透明图标副本
    /// </summary>
    /// <param name="icon">要显示的图标 Sprite</param>
    /// <param name="originalRect">原始槽位的 RectTransform，用于同步尺寸</param>
    private void CreateDragClone(Sprite icon, RectTransform originalRect)
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
    }

    /// <summary>
    /// 更新拖拽克隆体位置：将屏幕坐标转换为 Canvas 下的世界坐标
    /// </summary>
    /// <param name="screenPos">鼠标在屏幕上的坐标（来自 PointerEventData）</param>
    private void UpdateDragClonePosition(Vector2 screenPos)
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
}