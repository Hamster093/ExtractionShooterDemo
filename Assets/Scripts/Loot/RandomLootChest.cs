/****************************************************
    文件：RandomLootChest.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026/9/29 13:59:15
	功能：随机战利品箱子 - 场景挂载，Start 时生成容器并交给 LootPickup
*****************************************************/

using UnityEngine;

public class RandomLootChest : MonoBehaviour
{
    [SerializeField] private RandomLootConfig _config;
    [SerializeField] private LootPickup _lootPickup;

    [Tooltip("true = 每次打开都重新生成（会重置搜索状态）；false = 只在 Start 生成一次")]
    [SerializeField] private bool _regenerateOnOpen = false;

    private ItemContainer _container;

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        _container = RandomLootFactory.Create(_config);
        _lootPickup.SetContainer(_container);
    }

    // 可选：如果 _regenerateOnOpen = true，外部打开前调一次
    public void RegenerateIfNeeded()
    {
        if (_regenerateOnOpen) Generate();
    }
}