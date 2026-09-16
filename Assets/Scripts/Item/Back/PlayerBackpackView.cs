/****************************************************
    文件：PlayerBackpackView.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-05 20:02:30
	功能：背包表现类
*****************************************************/

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBackpackView : MonoBehaviour
{
    [SerializeField] private BackpackData _data;
    [SerializeField] private Transform _gridParent;       // 空的父节点，挂载 GridLayoutGroup
    [SerializeField] private SlotUI slotPrefab;           // 格子预制体

    private List<SlotUI> _slotUIs = new List<SlotUI>();

    /// <summary>
    /// 刷新单个格子（响应 OnSlotChanged）
    /// </summary>
    private void RefreshSlot(int index)
    {
        if (_slotUIs == null || index < 0 || index >= _slotUIs.Count) return;

        var item = _data.GetSlotContent(index);
        ApplyItemToSlot(_slotUIs[index].Icon, _slotUIs[index].ItemText, _slotUIs[index].Count, item);
    }

    /// <summary>
    /// 根据数据层容量动态重建整个网格
    /// </summary>
    private void RebuildGrid(int capacity)
    {
        // ① 清除旧格子
        for (int i = _slotUIs.Count - 1; i >= 0; i--)
        {
            if (_slotUIs[i] != null) Destroy(_slotUIs[i].gameObject);
        }
        _slotUIs.Clear();

        // 按新容量动态生成
        for (int i = 0; i < capacity; i++)
        {
            var slot = Instantiate(slotPrefab, _gridParent);
            slot.name = $"Slot_{i:D2}";
            _slotUIs.Add(slot);
        }

        // 全量刷新（新格子是空白的，必须用当前数据填充）
        int dataCount = Mathf.Min(capacity, _data.SlotCount);
        for (int i = 0; i < dataCount; i++)
        {
            RefreshSlot(i);
        }
    }

    #region 生命周期

    private void OnEnable()
    {
        if (_data == null) return;

        _data.OnSlotChanged += RefreshSlot;
        _data.OnCapacityChanged += RebuildGrid;

        // 首次打开：以当前容量为准重建
        RebuildGrid(_data.SlotCount);
    }

    private void OnDisable()
    {
        if (_data == null) return;

        _data.OnSlotChanged -= RefreshSlot;
        _data.OnCapacityChanged -= RebuildGrid;
    }
    #endregion

    /// <summary>
    /// 将物品数据渲染到指定的 UI 组件上
    /// </summary>
    /// <param name="icon">物品图标 Image 组件，为 null 时跳过图标刷新</param>
    /// <param name="ItemText">物品数量 Text 组件，为 null 时跳过数量刷新</param>
    /// <param name="item">物品数据实例，为 null 或数量为 0 时视为空格子</param>
    public static void ApplyItemToSlot(Image icon, Text ItemText, Text Count, ItemInstance item)
    {
        // 判断当前格子是否有有效物品
        bool hasItem = item != null && item.amount > 0;

        // 刷新图标显示
        if (icon != null)
        {
            // 有物品时加载对应图标资源，无物品时清空 Sprite
            icon.sprite = hasItem ? ResourceManager.LoadUISpriteByIconKey(item.Data.iconKey) : null;
            // 有物品时图标不透明，无物品时完全透明
            icon.color = hasItem ? Color.white : new Color(1, 1, 1, 0);
        }
        // 刷新数量文本显示
        if (Count != null)
        {
            Count.text = hasItem && item.amount > 1 ? item.amount.ToString() : "";
        }
    }
}