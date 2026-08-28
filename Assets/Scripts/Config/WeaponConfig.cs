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
    public string weaponName = "M1911";
    [Header("基础属性")]
    public int maxAmmo = 30;            // 弹匣容量
    public float reloadTime = 1.5f;     // 换弹时间
    public float damage = 10f;          // 伤害

    [Header("射击属性")]
    public float fireRate = 0.1f;       // 射击间隔(秒)
    public FireMode fireMode = FireMode.SemiAuto;

    [Header("弹药系统")]
    public string ammoType = "Ammo_9mm";   // 弹药类型ID
    public int initialReserveAmmo = 90;    // 仅用于游戏开始时初始化库存
}