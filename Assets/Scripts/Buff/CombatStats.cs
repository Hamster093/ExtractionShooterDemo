/****************************************************
    文件：CombatStats.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026/9/30
	功能：战斗修正属性，参与伤害公式的因子集合 buff数量少 使用聚合值 要注意对称性配错 且无法做如（护盾强度只取最大值）功能
*****************************************************/

using UnityEngine;

/// <summary>
/// 战斗修正属性
/// 攻防双方共用一套字段：攻击方读「攻击侧」字段，受击方读「受击侧」字段
/// </summary>
public class CombatStats : MonoBehaviour
{
    // ===== 攻击侧修正（作为 creator 时生效）=====

    /// <summary>增伤倍率：0.2 表示最终伤害 ×1.2</summary>
    public float DamageBonus { get; private set; }

    /// <summary>暴击率：0~1，0.3 表示 30% 概率暴击</summary>
    public float CritChance { get; private set; }

    /// <summary>暴击伤害倍率：1.5 表示暴击造成 150% 伤害</summary>
    public float CritMultiplier { get; private set; } = 1.5f;


    // ===== 受击侧修正（作为 target 时生效）=====

    /// <summary>易伤：0.3 表示受到的最终伤害 ×1.3</summary>
    public float Vulnerability { get; private set; }

    /// <summary>减伤：0.2 表示受到的最终伤害 ×0.8</summary>
    public float DamageReduction { get; private set; }



    // ===== 修改接口（供 Buff 模块调用，支持正负）=====

    public void AddDamageBonus(float v) => DamageBonus += v;
    public void AddCritChance(float v) => CritChance = Mathf.Clamp01(CritChance + v);
    public void AddCritMultiplier(float v) => CritMultiplier += v;
    public void AddVulnerability(float v) => Vulnerability = Mathf.Max(-1f, Vulnerability + v);
    public void AddDamageReduction(float v) => DamageReduction = Mathf.Clamp(DamageReduction + v, -1f, 0.9f);
}
