/****************************************************
    文件：PlayerAnimatorDriver.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 18:35:32
	功能：玩家动画驱动器
*****************************************************/

using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorDriver : MonoBehaviour
{
    private Animator _animator;
    // 缓存参数哈希值
    private int _moveBlendHash;    //移动
    private int _sprintBoolHash;   //冲刺
    private int _rollTriggerHash;  //翻滚

    public event Action OnRollAnimationCompleted;

    /// <summary>
    /// 安全锁标志位。当状态机正在执行 ChangeState 时，
    /// 此标志位会被置为 true，阻止 Tick 中的阻尼动画覆盖新状态的设置。
    /// </summary>
    private bool _isTransitioning;

    /// <summary>
    /// 标记状态机正在切换（由 StateMachine 调用）。
    /// </summary>
    public void BeginTransition() => _isTransitioning = true;

    /// <summary>
    /// 标记状态机切换完成（由 StateMachine 调用）。
    /// </summary>
    public void EndTransition() => _isTransitioning = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        _moveBlendHash = Animator.StringToHash(AnimParams.Blend);
        _sprintBoolHash = Animator.StringToHash(AnimParams.isSprinting);
        _rollTriggerHash = Animator.StringToHash(AnimParams.RollTrigger);
    }

    /// <summary>
    /// 设置移动混合值（带安全锁保护）
    /// </summary>
    public void SetMoveState(float Blend, bool immediate = false)
    {
        // 安全锁
        if (_isTransitioning && !immediate) return;

        if (_animator == null) return;

        if (immediate)
        {
            _animator.SetFloat(_moveBlendHash, Blend);
        }
        else
        {
            _animator.SetFloat(_moveBlendHash, Blend, 0.1f, Time.deltaTime);
        }
    }

    /// <summary>
    /// 设置 Bool 参数（例如：IsSprinting）
    /// </summary>
    public void SetBool(string parameterName, bool value)
    {
        if (_animator == null) return;
        // 根据参数名获取哈希（也可以像 MoveBlend 一样在 Awake 中缓存）
        int hash = Animator.StringToHash(parameterName);
        _animator.SetBool(hash, value);
    }

    /// <summary>
    /// 触发 Trigger 参数（例如：Roll）
    /// </summary>
    public void SetTrigger(string parameterName)
    {
        if (_animator == null) return;
        int hash = Animator.StringToHash(parameterName);
        _animator.SetTrigger(hash);
    }

    /// <summary>
    /// 暴露给 Unity Animation Event 调用的方法。
    /// 在翻滚动画的最后一帧打上 Event 调用此方法，即可驱动状态机切换。
    /// </summary>
    public void TriggerRollCompleted()
    {
        OnRollAnimationCompleted?.Invoke();
    }

}