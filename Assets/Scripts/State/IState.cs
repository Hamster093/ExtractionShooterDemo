/****************************************************
    文件：IState.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 16:32:01
	功能：状态接口。
*****************************************************/

using UnityEngine;

public interface IState
{
    void Enter();
    void Tick(float deltaTime);
    void Exit();
}