/****************************************************
    文件：WeaponBase.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-27 14:35:15
	功能：武器基类
*****************************************************/

using System;
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

    // === 运行时状态 ===
    protected int _currentAmmo;         //当前弹匣内剩余弹药数
    protected float _fireCooldownTimer; //射击冷却倒计时（秒）
    protected bool _isReloading;        //换弹进行中标记
    protected float _reloadTimer;       //换弹剩余时间倒计时（秒）

    //玩家动画驱动器引用
    protected PlayerAnimatorDriver _animDriver;

    /// <summary>
    /// 弹药变化事件
    /// - 参数1: 当前弹匣弹药 (_currentAmmo)
    /// - 参数2: 弹匣最大容量 (_config.maxAmmo)
    /// - 触发时机: 射击扣弹后、换弹完成后
    /// - 订阅方: UI面板（更新弹药数字）
    /// </summary> 
    public event Action<int, int> OnAmmoChanged; // 当前弹药, 总弹药

    /// <summary>
    /// 武器持有者
    /// 用于子弹的伤害归属、友军判定、击杀统计
    /// </summary>
    protected GameObject _owner;

    /// <summary>
    /// 武器初始化入口
    /// PlayerController 装备武器时主动调用
    /// </summary>
    public virtual void Initialize(PlayerAnimatorDriver animDriver, GameObject owner)
    {
        _animDriver = animDriver;
        _owner = owner; 
        _currentAmmo = _config.maxAmmo;
    }

    public virtual void Tick(float deltaTime)
    {
        // 1. 处理射击冷却
        if (_fireCooldownTimer > 0)
            _fireCooldownTimer -= deltaTime;

        // 2. 处理换弹逻辑
        if (_isReloading)
        {
            _reloadTimer -= deltaTime;
            if (_reloadTimer <= 0)
            {
                FinishReload();
            }
        }
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
    public void Reload()
    {
        // 已在换弹中 / 弹匣已满
        if (_isReloading || _currentAmmo == _config.maxAmmo) return;

        _isReloading = true;
        _reloadTimer = _config.reloadTime;

        _animDriver.SetBool("IsReloading", true);
    }

    /// <summary>
    /// 换弹完成处理（由 Tick 中的倒计时触发）
    /// </summary>
    protected virtual void FinishReload()
    {
        _isReloading = false;
        _currentAmmo = _config.maxAmmo;

        _animDriver.SetBool("IsReloading", false);
        //更新订阅事件
        OnAmmoChanged?.Invoke(_currentAmmo, _config.maxAmmo);
    }

    /// <summary>
    /// 抽象方法：具体武器的射击子类实现
    /// </summary>
    protected abstract void PerformFire(Vector3 fireDirection);
}