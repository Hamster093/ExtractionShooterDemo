/****************************************************
    文件：HotbarData.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-27 14:35:00
	功能：快捷栏数据类（6格，存放"绑定到背包物品实例"的引用）
*****************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 快捷栏数据层：底层依然使用 ItemContainer（列表存储），与背包/仓库对称。
/// 但语义不同：这里存的不是"物品的拥有权"，而是对背包中某个 ItemInstance 的【引用】。
/// 同一个实例对象同时挂在背包格与快捷栏格上，所以数量变化两边天然同步。
/// 由此产生三条约束：
///   1) 快捷栏格绝对不能参与 ItemContainer.MoveBetween，
///      否则同一实例会同时出现在两个容器里（读写会互相污染）；
///   2) 快捷栏不导出到存档（SaveGameService 不处理它），只随常驻 InventoryService 跨场景保留；
///   3) 实例是否"仍然在背包里"由 HotbarService.Validate() 负责校验，失效即自动解绑。
/// </summary>
[Serializable]
public class HotbarData : IItemContainer
{
    private ItemContainer _container;

    /// <summary>
    /// 快捷栏格数（只读）
    /// </summary>
    public int SlotCount => _container?.SlotCount ?? 0;

    /// <summary>
    /// 快捷栏格变化事件，参数为快捷栏格索引
    /// </summary>
    public event Action<int> OnSlotChanged;

    public HotbarData(int capacity)
    {
        _container = new ItemContainer(capacity);
        _container.OnSlotChanged += index => OnSlotChanged?.Invoke(index);
    }

    public ItemInstance GetItem(int index) => _container.GetItem(index);

    public void SetItem(int index, ItemInstance item) => _container.SetItem(index, item);

    /// <summary>
    /// 清空所有绑定（读档/新游戏/重置时调用，逐格触发 OnSlotChanged 让 UI 刷新）
    /// </summary>
    public void Clear() => _container.Clear();

    /// <summary>
    /// 导出当前绑定列表（调试/校验用，不参与存档）
    /// </summary>
    public List<ItemInstance> Snapshot()
    {
        var list = new List<ItemInstance>();
        for (int i = 0; i < SlotCount; i++)
            list.Add(GetItem(i));
        return list;
    }
}
