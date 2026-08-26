/****************************************************
    文件：BaseState.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 17:30:51
	功能：Nothing
*****************************************************/

using Unity.VisualScripting;
using UnityEngine;

public class BaseState : IState
{
    protected readonly PlayerController _player;
    protected readonly PlayerAnimatorDriver _animDriver;
    protected readonly PlayerMovementConfig _config;


    /// <summary>
    /// 标记该状态是否已经被 Exit
    /// </summary>
    private bool _hasExited;

    protected BaseState(PlayerController player, PlayerAnimatorDriver ani, PlayerMovementConfig config)
    {
        _player = player;
        _animDriver = ani;
        _config = config;
    }

    public virtual void Enter()
    {
        _hasExited = false;
    }

    public virtual void Exit()
    {
        _hasExited = true;
    }

    public virtual void Tick(float deltaTime)
    {
        if (_hasExited) return; //已退出的状态不再执行(防御逻辑
        OnTick(deltaTime);
    }

    /// <summary>
    /// 子类重写此方法
    /// </summary>
    protected virtual void OnTick(float deltaTime) { }
}