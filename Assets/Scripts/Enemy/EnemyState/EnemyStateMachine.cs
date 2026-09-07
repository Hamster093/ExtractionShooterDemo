/****************************************************
    文件：EnemyStateMachine.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-07 15:17:02
	功能：状态机控制器
*****************************************************/

using UnityEngine;

public class EnemyStateMachine
{
    public EnemyBaseState CurrentState { get; private set; }

    public void ChangeState(EnemyBaseState newState)
    {
        if (CurrentState == newState) return; // 防止重复切换到同一状态

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState?.Enter();
    }

    public void Tick(float deltaTime)
    {
        CurrentState?.Tick(deltaTime);
    }
}