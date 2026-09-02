/****************************************************
    文件：HotbarPanel.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-03 20:10:57
	功能：快捷栏面板
*****************************************************/

using UnityEngine;

public class HotbarPanel : BaseUIPanel
{
    public override UIPriority Priority => UIPriority.Hotbar;

    private void OnEnable()
    {
        // 订阅快捷栏更新事件
        //HotbarManager.Instance.OnHotbarUpdated += UpdateHotbarUI;
    }

    private void OnDestroy()
    {
        // 取消订阅快捷栏更新事件
        //HotbarManager.Instance.OnHotbarUpdated -= UpdateHotbarUI;
    }
}
