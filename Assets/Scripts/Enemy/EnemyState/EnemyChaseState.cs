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
    // —— 追逐阈值 ——
    private const float MIN_CHASE_DISTANCE = 5f;    // 到达此距离视为"已进攻击位"
    private const float MAX_CHASE_DISTANCE = 7f;    // 超过此距离才重新追

    // 到达"最后已知位置"的判定距离
    private const float ARRIVE_THRESHOLD = 0.5f;

    // 追踪最后位置时，超过这个距离就放弃
    private const float GIVE_UP_DISTANCE = 15f;

    // —— 近身随机游走参数 ——
    private const float STRAFE_MIN_RADIUS = 2.5f;  // 以玩家为圆心，随机目标点的最小半径
    private const float STRAFE_CHANGE_TIME = 1.5f;  // 多久换一次游走目标

    // 滞回标记：true = 已在攻击位（随机游走），false = 还没追上（直线追）
    private bool _isInAttackPosition;

    // 游走目标
    private Vector3 _strafeTarget;
    private float _strafeTimer;
    private bool _hasStrafeTarget;


    public EnemyChaseState(EnemyController enemy, EnemyStateMachine machine) : base(enemy, machine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _isInAttackPosition = false;   // 每次进 Chase 都从还没追上开始
        _hasStrafeTarget = false;
        _strafeTimer = 0f;
    }

    protected override void OnTick(float deltaTime)
    {
        // 玩家在范围内 → 正常追逐 / 攻击
        if (_enemy.IsPlayerInRange)
        {
            HandlePlayerInRange(deltaTime);
            return;
        }

        // 玩家不在范围内，但有最后已知位置 → 去那看看
        if (_enemy.HasLastKnownPos)
        {
            Vector3 diff = _enemy.LastKnownPlayerPos - _enemy.transform.position;
            diff.y = 0f;

            if (diff.sqrMagnitude < ARRIVE_THRESHOLD * ARRIVE_THRESHOLD
                || diff.magnitude > GIVE_UP_DISTANCE)
            {
                _enemy.ClearLastKnownPos();
                _enemy.ChangeState(EnemyState.Idle);
                return;
            }
            _enemy.MoveTowards(_enemy.LastKnownPlayerPos);
            return;
        }

        // 完全没线索 → 回 Idle
        _enemy.ChangeState(EnemyState.Idle);
    }

    public override void Exit()
    {
        base.Exit();
    }
    /// <summary>
    /// 处理玩家在范围内时
    /// </summary>
    /// <param name="deltaTime"></param>
    private void HandlePlayerInRange(float deltaTime)
    {
        // 能攻击就攻击
        if (_enemy.CanAttack() && _enemy.IsPlayerInAttackRange())
        {
            _enemy.ChangeState(EnemyState.Attack);
            return;
        }

        float dist = _enemy.GetDistanceToPlayer();

        // 更新滞回标记：只在两个阈值之外才切换
        if (dist <= MIN_CHASE_DISTANCE)
            _isInAttackPosition = true;
        else if (dist > MAX_CHASE_DISTANCE)
            _isInAttackPosition = false;
        // 5 < dist <= 7：不切换，保持原状态

        if (_isInAttackPosition)
        {
            DoStrafe(deltaTime);           // 已在攻击位 → 随机游走
        }
        else
        {
            _enemy.MoveTowardsPlayer();    // 还没追上 → 直线追
            _hasStrafeTarget = false;      // 重置游走，下次进来重新选点
        }
    }

    /// <summary>
    /// 以玩家为中心在环形区域内随机游走
    /// </summary>
    /// <param name="deltaTime"></param>
    private void DoStrafe(float deltaTime)
    {
        _strafeTimer -= deltaTime;

        // 需要新目标：没目标 或 到时间了
        if (!_hasStrafeTarget || _strafeTimer <= 0f)
        {
            PickStrafeTarget();
            _strafeTimer = STRAFE_CHANGE_TIME;
            _hasStrafeTarget = true;
        }

        Vector3 diff = _strafeTarget - _enemy.transform.position;
        diff.y = 0f;

        // 到了目标点就等着换下一个
        if (diff.sqrMagnitude < ARRIVE_THRESHOLD * ARRIVE_THRESHOLD)
            return;

        _enemy.MoveTowards(_strafeTarget);
    }

    /// <summary>
    /// 以玩家为圆心，在 MIN_CHASE_DISTANCE 内的环上随机取一个目标点
    /// </summary>
    private void PickStrafeTarget()
    {
        if (_enemy.player == null) return;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Random.Range(STRAFE_MIN_RADIUS, MIN_CHASE_DISTANCE - 0.5f);

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        _strafeTarget = _enemy.player.position + offset;
    }
}