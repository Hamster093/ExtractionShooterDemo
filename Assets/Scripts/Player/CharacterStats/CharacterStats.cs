/****************************************************
    文件：NewMonoBehaviourScript.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：#DATE#
	功能：角色基础属性管理类
*****************************************************/

using System;
using UnityEngine;

/// <summary>
/// 角色基础属性管理器
/// 统一管理生命值、护甲、抗性等，并作为 IDamageable 的实现载体
/// </summary>
public class CharacterStats : MonoBehaviour
{
    [Header("引用配置模板")]
    [SerializeField] private StatsConfig _config;

    // ===== 运行时属性（private set，外部只能通过方法修改）=====
    public int MaxHealth { get; private set; }
    public int Armor { get; private set; }
    public float ArmorDamageReduction { get; private set; }
    public float WalkSpeed { get; private set; }
    public float SprintSpeedMultiplier { get; private set; }
    public float JumpForce { get; private set; }
    public float Gravity { get; private set; }
    public float RollSpeed { get; private set; }
    public float RollDuration { get; private set; }


    private int _currentHealth;

    // 事件：供UI血条、受击特效、死亡动画等订阅
    //public event Action<int, int> OnHealthChanged; // 当生命值改变时current, max
   // public event Action<GameObject> OnDeath;       // killer

    // 事件（属性变化相关
    public event Action<int, int> OnMaxHealthChanged; // newMax, oldMax

    public bool IsAlive => _currentHealth > 0;

    private void Awake()
    {
        LoadFromConfig();
    }

    /// <summary>
    /// 从配置加载初始值（也可用于重置/Buff清除后恢复基础值）
    /// </summary>
    public void LoadFromConfig()
    {
        if (_config == null) return;

        MaxHealth = _config.maxHealth;
        Armor = _config.armor;
        ArmorDamageReduction = _config.armorDamageReduction;
        WalkSpeed = _config.walkSpeed;
        SprintSpeedMultiplier = _config.sprintSpeedMultiplier;
        JumpForce = _config.jumpForce;
        Gravity = _config.gravity;
        RollSpeed = _config.rollSpeed;
        RollDuration = _config.rollDuration;
    }

    // =============== 供 Buff/装备系统调用的修改接口 ===============

    /// <summary>
    /// 增加属性（Buff 用）
    /// </summary>
    public void AddArmor(int value) => Armor += value;
    public void AddWalkSpeed(float value) => WalkSpeed += value;

    public void AddMaxHealth(int value)
    {
        int oldMax = MaxHealth;
        MaxHealth += value;
        OnMaxHealthChanged?.Invoke(MaxHealth, oldMax);
        // ⚠️ 不再在这里 GetComponent<CharacterHealth>
        // 由 CharacterHealth 自己订阅 OnMaxHealthChanged 来同步
    }

    /// <summary>
    /// 计算实际受伤值 按护甲等级计算实际受伤值（离散阶梯减伤）
    /// 1级=20%, 2级=30%, 3级=40%, 4级=50%, 5级=60%, 6级=70%
    /// </summary>
    public int CalculateActualDamage(int rawDamage)
    {
        int level = Mathf.Clamp(Armor, 1, 6);

        // 等级1对应20%，之后每升1级增加10%
        float reductionPercent = 0.2f + (level - 1) * 0.1f;

        float multiplier = 1f - reductionPercent;
        return Mathf.Max(1, Mathf.RoundToInt(rawDamage * multiplier));
    }

}