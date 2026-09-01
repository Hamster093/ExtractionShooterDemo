/****************************************************
    文件：PlayerBackpack.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-28 18:28:15
	功能：玩家背包类
*****************************************************/

// InventoryService.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

/// <summary>
/// 玩家库存服务：管理所有可堆叠物品的数量
/// </summary>
public class PlayerBackpack : MonoBehaviour
{
    public static PlayerBackpack Instance { get; private set; }

    // 物品ID -> 数量 的字典  
    private Dictionary<int, int> _items = new Dictionary<int, int>();

    // 库存变化事件：物品ID, 新数量 
    public event Action<int, int> OnItemChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //加载场景不销毁
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 添加物品
    /// </summary>
    public bool AddItem(int itemId, int amount)
    {
        var itemData = ItemRegistry.Get(itemId);
        if (itemData == null)
        {
            Debug.LogError($"[Backpack] 未知物品ID: {itemId}");
            return false;
        }

        _items.TryGetValue(itemId, out int current);
        int newAmount = Mathf.Min(current + amount, itemData.maxStack); // ← 从配置表读

        if (newAmount == current) return false;
        _items[itemId] = newAmount;
        OnItemChanged?.Invoke(itemId, newAmount);
        return true;
    }

    /// <summary>
    /// 消耗物品
    /// </summary>
    public bool ConsumeItem(int itemid, int amount)
    {
        _items.TryGetValue(itemid, out int current);
        if (current < amount) return false;

        int newCount = current - amount;

        if (newCount <= 0)
            _items.Remove(itemid);
        else
            _items[itemid] = newCount;

        OnItemChanged?.Invoke(itemid, _items[itemid]);
        return true;
    }

    /// <summary>
    /// 查询物品数量
    /// </summary>
    public int GetItemCount(int itemid)
    {
        _items.TryGetValue(itemid, out int count);
        return count;
    }

    /// <summary>
    /// 初始化某种物品的初始携带量
    /// </summary>
    public void SetItem(int itemid, int amount)
    {
        _items[itemid] = amount;
        OnItemChanged?.Invoke(itemid, amount);
    }
}