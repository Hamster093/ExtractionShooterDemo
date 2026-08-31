/****************************************************
    文件：ProjectileBase.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-27 14:56:21
	功能：抛射物基类
*****************************************************/

using System;
using UnityEngine;
using UnityEngine.Pool;
using static UnityEngine.UI.GridLayoutGroup;

public abstract class ProjectileBase : MonoBehaviour
{
    [SerializeField] protected float _speed=5f;            //弹速
    [SerializeField] protected float _lifetime = 5f;    //子弹生命周期
    [SerializeField] protected int _damage=5;             //伤害
    [SerializeField] protected LayerMask _hitLayers;    //命中层

    private float _remainingLifetime;  //子弹剩余生命周期

    [NonSerialized] public ObjectPool<ProjectileBase> Pool;

    /// <summary>
    /// 发射方向（由武器在生成时设置）
    /// </summary>
    [NonSerialized] public Vector3 Direction;

    /// <summary>
    /// 伤害来源
    /// </summary>
    [NonSerialized] public GameObject Owner;


    internal void Initialize(GameObject owner, Vector3 spawnPosition, Vector3 fireDirection, int damage)
    {
        Owner = owner;
        Direction = fireDirection;
        _damage = damage;

        transform.SetPositionAndRotation(spawnPosition, Quaternion.LookRotation(fireDirection));
    }

    private void OnEnable()
    {
        //初始化生命周期
        _remainingLifetime = _lifetime;
    }

    private void Update()
    {
        _remainingLifetime -= Time.deltaTime;
        if (_remainingLifetime <= 0f)
        {
            DestroySelf();
            return;
        }
        Move(Time.deltaTime);
    }

    /// <summary>
    ///  子弹移动方法 可重写实现抛物线、追踪等行为
    /// </summary>
    /// <param name="deltaTime"></param>
    protected virtual void Move(float deltaTime)
    {
        transform.position += Direction.normalized * (_speed * deltaTime);
    }

    protected virtual void MoveWithSweep(float deltaTime)
    {
        Vector3 moveStep = Direction.normalized * (_speed * deltaTime);
        //向量模长 即(_speed * deltaTime)
        float stepDistance = moveStep.magnitude;

        if (stepDistance > 0.001f &&
            Physics.Raycast(transform.position, moveStep.normalized, out RaycastHit hit, stepDistance, _hitLayers))
        {
            // 忽略自身
            if (hit.collider.gameObject == Owner)
            {
                // 即使打到自己也要移动，否则子弹会卡在枪口
                transform.position += moveStep;
                return;
            }

            // 结算伤害
            if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(_damage, Owner);
            }
            else
            {
                Debug.LogWarning("没有在当前collider挂载的父物体上获取到IDamageable脚本");
            }

                // 命中后立即销毁，不再继续移动
                DestroySelf();
            return;
        }

        // 未命中，正常位移
        transform.position += moveStep;

        // 同步朝向让子弹模型始终朝前
        if (Direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(Direction);
    }
    

    protected virtual void DestroySelf()
    {
        //归还对象池 todo
        if (Pool != null)
            Pool.Release(this);
        else
        {
            gameObject.SetActive(false);
            Debug.LogWarning("对象池为空");
        }
    }

}