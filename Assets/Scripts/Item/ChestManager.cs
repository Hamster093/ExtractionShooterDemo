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

    [Tooltip("是否已经绑定了容器")]
    public bool IsBound => _container != null;

    //格子图片是否已收集
    private bool _slotsReady = false;

    void Awake()
    {
        EnsureInitialized();
    }

    /// <summary>
    /// 面板每次激活时全量刷新格子（如切场景恢复后、打开背包时），
    /// 兜底 RestoreEquipment 等"Awake 前写数据"的时序问题。
    /// </summary>
    private void OnEnable()
    {
        if (_container != null && slots != null && slots.Count > 0)
            RefreshUI();
    }

    /// <summary>
    /// 把一个外部容器绑定到本 UI，会替换旧容器并刷新全部格子。
    /// 同一个容器重复绑定只做刷新，不会重复订阅。
    /// </summary>
    public void BindContainer(ItemContainer container)
    {
        EnsureInitialized();

        if (_container == container)
        {
            RefreshUI();
            return;
        }

        // 解绑旧容器
        if (_container != null)
            _container.OnSlotChanged -= RefreshSlot;

        _container = container;

        // 订阅新容器
        if (_container != null)
            _container.OnSlotChanged += RefreshSlot;

        RefreshUI();
    }

    /// <summary>
    /// 解绑容器并清空 UI（关闭面板、对象池归还前调用）
    /// </summary>
    public void UnbindContainer()
    {
        if (_container != null)
            _container.OnSlotChanged -= RefreshSlot;

        _container = null;
        ClearUI();
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
        if (slots == null) return;

        int count = Mathf.Min(slots.Count, _container.SlotCount);
        for (int i = 0; i < count; i++)
        {
            RefreshSlot(i);
        }
        for (int i = count; i < slots.Count; i++)
            slots[i]?.SetItem(null);
    }
    /// <summary>
    /// 清空
    /// </summary>
    private void ClearUI()
    {
        if (slots == null) return;
        foreach (var s in slots)
            s?.SetItem(null);
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
        }
    }

    private void OnDestroy()
    {
        if (_container != null)
            _container.OnSlotChanged -= RefreshSlot;
    }
}