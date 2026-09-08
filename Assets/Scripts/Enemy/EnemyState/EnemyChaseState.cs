/****************************************************
    文件：ChaseState.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-07 14:44:05
	功能：敌人追逐状态
*****************************************************/

using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    // ⬇️ 最小追逐距离，小于此值停止移动
    private const float MIN_CHASE_DISTANCE = 5f;

    public override void Enter()
    {
        base.Enter();
    }

    protected override void OnTick(float deltaTime)
    {
        // 玩家不在触发器内 → 回到 Idle
        if (!_enemy.IsPlayerInRange)
        {
            _enemy.ChangeState(EnemyState.Idle);
            return;
        }

        // 攻击冷却好了 且 在攻击范围内 → 切换到 Attack
        if (_enemy.CanAttack() && _enemy.IsPlayerInAttackRange())
        {
            _enemy.ChangeState(EnemyState.Attack);
            return;
        }
        // 距离过近时停止追逐，但不切换状态（保持Chase等待玩家拉开距离）
        float distToPlayer = _enemy.GetDistanceToPlayer();
        if (distToPlayer <= MIN_CHASE_DISTANCE)
        {
            return; // 原地待机，不调用 MoveTowardsPlayer
        }

        // 否则继续追逐
        _enemy.MoveTowardsPlayer();
    }

    public override void Exit()
    {
        base.Exit();
    }
}