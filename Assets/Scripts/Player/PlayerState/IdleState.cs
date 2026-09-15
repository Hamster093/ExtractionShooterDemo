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
    public IdleState(PlayerController player, PlayerAnimatorDriver ani, CharacterStats _stats) : base(player, ani, _stats)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // 不再瞬间清零速度，改为 OnTick 中平滑减速，避免玩家骤停导致相机超调回摆
    }
    protected override void OnTick(float deltaTime)
    {
        _animDriver.SetMoveState(_animDriver.BLEND_IDLE);

        // 平滑减速至静止（而非立即清零）
        _player.SmoothHorizontalVelocity(Vector3.zero, deltaTime, _player.StopDeceleration);

        if (_player._moveDirection.sqrMagnitude > 0.1f)
        {

            _player._stateMachine.ChangeState<MoveState>();
            return;
        }

        if (_player._jumpPressed)
        {
            _player._jumpPressed = false;
            _player._stateMachine.ChangeState<JumpState>();
            return;
        }
    }
    public override void Exit()
    {
        base.Exit();
    }
}