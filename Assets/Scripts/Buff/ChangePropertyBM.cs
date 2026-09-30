/****************************************************
    文件：ChangePropertyBM.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026/9/30 15:39:54
	功能：Buff模块——修改角色属性（CharacterStats）
*****************************************************/


using UnityEngine;

/// <summary>
/// 属性变更Buff模块
/// 对目标身上的 CharacterStats 各属性做增量修改。
///
/// 使用约定：
///   · OnCreate 挂一个本模块资产，数值填正数（Buff生效时加成）
///   · OnRemove 挂另一个本模块资产，数值填相同绝对值但符号相反（Buff移除时还原）
///   · 用同一BuffData的叠加/移除逻辑保证正负对称即可
///
/// 若某字段数值为 0，则该字段跳过，不做任何修改。
/// </summary>
[CreateAssetMenu(fileName = "_ChangeStatsBM", menuName = "BuffSystem/ChangeStatsBM", order = 1)]
public class ChangeStatsBM : BaseBuffModule
{
    [Header("生命与护甲")]
    /// <summary>最大生命值增量（正为加，负为减）</summary>
    public int maxHealthDelta;

    /// <summary>护甲等级增量</summary>
    public int armorDelta;

    [Header("移动相关")]
    /// <summary>行走速度增量</summary>
    public float walkSpeedDelta;

    /// <summary>冲刺速度倍率增量</summary>
    public float sprintSpeedMultiplierDelta;

    /// <summary>跳跃力增量</summary>
    public float jumpForceDelta;

    /// <summary>重力增量</summary>
    public float gravityDelta;

    /// <summary>翻滚速度增量</summary>
    public float rollSpeedDelta;

    /// <summary>翻滚持续时间增量</summary>
    public float rollDurationDelta;

    /// <summary>
    /// 应用属性变更
    /// </summary>
    /// <param name="buffInfo">Buff运行时信息，target 上需挂有 CharacterStats</param>
    /// <param name="damageInfo">伤害信息（此模块中未使用）</param>
    public override void Apply(BuffInfo buffInfo, DamageInfo damageInfo = null)
    {
        if (buffInfo == null || buffInfo.target == null) return;

        var stats = buffInfo.target.GetComponent<CharacterStats>();
        if (stats == null)
        {
            Debug.LogWarning($"[ChangeStatsBM] 目标 {buffInfo.target.name} 上没有 CharacterStats 组件");
            return;
        }

        if (maxHealthDelta != 0) stats.AddMaxHealth(maxHealthDelta);
        if (armorDelta != 0) stats.AddArmor(armorDelta);
        if (walkSpeedDelta != 0f) stats.AddWalkSpeed(walkSpeedDelta);
        if (sprintSpeedMultiplierDelta != 0f) stats.AddSprintSpeedMultiplier(sprintSpeedMultiplierDelta);
        if (jumpForceDelta != 0f) stats.AddJumpForce(jumpForceDelta);
        if (gravityDelta != 0f) stats.AddGravity(gravityDelta);
        if (rollSpeedDelta != 0f) stats.AddRollSpeed(rollSpeedDelta);
        if (rollDurationDelta != 0f) stats.AddRollDuration(rollDurationDelta);
    }
}
