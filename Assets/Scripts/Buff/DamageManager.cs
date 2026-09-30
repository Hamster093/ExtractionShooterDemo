/****************************************************
    文件：DamageManager.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-30 15:58:34
	功能：伤害管理器，负责统一处理伤害提交，并触发相关Buff的伤害回调
*****************************************************/

using UnityEngine;

public class DamageManager : MonoBehaviour 
{
    public static DamageManager Instance;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 伤害加工：触发来源 OnHit 与目标 OnBehurt
    /// 调用后 damageInfo.damage 可能已被 Buff 修改
    /// </summary>
    public void ProcessDamage(DamageInfo damageInfo)
    {
        var creatorBuffHandler = damageInfo.creator != null ? damageInfo.creator.GetComponent<BuffHandler>() : null;
        var targetBuffHandler = damageInfo.target != null ? damageInfo.target.GetComponent<BuffHandler>() : null;

        if (creatorBuffHandler != null)
        {
            foreach (var b in creatorBuffHandler.buffList)
                b.buffData.OnHit?.Apply(b, damageInfo);
        }

        if (targetBuffHandler != null)
        {
            foreach (var b in targetBuffHandler.buffList)
                b.buffData.OnBehurt?.Apply(b, damageInfo);
        }
    }

    /// <summary>
    /// 致命前的回调：给免死Buff机会
    /// </summary>
    public void ProcessBekill(DamageInfo damageInfo)
    {
        var targetBuffHandler = damageInfo.target != null? damageInfo.target.GetComponent<BuffHandler>() : null;
        if (targetBuffHandler == null) return;

        foreach (var b in targetBuffHandler.buffList)
            b.buffData.OnBekill?.Apply(b, damageInfo);
    }

    /// <summary>
    /// 确认击杀后：触发来源 OnKill
    /// </summary>
    public void ProcessKill(DamageInfo damageInfo)
    {
        var creatorBuffHandler = damageInfo.creator != null
            ? damageInfo.creator.GetComponent<BuffHandler>() : null;
        if (creatorBuffHandler == null) return;

        foreach (var b in creatorBuffHandler.buffList)
            b.buffData.OnKill?.Apply(b, damageInfo);
    }

}