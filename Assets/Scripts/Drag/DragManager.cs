/****************************************************
    文件：DragManager.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-01 17:18:34
	功能：全局拖拽管理器
*****************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private ISlotOwner sourceSlotOwner;       // 拖拽起始的宝箱
    private int sourceIndex;                  // 拖拽起始的槽位索引
    private Image sourceSlotImage;            // 拖拽起始的槽位 UI 组件
    public DragVisualController visualController; // 拖拽视觉控制器，负责显示跟随鼠标的图标

    private Dictionary<Image, (ISlotOwner owner, int index, ISlotDragHandler handler)> _slotInfo = new();


    // 已绑定过拖拽事件的格子，避免重复添加 EventTrigger
    private readonly HashSet<Image> _boundSlots = new();

    private void Start()
    {
        // 如果 Inspector 中未手动指定宝箱列表，则自动查找场景中所有的 ChestManager
        // 注意：面板初始为 inactive（由 BaseUIPanel 控制显隐），必须包含 inactive 对象
        if (chests == null || chests.Count == 0)
        {
            chests = new List<ChestManager>(FindObjectsByType<ChestManager>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        }

        // 遍历所有宝箱及其槽位，为每个槽位动态添加拖拽事件监听
        foreach (var chest in chests)
        {
            chest.EnsureInitialized(); // 面板未激活时 Awake 未执行，这里手动收集格子
            for (int i = 0; i < chest.slotImages.Count; i++)
            {
                var slot = chest.slotImages[i];
                ISlotDragHandler handler = slot.GetComponent<ISlotDragHandler>() ?? slot.gameObject.AddComponent<DefaultSlotHandler>();
                if (!_boundSlots.Add(slot)) continue; // 已绑定则跳过
                _slotInfo[slot] = (chest, i, handler); //  O(1) 缓存
                AddEventTriggersToSlot(slot);
            }
        }

        var backpackUI = UIController.Instance != null ? UIController.Instance.backpackUI : null;
        if (backpackUI == null)
            backpackUI = FindFirstObjectByType<BackpackUI>(FindObjectsInactive.Include);
        if (backpackUI != null && backpackUI.SlotImages != null)
        {
            for (int i = 0; i < backpackUI.SlotImages.Count; i++)
            {
                var slot = backpackUI.SlotImages[i];
                ISlotDragHandler handler = slot.GetComponent<ISlotDragHandler>() ?? slot.gameObject.AddComponent<DefaultSlotHandler>();
                if (!_boundSlots.Add(slot)) continue;
                _slotInfo[slot] = (backpackUI, i, handler);
                AddEventTriggersToSlot(slot);
            }
        }

    }

    /// <summary>
    /// 为指定的槽位 Image 添加拖拽相关的 EventTrigger 事件
    /// </summary>
    /// <param name="slot">目标槽位的 Image 组件</param>
    private void AddEventTriggersToSlot(Image slot)
    {
        // 拖拽依赖 EventSystem 射线命中该格子。
        // 这里强制开启，保证所有参与拖拽的格子可被射线命中。
        slot.raycastTarget = true;

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
        if (!_slotInfo.TryGetValue(slot, out var info)) return;
        var (owner, index, handler) = info;

        if (!handler.CanBeginDrag(eventData, slot, owner, index))
            return;
        var container = info.owner.Container; // IItemContainer

        // 如果找不到归属宝箱或槽位无效，直接返回
        if (container == null || index == -1) return;

        // 检查该槽位是否有物品
        ItemInstance item = container.GetItem(index);
        //空壳实例检查
        if (item == null || item.amount <= 0) return;

        // 根据物品的 iconKey加载图标 Sprite
        Sprite icon = ResourceManager.LoadUISprite(item.Data.iconKey); ; 
        if (icon == null)
        {
            Debug.LogWarning($"图标未找到: UI/{item.Data.iconKey}");
            return;
        }

        handler.OnBeginDrag(eventData, slot, owner, index);

        // 记录拖拽源信息
        isDragging = true;
        sourceSlotOwner = info.owner;
        sourceIndex = index;
        sourceSlotImage = slot;

        // 创建跟随鼠标的视觉克隆体
        visualController.Show(icon, slot.rectTransform,eventData.position);
        // 将原始槽位设为半透明，提示用户该位置物品已被拿起
        slot.color = new Color(1, 1, 1, 0.3f);
    }

    /// <summary>
    /// 拖拽中回调：持续更新克隆体位置使其跟随鼠标
    /// </summary>
    private void OnDrag(PointerEventData eventData, Image slot)
    {
        if (!isDragging) return;
        visualController.Follow(eventData.position);
    }

    /// <summary>
    /// 结束拖拽回调：恢复原槽位显示，销毁克隆体，检测放置目标并执行物品移动
    /// </summary>
    private void OnEndDrag(PointerEventData eventData, Image slot)
    {

        if (!_slotInfo.TryGetValue(slot, out var sourceInfo)) return;
        var (sourceOwner, srcIdx, sourceHandler) = sourceInfo;

        if (!isDragging) return;

        // 恢复源槽位透明度
        if (sourceSlotImage != null)
        {
            Color c = sourceSlotImage.color;
            c.a = 1f;           // 只恢复不透明，保留颜色值
            sourceSlotImage.color = c;
        }

        // 销毁克隆体
        visualController.Hide();

        // 重置全局拖拽状态
        isDragging = false;
        sourceSlotOwner = null;
        sourceSlotImage = null;
        sourceIndex = -1;

        // 检测目标槽位
        RaycastResult raycast = eventData.pointerCurrentRaycast;
        Image targetSlot = raycast.isValid ? raycast.gameObject?.GetComponent<Image>() : null;

        (ISlotOwner owner, int index)? targetInfo = null;
        if (targetSlot != null && _slotInfo.TryGetValue(targetSlot, out var targetData))
            targetInfo = (targetData.owner, targetData.index);

        // 先让源处理器处理拖拽结束（可自定义逻辑）
        bool handled = sourceHandler.OnEndDrag(eventData, slot, sourceOwner, srcIdx, targetSlot, targetInfo);
        if (handled) return;

        // 默认行为：尝试移动到目标槽位
        if (targetSlot != null && targetInfo.HasValue)
        {
            var target = targetInfo.Value;
            var targetHandler = _slotInfo[targetSlot].handler;
            if (targetHandler.CanDrop(sourceOwner, srcIdx))
            {
                bool success = ItemContainer.MoveBetween(sourceOwner.Container, srcIdx,
                                                         target.owner.Container, target.index);
                if (success)
                {
                    sourceOwner.RefreshSlot(srcIdx);
                    // 刷新目标（如果是不同容器或不同索引）
                    if (!ReferenceEquals(target.owner, sourceOwner) || target.index != srcIdx)
                        target.owner.RefreshSlot(target.index);
                }

                //  触发事件：源槽位如果是装备槽
                if (sourceHandler is EquipmentSlotHandler)
                {
                    var newSrcItem = sourceOwner.Container.GetItem(srcIdx);
                    PlayerEvents.Instance.TriggerEquipmentSlotChanged(srcIdx, newSrcItem);
                }

                //  触发事件：目标槽位如果是装备槽
                if (targetHandler is EquipmentSlotHandler)
                {
                    var newDstItem = target.owner.Container.GetItem(target.index);
                    PlayerEvents.Instance.TriggerEquipmentSlotChanged(target.index, newDstItem);
                }
            }
        }
        else
        {
            // 拖拽到空白处：如果是装备槽，触发卸下事件
            if (sourceHandler is EquipmentSlotHandler)
            {

            }
        }

        

    }
    /// <summary>
    /// Drop 事件回调：当前实现中放置逻辑已在 OnEndDrag 中统一处理中处理，因此这里可以保留空实现
    /// </summary>
    private void OnDrop(PointerEventData eventData, Image slot)
    {
        // 可保留空实现
    }
}