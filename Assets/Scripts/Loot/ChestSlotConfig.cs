/****************************************************
    文件：ChestSlotConfig.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-03 18:02:59
	功能：宝箱初始物品配置
*****************************************************/


/// <summary>
/// 宝箱初始物品配置（通常由配置表或存档反序列化生成）
/// </summary>
[System.Serializable]
public struct ChestSlotConfig
{
    public int itemId;
    public int amount;

    public ChestSlotConfig(int itemId, int amount)
    {
        this.itemId = itemId;
        this.amount = amount;
    }
}
