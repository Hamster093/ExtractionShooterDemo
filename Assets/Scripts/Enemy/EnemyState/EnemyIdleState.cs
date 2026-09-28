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
    private const float PATROL_RADIUS = 5f;    // 巡逻半径
    private const float ARRIVE_THRESHOLD = 0.5f;  // 到达判定
    private const float WAIT_AFTER_ARRIVE = 2f;    // 到点后等待时间

    private Vector3 _patrolCenter;
    private Vector3 _patrolTarget;
    private float _waitTimer; //等待计时器
    private bool _hasTarget; //是否有目标
    public EnemyIdleState(EnemyController enemy, EnemyStateMachine machine) : base(enemy, machine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // 以当前位置为巡逻中心，避免从很远的地方回 Idle 后还要跑回出生点
        _patrolCenter = _enemy.transform.position;
        _hasTarget = false;
        _waitTimer = 0f;
    }

    protected override void OnTick(float deltaTime)
    {
        if (_enemy.IsPlayerInRange)
        {
            _enemy.ChangeState(EnemyState.Chase);
            return;
        }

        if (_hasTarget)
        {
            Vector3 diff = _patrolTarget - _enemy.transform.position;
            diff.y = 0f;

            if (diff.sqrMagnitude < ARRIVE_THRESHOLD * ARRIVE_THRESHOLD)
            {
                _hasTarget = false;
                _waitTimer = WAIT_AFTER_ARRIVE;
            }
            else
            {
                Debug.Log($"[Idle] 去巡逻点 {_patrolTarget}, 自己在 {_enemy.transform.position}, 玩家在 {_enemy.player.position}");
                _enemy.MoveTowards(_patrolTarget);
            }
        }
        else
        {
            _waitTimer -= deltaTime;
            if (_waitTimer <= 0f)
            {
                _patrolTarget = PickRandomPointInCircle(_patrolCenter, PATROL_RADIUS);
                _hasTarget = true;
            }
        }

    }

    public override void Exit()
    {
        base.Exit();
    }

    /// <summary>在以 center 为圆心、radius 为半径的圆内随机取一点（水平面）</summary>
    private Vector3 PickRandomPointInCircle(Vector3 center, float radius)
    {
        Vector2 offset = Random.insideUnitCircle * radius;
        return center + new Vector3(offset.x, 0f, offset.y);
    }
}