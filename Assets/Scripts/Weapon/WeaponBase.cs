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
using static UnityEngine.UI.GridLayoutGroup;
/// <summary>
/// 武器系统抽象基类
/// 职责：管理弹药、冷却、换弹等通用逻辑，并将具体射击行为延迟到子类实现
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    [Tooltip("武器静态配置")]
    [SerializeField] protected WeaponConfig _config;
    public WeaponConfig Config => _config;

    //查询背包内子弹携带量
    public int ReserveAmmo => PlayerBackpack.Instance?.GetItemCount(_config.ammoType) ?? 0;

    protected int _currentAmmo;         //当前弹匣内剩余弹药数
    protected float _fireCooldownTimer; //射击冷却倒计时（秒）
    protected bool _isReloading;        //换弹进行中标记

    public int CurrentAmmo => _currentAmmo;
    public int MaxAmmo => _config.maxAmmo;
    public bool IsReloading => _isReloading;

    //玩家动画驱动器引用
    protected PlayerAnimatorDriver _animDriver;

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

    /// <summary>
    /// 武器持有者
    /// 用于子弹的伤害归属、友军判定、击杀统计
    /// </summary>
    protected GameObject _owner;

    private Coroutine _reloadCoroutine; // 缓存句柄用于安全中断

    /// <summary>
    /// 武器初始化入口
    /// PlayerController 装备武器时主动调用
    /// </summary>
    public virtual void Initialize(PlayerAnimatorDriver animDriver, GameObject owner)
    {
        OnAmmoChanged?.Invoke(_currentAmmo, _config.maxAmmo);
        OnReserveAmmoChanged?.Invoke(ReserveAmmo);

        _animDriver = animDriver;
        _owner = owner; 
        _currentAmmo = _config.maxAmmo;

        var inv = PlayerBackpack.Instance;
        // 订阅库存变化，当该弹药类型数量变动时通知UI
        if (inv != null)
            inv.OnItemChanged += OnInventoryItemChanged;
        // 初始化备弹
        if (inv != null && inv.GetItemCount(_config.ammoType) == 0)
            inv.SetItem(_config.ammoType, _config.initialReserveAmmo);
        Debug.Log("初始化9mm备弹数量" + PlayerBackpack.Instance.GetItemCount(_config.ammoType));
    }

    private void OnDestroy()
    {
        if (PlayerBackpack.Instance != null)
            PlayerBackpack.Instance.OnItemChanged -= OnInventoryItemChanged;
    }

    public virtual void Tick(float deltaTime)
    {
        // 1. 处理射击冷却
        if (_fireCooldownTimer > 0)
            _fireCooldownTimer -= deltaTime;

    }

    /// <summary>
    /// 尝试开火
    /// </summary>
    public bool TryFire(Vector3 fireDirection)
    {
        // 换弹中 / 冷却未结束 / 弹药耗尽
        if (_isReloading || _fireCooldownTimer > 0 || _currentAmmo <= 0)
            return false;

        // 扣除弹药，重置冷却
        _currentAmmo--;
        _fireCooldownTimer = _config.fireRate;

        // 触发表现层
        _animDriver.SetTrigger("Fire");
        OnAmmoChanged?.Invoke(_currentAmmo, _config.maxAmmo);

        // 执行具体射击逻辑
        PerformFire(fireDirection);
        return true;
    }

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
        StartCoroutine(ReloadRoutine());
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
        //从背包扣减弹药
        bool consumed = PlayerBackpack.Instance.ConsumeItem(_config.ammoType, needed);

        if (!consumed)
        {
            CancelReload();
            yield break;
        }

        _currentAmmo += needed;

        // 重置状态
        _isReloading = false;
        _reloadCoroutine = null;
        _animDriver.SetBool("IsReloading", false);

        // 通知UI刷新
        OnAmmoChanged?.Invoke(_currentAmmo, _config.maxAmmo);
        OnReserveAmmoChanged?.Invoke(ReserveAmmo);
    }

    //背包（子弹）库存变化时回调
    private void OnInventoryItemChanged(string itemId, int newAmount)
    {
        if (itemId == _config.ammoType)
            OnReserveAmmoChanged?.Invoke(newAmount);
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

    private void OnDisable() => CancelReload();
    /// <summary>
    /// 抽象方法：具体武器的射击子类实现
    /// </summary>
    protected abstract void PerformFire(Vector3 fireDirection);
}