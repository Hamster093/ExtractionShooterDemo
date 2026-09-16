/****************************************************
    文件：VendingMachineController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-16
	功能：售货机控制器（挂在场景 3D 售货机物体上，持有商品配置与容器数据）
*****************************************************/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 售货机数据控制器：挂在 House/Furniture/VendingMachine 物体上。
/// Inspector 中配置 _slotConfigs（按格子摆货），运行时 Awake 创建容器并按配置生成商品。
/// 面板（VendingMachineUI）绑定本控制器读取容器数据；后续购买/补货逻辑也可通过 Data 操作。
/// 在 Inspector 修改商品配置后，可右键组件菜单"重新生成商品"立即生效。
/// </summary>
public class VendingMachineController : MonoBehaviour
{
    [Header("商品配置")]
    [Tooltip("按格子配置商品：slotIndex=格子索引(0开始), itemId=物品ID(见Resources/XML/Items.xml), amount=数量。运行时自动生成到容器对应格子")]
    [SerializeField] private List<VendingSlotConfig> _slotConfigs = new List<VendingSlotConfig>();

    [Tooltip("容器容量（格数），需与售货机面板格子数一致。由「一键搭建售货机UI」工具自动同步")]
    [SerializeField] private int _capacity = 20;

    private VendingMachineData _data;

    /// <summary>售货机容器数据（null 表示 Awake 尚未执行）</summary>
    public VendingMachineData Data => _data;

    /// <summary>以 IItemContainer 形式暴露容器（供 UI/业务读取）</summary>
    public IItemContainer Container => _data;

    /// <summary>当前商品配置列表（只读，供 Inspector/工具查询）</summary>
    public IReadOnlyList<VendingSlotConfig> SlotConfigs => _slotConfigs;

    private void Awake()
    {
        EnsureDataCreated();
        ApplyConfig();
    }

    /// <summary>
    /// 重建容器数据（扩容/初始化时调用；会丢失当前容器内的运行时改动，回到配置状态）
    /// </summary>
    public void EnsureDataCreated()
    {
        if (_data != null && _data.SlotCount == _capacity) return;

        if (_data != null)
        {
            Debug.Log($"[VendingMachineController] 容量由 {_data.SlotCount} 调整为 {_capacity}，容器已重建");
        }

        _data = new VendingMachineData(_capacity);
    }

    /// <summary>
    /// 按当前配置重新生成商品（清空后重填）。运行时修改配置后调用此方法立即生效。
    /// </summary>
    public void ApplyConfig()
    {
        if (_data == null) return;

        _data.ConfigureFromList(_slotConfigs);
        Debug.Log($"[VendingMachineController] 已按 {_slotConfigs.Count} 条配置生成售货机商品（容量 {_capacity}）");
    }

    /// <summary>
    /// 重新生成商品（Inspector 组件右键菜单：重新生成商品）
    /// </summary>
    [ContextMenu("重新生成商品")]
    public void Regenerate()
    {
        EnsureDataCreated();
        ApplyConfig();
    }
}