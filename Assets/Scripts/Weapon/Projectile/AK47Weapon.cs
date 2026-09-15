/****************************************************
    文件：AK47Weapon.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-31 12:15:00
	功能：AK47步枪类（实体子弹 + 对象池）
*****************************************************/

using UnityEngine;
using UnityEngine.Pool;

public class AK47Weapon : WeaponBase
{
    [Header("=== 弹道配置 ===")]
    [SerializeField] private ProjectileBase _bulletPrefab; // 复用 M1911 的子弹

    [Header("=== 对象池 ===")]
    [SerializeField] private ObjectPool<ProjectileBase> _bulletPool;

    [Header("=== 对象池配置 ===")]
    [SerializeField] private int _poolDefaultCapacity = 30;
    [SerializeField] private int _poolMaxSize = 150;

    private void Awake()
    {
        // 初始化对象池（和 M1911 完全一致）
        _bulletPool = new ObjectPool<ProjectileBase>(
            createFunc: () => Instantiate(_bulletPrefab),
            actionOnGet: (b) => b.gameObject.SetActive(true),
            actionOnRelease: (b) => b.gameObject.SetActive(false),
            actionOnDestroy: (b) => Destroy(b.gameObject),
            collectionCheck: true,
            defaultCapacity: _poolDefaultCapacity,
            maxSize: _poolMaxSize
        );
    }

    protected override void PerformFire(Vector3 fireDirection)
    {
        // 从对象池获取子弹
        ProjectileBase bullet = _bulletPool.Get();
        bullet.Pool = _bulletPool;

        // 初始化子弹
        bullet.Initialize(_owner, _muzzlePoint.position, fireDirection, _config.damage);
    }

    /// <summary>
    /// 扩散/后坐力参数统一从 AK47Config 读取（AK47 专属配置管理）
    /// </summary>
    private AK47Config Ak => _config as AK47Config;

    protected override float GetMaxSpread() => Ak != null ? Ak.maxSpread : base.GetMaxSpread();
    protected override float GetSpreadPerShot() => Ak != null ? Ak.spreadPerShot : base.GetSpreadPerShot();
    protected override float GetSpreadRecoverySpeed() => Ak != null ? Ak.spreadRecoverySpeed : base.GetSpreadRecoverySpeed();

    /// <summary>
    /// 垂直后坐力：按连发第 N 发查曲线
    /// </summary>
    protected override float GetVerticalRecoil(int shotIndex)
        => Ak != null && Ak.verticalRecoilCurve != null ? Ak.verticalRecoilCurve.Evaluate(shotIndex) : 0f;

    /// <summary>
    /// 水平后坐力：±随机范围
    /// </summary>
    protected override float GetHorizontalRecoilRandom()
        => Ak != null ? Ak.horizontalRecoilRandom : 0f;
}