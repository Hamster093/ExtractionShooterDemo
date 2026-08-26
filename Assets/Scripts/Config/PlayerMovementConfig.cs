/****************************************************
    文件：PlayerMovementConfig.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 00:50:45
	功能：玩家数据配置
*****************************************************/

using UnityEngine;

public class PlayerMovementConfig : MonoBehaviour
{


    [Header("基础移动")]
    [Tooltip("常规行走速度 (m/s)")]
    public float walkSpeed = 5f;

    [Tooltip("冲刺速度倍率")]
    public float sprintMultiplier = 1.6f;

    [Header("跳跃与重力")]
    [Tooltip("跳跃初始垂直速度 (m/s)")]
    public float jumpForce = 8f;

    [Tooltip("自定义重力值 (m/s²)")]
    public float gravity = -20f;

    [Tooltip("落地缓冲时间 (s)")]
    public float groundCheckSmoothTime = 0.1f;
}
