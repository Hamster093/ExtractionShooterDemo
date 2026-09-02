/****************************************************
    文件：SystemInput.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-03 15:57:43
	功能：全局输入管理器 - 处理与具体玩家实体无关的系统级输入
*****************************************************/

using System;
using UnityEngine;

public class SystemInput : MonoBehaviour
{
    public event Action OnBackpackTriggered;
    public event Action OnEscape;

    private PlayerControls _playerActions;

    private void Awake()
    {
        _playerActions = new PlayerControls();

        _playerActions.Player.Back.started += _ => OnBackpackTriggered?.Invoke();
        _playerActions.Player.ESC.started += _ => OnEscape?.Invoke();
    }

    private void OnEnable() => _playerActions?.Enable();
    private void OnDisable() => _playerActions?.Disable();
}