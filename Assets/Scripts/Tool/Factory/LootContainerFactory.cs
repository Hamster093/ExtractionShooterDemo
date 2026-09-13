/****************************************************
    文件：EnemyLootData.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-10 14:43:34
	功能：根据配置列表构建 ItemContainer
*****************************************************/
using System.Collections.Generic;
using UnityEngine;

public static class LootContainerFactory
{
    /// <summary>
    /// 根据配置列表创建容器。
    /// </summary>
    /// <param name="configs">战利品配置</param>
    /// <param name="slotCount">
    /// 传入和 UI 一致的数量 默认为10
    /// </param>
    public static ItemContainer Create(IList<ChestSlotConfig> configs, int slotCount = 10)
    {
        var container = new ItemContainer(slotCount);

        if (configs != null)
        {
            int limit = Mathf.Min(configs.Count, slotCount);
            for (int i = 0; i < limit; i++)
            {
                var cfg = configs[i];
                if (cfg.IsValid)
                    container.SetItem(i, new ItemInstance(cfg.itemId, cfg.amount));
            }
        }
        return container;
    }

    /// <summary>空容器（用于背包、装备栏初始化）</summary>
    public static ItemContainer CreateEmpty(int slotCount=10)
    {
        return new ItemContainer(slotCount);
    }
}