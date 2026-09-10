/****************************************************
    文件：TEXT.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-10 16:02:04
	功能：战利品箱子初始化物品
*****************************************************/

using System.Collections.Generic;
using UnityEngine;

public class TEXT : MonoBehaviour 
{
    [SerializeField] private List<ChestSlotConfig> _lootTable = new();
    [SerializeField] private int _slotCount = 10;

    public LootPickup lootPickup;

    private void Start()
    {
        var container = LootContainerFactory.Create(_lootTable, _slotCount);
        lootPickup.SetContainer(container);
    }
}