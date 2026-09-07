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
    /// 玩家背包数据服务
    /// </summary>
    public static BackpackData Backpack => InventoryService.Instance?.PlayerBackpack;

    /// <summary>
    /// 手动重置（仅用于测试或切换存档时）
    /// </summary>
    public static void Reset()
    {
        if (InventoryService.Instance != null)
        {
            InventoryService.Instance.PlayerBackpack?.Clear();
            Debug.Log("[GameService] 背包已清空");
        }
    }
}
