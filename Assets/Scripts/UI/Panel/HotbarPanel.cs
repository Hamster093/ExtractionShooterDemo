/****************************************************
    文件：HotbarPanel.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-27 14:41:00
	功能：HUD 快捷栏面板（只读镜像：显示背包面板里绑定好的 6 格消耗品）
*****************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD 快捷栏（游戏内常驻显示）：
/// 只做"镜像显示"——数据源是常驻的 InventoryService.Hotbar，与背包面板里的 6 格是同一份绑定。
/// 这里不参与拖拽、也不写入数据；绑定关系由背包面板的 HotbarUI + HotbarBindSlotHandler 维护。
/// 只改图标 Image，不动格子上的 Label 文本（Label 用作键号显示，且是内置 Arial 不支持中文）。
/// </summary>
public class HotbarPanel : BaseUIPanel
{
    [Tooltip("HUD 快捷栏 6 格的图标 Image（Slot_3_Item~Slot_8_Item 的 Icon 子物体），顺序 = 快捷栏第 1~6 格")]
    [SerializeField] private Image[] _hudIcons;

    public override UIPriority Priority => UIPriority.Hotbar;

    private bool _isBound;

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
    /// 尝试绑定快捷栏数据（InventoryService 就绪后才行）
    /// </summary>
    private void TryBind()
    {
        if (_isBound) return;

        var hotbar = GameService.Hotbar;
        if (hotbar == null) return;

        hotbar.OnSlotChanged += RefreshSlot;

        var backpack = GameService.Backpack;
        if (backpack != null)
            backpack.OnSlotChanged += OnBackpackChanged;

        _isBound = true;

        // 进入场景/面板激活时先清掉失效绑定（读档重建、物品被移出背包等）
        HotbarService.Validate();
        RefreshAll();
    }

    /// <summary>
    /// 解除绑定
    /// </summary>
    private void Unbind()
    {
        if (!_isBound) return;

        var hotbar = GameService.Hotbar;
        if (hotbar != null)
            hotbar.OnSlotChanged -= RefreshSlot;

        var backpack = GameService.Backpack;
        if (backpack != null)
            backpack.OnSlotChanged -= OnBackpackChanged;

        _isBound = false;
    }

    /// <summary>
    /// 等待快捷栏服务就绪（最多 3 秒，避免 InventoryService 尚未 Awake）
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
            Debug.LogError("[HotbarPanel] 等待快捷栏数据超时！请确认场景中存在 InventoryService。");
    }

    /// <summary>
    /// 背包数据变化：绑定的实例可能被拖到仓库/被消耗完/读档重建
    /// → 先做失效校验（会触发槽位刷新），再整体刷一次数量显示
    /// </summary>
    private void OnBackpackChanged(int slotIndex)
    {
        HotbarService.Validate();
        RefreshAll();
    }

    /// <summary>
    /// 全量刷新 HUD 快捷栏图标
    /// </summary>
    private void RefreshAll()
    {
        int count = _hudIcons != null ? _hudIcons.Length : 0;
        for (int i = 0; i < count; i++)
            RefreshSlot(i);
    }

    /// <summary>
    /// 单格刷新：只改图标（空格子时图标透明）
    /// </summary>
    private void RefreshSlot(int index)
    {
        if (_hudIcons == null || index < 0 || index >= _hudIcons.Length) return;

        var icon = _hudIcons[index];
        if (icon == null) return;

        var item = HotbarService.GetItem(index);
        var data = (item != null && item.amount > 0) ? item.Data : null;

        icon.sprite = data != null ? ResourceManager.LoadUISpriteByIconKey(data.iconKey) : null;
        icon.color = data != null ? Color.white : new Color(1, 1, 1, 0);
    }
}
