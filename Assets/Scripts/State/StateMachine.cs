/****************************************************
    文件：StateMachine.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 16:32:11
	功能：Nothing
*****************************************************/

using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public IState CurrentState { get; private set; }

    private readonly Dictionary<System.Type, IState> _states = new Dictionary<System.Type, IState>();

    /// <summary>
    /// 注册状态
    /// </summary>
    /// <param name="state"></param>
    public void RegisterState(IState state)
    {
        var type = state.GetType();
        if (_states.ContainsKey(type))
        {
            Debug.LogWarning($"[StateMachine] 状态 {type.Name} 已存在，跳过重复注册。");
            return;
        }
        _states[type] = state;
    }
    
    /// <summary>
    /// 跳转状态
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void ChangeState<T>() where T : IState
    {
        var targetType = typeof(T);

        if (!_states.TryGetValue(targetType, out var newState))
        {
            Debug.LogError($"[StateMachine] 未找到状态 {targetType.Name}，请确认已注册。");
            return;
        }

        if (CurrentState != null && CurrentState.GetType() == targetType) return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Tick(float deltaTime) => CurrentState?.Tick(deltaTime);


}