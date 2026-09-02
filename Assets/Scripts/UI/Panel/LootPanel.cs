/****************************************************
    文件：LootPanel.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-03 15:41:43
	功能：战利品面板
*****************************************************/

using UnityEngine;

/// <summary>
/// 战利品面板 - 重写基类方法，处理与背包的联动
/// </summary>
public class LootPanel : BaseUIPanel
{
    [Header("关联的背包面板")]
    [SerializeField] private BackpackPanel backpackPanel;

    public override UIPriority Priority => UIPriority.Loot;

    private ILootContainer _currentContainer;

    public override void OnOpen()
    {
        base.OnOpen();

        //  打开背包
        if (backpackPanel != null && !backpackPanel.gameObject.activeSelf)
        {
            UIController.Instance.OpenPanel(backpackPanel);
        }
    }

    public void Open(ILootContainer container)
    {
        _currentContainer = container;
    }

    public override void OnClose()
    {
        _currentContainer = null;
        base.OnClose();
        //关闭背包
        if (backpackPanel != null && backpackPanel.gameObject.activeSelf)
        {
            UIController.Instance.ClosePanel(backpackPanel);
        }
    }
}