/****************************************************
    文件：EnemyIdle.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-07 14:46:01
	功能：敌人待机状态
*****************************************************/

using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public override void Enter()
    {
        base.Enter();
    }

    protected override void OnTick(float deltaTime)
    {
        if (_enemy.IsPlayerInRange)
        {
            _enemy.ChangeState(EnemyState.Chase);
        }
        
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("[Enemy] Exit Idle");
    }
}