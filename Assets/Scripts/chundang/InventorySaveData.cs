/****************************************************
    文件：InventorySaveData.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-16
	功能：存档数据结构（背包/仓库/装备栏 + 玩家状态，方案A：单表 JSON 字段）
*****************************************************/

using System;
using System.Collections.Generic;

/// <summary>
/// 玩家完整存档数据（对应数据库表 duck_inventory 一行）。
/// - backpack/warehouse/equipment：列表存储，只含非空格子 {slotIndex, itemID, amount}
/// - health/activeSlot/magAmmo：玩家运行时状态（读档后经 PlayerStateData.Import 恢复）
/// - sceneName：存档时所在场景，读档后用于跳转
/// </summary>
[Serializable]
public class InventorySaveData
{
    public List<ItemSlotSaveData> backpack = new List<ItemSlotSaveData>();   // 背包物品
    public List<ItemSlotSaveData> warehouse = new List<ItemSlotSaveData>();   // 仓库物品
    public List<ItemSlotSaveData> equipment = new List<ItemSlotSaveData>();   // 装备栏物品（0=主武器/1=副武器/2=近战）
    public int health = -1;                                                   // 血量（-1=未保存，读档按满血）
    public int activeSlot = 0;                                                // 激活武器栏位
    public int[] magAmmo;                                                     // 每栏位弹匣弹药（null/越界按 -1=无武器处理）
    public string sceneName = "";                                             // 存档场景名
}

/// <summary>
/// JsonUtility 不支持序列化 List&lt;T&gt; 顶层，必须用包装类。
/// 序列化：JsonUtility.ToJson(new ItemSlotSaveListWrapper { items = list })
/// 反序列化：JsonUtility.FromJson&lt;ItemSlotSaveListWrapper&gt;(json)?.items
/// </summary>
[Serializable]
public class ItemSlotSaveListWrapper
{
    public List<ItemSlotSaveData> items = new List<ItemSlotSaveData>();
}