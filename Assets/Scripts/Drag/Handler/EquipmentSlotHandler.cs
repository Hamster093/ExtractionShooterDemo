/****************************************************
    文件：EquipmentSlotHandler.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-06 16:20:00
	功能：装备槽位行为（由 ChestManager.isEquipmentGrid 自动挂载）
*****************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 装备槽处理器：
/// 1. CanDrop：按同物体 SlotUI 上的类别标志（allowedType）过滤物品类型，None 表示不限制；
/// 2. OnEndDrag：拖到空白处（非槽位）时卸下装备，物品放回背包并广播装备槽变化事件。
/// 注意：此组件在运行时由 ChestManager 自动挂载，Inspector 无法配置，
///       因此格子的类别标志统一配置在同物体 SlotUI 的 allowedType 字段上。
/// </summary>
public class EquipmentSlotHandler : DefaultSlotHandler
{
    public override bool CanDrop(ISlotOwner sourceOwner, int sourceIndex)
    {
        var item = sourceOwner.Container.GetItem(sourceIndex);
        if (item == null) return false;

        var slotUI = GetComponent<SlotUI>();
        if (slotUI == null || slotUI.allowedType == ItemType.None) return true;

        return item.Data.type == slotUI.allowedType;
    }

    public override bool OnEndDrag(PointerEventData eventData, Image slot,
        ISlotOwner owner, int index, Image targetSlot, (ISlotOwner owner, int index)? targetInfo)
    {
        // 拖到空白处（非槽位）：卸下装备，放回背包
        if (targetSlot == null)
        {
            var item = owner.Container.GetItem(index);
            if (item != null)
            {
                // 从装备槽移除并放回背包（保留原数量）
                owner.Container.SetItem(index, null);
                GameService.Backpack.AddItem(item.itemID, item.amount);

                // 刷新装备槽与背包UI
                owner.RefreshSlot(index);
                var backpackUI = FindFirstObjectByType<BackpackUI>(FindObjectsInactive.Include);
                if (backpackUI != null) backpackUI.RefreshUI();

                // 广播卸下事件：武器栏等订阅方据此清空对应栏位
                PlayerEvents.Instance.TriggerEquipmentSlotChanged(index, null);
            }
            return true; // 已处理，阻止 DragManager 执行默认移动
        }

        // 目标是其他槽位：交回 DragManager 执行默认移动/交换
        return false;
    }
}
