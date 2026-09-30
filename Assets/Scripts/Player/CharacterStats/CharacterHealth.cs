/****************************************************
    文件：CharacterHealth.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-08 16:31:35
	功能：角色血量管理，实现 IDamageable 接口对接子弹系统(
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
    /// <summary>
    /// 被攻击事件：参数是攻击者（可能为 null，比如环境伤害）
    /// 供 AI 感知"谁打了我、在哪"使用
    /// </summary>
    public event Action<GameObject> OnHitByAttacker;

    private void Awake()
    {
        _stats = GetComponent<CharacterStats>();

    }

    private void Start()
    {
        // 满血初始化；跨场景进入时恢复为 PlayerStateData 保存的血量（ConsumeHealth 返回 -1 表示无存档/已消费）
        int restored = PlayerStateData.ConsumeHealth();
        CurrentHealth = restored >= 0 ? restored : _stats.MaxHealth;
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

        //  构造伤害信息，走Buff系统加工
        DamageInfo info = new DamageInfo
        {
            creator = attacker,
            target = gameObject,
            damage = rawDamage
        };
        if (DamageManager.Instance != null)
            DamageManager.Instance.ProcessDamage(info);

        DamageResult result = DamagePipeline.Calculate(info);

        // 扣血
        CurrentHealth = Mathf.Clamp(CurrentHealth - result.finalDamage, 0, _stats.MaxHealth);

        Debug.Log($"[{gameObject.name}] 原始:{rawDamage} → 最终:{result.finalDamage}" +
               (result.isCritical ? " [暴击]" : ""));

        // 触发受伤事件 → 飘字、受击特效、UI刷新 等
        OnDamaged?.Invoke(CurrentHealth, _stats.MaxHealth, result.finalDamage);

        // AI 感知用的受伤事件 EnemyController 订阅
        OnHitByAttacker?.Invoke(attacker);

        // 判断死亡
        if (CurrentHealth <= 0)
        {
            // 致命前回调：免死Buff可以在这里回血、把 info.damage 改成 0 等
            if (DamageManager.Instance != null)
                DamageManager.Instance.ProcessBekill(info);

            //如果被免死Buff救回来了，就不死
            if (CurrentHealth <= 0)
            {
                if (DamageManager.Instance != null)
                    DamageManager.Instance.ProcessKill(info);

                Die(attacker);
            }
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
    }
}