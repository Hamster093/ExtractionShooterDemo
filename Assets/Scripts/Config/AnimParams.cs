/****************************************************
    文件：NewMonoBehaviourScript.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：#DATE#
	功能：Nothing
*****************************************************/

using UnityEngine;

public class AnimParams : MonoBehaviour 
{
    public const string Blend = "Blend";

    public const string IsGrounded = "IsGrounded";
    public const string JumpTrigger = "Jump";

    public const string isSprinting = "IsSprinting";
    public const string RollTrigger = "Roll";

    public const int HOLD_GUN_LAYER_INDEX = 1;//用于设置持枪状态的动画层权重 HoldGun Layer 在 Animator Controller 中的索引
}