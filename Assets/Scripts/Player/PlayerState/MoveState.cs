/****************************************************
    文件：PlayerController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 13:39:18
	功能：移动状态
*****************************************************/


using UnityEngine.InputSystem.XR;

internal class MoveState : BaseState
{
    /// <summary>
    /// 动画驱动器
    /// </summary>
    

    public MoveState(PlayerController player, PlayerAnimatorDriver ani) : base(player,ani)
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
        bool hasMoveInput = _player._moveDirection.sqrMagnitude > 0.01f;
        _animDriver.SetMoveState(hasMoveInput);
    }
    public override void Exit()
    {
        base.Exit();
    }
}