/****************************************************
    文件：PlayerEvents.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-03 20:22:04
	功能：玩家状态事件广播
*****************************************************/

using UnityEngine;

using System;

/// <summary>
/// 玩家状态事件广播器
/// 只负责定义和触发玩家相关事件
/// </summary>
public class PlayerEvents
{
    //使用静态单例
    public static PlayerEvents Instance { get; private set; } = new PlayerEvents();

    public event Action<int, WeaponBase> OnWeaponChanged;//武器栏位切换或装备新武器时触发
    public event Action<bool> OnHasWeaponChanged;// 玩家持有武器状态改变时触发（用于更新持枪动画/UI）


    public event Action<int, int> OnCurrentAmmoChanged;      // 当前弹匣弹药变化时触发(当前弹匣, 最大弹匣)
    public event Action<int> OnReserveAmmoChanged;           // 备弹量变化时触发(备弹量)


    // ---（仅允许 PlayerController 内部调用）---

    internal void TriggerWeaponChanged(int slotIndex, WeaponBase weapon)=> OnWeaponChanged?.Invoke(slotIndex, weapon);

    internal void TriggerHasWeaponChanged(bool hasWeapon)=> OnHasWeaponChanged?.Invoke(hasWeapon);

    internal void TriggerCurrentAmmoChanged(int current, int max)
       => OnCurrentAmmoChanged?.Invoke(current, max);

    internal void TriggerReserveAmmoChanged(int reserve)
        => OnReserveAmmoChanged?.Invoke(reserve);
}