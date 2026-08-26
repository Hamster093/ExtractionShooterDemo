/****************************************************
    文件：PlayerController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 13:39:18
	功能：玩家控制器
*****************************************************/

internal class AttackState : BaseState
{
    public AttackState(PlayerController player, PlayerAnimatorDriver ani) : base(player, ani)
    {

    }

    public override void Enter()
    {
        //_player.SetFiring(true);
    }

    protected override void OnTick(float deltaTime)
    {
        // 持续检测输入，决定何时停止射击
        //if (!_player._attackHeld) // 注意：这里应该是"按住"而非"按下"
        //{
        //    _player.SetFiring(false);
        //    _player._stateMachine.ChangeState<IdleState>();
        //}

        // 处理实际射击逻辑（射线检测、弹药消耗等）
        //_player.TryFire(deltaTime);
    }

    public override void Exit()
    {
        // 兜底：确保退出时一定关闭射击标志
        //_player.SetFiring(false);
    }
}