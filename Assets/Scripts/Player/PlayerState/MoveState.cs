/****************************************************
    文件：PlayerController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 13:39:18
	功能：移动状态
*****************************************************/

using Unity.VisualScripting.FullSerializer;
using UnityEngine;

internal class MoveState : BaseState
{
    public MoveState(PlayerController player, PlayerAnimatorDriver ani, CharacterStats stats) : base(player, ani, stats)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }
    protected override void OnTick(float deltaTime)
    {

        if (_player._moveDirection.sqrMagnitude < 0.1f)
        {
            _player._stateMachine.ChangeState<IdleState>();
        }

        float currentSpeed = _stats.WalkSpeed;
        Vector3 targetVelocity = _player._moveDirection * currentSpeed;
        // 指数平滑逼近目标速度，避免 50Hz 物理步进下的速度阶跃
        _player.SmoothHorizontalVelocity(targetVelocity, deltaTime, _player.MoveAcceleration);

        bool hasMoveInput = _player._moveDirection.sqrMagnitude > 0.01f;
        _animDriver.SetMoveState(_animDriver.BLEND_WALK);
    }
    public override void Exit()
    {
        base.Exit();
    }
}