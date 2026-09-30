/****************************************************
    文件：BuffDesign.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026/9/30 15:06:36
	功能：Buff系统与伤害、属性相关的基础数据结构定义
*****************************************************/

using System;
using UnityEngine;

/// <summary>
/// Buff持续时间更新方式
/// 当目标身上已经存在相同Buff时，决定如何更新其持续时间
/// </summary>
public enum BuffUpdateTimeEnum
{
    Add,// 累加：在原有持续时间基础上增加新的持续时间
    Replace,// 保持：不改变原有持续时间
    Keep// 保持：不改变原有持续时间
}

/// <summary>
/// Buff移除时层数更新方式
/// 当移除一层Buff时，决定如何处理当前叠加层数
/// </summary>
public enum BuffRemoveStackUpdateEnum
{
    Clear,// 清除：直接清空所有层数
    Reduce// 减少：当前层数减一
}

/// <summary>
/// Buff运行时信息
/// 记录一个Buff实例在游戏运行时的状态数据
/// </summary>
public class BuffInfo
{
    public BuffData buffData;// Buff配置数据
    public GameObject creator;// Buff创建者（施加Buff的GameObject）
    public GameObject target;// Buff目标（被施加Buff的GameObject）
    public float durationTimer;// 持续时间计时器，用于记录剩余持续时间
    public float tickTimer;// 触发间隔计时器，用于周期性触发Buff效果
    public int curStack=1;// 当前叠加层数，默认1层
}

/// <summary>
/// 伤害信息
/// 用于在伤害计算与传递过程中携带相关数据
/// </summary>
public class DamageInfo
{
    public GameObject creator;// 伤害来源
    public GameObject target;// 伤害目标
    public float damage;// 伤害数值
}

/// <summary>
/// 测试角色属性数据
/// 可序列化，便于在Inspector中配置或保存
/// </summary>
[Serializable]
public class Property
{
    public float hp;// 生命值
    public float speed;// 移动速度
    public float atk;// 攻击力
}
