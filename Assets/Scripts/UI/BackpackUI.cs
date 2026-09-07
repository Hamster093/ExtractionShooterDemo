/****************************************************
    文件：BackpackUI.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-02 15:15:20
	功能：背包UI管理，实现ISlotOwner接口支持拖拽交互
	2026-09-07 重构：支持从子物体自动收集格子，数据桥接 GameService.Backpack（BackpackData）
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackpackUI : MonoBehaviour, ISlotOwner
{
    [Tooltip("手动绑定格子（可留空：留空时自动收集子物体的 SlotUI）")]
    [SerializeField] private List<SlotUI> _slotImages = new List<SlotUI>();

    public IItemContainer Container => GameService.Backpack;

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
        // 尝试立即绑定
        TryBind();

        // 如果未成功，启动协程等待 InventoryService 就绪
        if (!_isBound)
        {
            StartCoroutine(WaitForBackpack());
        }
    }

    /// <summary>
    /// 收集格子（幂等）：未手动绑定时自动从子物体获取。
    /// 面板初始为 inactive 时 Unity 不会调用 Awake，DragManager 注册拖拽前可经 Slots 属性触发。
    /// </summary>
    public void EnsureInitialized()
    {
        if (_slotsReady) return;
        _slotsReady = true;

        if (_slotImages == null || _slotImages.Count == 0)
        {
            _slotImages = new List<SlotUI>();
            // 包含 inactive 子物体，避免面板未激活时收集不到
            var children = GetComponentsInChildren<SlotUI>(true);
            if (children != null)
                _slotImages.AddRange(children);
        }

        Debug.Log($"[BackpackUI] {gameObject.name} 收集到格子: {_slotImages.Count}");
    }

    /// <summary>
    /// 背包格子数量变化时，重新构建格子UI
    /// 注意：当前格子由场景固定数量提供（需与 InventoryService 初始容量一致），
    /// 暂不实现动态增减（动态销毁格子会断开 DragManager 已绑定的一次性事件）
    /// </summary>
    private void RebuildSlots(int obj)
    {
        //todo 刷新UI格子数量，动态增减格子（需配合 DragManager 动态重绑事件）
    }

    private void OnDisable()
    {
        Unbind();
        StopAllCoroutines();
    }

    /// <summary>
    /// 尝试绑定背包数据
    /// </summary>
    private void TryBind()
    {
        if (_isBound) return;

        var backpack = GameService.Backpack;
        if (backpack == null) return;

        backpack.OnCapacityChanged += RebuildSlots;
        // 数据层槽位变化（拖拽/拾取/换弹消耗）实时刷新对应格子
        backpack.OnSlotChanged += RefreshSlot;
        RebuildSlots(backpack.SlotCount);
        _isBound = true;

        // 绑定成功后全量刷新一次，保证面板打开时显示最新数据
        RefreshUI();

        Debug.Log("[BackpackUI] 背包数据绑定成功");
    }

    /// <summary>
    /// 解除绑定
    /// </summary>
    private void Unbind()
    {
        if (!_isBound) return;

        var backpack = GameService.Backpack;
        if (backpack != null)
        {
            backpack.OnCapacityChanged -= RebuildSlots;
            backpack.OnSlotChanged -= RefreshSlot;
        }

        _isBound = false;
    }

    /// <summary>
    /// 等待背包服务就绪（最多等待3秒，避免无限循环）
    /// </summary>
    private IEnumerator WaitForBackpack()
    {
        float timeout = 3f;
        float elapsed = 0f;

        while (!_isBound && elapsed < timeout)
        {
            yield return null; // 等待下一帧
            elapsed += Time.deltaTime;
            TryBind();
        }

        if (!_isBound)
        {
            Debug.LogError("[BackpackUI] 等待背包服务超时！" +
                           "请确认 InventoryService 存在于场景中且未被销毁。");
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
