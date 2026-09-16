/****************************************************
    文件：PlayerStateData.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-16
	功能：玩家跨场景状态数据层（血量/装备栏物品/弹匣/激活栏位）
	说明：背包/仓库数据由常驻 InventoryService 自行保留，无需此处处理。
*****************************************************/

using UnityEngine;

/// <summary>
/// 玩家跨场景状态：切场景前 Save()，新场景 Player 生成后 RestoreEquipment()。
/// 静态存储（仅当前进程有效），主菜单"开始新游戏"时调用 Clear() 重置。
/// - 血量：由 CharacterHealth.Start 通过 ConsumeHealth() 一次性消费（-1=无存档，按满血处理）
/// - 装备栏：按栏位物品ID恢复（往装备格 ChestManager 写物品并触发装备事件，武器实例随之重建）
/// - 弹匣弹药：武器 Initialize 默认满弹，恢复时覆盖为保存值（备弹在背包里，自动保留）
/// - 激活栏位：恢复后切回
/// </summary>
public static class PlayerStateData
{
    private static int _health = -1;                                        // 血量（-1=未保存）
    private static int _activeSlotIndex = 0;                                // 激活栏位
    private static readonly int[] _equippedItemIds = new int[PlayerWeaponSlots.SlotCount]; // 栏位物品ID（-1=空）
    private static readonly int[] _magAmmo = new int[PlayerWeaponSlots.SlotCount];         // 弹匣弹药（-1=无武器）
    private static bool _hasSave = false;

    /// <summary>是否有已保存的玩家状态</summary>
    public static bool HasSave => _hasSave;

    /// <summary>
    /// 切场景前保存玩家状态（血量/激活栏位/装备栏物品ID/弹匣弹药）
    /// </summary>
    public static void Save(PlayerController player)
    {
        if (player == null) return;

        _health = player.CurrentHealth;
        _activeSlotIndex = player.ActiveSlotIndex;

        // 装备栏物品ID：从装备格容器读取（-1=空）
        for (int i = 0; i < PlayerWeaponSlots.SlotCount; i++)
            _equippedItemIds[i] = -1;

        var chest = FindEquipmentGrid();
        if (chest != null)
        {
            chest.EnsureInitialized();
            for (int i = 0; i < PlayerWeaponSlots.SlotCount && i < chest.Container.SlotCount; i++)
            {
                var item = chest.Container.GetItem(i);
                _equippedItemIds[i] = item != null ? item.itemID : -1;
            }
        }

        // 弹匣弹药：每栏位当前武器弹匣余量（-1=该栏位无武器）
        for (int i = 0; i < PlayerWeaponSlots.SlotCount; i++)
        {
            var weapon = player.GetWeaponAt(i);
            _magAmmo[i] = weapon != null ? weapon.CurrentAmmo : -1;
        }

        _hasSave = true;
        Debug.Log($"[PlayerStateData] 已保存玩家状态：HP={_health}, 激活栏位={_activeSlotIndex}, " +
                  $"武器ID=[{string.Join(",", _equippedItemIds)}], 弹匣=[{string.Join(",", _magAmmo)}]");
    }

    /// <summary>
    /// 消费待恢复血量（新场景 CharacterHealth.Start 调用一次）。
    /// 返回 -1 表示无存档或已被消费，调用方按满血处理。
    /// </summary>
    public static int ConsumeHealth()
    {
        int health = _health;
        _health = -1; // 一次性消费
        return health;
    }

