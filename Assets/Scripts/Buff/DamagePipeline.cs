/****************************************************
    文件：DamagePipeline.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026/9/30 19:27:20
	功能：伤害计算管线，把基础伤害经过各修正因子组合成最终伤害
*****************************************************/

using System;
using UnityEngine;

/// <summary>伤害计算结果</summary>
public struct DamageResult
{
    public int finalDamage;   // 最终扣血量
    public bool isCritical;   // 是否暴击
}

public static class DamagePipeline
{
    /// <summary>
    /// 计算最终伤害
    /// 顺序：基础伤害 → 攻击方增伤 → 暴击判定 → 受击方易伤 → 受击方减伤 → 护甲减伤
    /// </summary>
    public static DamageResult Calculate(DamageInfo info)
    {
        float dmg = info.damage;

        var attackerCS = info.creator != null ? info.creator.GetComponent<CombatStats>() : null;
        var defenderCS = info.target != null ? info.target.GetComponent<CombatStats>() : null;
        var defenderStats = info.target != null ? info.target.GetComponent<CharacterStats>() : null;

        // 1. 攻击方增伤
        if (attackerCS != null)
            dmg *= 1f + attackerCS.DamageBonus;

        // 2. 暴击判定（每次伤害独立判定，放在攻击侧）
        bool isCrit = false;
        if (attackerCS != null && attackerCS.CritChance > 0f && UnityEngine.Random.value < attackerCS.CritChance)
        {
            isCrit = true;
            dmg *= attackerCS.CritMultiplier;
        }

        // 3. 受击方易伤
        if (defenderCS != null)
            dmg *= 1f + defenderCS.Vulnerability;

        // 4. 受击方减伤
        if (defenderCS != null)
            dmg *= 1f - defenderCS.DamageReduction;

        // 5. 护甲减伤（原有的离散阶梯减伤）
        int final;
        if (defenderStats != null)
            final = defenderStats.CalculateActualDamage(Mathf.RoundToInt(dmg));
        else
            final = Mathf.Max(1, Mathf.RoundToInt(dmg));

        return new DamageResult { finalDamage = final, isCritical = isCrit };
    }
}