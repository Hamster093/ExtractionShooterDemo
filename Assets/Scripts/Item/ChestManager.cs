/****************************************************
    文件：ChestManager.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-01 17:06:11
	功能：宝箱管理类
*****************************************************/

using System.Collections.Generic;
using UnityEngine;

public class ChestManager : MonoBehaviour,ISlotOwner
{
    [Tooltip("按顺序绑定格子UI，或通过子物体自动获取")]
    public List<SlotUI> slots;

    [Tooltip("勾选后此容器作为装备栏使用：初始化时为未挂载处理器的格子自动添加 EquipmentSlotHandler，格子类别在其 SlotUI.allowedType 上配置")]
    public bool isEquipmentGrid = false;
    [Tooltip("勾选后此容器作为背包使用，初始为空，后续应根据存档配置读取物品")]
    public bool IsBack = false;

    private ItemContainer _container;
    public ItemContainer Container => _container;
    IItemContainer ISlotOwner.Container => Container;

    //是否初始化
    private bool _isInitialized = false;
    //格子图片是否已收集
    private bool _slotsReady = false;

    void Awake()
    {
        EnsureInitialized();
    }

    void Start()
    {
        //test。。。。。。。。。。
        // 新代码：创建 ItemInstance 实例，传入 (物品ID, 数量)
        // 装备栏不填充测试物品，只有宝箱用测试数据
        if (!isEquipmentGrid&&!IsBack)
        {
            _container.SetItem(0, new ItemInstance(1, 90));  // ID=1 的物品，5个
            _container.SetItem(1, new ItemInstance(2, 1));  // ID=2 的物品，1个
            _container.SetItem(2, new ItemInstance(4, 1));  // ID=2 的物品，1个
            _container.SetItem(3, new ItemInstance(3, 20)); // ID=3 的物品，20个
        }
        //test.。。。。。。。。。
        RefreshUI();
    }

    /// <summary>
    /// 外部调用此方法来初始化宝箱内容
    /// 应在 Awake 之后、UI 刷新之前调用
    /// </summary>
    public void Init(List<ChestSlotConfig> items)
    {
        if (_isInitialized)
        {
            Debug.LogWarning($"[ChestManager] {gameObject.name} 已初始化，请勿重复调用 Init！", this);
            return;
        }

        // 确保容器已创建（Awake 中已创建，这里做兜底）
        if (_container == null)
            _container = new ItemContainer(slots.Count);

        // 清空旧数据（防止对象池复用时残留）
        _container.Clear();

        // 按配置填充物品
        if (items != null)
        {
            for (int i = 0; i < items.Count && i < _container.SlotCount; i++)
            {
                var config = items[i];
                if (config.itemId > 0 && config.amount > 0)
                {
                    _container.SetItem(i, new ItemInstance(config.itemId, config.amount));
                }
            }
        }

        _container.OnSlotChanged -= RefreshSlot; 
        _container.OnSlotChanged += RefreshSlot;

        _isInitialized = true;
        RefreshUI();
    }


    public void RefreshSlot(int index)
    {
        if (_container == null || slots == null) return;
        if (index < 0 || index >= slots.Count || index >= _container.SlotCount) return;

        var slotUI = slots[index];
        if (slotUI == null) return;

        var item = _container.GetItem(index);
        slotUI.SetItem(item);
    }
            

    /// <summary>
    /// 全量刷新
    /// 注意：此方法不属于ISlotOwner接口，DragManager不应调用
    /// </summary>
    public void RefreshUI()
    {
        if (_container == null || slots == null) return;

        int count = Mathf.Min(slots.Count, _container.SlotCount);
        for (int i = 0; i < count; i++)
        {
            RefreshSlot(i);
        }
    }
    /// <summary>
    /// 收集格子图片并创建容器（幂等）。
    /// 面板初始为 inactive 时 Unity 不会调用 Awake，DragManager 注册拖拽前需手动调用此方法。
    /// </summary>
    public void EnsureInitialized()
    {
        if (_slotsReady) return;
        _slotsReady = true;

        // 若未在 Inspector 中绑定格子图片，则自动从子物体获取（Slot_01~Slot_10）
        if (slots == null || slots.Count == 0)
        {
            slots = new List<SlotUI>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var slotUI = transform.GetChild(i).GetComponent<SlotUI>();
                if (slotUI != null)
                    slots.Add(slotUI);
            }
        }
        if (_container == null)
            _container = new ItemContainer(slots.Count);

        // 装备栏模式：为未挂载自定义处理器的格子自动添加装备槽处理器
        if (isEquipmentGrid)
        {
            int attached = 0;
            foreach (var slot in slots)
            {
                if (slot == null) continue;
                if (slot.GetComponent<ISlotDragHandler>() == null)
                {
                    slot.gameObject.AddComponent<EquipmentSlotHandler>();
                    attached++;
                }
            }
            Debug.Log($"[ChestManager] {gameObject.name} 装备栏初始化：共 {slots.Count} 格，本次自动挂载 EquipmentSlotHandler {attached} 个", this);
        }
    }

    private void OnDestroy()
    {
        if (_container != null)
            _container.OnSlotChanged -= RefreshSlot;
    }
}