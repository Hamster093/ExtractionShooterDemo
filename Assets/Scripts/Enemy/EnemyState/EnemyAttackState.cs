/****************************************************
    文件：AttackState.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-07 14:43:51
	功能：敌人攻击状态
*****************************************************/

using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    private float _waitTimer;        // ⬅️ 停留计时器
    public float WAIT_DURATION = 1f; // ⬅️ 停留时长

    public override void Enter()
    {
        base.Enter();

        _waitTimer = WAIT_DURATION;
        _enemy.ShootAtPlayer();
    }

    public override void Exit()
    {
        base.Exit();
    }

    protected override void OnTick(float deltaTime)
    {

        _waitTimer -= deltaTime;
        if (_waitTimer > 0f) return;
        // 射击完成后，根据玩家是否在触发器内决定下一步状态
        if (_enemy.IsPlayerInRange)
        {
            _enemy.ChangeState(EnemyState.Chase); // 玩家还在 → 继续追
        }
        else
        {
            _enemy.ChangeState(EnemyState.Idle);  // 玩家跑了 → 回到待机
        }

    }

}