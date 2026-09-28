/****************************************************
    文件：HotbarService.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-27 14:36:00
	功能：快捷栏业务逻辑（绑定/解绑/交换/使用/失效校验）
    说明：快捷栏格保存的是对背包 ItemInstance 的引用（不是物品拥有权），
          因此"绑定"只是记引用，"使用"是扣共享实例的数量，两边 UI 天然同步。
          不使用默认拖拽移动（ItemContainer.MoveBetween），避免同一实例被搬进两个容器。
*****************************************************/

using System;
using UnityEngine;

public static class HotbarService
{
    /// <summary>
    /// 快捷栏格数（需与场景 Canvas/PlayerBackpackPanel/Hotbar 下的格子数一致）
    /// </summary>
    public const int SlotCount = 6;

    /// <summary>
    /// 快捷键起始键号：键 3~8 依次对应快捷栏第 1~6 格（与 HUD 的 Slot_3_Item~Slot_8_Item 命名一致）
    /// </summary>
    public const int KeyStart = 3;

    /// <summary>
    /// 快捷键结束键号
    /// </summary>
    public const int KeyEnd = KeyStart + SlotCount - 1;

    /// <summary>
    /// 物品被成功使用时触发（参数：物品实例、快捷栏格索引）
    /// 供将来接实际效果（回血/饥饿/特效/音效）使用
    /// </summary>
    public static event Action<ItemInstance, int> OnItemUsed;

    private static HotbarData Hotbar => GameService.Hotbar;
    private static BackpackData Backpack => GameService.Backpack;

    /// <summary>
    /// 索引是否落在快捷栏范围内
    /// </summary>
    public static bool IsValidIndex(int hotbarIndex) => hotbarIndex >= 0 && hotbarIndex < SlotCount;

    /// <summary>
    /// 读取某格的绑定物品（可能为 null）
    /// </summary>
    public static ItemInstance GetItem(int hotbarIndex)
        => IsValidIndex(hotbarIndex) ? Hotbar?.GetItem(hotbarIndex) : null;

    /// <summary>
    /// 建立绑定：把拖拽源（背包格）的物品实例【引用】写到快捷栏格上，源物品不移动。
    /// 只接受来自背包的消耗品。
    /// </summary>
    /// <param name="sourceContainer">拖拽源容器（必须是背包）</param>
    /// <param name="sourceIndex">拖拽源索引</param>
    /// <param name="hotbarIndex">目标快捷栏格索引</param>
    /// <returns>是否绑定成功</returns>
    public static bool TryBind(IItemContainer sourceContainer, int sourceIndex, int hotbarIndex)
    {
        if (!IsValidIndex(hotbarIndex)) return false;

        var hotbar = Hotbar;
        if (hotbar == null)
        {
            Debug.LogWarning("[HotbarService] 快捷栏数据未就绪（场景中缺少 InventoryService）");
            return false;
        }

        // 只允许绑定背包里的东西：仓库/装备栏/售货机的物品操作后容易失效（不在背包里就查不到）
        var backpack = Backpack;
        if (backpack == null || !ReferenceEquals(sourceContainer, backpack))
        {
            ToastManager.ShowMessage("只能绑定背包里的消耗品");
            return false;
        }

        var item = sourceContainer.GetItem(sourceIndex);
        if (item == null || item.amount <= 0) return false;

        var data = ItemRegistry.Get(item.itemID);
        if (data == null)
        {
            Debug.LogWarning($"[HotbarService] 物品配置缺失 itemID={item.itemID}");
            return false;
        }

        // 快捷栏只收消耗品
        if (data.type != ItemType.Consumable)
        {
            ToastManager.ShowMessage($"{data.itemName} 不能放入快捷栏（只支持消耗品）");
            return false;
        }

        hotbar.SetItem(hotbarIndex, item); // 写入的是同一个实例引用（触发 UI 刷新）
        ToastManager.ShowMessage($"已绑定 {data.itemName} x{item.amount}");
        return true;
    }

    /// <summary>
    /// 解绑：清空快捷栏格（背包里的物品不受影响）
    /// </summary>
    public static void Unbind(int hotbarIndex)
    {
        if (!IsValidIndex(hotbarIndex)) return;
        Hotbar?.SetItem(hotbarIndex, null);
    }

