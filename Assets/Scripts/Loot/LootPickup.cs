/****************************************************
    文件：LootPickup.cs
    作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-31 17:20:00
    功能：战利品拾取物（玩家靠近时在物体旁显示交互按钮，支持F键/鼠标点击触发）
*****************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// 战利品拾取物
/// 玩家进入触发范围后在物体旁显示“战利品”按钮，离开后隐藏
/// 交互方式：鼠标点击按钮，或玩家在范围内按下 F 键（两者都触发同一个点击事件）
/// </summary>
public class LootPickup : InteractableBase
{

    private ItemContainer _container;
    private bool _dataSent = false;//是否已经发送数据到UI


    public void SetContainer(ItemContainer container)
    {
        _container = container;
        _dataSent = false;
    }

  
    protected override void OnInteract()
    {
        if (!_dataSent)
        {
            UIController.Instance.OpenLoot(_container);
            _dataSent = true;
        }
        else
        {
            UIController.Instance.OpenLoot();
        }
    }
}