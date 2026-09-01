/****************************************************
    文件：WeaponConfig.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-27 14:33:04
	功能：武器配置类
*****************************************************/

using UnityEngine;

public enum FireMode { SemiAuto, FullAuto }//半自动/全自动

[CreateAssetMenu(menuName = "Game/Weapon/Weapon Config")]
public class WeaponConfig :ScriptableObject
{
    public string weaponName = "New Weapon";
    [Header("基础属性")]
    public int maxAmmo = 30;            // 弹匣容量
    public float reloadTime = 1.5f;     // 换弹时间
    public int damage = 10;             // 伤害

    [Header("射击属性")]
    public float fireRate = 0.1f;       // 射击间隔(秒)
    public FireMode fireMode = FireMode.SemiAuto;

    [Header("弹药系统")]
    public int DefaultAmmo;   // 弹药类型ID
    public int initialReserveAmmo = 90;    // 仅用于游戏开始时初始化库存

    [Header("全自动专属（仅 FullAuto 生效）")]
    [Tooltip("连续射击时的最大扩散角度(度)")]
    public float maxSpread = 3f;
    [Tooltip("每次射击增加的扩散值")]
    public float spreadPerShot = 0.5f;
}