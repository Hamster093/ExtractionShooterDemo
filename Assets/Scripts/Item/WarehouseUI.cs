/****************************************************
    文件：WarehouseUI.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-14 00:35:00
	功能：仓库UI管理，实现ISlotOwner接口支持拖拽交互（仿 BackpackUI）
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 仓库 UI：数据桥接 GameService.Warehouse（WarehouseData）。
/// 挂在仓库面板的 Content 上，支持拖拽交互（与背包/宝箱互拖）。
/// </summary>
public class WarehouseUI : MonoBehaviour, ISlotOwner
{
    [Tooltip("手动绑定格子（可留空：留空时自动收集子物体的 SlotUI）")]
    [SerializeField] private List<SlotUI> _slotImages = new List<SlotUI>();

    public IItemContainer Container => GameService.Warehouse;

    /// <summary>
    /// 格子列表（惰性初始化：未手动绑定时自动从子物体收集）
    /// </summary>
    public IReadOnlyList<SlotUI> Slots
    {
        get
        {
            EnsureInitialized();
            return _slotImages;
        }
    }

    private bool _isBound = false;
    private bool _slotsReady = false;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnEnable()
    {
        TryBind();

        if (!_isBound)
        {
            StartCoroutine(WaitForWarehouse());
        }
    }

    /// <summary>
    /// 收集格子（幂等）：未手动绑定时自动从子物体获取
    /// </summary>
    public void EnsureInitialized()
    {
        if (_slotsReady) return;
        _slotsReady = true;

        if (_slotImages == null || _slotImages.Count == 0)
        {
            _slotImages = new List<SlotUI>();
            var children = GetComponentsInChildren<SlotUI>(true);
            if (children != null)
                _slotImages.AddRange(children);
        }
    }

    /// <summary>
    /// 仓库格子数量变化时重建格子UI（当前由场景固定数量提供，暂不动态增减）
    /// </summary>
    private void RebuildSlots(int obj)
    {
        //todo 动态增减格子（需配合 DragManager 动态重绑事件）
    }

    private void OnDisable()
    {
        Unbind();
        StopAllCoroutines();
    }

    /// <summary>
    /// 尝试绑定仓库数据
    /// </summary>
    private void TryBind()
    {
        if (_isBound) return;

        var warehouse = GameService.Warehouse;
        if (warehouse == null) return;

        warehouse.OnCapacityChanged += RebuildSlots;
        warehouse.OnSlotChanged += RefreshSlot;
        RebuildSlots(warehouse.SlotCount);
        _isBound = true;

        RefreshUI();
    }

    /// <summary>
    /// 解除绑定
    /// </summary>
    private void Unbind()
    {
        if (!_isBound) return;

        var warehouse = GameService.Warehouse;
        if (warehouse != null)
        {
            warehouse.OnCapacityChanged -= RebuildSlots;
            warehouse.OnSlotChanged -= RefreshSlot;
        }

        _isBound = false;
    }

    /// <summary>
    /// 等待仓库服务就绪（最多等待3秒）
    /// </summary>
    private IEnumerator WaitForWarehouse()
    {
        float timeout = 3f;
        float elapsed = 0f;

        while (!_isBound && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
            TryBind();
        }

        if (!_isBound)
        {
            Debug.LogError("[WarehouseUI] 等待仓库服务超时！" +
                           "请确认 InventoryService 存在于场景中且已初始化 Warehouse。");
        }
    }

    /// <summary>
    /// 单格刷新（ISlotOwner接口契约，供DragManager等交互系统调用）
    /// </summary>
    public void RefreshSlot(int index)
    {
        if (index < 0 || index >= _slotImages.Count) return;

        var item = Container != null ? Container.GetItem(index) : null;
        _slotImages[index].SetItem(item);
    }

    /// <summary>
    /// 全量刷新（UI生命周期使用）
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
