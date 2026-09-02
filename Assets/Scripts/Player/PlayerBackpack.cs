/****************************************************
    文件：PlayerBackpack.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-28 18:28:15
	功能：玩家背包类
*****************************************************/

using System;
using UnityEngine;

/// <summary>
/// 玩家库存服务：管理所有可堆叠物品的数量
/// </summary>
public class PlayerBackpack : MonoBehaviour, IItemContainer
{
    public static PlayerBackpack Instance { get; private set; }

    [Tooltip("背包初始容量（格数）")]
    [SerializeField] private int _initialCapacity = 20; 

    [Header("引用")]
    private ItemContainer _container = new(20);

     /// <summary>
    /// 当前背包实际容量（只读，通过 SetCapacity 修改）
    /// </summary>
    public int Capacity => _container?.SlotCount ?? 0;

    public ItemInstance GetItem(int index) => _container.GetItem(index);
    public void SetItem(int index, ItemInstance item) => _container.SetItem(index, item);
    public int SlotCount => _container.SlotCount;

    /// <summary>
    /// 某个槽位的物品发生了变化，参数为槽位索引
    /// </summary>
    public event Action<int> OnSlotChanged;

    /// <summary>
    /// 背包容量发生变化时触发，参数为新容量
    /// UI层可监听此事件来动态增减格子
    /// </summary>
    public event Action<int> OnCapacityChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //加载场景不销毁
        DontDestroyOnLoad(gameObject);

        _container = new ItemContainer(_initialCapacity);
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
   /// 添加物品 只做一次溢出判定 返回剩余为加入格子的数量
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
}