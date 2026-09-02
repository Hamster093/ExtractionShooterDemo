/****************************************************
    文件：ChestManager.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-01 17:06:11
	功能：宝箱管理类
*****************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestManager : MonoBehaviour,ISlotOwner
{
    public List<Image> slotImages;   // 按顺序绑定（或通过子物体自动获取）
    private ItemContainer _container;
    public ItemContainer Container => _container;

    IItemContainer ISlotOwner.Container => Container;
    //是否初始化
    private bool _isInitialized = false;

    //格子图片是否已收集（Awake 与手动调用均幂等）
    private bool _slotsReady = false;

    /// <summary>
    /// 外部调用此方法来初始化宝箱内容
    /// 应在 Awake 之后、UI 刷新之前调用
    /// </summary>
    public void Init(List<ChestSlotConfig> items)
    {
        if (_isInitialized)
        {
            Debug.LogWarning($"[ChestManager] {gameObject.name} 已初始化，请勿重复调用 Init！", this);
            return;
        }

        // 确保容器已创建（Awake 中已创建，这里做兜底）
        if (_container == null)
            _container = new ItemContainer(slotImages.Count);

        // 清空旧数据（防止对象池复用时残留）
        _container.Clear();

        // 按配置填充物品
        if (items != null)
        {
            for (int i = 0; i < items.Count && i < _container.SlotCount; i++)
            {
                var config = items[i];
                if (config.itemId > 0 && config.amount > 0)
                {
                    _container.SetItem(i, new ItemInstance(config.itemId, config.amount));
                }
            }
        }

        _isInitialized = true;
        RefreshUI();
    }


    public void RefreshSlot(int index)
    {
        if (_container == null || slotImages == null) return;
        if (index < 0 || index >= slotImages.Count || index >= _container.SlotCount) return;

        var item = _container.GetItem(index);
        bool hasItem = item != null && item.amount > 0;

        slotImages[index].sprite = hasItem ? ResourceManager.LoadUISprite(item.Data.iconKey) : null;
        slotImages[index].color = hasItem ? Color.white : new Color(1, 1, 1, 0);

        // 如果格子上有数量文本，也在这里一并更新 todo
        // var amountText = slotImages[i].GetComponentInChildren<TextMeshProUGUI>();
        // if (amountText != null) 
        //     amountText.text = hasItem && item.amount > 1 ? item.amount.ToString() : "";
    }

    /// <summary>
    /// 全量刷新
    /// 注意：此方法不属于ISlotOwner接口，DragManager不应调用
    /// </summary>
    public void RefreshUI()
    {
        if (_container == null || slotImages == null) return;

        int count = Mathf.Min(slotImages.Count, _container.SlotCount);
        for (int i = 0; i < count; i++)
        {
            RefreshSlot(i);
        }
    }
    /// <summary>
    /// 收集格子图片并创建容器（幂等）。
    /// 面板初始为 inactive 时 Unity 不会调用 Awake，DragManager 注册拖拽前需手动调用此方法。
    /// </summary>
    public void EnsureInitialized()
    {
        if (_slotsReady) return;
        _slotsReady = true;

        // 若未在 Inspector 中绑定格子图片，则自动从子物体获取（Slot_01~Slot_10）
        if (slotImages == null || slotImages.Count == 0)
        {
            slotImages = new List<Image>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var slot = transform.GetChild(i);
                Image img = null;
                // 从子物体的子物体中获取 Image（Slot_XX/Image）
                for (int j = 0; j < slot.childCount; j++)
                {
                    img = slot.GetChild(j).GetComponent<Image>();
                    if (img != null) break;
                }
                if (img != null) slotImages.Add(img);
            }
        }
        if (_container == null)
            _container = new ItemContainer(slotImages.Count);
    }

    void Awake()
    {
        EnsureInitialized();
    }

    void Start()
    {
        //test。。。。。。。。。。
        // 新代码：创建 ItemInstance 实例，传入 (物品ID, 数量)
        _container.SetItem(0, new ItemInstance(1, 5));  // ID=1 的物品，5个
        _container.SetItem(1, new ItemInstance(2, 1));  // ID=2 的物品，1个
        _container.SetItem(2, new ItemInstance(3, 20)); // ID=3 的物品，20个
        //test.。。。。。。。。。
        RefreshUI();
    }
}