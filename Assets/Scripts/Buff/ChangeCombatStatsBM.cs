/****************************************************
    文件：ChangeCombatStatsBM.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026/9/30 19:29:40
	功能：Buff模块——修改战斗修正属性（增伤、暴击、易伤等）
*****************************************************/

using UnityEngine;

[CreateAssetMenu(fileName = "_ChangeCombatStatsBM", menuName = "BuffSystem/ChangeCombatStatsBM", order = 1)]
public class ChangeCombatStatsBM : BaseBuffModule
{
    [Header("攻击侧")]
    public float damageBonusDelta;      // 增伤倍率增量，+0.2 表示 +20%
    public float critChanceDelta;       // 暴击率增量，+0.15 表示 +15%
    public float critMultiplierDelta;   // 暴击伤害倍率增量

    [Header("受击侧")]
    public float vulnerabilityDelta;    // 易伤增量，+0.3 表示受伤 +30%
    public float damageReductionDelta;  // 减伤增量，+0.2 表示受伤 -20%

    public override void Apply(BuffInfo buffInfo, DamageInfo damageInfo = null)
    {
        if (buffInfo?.target == null) return;

        var cs = buffInfo.target.GetComponent<CombatStats>();
        if (cs == null)
        {
            Debug.LogWarning($"[ChangeCombatStatsBM] {buffInfo.target.name} 缺少 CombatStats 组件");
            return;
        }

        if (damageBonusDelta != 0f) cs.AddDamageBonus(damageBonusDelta);
        if (critChanceDelta != 0f) cs.AddCritChance(critChanceDelta);
        if (critMultiplierDelta != 0f) cs.AddCritMultiplier(critMultiplierDelta);
        if (vulnerabilityDelta != 0f) cs.AddVulnerability(vulnerabilityDelta);
        if (damageReductionDelta != 0f) cs.AddDamageReduction(damageReductionDelta);
    }
}
