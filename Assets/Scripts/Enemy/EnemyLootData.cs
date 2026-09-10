/****************************************************
    文件：EnemyLootData.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-10 14:43:34
	功能：死亡时候调用
*****************************************************/

using System.Collections.Generic;
using UnityEngine;

public class EnemyLootData : MonoBehaviour 
{
    [Header("掉落配置")]
    [Tooltip("这个敌人的战利品表，索引 0 对应宝箱第一个格子")]
    [SerializeField] private List<ChestSlotConfig> _lootTable = new();

    [Header("生成设置")]
    [Tooltip("战利品拾取物预制体（挂有 LootPickup）")]
    [SerializeField] private GameObject _lootPickupPrefab;

    [Tooltip("相对敌人位置的生成偏移")]
    [SerializeField] private Vector3 _spawnOffset = new Vector3(0f, 0.5f, 0f);

    [SerializeField] private int _slotCount = 10;

    public IReadOnlyList<ChestSlotConfig> LootTable => _lootTable;
    
    /// <summary>
    /// 死亡时调用
    /// </summary>
    public void DropLoot()
    {
        if (_lootPickupPrefab == null)
        {
            Debug.LogWarning($"[EnemyLootData] {gameObject.name} 未配置 LootPickup 预制体", this);
            return;
        }
        if (_lootTable == null || _lootTable.Count == 0)
        {
            Debug.Log($"[EnemyLootData] {gameObject.name} 无掉落物，跳过生成");
            return;
        }
        // 1. 生成容器
        var container = LootContainerFactory.Create(_lootTable, _slotCount);
        // 2. 生成拾取物
        var go = Instantiate(_lootPickupPrefab, transform.position + _spawnOffset, Quaternion.identity);
        go.GetComponent<LootPickup>()?.SetContainer(container);  
    }
}