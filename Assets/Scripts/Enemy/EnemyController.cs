/****************************************************
    文件：EnemyController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-07 15:19:00
	功能：敌人控制器
*****************************************************/

using System;
using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyController : MonoBehaviour
{
    [Header("=== 引用 ===")]
    public Transform player;
    public CharacterHealth _health;
    public Collider _collider;

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

    [Header("=== 状态组件 ===")]
    public EnemyIdleState _idleState;
    public EnemyChaseState _chaseState;
    public EnemyAttackState _attackState;

    [Header("=== 状态变量 ===")]
    public Vector3 LastKnownPlayerPos { get; private set; }//玩家最后位置
    public bool HasLastKnownPos { get; private set; }//是否存在玩家最后位置

    [Header("=== 敌人显隐变量 ===")]
    [SerializeField] private float fadeSpeed = 4f;   // 透明度变化速度
    private float _targetAlpha = 1f;   // 目标透明度
    private float _currentAlpha = 1f;  // 当前透明度
    private MaterialPropertyBlock _mpb;   // 用 MPB 改颜色，不额外实例化材质
    private Renderer[] _renderers;
    private int[] _colorIDs;           // 每个 Renderer 材质对应的颜色属性ID（URP=_BaseColor / 内置=_Color），-1=不支持
    private Color[] _baseColors;       // 每个 Renderer 的原始颜色，渐隐时只改 alpha 不动颜色
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor"); // 通用管线
    private static readonly int ColorID = Shader.PropertyToID("_Color");         // 内置管线

    [SerializeField] private WorldSpaceHealthBar healthBar;
    private Coroutine _shootCoroutine;//射击携程

    // 玩家是否在触发器范围内
    public bool IsPlayerInRange { get; private set; }

    // 距离上次攻击的时间
    public float TimeSinceLastAttack { get; set; }

    private EnemyStateMachine _stateMachine;
    private CharacterController _characterController;
    private bool _isDead=false;

    private void Awake()
    {
        _stateMachine = new EnemyStateMachine();
        _characterController = GetComponent<CharacterController>();
        _idleState = new EnemyIdleState(this, _stateMachine);
        _chaseState = new EnemyChaseState(this, _stateMachine);
        _attackState = new EnemyAttackState(this, _stateMachine);

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

        _renderers = GetComponentsInChildren<Renderer>(true);
        _mpb = new MaterialPropertyBlock();
        CacheMaterialColors();
    }

    /// <summary>
    /// 缓存每个 Renderer 材质的颜色属性ID与原始颜色。
    /// 颜色属性名两种管线不同（内置管线 Standard=_Color，通用管线 Lit=_BaseColor），
    /// 所以按材质实际拥有的属性来选，而不是写死一个。
    /// 注意：材质本身必须是透明(Fade)渲染模式（见 Assets/Materials/EnemyFade.mat），
    /// 否则混合是 Blend One Zero，alpha 会被直接忽略，怎么改都看不出渐隐。
    /// </summary>
    private void CacheMaterialColors()
    {
        _colorIDs = new int[_renderers.Length];
        _baseColors = new Color[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            var mat = _renderers[i].sharedMaterial;
            if (mat == null)
            {
                _colorIDs[i] = -1;
                _baseColors[i] = Color.white;
                continue;
            }

            _colorIDs[i] = mat.HasProperty(BaseColorID) ? BaseColorID
                         : (mat.HasProperty(ColorID) ? ColorID : -1);
            _baseColors[i] = _colorIDs[i] >= 0 ? mat.GetColor(_colorIDs[i]) : Color.white;

            // 不透明材质改 alpha 是无效的，提前报出来，免得又排查一遍
            if (mat.renderQueue < (int)UnityEngine.Rendering.RenderQueue.Transparent)
                Debug.LogWarning($"[Enemy] {name} 的材质「{mat.name}」不是透明渲染模式，渐隐不会生效，" +
                                 "请把该材质的 Rendering Mode 设为 Fade（参考 Assets/Materials/EnemyFade.mat）", this);
        }
    }

    private void OnEnable()
    {
        _health.OnDeath += HandleDeath;
        _health.OnHitByAttacker += HandleHitByAttacker;
    }

    private void OnDisable()
    {
        _health.OnDeath -= HandleDeath;
        _health.OnHitByAttacker -= HandleHitByAttacker;
    }

    private void Start()
    {
        // 初始进入 Idle 状态
        _stateMachine.ChangeState(_idleState);
    }

    private void Update()
    {
        // 累计攻击冷却时间
        TimeSinceLastAttack += Time.deltaTime;

        // 只要玩家在范围内，就持续刷新最后已知位置
        if (IsPlayerInRange && player != null)
        {
            LastKnownPlayerPos = player.position;
            HasLastKnownPos = true;
        }

        // 驱动状态机
        _stateMachine.Tick(Time.deltaTime);

        // 透明度插值
        if (!Mathf.Approximately(_currentAlpha, _targetAlpha))
        {
            _currentAlpha = Mathf.MoveTowards(_currentAlpha, _targetAlpha, fadeSpeed * Time.deltaTime);
            ApplyAlpha(_currentAlpha);
        }
    }

    /// <summary>
    /// 切换到指定状态
    /// </summary>
    public void ChangeState(EnemyState targetState)
    {
        switch (targetState)
        {
            case EnemyState.Idle:
                _stateMachine.ChangeState(_idleState);
                break;
            case EnemyState.Chase:
                _stateMachine.ChangeState(_chaseState);
                break;
            case EnemyState.Attack:
                _stateMachine.ChangeState(_attackState);
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
        if (player == null||_isDead) return;
        MoveTowards(player.position);
    }

    /// <summary>
    /// 射击
    /// </summary>
    public void ShootAtPlayer(int Count = 1, float interval = 0.15f)
    {
        if (_bulletPool == null || player == null) return;

        if (_shootCoroutine != null)
        {
            StopCoroutine(_shootCoroutine);
            _shootCoroutine = null;
        }

        _shootCoroutine = StartCoroutine(ShootRoutine(Count, interval));


    }
    /// <summary>
    /// 射击携程
    /// </summary>
    /// <param name="count"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    private IEnumerator ShootRoutine(int count, float interval)
    {
        Transform spawn = firePoint != null ? firePoint : transform;
        yield return FacePlayer(0.2f);

        for (int i = 0; i < count; i++)
        {
            if (player == null) yield break;

            Vector3 targetPos = player.position;
            targetPos.y = spawn.position.y;
            Vector3 dir = (targetPos - spawn.position).normalized;

            if (dir.sqrMagnitude >= 0.001f)
            {
                ProjectileBase bullet = _bulletPool.Get();
                if (bullet == null)
                {
                    Debug.LogWarning("[Enemy] 对象池返回null");
                    yield break;
                }
                // 射击前瞬间转向玩家
                FacePlayerInstant();
                bullet.Initialize(gameObject, spawn.position, dir, bulletDamage);
            }

            if (i < count - 1)
                yield return new WaitForSeconds(interval);
        }

        TimeSinceLastAttack = 0f;
        _shootCoroutine = null;
    }

    // ========== 3D触发器检测 ==========
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[TriggerEnter] other={other.name}, tag={other.tag}, 时间={Time.time:F2}");
        if (other.CompareTag("Player")) IsPlayerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) IsPlayerInRange = false;
    }

    /// <summary>
    /// 死亡事件回调
    /// </summary>
    private void HandleDeath()
    {
        //======立即切断所有交互======
        // 禁用移动/攻击输入
        _isDead=true;

        var sc = GetComponent<SphereCollider>();
        if (sc != null) sc.enabled = false;

        // 冻结物理 & 关闭碰撞
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        _collider.enabled = false;


        // ====== 表现层 ======
        // 播放死亡动画 todo
        //GetComponent<Animator>()?.SetTrigger("Die");

        // 全局广播（UI、音效）
        PlayerEvents.Instance?.TriggerPlayerDied(gameObject);

        //生成宝箱
        gameObject.GetComponentInChildren<EnemyLootData>().DropLoot();
        enabled = false;

        Destroy(gameObject);
    }
    /// <summary>
    /// 设置可见性：只改目标透明度，真正的显隐由 Update 里的 alpha 插值完成
    /// </summary>
    /// <param name="visible"></param>
    public void SetVisible(bool visible)
    {
        _targetAlpha = visible ? 1f : 0f;

        // 只有"要显示"时才兜底打开 Renderer（隐身时 Renderer 是被 ApplyAlpha 关掉的，
        // 而 alpha 已经等于目标值时插值块不再执行，需要这里补一刀）。
        // 注意：visible=false 时绝不能碰 r.enabled —— 检测是每 0.1s 跑一次的，
        // 那样会把刚隐身的敌人反复打开，表现就是"渐隐完全没效果"
        if (!visible) return;

        foreach (var r in _renderers)
            r.enabled = true;
    }

    private void ApplyAlpha(float alpha)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i];

            // 用缓存的原始颜色只改 alpha（不能用 GetPropertyBlock 取色，那拿到的是上一次写进去的白色）
            if (_colorIDs[i] >= 0)
            {
                Color c = _baseColors[i];
                c.a = alpha;

                r.GetPropertyBlock(_mpb);
                _mpb.SetColor(_colorIDs[i], c);
                r.SetPropertyBlock(_mpb);
            }

            r.enabled = alpha > 0.01f;
        }
    }
    /// <summary>
    /// <summary>在 duration 秒内平滑转向玩家
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator FacePlayer(float duration)
    {
        if (player == null) yield break;

        // 记录起始朝向
        Quaternion startRot = transform.rotation;

        // 算出目标朝向（只绕 Y 轴）
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) yield break;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            // SmoothStep 平滑 让起步和收尾柔和一点
            transform.rotation = Quaternion.Slerp(startRot, targetRot, Mathf.SmoothStep(0f, 1f, k));
            yield return null;
        }

        // 最后强制对齐，避免浮点误差留下一点偏差
        transform.rotation = targetRot;
    }
    /// <summary>
    /// 瞬间转向玩家 用于连射
    /// </summary>
    private void FacePlayerInstant()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;   // 重合，不转

        transform.rotation = Quaternion.LookRotation(dir);
    }

    public void MoveTowards(Vector3 targetPos)
    {
        if (_isDead) return;
        if (_characterController == null) return;

        Vector3 direction = targetPos - transform.position;  
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f) return;

        direction.Normalize();

        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        _characterController.Move(direction * moveSpeed * Time.deltaTime);
    }
    /// <summary>
    /// HasLastKnownPos清除方法
    /// </summary>
    public void ClearLastKnownPos()
    {
        HasLastKnownPos = false;
    }

    /// <summary>被打了：把攻击者位置当作最后已知位置，进入追逐</summary>
    private void HandleHitByAttacker(GameObject attacker)
    {
        if (attacker == null) return;
        if (_isDead) return;   // 已死

        LastKnownPlayerPos = attacker.transform.position;
        HasLastKnownPos = true;

        // 正在 Attack 中就让它打完再自己切 Chase，不打断
        if (_stateMachine.CurrentState == _attackState) return;

        _stateMachine.ChangeState(_chaseState);
    }
}