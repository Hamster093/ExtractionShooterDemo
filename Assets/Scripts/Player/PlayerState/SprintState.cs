/****************************************************
    文件：SprintState.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 13:39:18
	功能：冲刺状态
*****************************************************/


using Unity.VisualScripting.FullSerializer;
using UnityEngine;

internal class SprintState : BaseState
{
    public SprintState(PlayerController player, PlayerAnimatorDriver ani, PlayerMovementConfig config) : base(player, ani, config)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _animDriver.SetBool("IsSprinting", true);

    }
    protected override void OnTick(float deltaTime)
    {
        if (_player._moveDirection.sqrMagnitude < 0.01f || !_player._isSprinting)
        {
            _player._stateMachine.ChangeState<IdleState>();
        }
        _animDriver.SetMoveState(_config.BLEND_Sprint);
        float currentSpeed = _config.sprintSpeed;
        Vector3 horizontalVelocity = _player._moveDirection * currentSpeed;
        _player._rb.linearVelocity = new Vector3(horizontalVelocity.x, _player._rb.linearVelocity.y, horizontalVelocity.z);
    }
    public override void Exit()
    {
        base.Exit();
        _animDriver.SetBool("IsSprinting", false);
    }
}