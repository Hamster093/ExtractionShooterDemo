/****************************************************
    文件：BackpackData.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-05 19:45:14
	功能：背包数据类
*****************************************************/

using System;
using UnityEngine;
[Serializable]
public class BackpackData : IItemContainer
{
    private ItemContainer _container;

    public int SlotCount => _container?.SlotCount ?? 0;

    public event Action<int> OnSlotChanged;//槽位变化事件，参数为槽位索引
    public event Action<int> OnCapacityChanged;//容量变化事件，参数为新容量

    public BackpackData(int initialCapacity)
    {
        _container = new ItemContainer(initialCapacity);
        BindContainerEvents();
    }

    /// <summary>
    /// 添加物品 只做一次溢出判定 返回剩余未加入格子的数量
    /// </summary>
    /// <param name="itemId">物品id</param>
    /// <param name="amount">添加数量</param>
    /// <returns></returns>
    public int AddItem(int itemId, int amount)
    {
        var data = ItemRegistry.Get(itemId);
        if (data == null) return -1;

        int remaining = amount;

        // 第一轮：尝试往已有同类物品的格子堆叠
        for (int i = 0; i < _container.SlotCount && remaining > 0; i++)
        {
            var slot = _container.GetItem(i);
            if (slot != null && slot.itemID == itemId && slot.amount < data.maxStack)
            {
                //取当前要加入的物品量 和最大容量中的小值
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

        return remaining; // 返回剩余数量
    }
    /// <summary>
    /// 消耗物品
    /// </summary>
    public bool ConsumeItem(int itemId, int amount)
    {
        int remaining = amount;
        // 从后往前消耗（优先消耗零散的）
        for (int i = _container.SlotCount - 1; i >= 0 && remaining > 0; i--)
        {
            var slot = _container.GetItem(i);
            if (slot != null && slot.itemID == itemId)
            {
                int canRemove = Mathf.Min(remaining, slot.amount);
                slot.amount -= canRemove;
                remaining -= canRemove;

                //将索引的格子置空
                if (slot.amount <= 0)
                    _container.SetItem(i, null);

                OnSlotChanged?.Invoke(i);
            }
        }
        return remaining == 0; // 返回是否消耗完全
    }

    /// <summary>
    /// 查询物品数量
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
    /// 动态调整背包容量
    /// 扩容时新增空格子；缩容时从末尾截断（超出部分的物品会被丢弃并警告）
    /// </summary>
    /// <param name="newCapacity">新容量，必须 >= 1</param>
    public void SetCapacity(int newCapacity)
    {
        if (newCapacity < 1)
        {
            Debug.LogError($"[PlayerBackpack] 容量不能小于1，传入值: {newCapacity}");
            return;
        }

        int oldCapacity = _container.SlotCount;
        if (newCapacity == oldCapacity) return;

        // 缩容安全检查：警告被截断的物品
        if (newCapacity < oldCapacity)
        {
            for (int i = newCapacity; i < oldCapacity; i++)
            {
                var item = _container.GetItem(i);
                if (item != null)
                {
                    Debug.LogWarning($"[PlayerBackpack] 缩容导致索引 {i} 的物品被丢弃: " +
                                     $"ID={item.itemID}, Amount={item.amount}");
                    //todo 丢弃物品方法 将物品丢到地上
                }
            }
        }
        // 重建容器并迁移数据
        var newContainer = new ItemContainer(newCapacity);
        int migrateCount = Mathf.Min(oldCapacity, newCapacity);
        for (int i = 0; i < migrateCount; i++)
        {
            newContainer.SetItem(i, _container.GetItem(i));
        }

        _container = newContainer;
        BindContainerEvents();

        // 通知UI刷新
        OnCapacityChanged?.Invoke(newCapacity);

        // 缩容时，被截断的格子也需要通知UI清除显示
        if (newCapacity < oldCapacity)
        {
            for (int i = newCapacity; i < oldCapacity; i++)
                OnSlotChanged?.Invoke(i);
        }
    }
    /// <summary>
    /// 将底层容器的事件转发到背包数据层的事件
    /// </summary>
    private void BindContainerEvents()
    {
         _container.OnSlotChanged += index => OnSlotChanged?.Invoke(index);
    }
    /// <summary>
    /// 获取指定索引位置的物品实例
    /// </summary>
    /// <param name="index">格子索引</param>
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
            OnSlotChanged?.Invoke(i); // 逐个通知 UI 刷新
        }
    }

    public ItemInstance GetItem(int index) => _container.GetItem(index);

    public void SetItem(int index, ItemInstance item) => _container.SetItem(index, item);
}