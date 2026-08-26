/****************************************************
    文件：aniTest.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-26 14:36:24
	功能：Nothing
*****************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

public class aniTest : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string _triggerName = "Sprint";

    private void Update()
    {
        // New Input System 的键盘读取方式
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            if (_animator != null)
            {
                _animator.SetTrigger(_triggerName);
                Debug.Log($"[SprintAnimTester] Left Shift pressed → SetTrigger(\"{_triggerName}\")");
            }
        }
    }
}