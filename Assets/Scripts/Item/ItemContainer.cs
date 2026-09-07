/****************************************************
    文件：ItemContainer.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-02 13:59:47
	功能：容器逻辑类
*****************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainer : IItemContainer
{
    private readonly List<ItemInstance> _slots;

    public event Action<int> OnSlotChanged;

    public int SlotCount => _slots.Count;

    //构造函数 初始化容器容量
    public ItemContainer(int capacity)
    {
        _slots = new List<ItemInstance>(new ItemInstance[capacity]);
    }

    /// <summary>
    /// 提供外部获取方法，获取指定索引的物品实例
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemInstance GetItem(int index) =>
       (index >= 0 && index < _slots.Count) ? _slots[index] : null;

    /// <summary>
    /// 添加物品
    /// </summary>
    /// <param name="index">格子索引</param>
    /// <param name="item">物品实例</param>
    public void SetItem(int index, ItemInstance item)
    {
        if (index >= 0 && index < _slots.Count)
        {
            _slots[index] = item;
            OnSlotChanged?.Invoke(index);
        }
    }

    /// <summary>
    /// // 交换 _slots[a] 与 _slots[b] 的值
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public void Swap(int a, int b)
    {
        (_slots[a], _slots[b]) = (_slots[b], _slots[a]);
        OnSlotChanged?.Invoke(a);
        OnSlotChanged?.Invoke(b);
    }

    /// <summary>
    /// 跨容器移动物品（返回是否成功）
    /// </summary>
    /// <param name="src">源容器</param>
    /// <param name="srcIdx">源容器中的物品索引</param>
    /// <param name="dst">目标容器</param>
    /// <param name="dstIdx">目标容器中的物品索引</param>
    /// <returns>移动成功返回 true；源位置无物品时返回 false</returns>
    public static bool MoveBetween(IItemContainer src, int srcIdx, IItemContainer dst, int dstIdx)
    {
        // 获取源位置的物品，若为空则说明该槽位无物品，直接返回失败
        var srcItem = src.GetItem(srcIdx);
        if (srcItem == null) return false;

        // 获取目标位置的物品（可能为 null，表示目标槽位为空）
        var dstItem = dst.GetItem(dstIdx);

        //  新增：如果目标格有同类物品且未满叠，优先尝试合并
        if (dstItem != null && dstItem.itemID == srcItem.itemID)
        {
            var data = ItemRegistry.Get(srcItem.itemID);
            if (data != null && dstItem.amount < data.maxStack)
            {
                int canAdd = Mathf.Min(srcItem.amount, data.maxStack - dstItem.amount);
                dstItem.amount += canAdd;
                srcItem.amount -= canAdd;

                dst.SetItem(dstIdx, dstItem); // 触发UI刷新
                src.SetItem(srcIdx, srcItem.amount > 0 ? srcItem : null);
                return true;
            }
        }

        // 无法合并时，执行原始交换逻辑
        src.SetItem(srcIdx, dstItem);
        dst.SetItem(dstIdx, srcItem);
        return true;
    }

    /// <summary>
    /// 清空容器内所有物品（保留容量）
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i] = null;
            OnSlotChanged?.Invoke(i);
        }
    }
}