/****************************************************
    文件：M1911Config.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-31 12:48:00
	功能：M1911手枪专属配置
*****************************************************/

using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapon/M1911 Config")]
public class M1911Config : WeaponConfig
{
    [Header("手枪专属属性")]
    [Tooltip("腰射时的基础扩散角度(度)")]
    public float hipFireSpread = 1.5f;

    [Tooltip("开镜时扩散倍率(0~1)")]
    [Range(0f, 1f)]
    public float adsSpreadMultiplier = 0.05f;

    [Tooltip("切枪/拔枪时间(秒)")]
    public float equipTime = 0.4f;
}