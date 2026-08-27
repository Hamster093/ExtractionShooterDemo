/****************************************************
    文件：WeaponConfig.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-27 14:33:04
	功能：武器配置类
*****************************************************/

using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapon/Weapon Config")]
public class WeaponConfig :ScriptableObject
{
    public string weaponName = "M1911";
    [Header("Combat Stats")]
    public float fireRate = 0.1f;       // 射击间隔(秒)
    public int maxAmmo = 11;            // 弹匣容量
    public float reloadTime = 1.5f;     // 换弹时间
    public float damage = 10f;          // 伤害
}