/****************************************************
    文件：RandomLootFactory.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026/9/29 13:58:35
	功能：根据 RandomLootConfig 随机生成 ItemContainer
*****************************************************/

using System.Collections.Generic;
using UnityEngine;

public static class RandomLootFactory
{
    public static ItemContainer Create(RandomLootConfig config)
    {
        if (config == null)
        {
            Debug.LogError("[RandomLootFactory] config 为 null");
            return new ItemContainer(0);
        }

        var container = new ItemContainer(config.slotCount);

        // 1. 拿到候选物品
        var types = config.ResolveAllowedTypes();
        var candidates = ItemRegistry.GetAllByTypes(types);

        if (candidates == null || candidates.Count == 0)
        {
            Debug.LogWarning($"[RandomLootFactory] 没有匹配 {string.Join("/", types)} 的物品");
            return container;
        }

        // 2. 打乱格子位置，让物品随机分布在格子里
        var positions = new List<int>();
        for (int i = 0; i < config.slotCount; i++) positions.Add(i);
        Shuffle(positions);

        // 3. 决定本次生成多少件
        int want = Random.Range(config.minItemCount, config.maxItemCount + 1);
        want = Mathf.Clamp(want, 0, config.slotCount);

        // 4. 逐个生成
        var usedCount = new Dictionary<string, int>(); // itemID → 已生成次数
        int placed = 0;
        int guard = 0;                                  // 防止候选太少时死循环
        int maxAttempts = want * 30;

        while (placed < want && guard++ < maxAttempts)
        {
            var data = candidates[Random.Range(0, candidates.Count)];

            // 去重：同物品最多出现 N 次
            if (config.maxSameItem > 0)
            {
                usedCount.TryGetValue(data.itemName, out int c);
                if (c >= config.maxSameItem) continue;
                usedCount[data.itemName] = c + 1;
            }

            // 堆叠数量：受物品自身 maxStack 限制
            int stack = Random.Range(config.minStack, config.maxStack + 1);
            stack = Mathf.Clamp(stack, 1, Mathf.Max(1, data.maxStack));

            // 生成实例
            var item = new ItemInstance(data.id, stack);

            container.SetItem(positions[placed], item);
            placed++;
        }

        return container;
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}