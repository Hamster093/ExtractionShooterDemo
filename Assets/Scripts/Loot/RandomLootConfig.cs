/****************************************************
    文件：RandomLootConfig.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026/9/29 13:56:48
	功能：随机战利品配置
*****************************************************/

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomLootConfig", menuName = "Loot/Random Loot Config")]
public class RandomLootConfig : ScriptableObject
{
    [Header("格子")]
    [Tooltip("容器总格子数")]
    public int slotCount = 10;

    [Header("物品数量（生成的物品件数，不是格子数）")]
    public int minItemCount = 3;
    public int maxItemCount = 8;

    [Header("堆叠（仅对 maxStack > 1 的物品生效，自动被物品自身上限截断）")]
    public int minStack = 1;
    public int maxStack = 1;

    [Header("允许的类别（至少勾一个）")]
    public bool includeConsumable = true;   // 消耗品
    public bool includeEquipment = true;   // 装备（含近战/头盔/护甲/背包/耳机/面部/饰品）
    public bool includeAmmo = true;   // 子弹
    public bool includeMaterial = true;   // 材料/任务道具
     public bool includeCollectible;      // 收集品

    [Header("精确指定类型（可选，与上面的开关叠加）")]
    public List<ItemType> extraTypes = new();

    [Header("去重")]
    [Tooltip("同一物品最多出现几次（0 = 不限）")]
    public int maxSameItem = 2;

    /// <summary>把勾选项展开为 ItemType 集合</summary>
    public List<ItemType> ResolveAllowedTypes()
    {
        var set = new HashSet<ItemType>();

        if (includeConsumable) set.Add(ItemType.Consumable);
        if (includeAmmo) set.Add(ItemType.Ammo);
        if (includeMaterial) set.Add(ItemType.Material);

        if (includeEquipment)
        {
            // 装备大类的所有子类型
            set.Add(ItemType.Equipment);
            set.Add(ItemType.MeleeWeapon);
            set.Add(ItemType.Headset);
            set.Add(ItemType.Helmet);
            set.Add(ItemType.Body);
            set.Add(ItemType.Backpack);
            set.Add(ItemType.Face);
            set.Add(ItemType.Accessory);
        }

        foreach (var t in extraTypes)
            set.Add(t);

        return new List<ItemType>(set);
    }
}
