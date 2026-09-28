/****************************************************
    文件：EnemyDeadState.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026/9/28 14:12:48
	功能：死亡状态 目前写在EnemyController.HandleDeath里
*****************************************************/

internal class EnemyDeadState : EnemyBaseState
{
    protected EnemyDeadState(EnemyController enemy, EnemyStateMachine machine) : base(enemy, machine)
    {
    }


}