    /// <summary>
    /// 交换两个快捷栏格的绑定
    /// </summary>
    public static void SwapBinding(int a, int b)
    {
        if (!IsValidIndex(a) || !IsValidIndex(b) || a == b) return;

        var hotbar = Hotbar;
        if (hotbar == null) return;

        var itemA = hotbar.GetItem(a);
        var itemB = hotbar.GetItem(b);
        hotbar.SetItem(a, itemB);
        hotbar.SetItem(b, itemA);
    }

    /// <summary>
    /// 按键使用方法（键 3~8 → 快捷栏第 1~6 格）
    /// </summary>
    public static bool TryUseByKey(int key)
    {
        if (key < KeyStart || key > KeyEnd)
        {
            Debug.LogWarning($"[HotbarService] 键位 {key} 不在快捷键范围 {KeyStart}~{KeyEnd}");
            return false;
        }
        return TryUse(key - KeyStart);
    }

    /// <summary>
    /// 使用某格绑定的物品：扣 1 个，背包格与快捷栏格同步；数量归零则两边一起清空（自动解绑）。
    /// </summary>
    /// <param name="hotbarIndex">快捷栏格索引</param>
    /// <returns>是否成功使用</returns>
    public static bool TryUse(int hotbarIndex)
    {
        if (!IsValidIndex(hotbarIndex)) return false;

        var hotbar = Hotbar;
        var backpack = Backpack;
        if (hotbar == null || backpack == null)
        {
            Debug.LogWarning("[HotbarService] 快捷栏/背包数据未就绪，无法使用物品");
            return false;
        }

        var item = hotbar.GetItem(hotbarIndex);
        if (item == null || item.amount <= 0) return false; // 空格子：静默忽略

        var data = ItemRegistry.Get(item.itemID);
        if (data == null) return false;

        if (data.type != ItemType.Consumable)
        {
            ToastManager.ShowMessage($"{data.itemName} 不是消耗品，无法使用");
            return false;
        }

        // 绑定的实例必须仍然在背包里（被拖到仓库/装备栏，或读档重建后即为失效）
        int backpackIndex = FindIndexInBackpack(item);
        if (backpackIndex < 0)
        {
            hotbar.SetItem(hotbarIndex, null); // 失效绑定：清掉快捷栏格
            ToastManager.ShowMessage("快捷栏物品已不在背包中");
            return false;
        }

        // 扣减共享实例，然后写回两个容器（ItemContainer.SetItem 会触发各自的 OnSlotChanged）
        item.amount--;
        var alive = item.amount > 0 ? item : null;
        backpack.SetItem(backpackIndex, alive);
        hotbar.SetItem(hotbarIndex, alive);

        ToastManager.ShowMessage($"使用了 {data.itemName}");

        //TODO 实际效果：在此接回血/体力/饥饿等（例：CharacterHealth.Heal(数值)），
        //     消耗品数值建议后续加到 Items.xml（ItemData）里，避免按 itemID 硬编码。
        OnItemUsed?.Invoke(item, hotbarIndex);

        return true;
    }

    /// <summary>
    /// 查找某个物品实例当前在背包中的索引；不在背包里返回 -1
    /// </summary>
    public static int FindIndexInBackpack(ItemInstance item)
    {
        var backpack = Backpack;
        if (backpack == null || item == null) return -1;

        for (int i = 0; i < backpack.SlotCount; i++)
        {
            if (ReferenceEquals(backpack.GetItem(i), item)) return i;
        }
        return -1;
    }

    /// <summary>
    /// 校验所有绑定是否仍然有效（实例还在背包且数量大于 0），失效的自动解绑。
    /// 由常驻的 HUD 视图在背包数据变化时调用（物品被拖到仓库、被消耗完、读档重建等）。
    /// </summary>
    /// <returns>是否清理过失效绑定</returns>
    public static bool Validate()
    {
        var hotbar = Hotbar;
        if (hotbar == null) return false;

        bool changed = false;
        for (int i = 0; i < hotbar.SlotCount; i++)
        {
            var item = hotbar.GetItem(i);
            if (item == null) continue;

            if (item.amount <= 0 || FindIndexInBackpack(item) < 0)
            {
                hotbar.SetItem(i, null); // 触发 OnSlotChanged → 视图自动刷新
                changed = true;
            }
        }
        return changed;
    }
}
