/****************************************************
    文件：IDamage.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-27 15:15:53
	功能：受伤接口
*****************************************************/

using UnityEngine;

/// <summary>
/// 任何能被攻击的实体都必须实现此接口
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 接受伤害
    /// </summary>
    /// <param name="amount">原始伤害值（未经护甲/抗性计算）</param>
    /// <param name="source">伤害来源（用于击杀判定、仇恨系统、友军过滤）</param>
    void TakeDamage(int amount, GameObject source);

    /// <summary>
    /// 当前是否存活（避免对已死亡对象重复结算）
    /// </summary>
    bool IsAlive { get; }
}