/****************************************************
    文件：RifleWeapon.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-27 15:18:49
	功能：M1911枪械类
*****************************************************/

using UnityEngine;
using UnityEngine.Pool;

public class M1911Weapon : WeaponBase
{
    [Header("=== 弹道配置 ===")]
    [SerializeField] private ProjectileBase _bulletPrefab; // 引用子弹预制体

    [Header("=== 对象池 ===")]
    [SerializeField] private ObjectPool<ProjectileBase> _bulletPool;

    [Header("=== 对象池配置 ===")]
    [SerializeField] private int _poolDefaultCapacity = 20;
    [SerializeField] private int _poolMaxSize = 100;

    private void Awake()
    {
        _bulletPool = new ObjectPool<ProjectileBase>(
            createFunc: () => Instantiate(_bulletPrefab),           //池子用完新建实例
            actionOnGet: (b) => b.gameObject.SetActive(true),       //取出对象时自动激活
            actionOnRelease: (b) => b.gameObject.SetActive(false),  //放回池中时自动禁用
            actionOnDestroy: (b) => Destroy(b.gameObject),          //销毁超出容量的 GameObject
            collectionCheck: true,          // 开发阶段开启，防止重复 Release
            defaultCapacity: _poolDefaultCapacity,
            maxSize: _poolMaxSize
        );
    }

    protected override void PerformFire(Vector3 fireDirection)
    {
        ProjectileBase bullet;

        if (_bulletPool != null)
        {
            bullet = _bulletPool.Get();
            bullet.Pool = _bulletPool;
        }
        else
        bullet = Instantiate(_bulletPrefab);

        bullet.Owner = _owner; // 标记伤害归属
        bullet.Direction = fireDirection;
        bullet.transform.position = _muzzlePoint.position;
        bullet.transform.rotation = Quaternion.LookRotation(fireDirection);

    }
}