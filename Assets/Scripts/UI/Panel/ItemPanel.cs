/****************************************************
    文件：ItemPanel.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-16
	功能：商品详情面板（点击售货机格子弹出，显示商品名称与图标，提供购买接口）
*****************************************************/

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 商品详情面板：由 VendingMachinePanel 在点击售货机格子时调用 OpenWithItem 打开。
/// - 显示商品名称 + 图标（ResourceManager.LoadUISpriteByIconKey 按 iconKey 加载）
/// - 点击【购买】→ 走 OnTryPurchase 购买事件接口（暂默认成功）→ 物品放入背包 → 关闭本面板
/// - 点击面板以外区域（全屏遮罩）或按 ESC → 关闭本面板（不影响下层售货机面板）
/// - 售货机库存暂不减少
/// </summary>
public class ItemPanel : BaseUIPanel
{
    [Header("引用")]
    [Tooltip("商品名称文本")]
    [SerializeField] private Text itemNameText;
    [Tooltip("商品图标")]
    [SerializeField] private Image itemIconImage;
    [Tooltip("购买按钮")]
    [SerializeField] private Button buyButton;
    [Tooltip("全屏遮罩（点击面板以外区域关闭）")]
    [SerializeField] private Image maskImage;

    private ItemInstance _currentItem;

    /// <summary>
    /// 尝试购买事件接口：参数(物品ID, 数量)，返回是否购买成功。
    /// 暂未实现真实购买逻辑，null 时默认返回成功（由 ItemPanel 内部兜底）。
    /// 后续接入：itemPanel.OnTryPurchase = (itemId, amount) => 余额检查/扣款逻辑;
    /// </summary>
    public Func<int, int, bool> OnTryPurchase;

    public override UIPriority Priority => UIPriority.Dialog;

    private void Awake()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(HandleBuy);

        BindMaskClick();
    }

    private void OnDestroy()
    {
        if (buyButton != null)
            buyButton.onClick.RemoveListener(HandleBuy);
    }

    /// <summary>
    /// 打开本面板并显示指定商品（由售货机面板点击格子时调用）
    /// </summary>
    public void OpenWithItem(ItemInstance item)
    {
        _currentItem = item;
        RefreshUI();

        UIController.Instance.OpenPanel(this);
    }

    public override void OnClose()
    {
        base.OnClose();
        _currentItem = null;
    }

    /// <summary>
    /// 点击购买：先走购买事件接口（默认成功），成功后物品放入背包并关闭面板。
    /// 售货机内物品数量暂不减少。
    /// </summary>
    private void HandleBuy()
    {
        if (_currentItem == null) return;

        bool success = OnTryPurchase != null
            ? OnTryPurchase(_currentItem.itemID, _currentItem.amount)
            : true; // 默认购买成功（购买逻辑接口留待后续实现）

        if (!success) return; // 购买失败（暂不处理，默认成功不会走到这里）

        // 物品放入背包
        var backpack = GameService.Backpack;
        int remain = backpack != null
            ? backpack.AddItem(_currentItem.itemID, _currentItem.amount)
            : _currentItem.amount;
        if (remain > 0)
        {
            Debug.LogWarning($"[ItemPanel] 背包空间不足，「{_currentItem.Data?.itemName}」有 {remain} 个未能放入背包");
        }

        UIController.Instance.ClosePanel(this);
    }

    /// <summary>
    /// 点击面板以外区域（全屏遮罩）关闭本面板
    /// </summary>
    private void BindMaskClick()
    {
        if (maskImage == null) return;

        var trigger = maskImage.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = maskImage.gameObject.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener(_ => UIController.Instance?.ClosePanel(this));
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// 刷新名称与图标显示
    /// </summary>
    private void RefreshUI()
    {
        if (itemNameText != null)
        {
            itemNameText.text = _currentItem != null ? _currentItem.Data.itemName : "";
        }

        if (itemIconImage != null)
        {
            if (_currentItem != null && !string.IsNullOrEmpty(_currentItem.Data.iconKey))
            {
                itemIconImage.sprite = ResourceManager.LoadUISpriteByIconKey(_currentItem.Data.iconKey);
                itemIconImage.color = Color.white;
            }
            else
            {
                itemIconImage.sprite = null;
                itemIconImage.color = new Color(1f, 1f, 1f, 0f);
            }
        }
    }
}