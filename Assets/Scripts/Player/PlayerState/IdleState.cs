/****************************************************
    文件：PlayerController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 13:39:18
	功能：玩家控制器
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

internal class IdleState : BaseState
{
    public IdleState(PlayerController player, PlayerAnimatorDriver ani, PlayerMovementConfig config) : base(player, ani, config)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _animDriver.SetMoveState(_config.BLEND_IDLE, true);
        _player._rb.linearVelocity = Vector3.zero;
    }
    protected override void OnTick(float deltaTime)
    {
        if (_player._moveDirection.sqrMagnitude > 0.1f)
        {
            if (_player._isSprinting)
                _player._stateMachine.ChangeState<SprintState>();
            else
                _player._stateMachine.ChangeState<MoveState>();
            return;
        }

        if (_player._jumpPressed)
        {
            _player._jumpPressed = false;
            _player._stateMachine.ChangeState<JumpState>();
            return;
        }

        if (_player._attackPressed)
        {
            _player._attackPressed = false;
            _player._stateMachine.ChangeState<AttackState>();
        }
    }
    public override void Exit()
    {
        base.Exit();
    }
}