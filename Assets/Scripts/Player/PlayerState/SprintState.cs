/****************************************************
    文件：PlayerController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 13:39:18
	功能：玩家控制器
*****************************************************/

internal class SprintState : BaseState
{
    public SprintState(PlayerController player, PlayerAnimatorDriver ani) : base(player, ani)
    {

    }

    public override void Enter()
    {
        base.Enter();
    }
    protected override void OnTick(float deltaTime)
    {
        base.Tick(deltaTime);
    }
    public override void Exit()
    {
        base.Exit();
    }
}