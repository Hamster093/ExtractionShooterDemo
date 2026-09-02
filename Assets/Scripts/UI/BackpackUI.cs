/****************************************************
    文件：BackpackUI.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-02 15:15:20
	功能：背包UI管理，实现ISlotOwner接口支持拖拽交互
*****************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackpackUI : MonoBehaviour,ISlotOwner
{
    [SerializeField] private List<Image> _slotImages;

    public IItemContainer Container => PlayerBackpack.Instance;

    public IReadOnlyList<Image> SlotImages => _slotImages;

    private void OnEnable()
    {
        PlayerBackpack.Instance.OnCapacityChanged += RebuildSlots;
        RebuildSlots(PlayerBackpack.Instance.Capacity);
    }
    /// <summary>
    /// 背包格子数量变化时，重新构建格子UI
    /// </summary>
    /// <param name="obj">新背包格子数量</param>
    private void RebuildSlots(int obj)
    {
        //todo 刷新UI格子数量，动态增减格子
    }

    private void OnDisable()
    {
        PlayerBackpack.Instance.OnCapacityChanged -= RebuildSlots;
    }

    /// <summary>
    /// 单格刷新（ISlotOwner接口契约，供DragManager等交互系统调用）
    /// </summary>
    public void RefreshSlot(int index)
    {
        if (index < 0 || index >= _slotImages.Count) return;

        var item = Container.GetItem(index);
        bool hasItem = item != null && item.amount > 0;

        _slotImages[index].sprite = hasItem ? ResourceManager.LoadUISprite(item.Data.iconKey) : null;
        _slotImages[index].color = hasItem ? Color.white : new Color(1, 1, 1, 0);
    }

    /// <summary>
    /// 全量刷新（UI生命周期使用，如OnEnable/切换标签页/加载存档）
    /// 注意：此方法不属于ISlotOwner接口，DragManager不应调用
    /// </summary>
    public void RefreshUI()
    {
        if (Container == null) return;
        for (int i = 0; i < _slotImages.Count; i++)
        {
            RefreshSlot(i);
        }
    }

}