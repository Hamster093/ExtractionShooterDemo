/****************************************************
    文件：PlayerController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 13:39:18
	功能：翻滚状态
*****************************************************/

using UnityEngine;

internal class RollState : BaseState
{
    private Vector3 _rollDirection;   // 锁定的翻滚方向
    private float _rollTimer;         // 当前翻滚持续时间
    private float _currentSpeed;      // 当前帧的实际速度

    public RollState(PlayerController player, PlayerAnimatorDriver ani, PlayerMovementConfig config) : base(player, ani, config)
    {
    }

    public override void Enter()
    {
        base.Enter();
        //关闭视角跟随
        _player.AimFollowEnabled(false);

        // 1.锁定方向：优先使用输入方向，若无输入则使用角色朝向
        if (_player._moveDirection.sqrMagnitude > 0.01f)
            _rollDirection = _player._moveDirection.normalized;
        else
            _rollDirection = _player.transform.forward;
        //面向翻滚方向
        Quaternion targetRotation = Quaternion.LookRotation(_rollDirection, Vector3.up);
        _player.transform.rotation = targetRotation;

        // 2. 重置计时器
        _rollTimer = 0f;

        _animDriver.SetTrigger("Roll");
        _animDriver.OnRollAnimationCompleted += HandleRollFinished;
    }
    protected override void OnTick(float deltaTime)
    {
        // 防止动画事件丢失导致永远卡在翻滚状态
        _rollTimer += deltaTime;
        if (_rollTimer >= _config.rollDuration)
        {
            HandleRollFinished();
            return;
        }

        // 计算翻滚速度
        float progress = _rollTimer / _config.rollDuration;
        _currentSpeed = Mathf.Lerp(_config.rollSpeed, 0f, progress);

        // 应用位移
        Vector3 velocity = _rollDirection * _currentSpeed/2;
        _player._rb.linearVelocity = new Vector3(velocity.x, _player._rb.linearVelocity.y, velocity.z);
    }
    public override void Exit()
    {
        base.Exit();
        _player.AimFollowEnabled(true);
        _animDriver.OnRollAnimationCompleted -= HandleRollFinished;
    }
    private void HandleRollFinished()
    {
        if (_player._stateMachine.CurrentState != this) return;
        // 动画自然播完，切回正常状态
        _player._stateMachine.ChangeState<MoveState>();
    }

}