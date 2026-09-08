/****************************************************
    文件：CharacterHealth.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-08 16:31:35
	功能：角色血量管理，实现 IDamageable 接口对接子弹系统
*****************************************************/

using System;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CharacterHealth : MonoBehaviour, IDamageable
{
    private CharacterStats _stats;

    /// <summary>
    /// 当前血量
    /// </summary>
    public int CurrentHealth { get; private set; }

    /// <summary>
    /// 是否已死亡
    /// </summary>
    public bool IsDead { get; private set; }

    // =============== 事件（供 UI、特效等外部系统订阅） ===============

    /// <summary>
    /// 受伤事件：(当前血量, 最大血量, 实际伤害值)
    /// </summary>
    public event Action<int, int, int> OnDamaged;

    /// <summary>
    /// 死亡事件
    /// </summary>
    public event Action OnDeath;

    /// <summary>
    /// 血量变化事件（治疗也触发）：(当前血量, 最大血量)
    /// </summary>
    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        _stats = GetComponent<CharacterStats>();
    }

    private void Start()
    {
        // 满血初始化
        CurrentHealth = _stats.MaxHealth;
    }

    private void OnEnable()
    {
        _stats.OnMaxHealthChanged += HandleMaxHealthChanged;
    }

    private void OnDisable()
    {
        _stats.OnMaxHealthChanged -= HandleMaxHealthChanged;
    }

    private void HandleMaxHealthChanged(int newMax, int oldMax)
    {
        // 按比例保持血量，或直接补满差值
        int delta = newMax - oldMax;
        CurrentHealth = Mathf.Min(CurrentHealth + delta, newMax);
        OnHealthChanged?.Invoke(CurrentHealth, newMax);
    }

    // =============== IDamageable 接口实现 ===============

    public void TakeDamage(int rawDamage, GameObject attacker)
    {
        if (IsDead) return;

        // 通过属性系统计算护甲减伤后的实际伤害
        int actualDamage = _stats.CalculateActualDamage(rawDamage);

        // 扣血
        CurrentHealth = Mathf.Clamp(CurrentHealth - actualDamage, 0, _stats.MaxHealth);

        Debug.Log($"[{gameObject.name}] 原始伤害:{rawDamage} → 实际伤害:{actualDamage}，" +
                  $"剩余血量:{CurrentHealth}/{_stats.MaxHealth}");

        // 触发受伤事件 → 飘字、受击特效、UI刷新 等
        OnDamaged?.Invoke(CurrentHealth, _stats.MaxHealth, actualDamage);

        // 判断死亡
        if (CurrentHealth <= 0)
        {
            Die(attacker);
        }
    }

    // =============== 治疗 ===============

    public void Heal(int amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, _stats.MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, _stats.MaxHealth);
    }

    // =============== 内部方法 ===============

    /// <summary>
    /// 当 MaxHealth 被 Buff 修改时调用，保持血量不超过新上限
    /// </summary>
    public void OnMaxHealthChanged()
    {
        CurrentHealth = Mathf.Min(CurrentHealth, _stats.MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, _stats.MaxHealth);
    }

    private void Die(GameObject attacker)
    {
        IsDead = true;
        Debug.Log($"[{gameObject.name}] 被 {attacker?.name} 击杀！");

        OnDeath?.Invoke();

        // TODO: 播放死亡动画、禁用控制、掉落物品等
        // GetComponent<Animator>()?.SetTrigger("Die");
        // GetComponent<PlayerInputHandler>()?.enabled = false;
    }
}