/****************************************************
    文件：PlayerBackpackView.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-05 20:02:30
	功能：背包表现类
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    public Image Icon;
    public Text ItemText;
    public Text Count;

    [Tooltip("装备格类别标志：装备栏中此格子只接受该类型物品（None = 不限制）；普通背包/宝箱格子忽略此字段")]
    public ItemType allowedType = ItemType.None;

    /// <summary>
    /// 统一刷新格子显示，避免外部直接操作UI组件
    /// </summary>
    public void SetItem(ItemInstance item)
    {
        bool hasItem = item != null && item.amount > 0;

        // 图标
        if (Icon != null)
        {
            Icon.sprite = hasItem ? ResourceManager.LoadUISpriteByIconKey(item.Data.iconKey) : null;
            Icon.color = hasItem ? Color.white : new Color(1, 1, 1, 0);
        }

        // 数量（仅大于1时显示）
        if (Count != null)
            Count.text = hasItem && item.amount > 1 ? item.amount.ToString() : "";

        // 物品名称
        if (ItemText != null)
            ItemText.text = hasItem ? item.Data.itemName : "";
    }

    /// <summary>
    /// 清空格子显示
    /// </summary>
    public void Clear()
    {
        if (Icon != null)
        {
            Icon.sprite = null;
            Icon.color = new Color(1, 1, 1, 0);
        }
        if (Count != null) Count.text = "";
        if (ItemText != null) ItemText.text = "";
    }
}