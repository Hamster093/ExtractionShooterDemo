/****************************************************
    文件：SaveGameService.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-16
	功能：存档服务（收集背包/仓库/装备栏/玩家状态 → 数据库；读档反向恢复）
	方案A：单表 JSON 字段（见 duck_inventory.sql）
*****************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 存档服务编排：
/// - SaveGame(playerId)：收集 背包 + 仓库 + 装备栏 + 玩家状态（血量/激活栏位/弹匣/场景名）→ DBMsg.SaveInventory 写库
/// - LoadGame(playerId)：DBMsg.LoadInventory 读库 → 恢复背包/仓库（LoadFromSaveList）+ 装备栏/血量/弹匣/激活栏位（PlayerStateData.Import，
///   由新场景 PlayerController.Start → RestoreEquipment 与 CharacterHealth.Start → ConsumeHealth 自动恢复）
/// 调用时机建议：切场景前 / 玩家退出 / 定时保存（由外部决定）。
/// </summary>
public static class SaveGameService
{
    /// <summary>
    /// 保存当前玩家完整状态到数据库
    /// </summary>
    /// <param name="playerId">玩家ID（duck.id，登录后 GameService.CurrentPlayer.id）</param>
    public static void SaveGame(int playerId)
    {
        var backpack = GameService.Backpack != null
            ? GameService.Backpack.ExportToSaveList()
            : new List<ItemSlotSaveData>();
        var warehouse = GameService.Warehouse != null
            ? GameService.Warehouse.ExportToSaveList()
            : new List<ItemSlotSaveData>();
        var equipment = ExportEquipment();

        var (health, activeSlot, magAmmo) = PlayerStateData.Export();
        string sceneName = SceneManager.GetActiveScene().name;

        DBMsg.Instance.SaveInventory(playerId, backpack, warehouse, equipment, health, activeSlot, magAmmo, sceneName);
        Debug.Log($"[SaveGameService] 已保存玩家 {playerId} 存档：背包 {backpack.Count} 条、仓库 {warehouse.Count} 条、" +
                  $"装备栏 {equipment.Count} 条、HP={health}、场景={sceneName}");
    }

    /// <summary>
    /// 从数据库加载玩家存档并恢复到当前数据层。
    /// 装备栏/血量/弹匣/激活栏位经 PlayerStateData.Import 注入，由场景内 Player 的 Start 自动恢复。
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <returns>是否成功读档（无存档返回 false）</returns>
    public static bool LoadGame(int playerId)
    {
        var save = DBMsg.Instance.LoadInventory(playerId);
        if (save == null)
        {
            Debug.Log($"[SaveGameService] 玩家 {playerId} 暂无存档，按新玩家处理");
            return false;
        }

        GameService.Backpack?.LoadFromSaveList(save.backpack);
        GameService.Warehouse?.LoadFromSaveList(save.warehouse);
        PlayerStateData.Import(save);

        Debug.Log($"[SaveGameService] 已读取玩家 {playerId} 存档：背包 {save.backpack.Count} 条、仓库 {save.warehouse.Count} 条、" +
                  $"装备栏 {save.equipment.Count} 条、HP={save.health}、场景={save.sceneName}");
        return true;
    }

    /// <summary>
    /// 导出装备栏物品（装备格 ChestManager isEquipmentGrid 容器，只含非空格子）
    /// </summary>
    private static List<ItemSlotSaveData> ExportEquipment()
    {
        var list = new List<ItemSlotSaveData>();

        var chest = FindEquipmentGrid();
        if (chest == null) return list;

        chest.EnsureInitialized();
        for (int i = 0; i < chest.Container.SlotCount; i++)
        {
            var item = chest.Container.GetItem(i);
            if (item != null && item.amount > 0)
                list.Add(new ItemSlotSaveData(i, item));
        }
        return list;
    }

    /// <summary>
    /// 查找场景中的装备栏容器（isEquipmentGrid 的 ChestManager）
    /// </summary>
    private static ChestManager FindEquipmentGrid()
    {
        var chests = Object.FindObjectsByType<ChestManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var chest in chests)
        {
            if (chest != null && chest.isEquipmentGrid)
                return chest;
        }
        return null;
    }
}