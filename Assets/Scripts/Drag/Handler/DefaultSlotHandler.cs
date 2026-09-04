/****************************************************
    文件：DefaultSlotHandler.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-04 16:12:01
	功能：默认背包/宝箱槽位行为
*****************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DefaultSlotHandler : MonoBehaviour, ISlotDragHandler
{
    public virtual bool CanBeginDrag(PointerEventData eventData, Image slot, ISlotOwner owner, int index)
    {
        var item = owner.Container.GetItem(index);
        return item != null && item.amount > 0;
    }

    public virtual void OnBeginDrag(PointerEventData eventData, Image slot, ISlotOwner owner, int index)
    {
        // 由 DragManager 统一处理视觉克隆和透明度，这里可以留空
        // 也可以在这里自定义视觉效果，但为了复用现有流程，建议交给 DragManager 统一处理
    }

    public virtual void OnDrag(PointerEventData eventData, Image slot) { }

    public virtual bool OnEndDrag(PointerEventData eventData, Image slot, ISlotOwner owner, int index,
                          Image targetSlot, (ISlotOwner owner, int index)? targetInfo)
    {
        return false; // 未处理，让 DragManager 可能执行丢弃等逻辑
    }

    public virtual bool CanDrop(ISlotOwner sourceOwner, int sourceIndex)
    {
        // 默认总是接受（由容器自身容量决定）
        return true;
    }
}