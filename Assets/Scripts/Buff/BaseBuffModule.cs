/****************************************************
    文件：BaseBuffModule.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026/9/30 15:07:16
	功能：Buff基类
*****************************************************/

using UnityEngine;

public abstract class BaseBuffModule : ScriptableObject
{
    public abstract void Apply(BuffInfo buffInfo, DamageInfo damageInfo=null);
}