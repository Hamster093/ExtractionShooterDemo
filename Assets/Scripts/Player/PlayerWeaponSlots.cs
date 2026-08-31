/****************************************************
    文件：PlayerWeaponSlots.cs
    作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-31 16:52:00
    功能：玩家武器栏位管理（仅记录当前栏位与武器引用）
*****************************************************/

using System;
using UnityEngine;

/// <summary>
/// 玩家武器栏位管理器
/// 职责：记录当前激活的武器栏位索引(0/1)及对应WeaponBase引用
/// </summary>
public class PlayerWeaponSlots : MonoBehaviour
{
    /// <summary>
    /// 最大栏位数（固定为3：主武器+副武器）
    /// </summary>
    public const int SlotCount = 3;

    /// <summary>
    /// 当前激活的栏位索引 (0 = 主武器, 1 = 副武器,2 = 近战武器)
    /// </summary>
    [SerializeField] private int _activeSlotIndex;

    /// <summary>
    /// 各栏位绑定的武器实例（由外部装备系统赋值）
    /// </summary>
    [SerializeField] private WeaponBase[] _weapons = new WeaponBase[SlotCount];

    /// <summary>
    /// 栏位切换事件
    /// - 参数1: 新激活的栏位索引
    /// - 参数2: 新激活的武器实例（可能为null）
    /// </summary>
    public event Action<int, WeaponBase> OnSlotChanged;

    /// <summary>
    /// 当前激活栏位索引
    /// </summary>
    public int ActiveSlotIndex => _activeSlotIndex;

    /// <summary>
    /// 当前激活栏位的武器实例（只读，可能为null）
    /// </summary>
    public WeaponBase ActiveWeapon => GetWeapon(_activeSlotIndex);

    /// <summary>
    /// 获取指定栏位的武器实例
    /// </summary>
    /// <param name="slotIndex">栏位索引</param>
    /// <returns>武器实例，无效索引返回null</returns>
    public WeaponBase GetWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount) return null;
        return _weapons[slotIndex];
    }

    /// <summary>
    /// 设置指定栏位的武器实例（装备/卸下时调用）
    /// </summary>
    /// <param name="slotIndex">栏位索引</param>
    /// <param name="weapon">武器实例，传null表示卸下</param>
    public void SetWeapon(int slotIndex, WeaponBase weapon)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount) return;
        _weapons[slotIndex] = weapon;

        // 如果设置的是当前激活栏位，触发事件通知UI等订阅方
        if (slotIndex == _activeSlotIndex)
            OnSlotChanged?.Invoke(_activeSlotIndex, weapon);
    }

    /// <summary>
    /// 切换到指定栏位
    /// </summary>
    /// <param name="slotIndex">目标栏位索引</param>
    public void SwitchTo(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount) return;
        if (slotIndex == _activeSlotIndex) return; // 已在该栏位，不重复触发

        _activeSlotIndex = slotIndex;
        OnSlotChanged?.Invoke(_activeSlotIndex, ActiveWeapon);
    }

    /// <summary>
    /// 切换到下一个栏位
    /// </summary>
    public void SwitchNext()
    {
        SwitchTo((_activeSlotIndex + 1) % SlotCount);
    }

    /// <summary>
    /// 清空所有栏位（死亡/重置时调用）
    /// </summary>
    public void ClearAll()
    {
        for (int i = 0; i < SlotCount; i++)
            _weapons[i] = null;

        _activeSlotIndex = 0;
        OnSlotChanged?.Invoke(0, null);
    }
}