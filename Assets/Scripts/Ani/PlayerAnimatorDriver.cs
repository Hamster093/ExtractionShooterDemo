/****************************************************
    文件：PlayerAnimatorDriver.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 18:35:32
	功能：玩家动画驱动器
*****************************************************/

using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorDriver : MonoBehaviour
{
    private Animator _animator;
    // 缓存参数哈希值
    private int _moveBlendHash;

    private const float BLEND_IDLE = 0f;
    private const float BLEND_WALK = 1f;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _moveBlendHash = Animator.StringToHash(AnimParams.Blend);
    }

    /// <summary>
    /// 根据是否有输入切换动画状态
    /// </summary>
    /// <param name="hasInput">是否有输入</param>
    /// <param name="immediate">是否立即过度</param>
    public void SetMoveState(bool hasInput, bool immediate = false)
    {
        float targetValue = hasInput ? BLEND_WALK : BLEND_IDLE;
        if (_animator == null) return;
        //是否平滑过度
        if (immediate)
        {
            _animator.SetFloat(_moveBlendHash, targetValue);
        }
        else
        {
            _animator.SetFloat(_moveBlendHash, targetValue, 0.1f, Time.deltaTime);
        }
    }

}