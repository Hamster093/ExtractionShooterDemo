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
    public SprintState(PlayerController player, PlayerAnimatorDriver ani, CharacterStats stats) : base(player, ani, stats)
    {
    }

    public override void Enter()
    {
        base.Enter();
        //冲刺目前是在混合树中进行动画表现 暂不需要使用"IsSprinting"控制 该参数后续可能会用于冲刺状态其他动画表现
        _animDriver.SetBool("IsSprinting", true);

    }
    protected override void OnTick(float deltaTime)
    {
        if (_player._moveDirection.sqrMagnitude < 0.01f || !_player._isSprinting)
        {
            _player._stateMachine.ChangeState<IdleState>();
        }
        _animDriver.SetMoveState(_animDriver.BLEND_Sprint);
        float currentSpeed = _stats.WalkSpeed* _stats.SprintSpeedMultiplier;
        Vector3 targetVelocity = _player._moveDirection * currentSpeed;
        // 指数平滑逼近目标速度，避免 50Hz 物理步进下的速度阶跃
        _player.SmoothHorizontalVelocity(targetVelocity, deltaTime, _player.MoveAcceleration);
    }
    public override void Exit()
    {
        base.Exit();
        _animDriver.SetBool("IsSprinting", false);
    }
}