/****************************************************
    文件：GameInputManager.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-03 16:00:44
	功能：其他系统级输入管理器
*****************************************************/

using UnityEngine;

/// <summary>
/// 全局输入管理器 - 处理与具体玩家实体无关的系统级输入
/// 如：ESC暂停、打开背包、截图、显示FPS等
/// </summary>
public class GameInputManager : MonoBehaviour
{
    [SerializeField] private SystemInput _systemInput;

    private void OnEnable()
    {
        if (_systemInput == null) return;

        _systemInput.OnBackpackTriggered += HandleBackpack;
        _systemInput.OnEscape += HandlePause; // 需在 InputHandler 中新增此事件
    }

    private void OnDisable()
    {
        if (_systemInput == null) return;

        _systemInput.OnBackpackTriggered -= HandleBackpack;
        _systemInput.OnEscape -= HandlePause;
    }

    private void HandleBackpack()
    {
        UIController.Instance.OpenBackpack();
    }

    private void HandlePause()
    {
        UIController.Instance?.HandleEscapeKey();
    }
}