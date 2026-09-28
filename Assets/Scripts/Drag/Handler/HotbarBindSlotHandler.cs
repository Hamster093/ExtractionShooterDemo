/****************************************************
    文件：HotbarBindSlotHandler.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-27 14:38:00
	功能：快捷栏格专用拖拽处理器（绑定语义：只记引用，不搬运物品）
*****************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 快捷栏格拖拽处理器。快捷栏存的不是物品拥有权，而是对背包 ItemInstance 的引用，
/// 因此这里必须绕开 DragManager 的默认"移动/交换"逻辑：
///   1) 作为拖入目标：CanDrop 恒为 false（绝不让 MoveBetween 把实例搬进快捷栏容器）；
///      真正的"绑定"由 ISlotDropReceiver.TryReceiveDrop 在默认移动之前完成；
///   2) 作为拖出源：OnEndDrag 返回 true（已处理），目标是另一个快捷栏格则交换绑定，
///      拖到背包/仓库/空白处则解绑；同样避免共享实例被搬进别的容器。
/// 注意：绑定只接受"来自背包的消耗品"，规则集中在 HotbarService。
/// </summary>
public class HotbarBindSlotHandler : DefaultSlotHandler, ISlotDropReceiver
{
    [Tooltip("本格在快捷栏中的索引；-1 = 运行时从父级 HotbarUI 自动解析")]
    [SerializeField] private int _slotIndex = -1;

    /// <summary>
    /// 本格在快捷栏中的索引（首次访问时从父级 HotbarUI 解析并缓存）
    /// </summary>
    public int SlotIndex
    {
        get
        {
            if (_slotIndex < 0)
            {
                var ui = GetComponentInParent<HotbarUI>(true);
                var slot = GetComponent<SlotUI>();
                if (ui != null && slot != null)
                    _slotIndex = ui.IndexOf(slot);
            }
            return _slotIndex;
        }
    }

    /// <summary>
    /// 有绑定才允许拖出（空格子拖不动）
    /// </summary>
    public override bool CanBeginDrag(PointerEventData eventData, Image slot, ISlotOwner owner, int index)
    {
        var item = owner.Container.GetItem(index);
        return item != null && item.amount > 0;
    }

    /// <summary>
    /// 作为拖入目标：恒不接受默认移动（绑定走 TryReceiveDrop）
    /// </summary>
    public override bool CanDrop(ISlotOwner sourceOwner, int sourceIndex) => false;

    /// <summary>
    /// 接管"拖入快捷栏"的放置：只建立绑定、不移动源物品。
    /// 无论成功与否都返回 true —— 失败的（非消耗品/非背包来源）由 HotbarService 给提示，
    /// 但绝不能落到默认移动分支（否则共享实例会被搬进快捷栏容器）。
    /// </summary>
    public bool TryReceiveDrop(IItemContainer sourceContainer, int sourceIndex)
    {
        HotbarService.TryBind(sourceContainer, sourceIndex, SlotIndex);
        return true;
    }

    /// <summary>
    /// 作为拖出源：目标是另一个快捷栏格 → 交换绑定；拖到别处（背包格/仓库格/空白）→ 解绑。
    /// 返回 true 阻止 DragManager 的默认移动。
    /// </summary>
    public override bool OnEndDrag(PointerEventData eventData, Image slot, ISlotOwner owner, int index,
                                   Image targetSlot, (ISlotOwner owner, int index)? targetInfo)
    {
        if (targetInfo.HasValue && ReferenceEquals(targetInfo.Value.owner, owner))
        {
            // 同一个快捷栏容器内拖动 = 交换两格绑定
            HotbarService.SwapBinding(index, targetInfo.Value.index);
        }
        else
        {
            // 拖到背包/仓库/空白处 = 解绑（背包里的物品保持不动）
            HotbarService.Unbind(index);
        }
        return true;
    }
}
