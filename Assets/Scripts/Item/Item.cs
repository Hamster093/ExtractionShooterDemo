/****************************************************
    文件：Item.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-01 15:13:51
	功能：物品类
*****************************************************/

using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot("Items")]
public class ItemDataList
{
    [XmlElement("Item")]
    public List<ItemData> Items = new();
}

[System.Serializable]
public class ItemData
{
    [XmlAttribute("id")]
    public int id;

    [XmlAttribute("itemName")]
    public string itemName;

    [XmlAttribute("maxStack")]
    public int maxStack;

    [XmlAttribute("iconKey")]
    public string iconKey;

    [XmlAttribute("type")]
    public ItemType type;
}

public enum ItemType
{
    Consumable,   // 消耗品（鸡腿、药水）
    Equipment,    // 装备（手枪、步枪）
    Ammo,         // 弹药
    Material      // 材料/任务道具
}

/// <summary>
/// 运行时物品实例
/// </summary>
[System.Serializable]
public class ItemInstance
{
    public int itemID;
    public int amount;

    // 便捷访问静态数据，避免序列化冗余
    public ItemData Data => ItemRegistry.Get(itemID);

    public ItemInstance() { }
    public ItemInstance(int id, int count) { itemID = id; amount = count; }
}

