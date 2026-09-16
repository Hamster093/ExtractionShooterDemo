/****************************************************
    文件：VendingMachinePanel.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-16
	功能：售货机面板（独立开关，不联动背包；无需拖拽）
	说明：订阅售货机格子点击事件，格子内有商品时打开商品详情面板（ItemPanel）
*****************************************************/

using UnityEngine;

/// <summary>
/// 售货机面板：按 F 或点击交互后由 UIController.OpenVendingMachine 打开/关闭。
/// 数据由场景 3D 售货机上的 VendingMachineController 提供（经 VendingMachineUI 展示），
/// 与仓库面板不同：不联动背包、格子不支持拖拽。
/// 点击格子（VendingMachineUI.OnSlotClicked）：格子内有商品 → 打开 ItemPanel 显示详情与购买。
/// </summary>
public class VendingMachinePanel : BaseUIPanel
{
    [Tooltip("售货机格子UI（挂在该面板的 Content 上，数据来自 VendingMachineController）")]
    [SerializeField] private VendingMachineUI vendingMachineUI;

    [Tooltip("商品详情面板（点击格子内有商品时打开，由 VendingMachineUIBuilder 自动绑定）")]
    [SerializeField] private ItemPanel itemPanel;

    public override UIPriority Priority => UIPriority.VendingMachine;

    /// <summary>绑定的售货机格子UI（只读查询用；由 VendingMachineUIBuilder 绑定）</summary>
    public VendingMachineUI VendingMachineUI => vendingMachineUI;

    /// <summary>绑定的商品详情面板（只读查询用）</summary>
    public ItemPanel ItemPanel => itemPanel;

    private void Awake()
    {
        // 面板首次激活时订阅格子点击（VendingMachineUI.OnSlotClicked 由 VendingMachineUIBuilder 接入）
        if (vendingMachineUI != null)
            vendingMachineUI.OnSlotClicked += HandleSlotClicked;
    }

    private void OnDestroy()
    {
        if (vendingMachineUI != null)
            vendingMachineUI.OnSlotClicked -= HandleSlotClicked;
    }

    /// <summary>
    /// 点击格子：格子内有商品 → 打开商品详情面板；空格子忽略
    /// </summary>
    private void HandleSlotClicked(int index)
    {
        if (itemPanel == null)
        {
            Debug.LogWarning("[VendingMachinePanel] 未绑定 ItemPanel，无法打开商品详情（请运行 Tools/售货机/一键搭建售货机UI）");
            return;
        }

        var item = vendingMachineUI != null ? vendingMachineUI.GetSlotContent(index) : null;
        if (item == null) return; // 空格子不弹详情

        itemPanel.OpenWithItem(item);
    }

    /// <summary>
    /// 绑定格子UI（编辑器工具/Inspector 使用）
    /// </summary>
    public void BindVendingMachineUI(VendingMachineUI ui) => vendingMachineUI = ui;

    /// <summary>
    /// 绑定商品详情面板（编辑器工具/Inspector 使用）
    /// </summary>
    public void BindItemPanel(ItemPanel panel) => itemPanel = panel;
}