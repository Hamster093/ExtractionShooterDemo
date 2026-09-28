/****************************************************
    文件：HotbarUI.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-27 14:39:00
	功能：快捷栏面板视图（背包面板内的 6 格，支持拖入建立绑定）
    说明：挂在 Canvas/PlayerBackpackPanel/Hotbar 上；数据源是常驻的 GameService.Hotbar，
          与 BackpackUI 同款"惰性收集子物体 SlotUI + 等待服务就绪"的写法。
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarUI : MonoBehaviour, ISlotOwner
{
    [Tooltip("手动绑定格子（可留空：留空时自动收集子物体的 SlotUI）")]
    [SerializeField] private List<SlotUI> _slotImages = new List<SlotUI>();

    /// <summary>
    /// 快捷栏数据（6 格绑定引用）
    /// </summary>
    private HotbarData Data => GameService.Hotbar;

    public IItemContainer Container => GameService.Hotbar;

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

    private bool _isBound;
    private bool _slotsReady;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnEnable()
    {
        TryBind();
        if (!_isBound)
            StartCoroutine(WaitForService());
    }

    private void OnDisable()
    {
        Unbind();
        StopAllCoroutines();
    }

    /// <summary>
    /// 收集格子（幂等）：未手动绑定时自动从子物体获取（含 inactive）。
    /// 面板初始为 inactive 时 Unity 不会调用 Awake，DragManager 注册拖拽前会先取 Slots 触发此方法。
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
    /// 反查某个格子在本快捷栏中的索引（供 HotbarBindSlotHandler 使用）
    /// </summary>
    public int IndexOf(SlotUI slot)
    {
        EnsureInitialized();
        return _slotImages != null ? _slotImages.IndexOf(slot) : -1;
    }

    /// <summary>
    /// 尝试绑定快捷栏数据
    /// </summary>
    private void TryBind()
    {
        if (_isBound) return;

        var data = Data;
        if (data == null) return;

        data.OnSlotChanged += RefreshSlot;
        _isBound = true;

        // 打开面板时先清掉失效绑定（物品已被拖到仓库/被消耗/读档重建）
        HotbarService.Validate();
        RefreshUI();
    }

    /// <summary>
    /// 解除绑定
    /// </summary>
    private void Unbind()
    {
        if (!_isBound) return;

        var data = Data;
        if (data != null)
            data.OnSlotChanged -= RefreshSlot;

        _isBound = false;
    }

    /// <summary>
    /// 等待快捷栏服务就绪（最多 3 秒，与 BackpackUI 同款兜底）
    /// </summary>
    private IEnumerator WaitForService()
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
            Debug.LogError("[HotbarUI] 等待快捷栏数据超时！请确认场景中存在 InventoryService。");
    }

    /// <summary>
    /// 单格刷新（ISlotOwner 接口契约，供 DragManager 调用）
    /// </summary>
    public void RefreshSlot(int index)
    {
        EnsureInitialized();
        if (index < 0 || index >= _slotImages.Count) return;

        var data = Data;
        _slotImages[index].SetItem(data != null ? data.GetItem(index) : null);
    }

    /// <summary>
    /// 全量刷新（UI 生命周期使用）
    /// </summary>
    public void RefreshUI()
    {
        EnsureInitialized();
        for (int i = 0; i < _slotImages.Count; i++)
            RefreshSlot(i);
    }
}
