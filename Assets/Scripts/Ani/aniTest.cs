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
    [SerializeField] private string _triggerName = "Fire";

    [Header("全自动设置")]
    [SerializeField] private float fireRate = 0.1f; // 射速间隔(秒)，根据射击动画时长调整

    private float _fireTimer;

    private void Update()
    {
        if (_animator == null) return;

        bool isHolding = Mouse.current.leftButton.isPressed;
        bool justPressed = Mouse.current.leftButton.wasPressedThisFrame;

        // 【关键】松开时重置计时器，确保下次按下第一发无延迟
        if (!isHolding)
        {
            _fireTimer = fireRate;
            return;
        }

        // 按下瞬间立即发射第一发（无延迟手感）
        if (justPressed)
        {
            Fire();
            return;
        }

        // 按住期间按固定间隔持续发射
        _fireTimer += Time.deltaTime;
        if (_fireTimer >= fireRate)
        {
            Fire();
        }
    }

    private void Fire()
    {
        _animator.SetTrigger(_triggerName);
        _fireTimer = 0f;
        Debug.Log($"[aniTest] Fire triggered");
    }
}