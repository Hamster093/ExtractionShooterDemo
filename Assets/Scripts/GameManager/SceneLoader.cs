/****************************************************
    文件：SceneLoader.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-16
	功能：场景切换入口封装（保存玩家状态 → 加载场景 一条链路）
*****************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景切换统一入口：
/// - LoadSceneWithPlayerState：游戏场景之间切换，先保存玩家状态（血量/装备/弹药）再加载，
///   新场景 Player 生成后由 PlayerStateData.RestoreEquipment 自动恢复。
/// - StartNewGame：主菜单"开始新游戏"，清空玩家状态后加载场景。
/// 背包/仓库数据由常驻 InventoryService 自行保留，无需此处处理。
/// </summary>
public static class SceneLoader
{
    /// <summary>
    /// 保存玩家状态并切换场景（游戏场景之间使用，如 Door 进出房间）
    /// </summary>
    /// <param name="sceneName">目标场景名称（须已加入 Build Settings）</param>
    public static void LoadSceneWithPlayerState(string sceneName)
    {
        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            PlayerStateData.Save(player);
        }
        else
        {
            Debug.LogWarning("[SceneLoader] 当前场景没有 PlayerController，跳过玩家状态保存");
        }

        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 进入游戏场景（主菜单"开始/继续游戏"使用）。
    /// 注意：这里【不能】清空 PlayerStateData——登录成功后 LoadGame 已把读档数据暂存进去，
    /// 进场景后 PlayerController.Start 的 ApplyLoadedInventory 需要读取。清空动作在登录成功时由 LoginPanel 完成。
    /// </summary>
    /// <param name="sceneName">目标场景名称（须已加入 Build Settings）</param>
    public static void StartNewGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}