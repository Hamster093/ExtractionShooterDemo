/****************************************************
    文件：StatsConfig.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-08 16:24:10
	功能：Nothing
*****************************************************/

using UnityEngine;

[CreateAssetMenu(fileName = "NewStatsConfig", menuName = "DADI/StatsConfig")]
public class StatsConfig : ScriptableObject
{
    [Header("生存")]
    public int maxHealth = 100;
    public int armor = 0;
    public float armorDamageReduction = 0.5f;

    [Header("移动")]
    public float walkSpeed = 5f;
    public float sprintSpeedMultiplier = 1.6f;
    public float jumpForce = 8f;
    public float gravity = -20f;
    public float rollSpeed = 12f;
    public float rollDuration = 0.6f;
}