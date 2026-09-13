/****************************************************
    文件：WeaponBase.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-05 21:06:15
	功能：游戏全局服务入口
*****************************************************/
using UnityEngine;

public static class GameService
{
    /// <summary>
    /// 数据库服务
    /// </summary>
    public static DBMsg DB => DBMsg.Instance;
    /// <summary>
    /// 当前玩家登录数据
    /// </summary>
    public static PlayerData CurrentPlayer { get; private set; }
    /// <summary>
    /// 玩家背包数据服务
    /// </summary>
    public static BackpackData Backpack => InventoryService.Instance?.PlayerBackpack;

    /// <summary>
    /// 玩家仓库数据服务（列表存储，供拖拽存放与数据库存档）
    /// </summary>
    public static WarehouseData Warehouse => InventoryService.Instance?.Warehouse;

    /// <summary>
    /// 登录成功后，由 LoginPanel 调用，初始化全局游戏数据
    /// </summary>
    public static void InitGame(PlayerData playerData)
    {
        CurrentPlayer = playerData;
        Debug.Log($"[GameService] 玩家 {CurrentPlayer.name} (ID: {CurrentPlayer.id}) 初始化游戏数据");

        // TODO 1: 根据玩家ID，从数据库加载更多数据（背包 + 仓库）
        // 例如：
        //   var data = DB.LoadInventory(CurrentPlayer.id);
        //   Backpack?.LoadFromSaveList(data.backpack);
        //   Warehouse?.LoadFromSaveList(data.warehouse);

        // TODO 2: 如果是从重新登录进入，需要重置背包/仓库数据（调用下面的 Reset）

        // TODO 3: 数据加载完毕后，跳转到游戏场景
        // SceneManager.LoadScene("GameScene");
    }


    /// <summary>
    /// 手动重置（仅用于测试或切换存档时）
    /// </summary>
    public static void Reset()
    {
        if (InventoryService.Instance != null)
        {
            InventoryService.Instance.PlayerBackpack?.Clear();
            InventoryService.Instance.Warehouse?.Clear();
            Debug.Log("[GameService] 背包与仓库已清空");
        }
        CurrentPlayer = null;
    }
}
