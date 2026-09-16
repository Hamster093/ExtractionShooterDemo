/****************************************************
    文件：DoorInteractable.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-16
	功能：门交互（靠近显示 F 提示，按 F 切换场景；经 SceneLoader 保存玩家状态）
*****************************************************/

using UnityEngine;

public class DoorInteractable : InteractableBase
{
    [Header("场景")]
    [Tooltip("点击出发时加载的游戏场景名称")]
    [SerializeField] private string gameSceneName = "";

    private bool _isOpen = false;

    protected override void OnInteract()
    {
        _isOpen = !_isOpen;

        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[MainMenuPanel] 未填写游戏场景名称，请在 Inspector 的 gameSceneName 字段中填写");
            return;
        }
        Debug.Log($"[MainMenuPanel] 加载游戏场景：{gameSceneName}");
        // 经 SceneLoader 切换：先保存玩家状态（血量/装备/弹药），新场景 Player 生成后自动恢复
        SceneLoader.LoadSceneWithPlayerState(gameSceneName);
    }
}