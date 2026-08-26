/****************************************************
    文件：PlayerActionHandler.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-26 14:47:45
	功能：Nothing
*****************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _animator;

    [Header("Settings")]
    [SerializeField] private float _tapWindow = 0.2f; // 点按判定窗口（秒）

    // 内部状态
    private float _sPressTime;
    private bool _isSPressed;
    private bool _actionResolved; // 标记本次按下是否已处理

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool sDown = keyboard.leftShiftKey.wasPressedThisFrame;// S键按下瞬间
        bool sUp = keyboard.leftShiftKey.wasReleasedThisFrame;// S键松开瞬间
        bool sHeld = keyboard.leftShiftKey.isPressed;// S键持续按住

        // 【按下瞬间】记录时间，重置状态
        if (sDown)
        {
            _sPressTime = Time.time;
            _isSPressed = true;
            _actionResolved = false;
        }

        // 【按住期间】超过窗口时间且未处理 → 长按冲刺
        if (_isSPressed && !_actionResolved && sHeld)
        {
            if (Time.time - _sPressTime >= _tapWindow)
            {
                OnSprintStart();
                _actionResolved = true; // 标记已处理，防止重复触发
            }
        }

        // 【松开瞬间】
        if (sUp)
        {
            if (!_actionResolved)
            {
                // 未超过窗口就松开了 → 点按翻滚
                OnRoll();
            }
            else
            {
                // 已经超过窗口（正在冲刺中）→ 结束冲刺
                OnSprintEnd();
            }

            _isSPressed = false;
            _actionResolved = false;
        }
    }

    private void OnRoll()
    {
        Debug.Log("[Action] 点按 S → 翻滚");
        _animator.SetTrigger("Roll");
    }

    private void OnSprintStart()
    {
        Debug.Log("[Action] 长按 S → 开始冲刺");
        _animator.SetBool("IsSprinting", true);
    }

    private void OnSprintEnd()
    {
        Debug.Log("[Action] 松开 S → 结束冲刺");
        _animator.SetBool("IsSprinting", false);
    }
}