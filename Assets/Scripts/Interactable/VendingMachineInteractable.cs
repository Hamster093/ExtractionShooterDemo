/****************************************************
    文件：VendingMachineInteractable.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-16
	功能：售货机交互（靠近显示 F 提示按钮，按 F 打开/关闭售货机面板）
*****************************************************/

using UnityEngine;

/// <summary>
/// 售货机交互：参考 DoorInteractable，挂载于场景 3D 售货机物体上。
/// 由 InteractableBase 负责范围检测与 F 键触发（进入触发区显示 _buttonRoot 提示按钮）。
/// 按下 F 时调用 UIController.OpenVendingMachine 打开/关闭售货机面板。
/// </summary>
public class VendingMachineInteractable : InteractableBase
{
    protected override void OnInteract()
    {
        if (UIController.Instance == null)
        {
            Debug.LogError("[VendingMachineInteractable] 场景中没有 UIController，无法打开售货机面板");
            return;
        }

        UIController.Instance.OpenVendingMachine();
    }
}