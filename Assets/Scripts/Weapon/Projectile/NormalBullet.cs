/****************************************************
    文件：NormalBullet.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-27 18:07:57
	功能：普通有实体子弹
*****************************************************/

using UnityEngine;

public class NormalBullet : ProjectileBase
{
    /// <summary>
    /// 重写移动：确保方向归一化，避免斜向移动速度异常
    /// </summary>
    protected override void Move(float deltaTime)
    {
        MoveWithSweep(deltaTime);
    }

    /// <summary>
    /// 补全基类 TODO：安全归还对象池
    /// </summary>
    protected override void DestroySelf()
    {
        // 先执行基类的 CancelInvoke 和基础清理
        base.DestroySelf();
    }
}