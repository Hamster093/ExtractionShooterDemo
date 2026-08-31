/****************************************************
    文件：AK47Config.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-31 12:48:00
	功能：AK47步枪专属配置
*****************************************************/

using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapon/AK47 Config")]
public class AK47Config : WeaponConfig
{

    [Header("全自动后坐力")]
    [Tooltip("垂直后坐力曲线(X=连发第N发, Y=上抬角度)")]
    public AnimationCurve verticalRecoilCurve = AnimationCurve.Linear(0, 0.5f, 30, 2f);

    [Tooltip("水平后坐力随机范围(度)")]
    public float horizontalRecoilRandom = 0.3f;

    [Header("扩散恢复")]
    [Tooltip("停止射击后每秒恢复的扩散值")]
    public float spreadRecoverySpeed = 8f;

    [Header("步枪弹道")]
    [Tooltip("伤害开始衰减的距离(米)")]
    public float rangeFalloffStart = 25f;

    [Tooltip("最大有效射程(米)，超出后伤害为0")]
    public float maxRange = 40f;
}