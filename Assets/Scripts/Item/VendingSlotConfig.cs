/****************************************************
    文件：VendingSlotConfig.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-16
	功能：售货机商品配置（在 VendingMachineController Inspector 中配置，按格子生成商品）
*****************************************************/

/// <summary>
/// 售货机商品配置：在 VendingMachineController 的 Inspector 中配置，
/// 指定某个格子的物品ID与数量，运行时按此列表生成售货机容器内的商品。
/// slotIndex 越界或 itemId&lt;=0 / amount&lt;=0 的条目会被忽略（IsValid == false）。
/// </summary>
[System.Serializable]
public struct VendingSlotConfig
{
    public int slotIndex;   // 格子索引（对应售货机面板格子的序号，从 0 开始）
    public int itemId;      // 物品ID（见 Assets/Resources/XML/Items.xml）
    public int amount;      // 数量（不能超过该物品 maxStack）

    public VendingSlotConfig(int slotIndex, int itemId, int amount)
    {
        this.slotIndex = slotIndex;
        this.itemId = itemId;
        this.amount = amount;
    }

    public bool IsValid => slotIndex >= 0 && itemId > 0 && amount > 0;
}