/****************************************************
    文件：WarehousePanel.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-14 00:40:00
	功能：仓库面板（与背包联动打开/关闭，便于两边拖拽物品）
*****************************************************/

using UnityEngine;

/// <summary>
/// 仓库面板 - 打开时联动打开背包，关闭时联动关闭背包（仿 LootPanel）。
/// 数据由 GameService.Warehouse（WarehouseData）提供，拖拽由 DragManager 处理。
/// </summary>
public class WarehousePanel : BaseUIPanel
{
    [Header("关联的背包面板（联动打开/关闭）")]
    [SerializeField] private BackpackPanel backpackPanel;

    public override UIPriority Priority => UIPriority.Warehouse;

    /// <summary>
    /// 绑定关联的背包面板（编辑器工具/Inspector 使用）
    /// </summary>
    public void BindBackpackPanel(BackpackPanel panel) => backpackPanel = panel;

    public override void OnOpen()
    {
        base.OnOpen();

        // 联动打开背包，方便把背包物品拖入仓库
        if (backpackPanel != null && !backpackPanel.gameObject.activeSelf)
        {
            UIController.Instance.OpenPanel(backpackPanel);
        }
    }

    public override void OnClose()
    {
        base.OnClose();

        // 联动关闭背包
        if (backpackPanel != null && backpackPanel.gameObject.activeSelf)
        {
            UIController.Instance.ClosePanel(backpackPanel);
        }
    }
}
