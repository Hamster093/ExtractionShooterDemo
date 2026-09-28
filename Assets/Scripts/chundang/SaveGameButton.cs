/****************************************************
    文件：SaveGameButton.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-17
	功能：HUD 存档按钮（点击保存玩家完整存档到数据库，提示走 Toast）
	说明：游戏内不设自动存档，手动点此按钮保存。
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 存档按钮：挂在 HUD 下的按钮上，点击调用 SaveGameService.SaveGame。
/// 保存内容：背包 + 仓库 + 装备栏 + 玩家状态（血量/激活栏位/弹匣）。
/// 登录玩家未就绪时给出 Toast 提示（如直接进场景开发测试）。
/// </summary>
public class SaveGameButton : MonoBehaviour
{
    [Tooltip("存档按钮（留空自动获取本物体上的 Button）")]
    [SerializeField] private Button _button;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        if (_button != null)
            _button.onClick.AddListener(OnSaveClicked);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnSaveClicked);
    }

    private void OnSaveClicked()
    {
        var player = GameService.CurrentPlayer;
        if (player == null || player.id <= 0)
        {
            ToastManager.ShowMessage("当前未登录，无法存档");
            return;
        }

        SaveGameService.SaveGame(player.id);
        ToastManager.ShowMessage("存档成功");
    }
}