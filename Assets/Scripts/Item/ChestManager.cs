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

public class ChestManager : MonoBehaviour
{
    public List<Image> slotImages;   // 按顺序绑定（或通过子物体自动获取）
    public List<ItemData> items;         // 长度与 slotImages 一致

    void Awake()
    {
        // 若未在 Inspector 中绑定格子图片，则自动从子物体获取（Slot_01~Slot_10）
        // 放在 Awake 中保证先于任何 Start()（如 DragManager.Start() 绑定拖拽事件）执行
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

        if (items == null || items.Count != slotImages.Count)
            items = new List<ItemData>(new ItemData[slotImages.Count]);
    }

    void Start()
    {
        //test。。。。。。。。。。
        SetItem(0, ItemRegistry.Get(1));
        SetItem(1, ItemRegistry.Get(2));
        SetItem(2, ItemRegistry.Get(3));
        //test.。。。。。。。。。
        RefreshUI();
    }

    /// <summary>
    /// 更新UI
    /// </summary>
    public void RefreshUI()
    {
        for (int i = 0; i < slotImages.Count; i++)
        {
            ItemData item = items[i];
            if (item != null && item.iconKey != null)
            {
                slotImages[i].sprite = ResourceManager.LoadUISprite(item.iconKey);
                slotImages[i].color = Color.white;
            }
            else
            {
                slotImages[i].sprite = null;
                slotImages[i].color = new Color(1, 1, 1, 0); // 透明
            }
        }
    }

    public ItemData GetItem(int index) => index >= 0 && index < items.Count ? items[index] : null;
    /// <summary>
    /// 添加物品
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    public void SetItem(int index, ItemData item)
    {
        if (index >= 0 && index < items.Count) items[index] = item;
    }

    /// <summary>
    /// 交换（用于同箱移动）
    /// </summary>
    /// <param name="indexA"></param>
    /// <param name="indexB"></param>
    public void SwapItems(int indexA, int indexB)
    {
        ItemData temp = items[indexA];
        items[indexA] = items[indexB];
        items[indexB] = temp;
        RefreshUI();
    }

    /// <summary>
    /// 跨箱移动（由 DragManager 调用）
    /// </summary>
    /// <param name="fromIndex"></param>
    /// <param name="targetChest"></param>
    /// <param name="toIndex"></param>
    public void MoveItemFromTo(int fromIndex, ChestManager targetChest, int toIndex)
    {
        ItemData item = GetItem(fromIndex);
        if (item == null) return;

        ItemData targetItem = targetChest.GetItem(toIndex);
        SetItem(fromIndex, targetItem);
        targetChest.SetItem(toIndex, item);

        RefreshUI();
        targetChest.RefreshUI();
    }
}