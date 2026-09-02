/****************************************************
    文件：WeaponBase.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-27 14:35:15
	功能：武器基类
*****************************************************/

using System;
using System.Collections;
using UnityEngine;
using static UnityEditor.Progress;
/// <summary>
/// 武器系统抽象基类
/// 职责：管理弹药、冷却、换弹等通用逻辑，并将具体射击行为延迟到子类实现
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    [Tooltip("武器静态配置")]
    [SerializeField] protected WeaponConfig _config;
    public WeaponConfig Config => _config;
    [SerializeField] protected Transform _muzzlePoint;

    //查询背包内子弹携带量
    public int ReserveAmmo => PlayerBackpack.Instance?.GetItemCount(_config.DefaultAmmo) ?? 0;

    // ─── 弹药状态 ───
    protected int _currentAmmo;         //当前弹匣内剩余弹药数
    protected bool _isReloading;        //换弹进行中标记
    private bool _isFireRequested;      //是否请求开火
    private float _nextFireTime;        //开火间隔
    private bool _isInitialized;       // 防止重复初始化
    private int _lastReserveAmmo;      //上一次的备弹数量

    public int CurrentAmmo => _currentAmmo;
    public int MaxAmmo => _config.maxAmmo;
    public bool IsReloading => _isReloading;

    //─── 引用 ───
    protected PlayerAnimatorDriver _animDriver;
    protected GameObject _owner;
    private Coroutine _reloadCoroutine; // 缓存句柄用于安全中断
    private Func<Vector3> _getAimTargetWorldPos;

    // ─── 事件 ───
    /// <summary>
    /// 弹药变化事件
    /// - 参数1: 当前背包内弹药
    /// - 触发时机: 拾取弹药 填装消耗弹药
    /// - - 订阅方: UI面板（更新换弹后弹药数字）
    /// </summary> 
    public event Action<int> OnReserveAmmoChanged;//身上的子弹（背包/备弹）跟随弹药类型
    /// <summary>
    /// 弹药变化事件
    /// - 参数1: 当前弹匣弹药 (_currentAmmo)
    /// - 参数2: 弹匣最大容量 (_config.maxAmmo)
    /// - 触发时机: 射击扣弹后、换弹完成后
    /// - 订阅方: UI面板（更新射击后弹药数字）
    /// </summary> 
    public event Action<int, int> OnAmmoChanged; //枪里的子弹（弹匣）跟随武器实例

    #region 初始化与生命周期
    /// <summary>
    /// 武器初始化入口
    /// PlayerController 装备武器时主动调用
    /// </summary>
    public virtual void Initialize(PlayerAnimatorDriver animDriver, GameObject owner)
    {
        if (_isInitialized) return;
        _animDriver = animDriver;
        _owner = owner;
        _getAimTargetWorldPos = null;

        // 只有首次初始化才设置弹药和订阅背包
        if (!_isInitialized)
        {
            _currentAmmo = _config.maxAmmo;
            _isInitialized = true;

            OnAmmoChanged?.Invoke(_currentAmmo, _config.maxAmmo);
            _lastReserveAmmo = ReserveAmmo;
            OnReserveAmmoChanged?.Invoke(ReserveAmmo);

            var inv = PlayerBackpack.Instance;
            // 订阅库存变化，当该弹药类型数量变动时通知UI
            if (inv != null)
            {
                inv.OnSlotChanged += OnInventoryItemChanged;
                ///初始化备弹 后续删掉 todo
                if (inv.GetItemCount(_config.DefaultAmmo) == 0)
                    inv.AddItem(_config.DefaultAmmo, _config.initialReserveAmmo);
            }
        }
        else
        {
            // 重新装备时，仅通知UI刷新当前真实弹药状态
            OnAmmoChanged?.Invoke(_currentAmmo, _config.maxAmmo);
            _lastReserveAmmo = ReserveAmmo;
            OnReserveAmmoChanged?.Invoke(ReserveAmmo);
        }
    }

    private void OnDestroy()
    {
        if (PlayerBackpack.Instance != null)
            PlayerBackpack.Instance.OnSlotChanged -= OnInventoryItemChanged;
        Uninitialize();
    }

    /// <summary>
    /// 反初始化（从栏位移除/销毁前调用）
    /// </summary>
    public virtual void Uninitialize()
    {
        CancelReload();
        _isFireRequested = false;

        _animDriver = null;
        _owner = null;
        _getAimTargetWorldPos = null;

        if (PlayerBackpack.Instance != null)
            PlayerBackpack.Instance.OnSlotChanged -= OnInventoryItemChanged;
    }

    private void OnDisable()
    {
        CancelReload();
        _isFireRequested = false;
    }
    #endregion

    #region 开火系统
    /// <summary>
    /// 外部请求开火（由 PlayerController 调用）
    /// </summary>
    public void RequestFire(Vector3 direction)
    {

        // 状态校验
        if (!CanFire()) return;

        // 按下瞬间立即尝试打第一发
        TryFireInternal(GetSafeFireDirection());

        // 全自动武器：标记持续请求
        if (_config.fireMode == FireMode.FullAuto)
            _isFireRequested = true;
    }

    /// <summary>
    /// 外部取消开火
    /// </summary>
    public void CancelFire()
    {
        _isFireRequested = false;
    }


    public virtual void Tick(float deltaTime)
    {
        // 全自动连发
        if (_isFireRequested && Time.time >= _nextFireTime)
        {
            if (CanFire())
                TryFireInternal(GetSafeFireDirection());
            else
                _isFireRequested = false; // 没弹药自动停止连发
        }

    }

    /// <summary>
    /// 开火前置校验
    /// </summary>
    protected virtual bool CanFire()
    {
        if (CurrentAmmo <= 0 || IsReloading) return false;

        // 安全获取玩家控制器状态，非玩家持有或引用丢失时默认允许
        if (_owner != null)
        {
            var pc = _owner.GetComponent<PlayerController>();
            if (pc != null && pc._stateMachine != null)
            {
                var state = pc._stateMachine.CurrentState;
                return state is IdleState or MoveState;
            }
        }
        return true;
    }

    private void TryFireInternal(Vector3 fireDirection)
    {
        // 扣除弹药
        _currentAmmo--;

        // 驱动动画
        _animDriver.SetTrigger("Fire");

        // 通知UI刷新弹匣
        OnAmmoChanged?.Invoke(_currentAmmo, _config.maxAmmo);

        // 执行子类具体射击逻辑
        PerformFire(fireDirection);

        // 设定下次可开火时间
        _nextFireTime = Time.time + _config.fireRate;
    }
    #endregion

    #region 换弹系统
    /// <summary>
    /// 尝试换弹（状态机在检测到换弹输入时调用）
    /// </summary>
    public void TryReload()
    {
        // 已在换弹中 / 弹匣已满//无备弹
        if (_isReloading || _currentAmmo == _config.maxAmmo|| ReserveAmmo <= 0) return;

        _isReloading = true;

        _animDriver.SetBool("IsReloading", true);

        // 使用协程等待动画播放完毕
        _reloadCoroutine = StartCoroutine(ReloadRoutine());
    }
    /// <summary>
    /// ****Warning 如果后续做切枪系统，需在切枪时检查 _isReloading 并强制中断协程 + 重置 Bool 参数，防止新武器继承换弹状态。
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReloadRoutine()
    {
        // 等待换弹动画时长
        yield return new WaitForSeconds(_config.reloadTime);

        if (!_isReloading || this == null || !gameObject.activeInHierarchy)
            yield break;
        int needed = _config.maxAmmo - _currentAmmo;
        // 计算实际需要填装的弹药量
        int available = ReserveAmmo;
        int actualReload = Mathf.Min(needed, available);

        if(actualReload <= 0)
        {
            CancelReload();
            yield break;
        }
        //从背包扣减弹药
        bool consumed = PlayerBackpack.Instance != null
           && PlayerBackpack.Instance.ConsumeItem(_config.DefaultAmmo, actualReload);

        if (!consumed)
        {
            CancelReload();
            yield break;
        }

        _currentAmmo += actualReload;

        // 重置状态
        _isReloading = false;
        _reloadCoroutine = null;
        _animDriver.SetBool("IsReloading", false);

        // 通知UI刷新
        OnAmmoChanged?.Invoke(_currentAmmo, _config.maxAmmo);
        OnReserveAmmoChanged?.Invoke(ReserveAmmo);
    }

    /// <summary>
    /// 切枪/死亡/禁用时调用，防止幽灵换弹
    /// </summary>
    public void CancelReload()
    {
        if (!_isReloading) return;
        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
        }
        _isReloading = false;
        _animDriver.SetBool("IsReloading", false);
    }
    #endregion

    #region 背包回调

    private void OnInventoryItemChanged(int slotIndex)
    {
        // 槽位变化时，重新查询当前弹药类型的真实数量
        int currentReserve = ReserveAmmo;

        // 只有数量真正发生变化时才通知UI，避免每个槽位变动都刷新
        if (currentReserve != _lastReserveAmmo)
        {
            _lastReserveAmmo = currentReserve;
            OnReserveAmmoChanged?.Invoke(currentReserve);
        }
    }

    #endregion


    /// <summary>
    /// 注入实时瞄准目标世界坐标获取器
    /// </summary>
    public void SetAimTargetProvider(Func<Vector3> provider)
    {
        _getAimTargetWorldPos = provider;
    }

    private Vector3 GetSafeFireDirection()
    {
        if (_getAimTargetWorldPos == null)
            return transform.forward;

        Vector3 targetWorld = _getAimTargetWorldPos.Invoke();
        Vector3 origin = _muzzlePoint != null ? _muzzlePoint.position : transform.position;

        // 将目标点拉到与枪口相同的高度
        targetWorld.y = origin.y;

        Vector3 direction = (targetWorld - origin).normalized;

        if (direction.sqrMagnitude < 0.001f)
            return transform.forward;

        return direction;
    }
    /// <summary>
    /// 抽象方法：具体武器的射击子类实现
    /// </summary>
    protected abstract void PerformFire(Vector3 fireDirection);
}