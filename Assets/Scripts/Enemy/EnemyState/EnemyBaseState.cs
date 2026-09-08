/****************************************************
    文件：Enemy.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-07 14:39:38
	功能: 敌人AI
*****************************************************/

using UnityEngine;

public enum EnemyState
{
    Idle,
    Chase,
    Attack,
}

public class EnemyBaseState : MonoBehaviour , IState
{
    protected EnemyController _enemy;

    private void Awake()
    {
        _enemy = GetComponentInParent<EnemyController>();
    }

    /// <summary>
    /// 标记该状态是否已经被 Exit
    /// </summary>
    private bool _hasExited;

    public virtual void Enter()
    {
        _hasExited = false;
    }

    public virtual void Exit()
    {
        _hasExited = true;
    }

    public virtual void Tick(float deltaTime)
    {
        if (_hasExited) return; //已退出的状态不再执行(防御逻辑
        OnTick(deltaTime);
    }

    /// <summary>
    /// 子类重写此方法
    /// </summary>
    protected virtual void OnTick(float deltaTime) { }

}