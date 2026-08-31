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
        // bullet.Speed = 50f; // 如果你有速度参数
    }
}