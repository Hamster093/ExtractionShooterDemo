/****************************************************
    文件：LootPanel.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-03 15:41:43
	功能：战利品面板
*****************************************************/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战利品面板 - 重写基类方法，处理与背包的联动
/// </summary>
public class LootPanel : BaseUIPanel
{
    [SerializeField] private ChestManager _chestManager;
    [SerializeField] private BackpackPanel backpackPanel;

    [Header("搜索参数")]
    [Tooltip("每格揭晓的间隔（秒）")]
    [SerializeField] private float _slotInterval = 0.3f;
    [SerializeField] private bool _openBackpackAfterSearch = true;

    private ItemContainer _container;
    private SlotUI[] _slotViews;

    private readonly List<int> _pendingReveal = new();    // 待揭晓的格子索引队列
    private int _revealCursor;                            // 队列游标

    private float _timer;
    private bool _isSearching; //正在搜索
    private bool _backpackOpened;//be

    public override UIPriority Priority => UIPriority.Loot;

    [Header("销毁相关")]
    /// <summary>当前正在打开的战利品箱子（同一时刻只有一个）</summary>
    public static LootPickup CurrentPickup { get; private set; }
    public static void SetCurrentPickup(LootPickup p) => CurrentPickup = p;

    /// <summary>
    /// 设置容器
    /// </summary>
    /// <param name="c"></param>
    public void SetPendingContainer(ItemContainer c) => _container = c;


    public override void OnOpen()
    {
        base.OnOpen();

        _backpackOpened = false;
        _pendingReveal.Clear();
        _revealCursor = 0;
        _timer = 0f;
        _isSearching = false;

        if (_container == null)
        {
            Debug.LogWarning("[LootPanel] 打开时没有 pending container");
            return;
        }

        // 绑定容器，生成格子
        _chestManager?.BindContainer(_container);
        _slotViews = (_chestManager != null && _chestManager.slots != null)
             ? _chestManager.slots.ToArray()
             : null;

        if (_slotViews == null || _slotViews.Length == 0)
        {
            TryOpenBackpack();
            return;
        }

        int count = Mathf.Min(_slotViews.Length, _container.SlotCount);

        // 初始化每格 + 收集待搜索格子
        for (int i = 0; i < count; i++)
        {
            var item = _container.GetItem(i);
            bool hasItem = item != null && item.amount > 0;

            if (!hasItem)
            {
                // 空格子：直接清空，不参与搜索，不显示遮罩
                _slotViews[i].Clear();
                continue;
            }

            if (_container.IsSearched(i))
            {
                // 已搜过：直接显示
                _slotViews[i].SetItem(item);
            }
            else
            {
                // 有物品且未搜：显示问号遮罩，加入待揭晓队列
                _slotViews[i].SetUnknown();
                _pendingReveal.Add(i);
            }
        }

        // 视图多于容器时，多余的格子清空
        for (int i = count; i < _slotViews.Length; i++)
            _slotViews[i].Clear();

        // 没有需要搜的格子 → 直接开背包
        if (_pendingReveal.Count == 0)
        {
            TryOpenBackpack();
            return;
        }

        _isSearching = true;
    }

    private void Update()
    {
        if (!_isSearching || _container == null || _slotViews == null) return;

        _timer += Time.deltaTime;

        // 用 while 保证掉帧时节奏均匀
        while (_timer >= _slotInterval)
        {
            _timer -= _slotInterval;

            if (_revealCursor >= _pendingReveal.Count)
            {
                _isSearching = false;
                TryOpenBackpack();
                return;
            }

            int idx = _pendingReveal[_revealCursor];
            _revealCursor++;

            _container.MarkSearched(idx);
            _slotViews[idx].SetItem(_container.GetItem(idx));
            // 音效：AudioManager.Play("loot_reveal");
        }
    }

    private void TryOpenBackpack()
    {
        if (_backpackOpened) return;
        if (!_openBackpackAfterSearch) return;
        if (backpackPanel != null && !backpackPanel.gameObject.activeSelf)
            UIController.Instance.OpenPanel(backpackPanel);
        _backpackOpened = true;
    }

    public override void OnClose()
    {
        base.OnClose();
        _isSearching = false;
        _backpackOpened = false;
        _pendingReveal.Clear();

        if (backpackPanel != null && backpackPanel.gameObject.activeSelf)
            UIController.Instance.ClosePanel(backpackPanel);

        // 通知箱子，触发可能的延迟销毁
        CurrentPickup?.OnPanelClosed();
        CurrentPickup = null;
    }

    //==========宝箱销毁==========//
    public static void ClearCurrentPickup(LootPickup p)
    {
        if (CurrentPickup == p) CurrentPickup = null;
    }
}