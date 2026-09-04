using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotHandler : DefaultSlotHandler // 继承默认，覆盖判断
{
    [Header("装备槽配置")]
    public ItemType allowedSlot = ItemType.Equipment;

    [Header("装备效果（武器）")]
    [SerializeField] private Transform _handSocket;      // 手部武器挂载点
    [SerializeField] private PlayerController _playerController; // 玩家控制器
    private GameObject _currentWeaponInstance;          // 当前装备的武器模型

    public override bool CanDrop(ISlotOwner sourceOwner, int sourceIndex)
    {
        var item = sourceOwner.Container.GetItem(sourceIndex);
        if (item == null) return false;

        return item.Data.type == allowedSlot;
    }

    public override bool OnEndDrag(PointerEventData eventData, Image slot, 
        ISlotOwner owner, int index, Image targetSlot, (ISlotOwner owner, int index)? targetInfo)
    {

        // 如果拖拽到空白处（非槽位），执行卸下逻辑
        if (targetSlot == null)
        {
            // 示例：将装备从当前槽位移除，添加到背包
            var item = owner.Container.GetItem(index);
            if (item != null)
            {
                // 从当前容器移除
                owner.Container.SetItem(index, null);
                // 添加到背包（具体逻辑取决于你的背包添加方法）
                PlayerBackpack.Instance.AddItem(item.itemID,1);

                // 刷新 UI
                owner.RefreshSlot(index);

                // 触发事件，通知装备槽已卸下
                PlayerEvents.Instance.TriggerEquipmentSlotChanged(index, null);
            }
            return true; // 告诉管理器已处理，不要再执行默认移动
        }

        // 如果目标是另一个槽位，调用基类逻辑（走移动/交换）
        return false;
    }

   
}