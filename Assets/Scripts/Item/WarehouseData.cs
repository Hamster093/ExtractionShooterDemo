/****************************************************
    文件：WarehouseData.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-14 00:30:00
	功能：仓库数据类（列表存储 + 数据库存档接口预留）
*****************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 仓库数据层：与 BackpackData 对称。
/// 底层使用 ItemContainer（内部即 List&lt;ItemInstance&gt; 列表存储），
/// 提供增删查改、动态扩容，以及导出/导入存档列表接口（供数据库保存/加载）。
/// </summary>
[Serializable]
public class WarehouseData : IItemContainer
{
    private ItemContainer _container;

    public int SlotCount => _container?.SlotCount ?? 0;

    /// <summary>槽位变化事件，参数为槽位索引</summary>
    public event Action<int> OnSlotChanged;
    /// <summary>容量变化事件，参数为新容量</summary>
    public event Action<int> OnCapacityChanged;

    public WarehouseData(int initialCapacity)
    {
        _container = new ItemContainer(initialCapacity);
        BindContainerEvents();
    }

    /// <summary>
    /// 添加物品，只做一次溢出判定，返回剩余未加入格子的数量
    /// </summary>
    public int AddItem(int itemId, int amount)
    {
        var data = ItemRegistry.Get(itemId);
        if (data == null) return -1;

        int remaining = amount;

        // 第一轮：往已有同类物品的格子堆叠
        for (int i = 0; i < _container.SlotCount && remaining > 0; i++)
        {
            var slot = _container.GetItem(i);
            if (slot != null && slot.itemID == itemId && slot.amount < data.maxStack)
            {
                int canAdd = Mathf.Min(remaining, data.maxStack - slot.amount);
                slot.amount += canAdd;
                remaining -= canAdd;
                OnSlotChanged?.Invoke(i);
            }
        }

        // 第二轮：剩余数量放入空格子
        for (int i = 0; i < _container.SlotCount && remaining > 0; i++)
        {
            if (_container.GetItem(i) == null)
            {
                int canAdd = Mathf.Min(remaining, data.maxStack);
                _container.SetItem(i, new ItemInstance(itemId, canAdd));
                remaining -= canAdd;
                OnSlotChanged?.Invoke(i);
            }
        }

        return remaining;
    }

    /// <summary>
    /// 消耗物品，返回是否消耗完全
    /// </summary>
    public bool ConsumeItem(int itemId, int amount)
    {
        int remaining = amount;
        for (int i = _container.SlotCount - 1; i >= 0 && remaining > 0; i--)
        {
            var slot = _container.GetItem(i);
            if (slot != null && slot.itemID == itemId)
            {
                int canRemove = Mathf.Min(remaining, slot.amount);
                slot.amount -= canRemove;
                remaining -= canRemove;

                if (slot.amount <= 0)
                    _container.SetItem(i, null);

                OnSlotChanged?.Invoke(i);
            }
        }
        return remaining == 0;
    }

    /// <summary>
    /// 查询指定物品总数
    /// </summary>
    public int GetItemCount(int itemId)
    {
        int total = 0;
        for (int i = 0; i < _container.SlotCount; i++)
        {
            var slot = _container.GetItem(i);
            if (slot != null && slot.itemID == itemId)
                total += slot.amount;
        }
        return total;
    }

    /// <summary>
    /// 动态调整仓库容量（扩容补空格子；缩容从末尾截断并警告丢弃物品）
    /// </summary>
    public void SetCapacity(int newCapacity)
    {
        if (newCapacity < 1)
        {
            Debug.LogError($"[WarehouseData] 容量不能小于1，传入值: {newCapacity}");
            return;
        }

        int oldCapacity = _container.SlotCount;
        if (newCapacity == oldCapacity) return;

        if (newCapacity < oldCapacity)
        {
            for (int i = newCapacity; i < oldCapacity; i++)
            {
                var item = _container.GetItem(i);
                if (item != null)
                    Debug.LogWarning($"[WarehouseData] 缩容导致索引 {i} 的物品被丢弃: ID={item.itemID}, Amount={item.amount}");
            }
        }

        var newContainer = new ItemContainer(newCapacity);
        int migrateCount = Mathf.Min(oldCapacity, newCapacity);
        for (int i = 0; i < migrateCount; i++)
            newContainer.SetItem(i, _container.GetItem(i));

        _container = newContainer;
        BindContainerEvents();

        OnCapacityChanged?.Invoke(newCapacity);

        if (newCapacity < oldCapacity)
        {
            for (int i = newCapacity; i < oldCapacity; i++)
                OnSlotChanged?.Invoke(i);
        }
    }

    /// <summary>
    /// 获取指定索引位置的物品实例
    /// </summary>
    public ItemInstance GetSlotContent(int index)
    {
        if (_container == null || index < 0 || index >= _container.SlotCount)
            return null;
        return _container.GetItem(index);
    }

    /// <summary>
    /// 清空所有物品
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < _container.SlotCount; i++)
        {
            _container.SetItem(i, null);
            OnSlotChanged?.Invoke(i);
        }
    }

    // ---- IItemContainer 接口实现（供 DragManager/UI 使用）----

    public ItemInstance GetItem(int index) => _container.GetItem(index);
    public void SetItem(int index, ItemInstance item) => _container.SetItem(index, item);

    // ---- 数据库存档接口（列表存储导出/导入）----

    /// <summary>
    /// 导出所有非空格子为可序列化列表（供数据库保存；列表存储）
    /// </summary>
    public List<ItemSlotSaveData> ExportToSaveList()
    {
        var list = new List<ItemSlotSaveData>();
        if (_container == null) return list;

        for (int i = 0; i < _container.SlotCount; i++)
        {
            var item = _container.GetItem(i);
            if (item != null && item.amount > 0)
                list.Add(new ItemSlotSaveData(i, item));
        }
        return list;
    }

    /// <summary>
    /// 从存档列表恢复仓库数据（先清空再按索引填充；供数据库加载）
    /// </summary>
    public void LoadFromSaveList(List<ItemSlotSaveData> saveData)
    {
        if (_container == null || saveData == null) return;

        Clear();

        foreach (var entry in saveData)
        {
            if (entry == null || entry.amount <= 0) continue;
            if (entry.slotIndex < 0 || entry.slotIndex >= _container.SlotCount) continue;

            _container.SetItem(entry.slotIndex, new ItemInstance(entry.itemID, entry.amount));
        }
    }

    /// <summary>
    /// 将底层容器的事件转发到数据层事件
    /// </summary>
    private void BindContainerEvents()
    {
        _container.OnSlotChanged += index => OnSlotChanged?.Invoke(index);
    }
}
