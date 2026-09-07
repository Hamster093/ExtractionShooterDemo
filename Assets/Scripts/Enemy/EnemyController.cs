/****************************************************
    文件：EnemyController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-07 15:19:00
	功能：敌人控制器
*****************************************************/

using UnityEngine;
using UnityEngine.Pool;

public class EnemyController : MonoBehaviour
{
    [Header("=== 引用 ===")]
    public Transform player;

    [Header("=== 移动参数 ===")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f; // 转向速度

    [Header("=== 攻击参数 ===")]
    public float attackDistance = 10f;     // 攻击距离（在此距离内可攻击）
    public float attackInterval = 3f;     // 攻击间隔（秒）

    [Header("=== 子弹 ===")]
    [SerializeField] private GameObject _bulletPrefab;
    public Transform firePoint;              // 子弹发射点
    public int bulletDamage = 5;

    [Header("=== 对象池配置 ===")]
    [SerializeField] private int _poolDefaultCapacity = 10;
    [SerializeField] private int _poolMaxSize = 30;

    private ObjectPool<ProjectileBase> _bulletPool;

    [Header("=== 状态组件（Inspector 中拖入） ===")]
    public EnemyIdleState idleState;
    public EnemyChaseState chaseState;
    public EnemyAttackState attackState;

    // 玩家是否在触发器范围内
    public bool IsPlayerInRange { get; private set; }

    // 距离上次攻击的时间
    public float TimeSinceLastAttack { get; set; }

    private EnemyStateMachine _stateMachine;
    private CharacterController _characterController;

    private void Awake()
    {
        _stateMachine = new EnemyStateMachine();
        _characterController = GetComponent<CharacterController>();

        _bulletPool = new ObjectPool<ProjectileBase>(
        createFunc: () =>
        {
            var instance = Instantiate(_bulletPrefab);
            if (instance == null)
                throw new System.Exception("[Enemy] Bullet prefab instantiation failed!");
            return instance.GetComponent<ProjectileBase>();
        },
        actionOnGet: (b) => 
        {
            b.Pool = _bulletPool; // ⬅️ 注入池引用，DestroySelf才能正确归还
            b.gameObject.SetActive(true);
        },
        actionOnRelease: (b) => b.gameObject.SetActive(false),
        actionOnDestroy: (b) => Destroy(b.gameObject),
        collectionCheck: Application.isEditor || Debug.isDebugBuild,
        defaultCapacity: _poolDefaultCapacity,
        maxSize: _poolMaxSize
    );
    }

    private void Start()
    {
        // 初始进入 Idle 状态
        _stateMachine.ChangeState(idleState);
    }

    private void Update()
    {
        // 累计攻击冷却时间
        TimeSinceLastAttack += Time.deltaTime;

        // 驱动状态机
        _stateMachine.Tick(Time.deltaTime);
    }

    /// <summary>
    /// 切换到指定状态
    /// </summary>
    public void ChangeState(EnemyState targetState)
    {
        switch (targetState)
        {
            case EnemyState.Idle:
                _stateMachine.ChangeState(idleState);
                break;
            case EnemyState.Chase:
                _stateMachine.ChangeState(chaseState);
                break;
            case EnemyState.Attack:
                _stateMachine.ChangeState(attackState);
                break;
        }
    }

    // ========== 工具方法 ==========

    /// <summary>
    /// 获取敌人到玩家的距离
    /// </summary>
    public float GetDistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        // 3D距离计算（忽略Y轴高度差，只算水平距离）
        Vector3 diff = player.position - transform.position;
        diff.y = 0;
        return diff.magnitude;
    }

    public bool IsPlayerInAttackRange() => GetDistanceToPlayer() <= attackDistance;
    public bool CanAttack() => TimeSinceLastAttack >= attackInterval;

    /// <summary>
    /// 3D移动：朝玩家移动并平滑转向
    /// </summary>
    public void MoveTowardsPlayer()
    {
        if (player == null || _characterController == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // 保持水平移动，不上下飞

        if (direction.sqrMagnitude > 0.001f)
        {
            // 平滑转向
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // 移动
            _characterController.Move(direction * moveSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 射击
    /// </summary>
    public void ShootAtPlayer()
    {
        if (_bulletPool == null || player == null) return;

        Transform spawnPoint = firePoint != null ? firePoint : transform;

        Vector3 targetPos = player.position;
        targetPos.y = spawnPoint.position.y;

        Vector3 direction = (targetPos - spawnPoint.position).normalized;

        if (direction.sqrMagnitude < 0.001f) return;

        // 从对象池获取子弹
        ProjectileBase bullet = _bulletPool.Get();
        if (bullet == null)
        {
            Debug.LogWarning("[Enemy] 对象池返回null，请检查池配置");
            return;
        }

        // 初始化子弹（位置、方向、伤害、来源）
        bullet.Initialize(gameObject, spawnPoint.position, direction, bulletDamage);

        // 重置攻击冷却
        TimeSinceLastAttack = 0f;
    }

    // ========== 3D触发器检测 ==========
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) IsPlayerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) IsPlayerInRange = false;
    }
}