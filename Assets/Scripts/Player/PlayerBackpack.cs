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

/// <summary>
/// 玩家库存服务：管理所有可堆叠物品的数量
/// </summary>
public class PlayerBackpack : MonoBehaviour
{
    public static PlayerBackpack Instance { get; private set; }

    // 物品ID -> 数量 的字典  应提取数据类作为键 todo
    private Dictionary<string, int> _items = new Dictionary<string, int>();

    // 库存变化事件：物品ID, 新数量 应提取数据类作为键 todo
    public event Action<string, int> OnItemChanged;

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
    public bool AddItem(string itemId, int amount, int maxStack = 999)
    {
        _items.TryGetValue(itemId, out int current);
        int newAmount = Mathf.Min(current + amount, maxStack);

        if (newAmount == current) return false; // 已满，未实际增加
        //更新数量，发送物品变化事件
        _items[itemId] = newAmount;
        OnItemChanged?.Invoke(itemId, newAmount);
        return true;
    }

    /// <summary>
    /// 消耗物品
    /// </summary>
    public bool ConsumeItem(string itemId, int amount)
    {
        _items.TryGetValue(itemId, out int current);
        if (current < amount) return false;

        _items[itemId] = current - amount;
        OnItemChanged?.Invoke(itemId, _items[itemId]);
        return true;
    }

    /// <summary>
    /// 查询物品数量
    /// </summary>
    public int GetItemCount(string itemId)
    {
        _items.TryGetValue(itemId, out int count);
        return count;
    }

    /// <summary>
    /// 初始化某种物品的初始携带量
    /// </summary>
    public void SetItem(string itemId, int amount)
    {
        _items[itemId] = amount;
        OnItemChanged?.Invoke(itemId, amount);
    }
}