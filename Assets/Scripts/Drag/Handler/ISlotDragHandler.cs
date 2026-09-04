using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 槽位拖拽行为接口，每个槽位可挂载不同的实现。
/// </summary>
public interface ISlotDragHandler
{
    /// <summary>
    /// 是否允许开始拖拽（例如检查物品是否存在、是否符合拖拽条件）
    /// </summary>
    bool CanBeginDrag(PointerEventData eventData, Image slot, ISlotOwner owner, int index);

    /// <summary>
    /// 开始拖拽时的回调（用于创建视觉克隆、改变颜色等）
    /// </summary>
    void OnBeginDrag(PointerEventData eventData, Image slot, ISlotOwner owner, int index);

    /// <summary>
    /// 拖拽中的回调（通常用于更新位置）
    /// </summary>
    void OnDrag(PointerEventData eventData, Image slot);

    /// <summary>
    /// 拖拽结束时的回调（在源槽位被释放时调用）
    /// </summary>
    /// <param name="targetSlot">鼠标释放时命中的目标槽位（可能为null）</param>
    /// <param name="targetInfo">目标槽位的所有者及索引（若有效）</param>
    /// <returns>返回 true 表示已经处理了拖拽结束逻辑，无需再执行默认移动</returns>
    bool OnEndDrag(PointerEventData eventData, Image slot, ISlotOwner owner, int index,Image targetSlot, (ISlotOwner owner, int index)? targetInfo);

    /// <summary>
    /// 目标槽位接受放置的判定（当其他物品被拖到这个槽位时调用）
    /// </summary>
    /// <param name="sourceOwner">拖拽源所有者</param>
    /// <param name="sourceIndex">拖拽源索引</param>
    /// <returns>true 表示允许放置，false 拒绝</returns>
    bool CanDrop(ISlotOwner sourceOwner, int sourceIndex);
}