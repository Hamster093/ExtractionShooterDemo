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
public class PlayerWeaponSlots
{
    private PlayerController _playerController;
    private bool _isRegistered = false; // 防止重复订阅

    public PlayerWeaponSlots(PlayerController playerController)
    {
        _playerController=playerController;
    }
    /// <summary>
    /// 由 PlayerControlle调用
    /// </summary>
    public void Enable()
    {
        if (_isRegistered) return;

        // 初始化计数器
        _equippedCount = 0;
        for (int i = 0; i < SlotCount; i++)
        {
            if (_weapons[i] != null) _equippedCount++;
        }

        PlayerEvents.Instance.OnEquipmentSlotChanged += HandleEquipmentSlotChanged;
        _isRegistered = true;
    }
    /// <summary>
    /// 由 PlayerController中调用
    /// </summary>
    public void Disable()
    {
        if (!_isRegistered) return;

        PlayerEvents.Instance.OnEquipmentSlotChanged -= HandleEquipmentSlotChanged;
        _isRegistered = false;
    }
    /// <summary>
    /// 最大栏位数（固定为3：主武器+副武器）
    /// </summary>
    public const int SlotCount = 3;

    /// <summary>
    /// 当前激活的栏位索引 (0 = 主武器, 1 = 副武器,2 = 近战武器)
    /// </summary>
     private int _activeSlotIndex;

    /// <summary>
    /// 各栏位绑定的武器实例（由外部装备系统赋值）
    /// </summary>
     private WeaponBase[] _weapons = new WeaponBase[SlotCount];

    /// <summary>
    /// 已装备的武器数量（用于UI显示）
    /// </summary>
    private int _equippedCount;
    /// <summary>
    /// 栏位切换事件
    /// - 参数1: 新激活的栏位索引
    /// - 参数2: 新激活的武器实例（可能为null）
    /// </summary>
    public event Action<int, WeaponBase> OnSlotChanged;

    /// <summary>
    /// 武器持有状态变化事件
    /// true = 至少有一把武器, false = 全部卸下
    /// 动画系统订阅此事件来控制层级开关
    /// </summary>
    public event Action<bool> OnHasWeaponChanged;

    /// <summary>
    /// 当前激活栏位索引
    /// </summary>
    public int ActiveSlotIndex => _activeSlotIndex;

    /// <summary>
    /// 当前激活栏位的武器实例（只读，可能为null）
    /// </summary>
    public WeaponBase ActiveWeapon => GetWeapon(_activeSlotIndex);

    /// <summary>
    /// 当前是否持有任何武器
    /// </summary>
    public bool HasAnyWeapon => _equippedCount > 0;

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

        var oldWeapon = _weapons[slotIndex];
        // 防止重复设置同一武器
        if (oldWeapon == weapon) return;

        _weapons[slotIndex] = weapon;

        // 【核心】增量更新计数器，而非遍历
        if (oldWeapon != null && weapon == null) _equippedCount--;
        else if (oldWeapon == null && weapon != null) _equippedCount++;

        // 触发武器持有状态变化事件
        // 没有装备武器/装备一把武器
        if (_equippedCount == 0 || (_equippedCount == 1 && oldWeapon == null))
            //传出bool值
            OnHasWeaponChanged?.Invoke(HasAnyWeapon);


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
        bool hadWeapon = _equippedCount > 0;
        for (int i = 0; i < SlotCount; i++) _weapons[i] = null;

        _equippedCount = 0;
        _activeSlotIndex = 0;

        if (hadWeapon) OnHasWeaponChanged?.Invoke(false);
        OnSlotChanged?.Invoke(0, null);
    }
    private void HandleEquipmentSlotChanged(int slotIndex, ItemInstance item)
    {
        if (item == null)
        {
            // 卸下：将对应栏位清空
            SetWeapon(slotIndex, null);
            return;
        }

        // 根据物品ID创建武器实例（使用工厂类）
        WeaponBase weapon = WeaponFactory.CreateWeapon(item.itemID);
        if (weapon == null)
        {
            return;
        }

        // 装备到指定栏位
        _playerController.PickupWeapon(weapon, slotIndex);
    }
}