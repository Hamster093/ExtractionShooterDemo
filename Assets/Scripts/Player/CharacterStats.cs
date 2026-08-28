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
public class CharacterStats : MonoBehaviour, IDamageable
{
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _armor = 0;

    private int _currentHealth;

    // 事件：供UI血条、受击特效、死亡动画等订阅
    public event Action<int, int> OnHealthChanged; // current, max
    public event Action<GameObject> OnDeath;       // killer

    public bool IsAlive => _currentHealth > 0;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int amount, GameObject source)
    {
        if (!IsAlive) return;

        // 伤害计算公式集中管理
        int actualDamage = Mathf.Max(1, amount - _armor);

        _currentHealth -= actualDamage;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            OnDeath?.Invoke(source);
        }
    }

    /// <summary>
    /// 治疗
    /// </summary>
    public void Heal(int amount)
    {
        if (!IsAlive) return;
        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
}