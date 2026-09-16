/****************************************************
    文件：VendingMachineData.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-16
	功能：售货机数据层（列表存储 + 按配置生成商品）
*****************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 售货机数据层：底层使用 ItemContainer（List&lt;ItemInstance&gt; 列表存储）。
/// 与 WarehouseData 对称，但售货机只负责"摆货"，不参与背包/仓库拖拽，
/// 因此不提供 AddItem/ConsumeItem 等玩家写入操作，只提供按配置生成与读取接口。
/// </summary>
[Serializable]
public class VendingMachineData : IItemContainer
{
    private ItemContainer _container;

    public int SlotCount => _container?.SlotCount ?? 0;

    /// <summary>槽位变化事件，参数为槽位索引</summary>
    public event Action<int> OnSlotChanged;

    public VendingMachineData(int capacity)
    {
        _container = new ItemContainer(capacity);
        BindContainerEvents();
    }

    /// <summary>
    /// 按配置列表生成商品：先清空容器，再把每条合法配置写入对应格子。
    /// 越界/非法的配置条目会跳过并打印警告。
    /// </summary>
    public void ConfigureFromList(IEnumerable<VendingSlotConfig> configs)
    {
        if (_container == null || configs == null) return;

        _container.Clear();

        foreach (var config in configs)
        {
            if (!config.IsValid)
            {
                Debug.LogWarning($"[VendingMachineData] 跳过非法商品配置：slotIndex={config.slotIndex}, itemId={config.itemId}, amount={config.amount}");
                continue;
            }

            if (config.slotIndex >= _container.SlotCount)
            {
                Debug.LogWarning($"[VendingMachineData] 格子索引越界，跳过 slotIndex={config.slotIndex}（容量 {_container.SlotCount}）");
                continue;
            }

            var data = ItemRegistry.Get(config.itemId);
            if (data == null) continue;

            // 数量上限钳制为该物品的最大堆叠数
            int amount = Math.Min(config.amount, data.maxStack);
            _container.SetItem(config.slotIndex, new ItemInstance(config.itemId, amount));
        }
    }

    /// <summary>
    /// 获取指定索引位置的物品实例（点击格子读取商品信息时使用）
    /// </summary>
    public ItemInstance GetSlotContent(int index)
    {
        if (_container == null || index < 0 || index >= _container.SlotCount)
            return null;
        return _container.GetItem(index);
    }

    /// <summary>
    /// 清空所有商品（保留容量）
    /// </summary>
    public void Clear()
    {
        _container?.Clear();
    }

    // ---- IItemContainer 接口实现（供 VendingMachineUI 等读取）----

    public ItemInstance GetItem(int index) => _container.GetItem(index);
    public void SetItem(int index, ItemInstance item) => _container.SetItem(index, item);

    /// <summary>
    /// 将底层容器的事件转发到数据层事件
    /// </summary>
    private void BindContainerEvents()
    {
        _container.OnSlotChanged += index => OnSlotChanged?.Invoke(index);
    }
}