    /// <summary>
    /// 新场景 Player 生成后恢复装备与弹药（PlayerController.Start 调用）。
    /// 恢复链路：装备格写物品 → 触发装备槽事件（PlayerWeaponSlots 按物品ID重建武器并登记映射）
    /// → 覆盖弹匣弹药 → 切回激活栏位。持枪动画由 OnHasWeaponChanged 自动恢复。
    /// </summary>
    public static void RestoreEquipment(PlayerController player)
    {
        if (!_hasSave || player == null) return;

        // 1. 装备格容器恢复物品 + 触发装备事件（item 与容器内为同一实例，保证 item→weapon 映射有效）
        var chest = FindEquipmentGrid();
        if (chest != null)
        {
            chest.EnsureInitialized();
            for (int i = 0; i < PlayerWeaponSlots.SlotCount && i < chest.Container.SlotCount; i++)
            {
                int itemId = _equippedItemIds[i];
                if (itemId <= 0) continue;

                var item = new ItemInstance(itemId, 1);
                chest.Container.SetItem(i, item);
                PlayerEvents.Instance.TriggerEquipmentSlotChanged(i, item);
            }

            // 强制全量刷新装备栏 UI：
            // 装备格在 PlayerBackpackPanel（inactive）下时 ChestManager.Awake 不执行，
            // 若 RestoreEquipment 先于格子收集执行，SetItem 触发的 RefreshSlot 会落空，
            // 这里在数据全部写回后统一刷新一次，保证图标/名称显示。
            chest.RefreshUI();
        }
        else
        {
            Debug.LogWarning("[PlayerStateData] 新场景未找到装备栏（ChestManager.isEquipmentGrid），武器栏位无法恢复");
        }

        // 2. 弹匣弹药恢复（Initialize 默认满弹，这里覆盖为保存值）
        for (int i = 0; i < PlayerWeaponSlots.SlotCount; i++)
        {
            var weapon = player.GetWeaponAt(i);
            if (weapon != null && _magAmmo[i] >= 0)
                weapon.RestoreAmmo(_magAmmo[i]);
        }

        // 3. 激活栏位
        if (_activeSlotIndex >= 0 && _activeSlotIndex < PlayerWeaponSlots.SlotCount)
            player.SwitchWeaponSlot(_activeSlotIndex);

        Debug.Log($"[PlayerStateData] 玩家装备状态已恢复（激活栏位={_activeSlotIndex}）");
    }

    /// <summary>
    /// 清空玩家状态（主菜单"开始新游戏"/死亡重开时调用）
    /// </summary>
    public static void Clear()
    {
        _health = -1;
        _activeSlotIndex = 0;
        for (int i = 0; i < PlayerWeaponSlots.SlotCount; i++)
        {
            _equippedItemIds[i] = -1;
            _magAmmo[i] = -1;
        }
        _hasSave = false;
    }

    /// <summary>
    /// 导出玩家状态（持久化存档用）：血量 / 激活栏位 / 弹匣数组副本
    /// </summary>
    public static (int health, int activeSlot, int[] magAmmo) Export()
    {
        return (_health, _activeSlotIndex, (int[])_magAmmo.Clone());
    }

    /// <summary>
    /// 从持久化存档导入玩家状态（数据库读档后调用，与 SaveGameService.LoadGame 配合）：
    /// 装备栏物品ID / 血量 / 弹匣 / 激活栏位 → 由 PlayerController.Start 的 RestoreEquipment
    /// 与 CharacterHealth.Start 的 ConsumeHealth 在新场景自动恢复。
    /// </summary>
    public static void Import(InventorySaveData save)
    {
        if (save == null) return;

        _hasSave = true;
        _health = save.health;
        _activeSlotIndex = save.activeSlot;

        for (int i = 0; i < _magAmmo.Length; i++)
            _magAmmo[i] = save.magAmmo != null && i < save.magAmmo.Length ? save.magAmmo[i] : -1;

        for (int i = 0; i < _equippedItemIds.Length; i++)
            _equippedItemIds[i] = -1;

        if (save.equipment != null)
        {
            foreach (var entry in save.equipment)
            {
                if (entry == null || entry.amount <= 0) continue;
                if (entry.slotIndex >= 0 && entry.slotIndex < _equippedItemIds.Length)
                    _equippedItemIds[entry.slotIndex] = entry.itemID;
            }
        }
    }

    /// <summary>
    /// 查找场景中的装备栏容器（isEquipmentGrid 的 ChestManager）
    /// </summary>
    private static ChestManager FindEquipmentGrid()
    {
        var chests = Object.FindObjectsByType<ChestManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var chest in chests)
        {
            if (chest != null && chest.isEquipmentGrid)
                return chest;
        }
        return null;
    }
}