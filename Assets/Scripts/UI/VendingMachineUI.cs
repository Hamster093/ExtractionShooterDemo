/****************************************************
    文件：VendingMachineUI.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-16
	功能：售货机UI管理（提供格子点击事件接口）
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 售货机 UI：数据桥接场景中 VendingMachineController（VendingMachineData）。
/// 挂在售货机面板的 Content 上。
/// 与 WarehouseUI 的区别：不实现 ISlotOwner、不注册 DragManager（无需拖拽），
/// 仅展示商品并暴露【格子点击事件】接口，供后续商品详情面板使用：
/// <code>
/// vendingMachineUI.OnSlotClicked += index =>
/// {
///     var item = vendingMachineUI.GetSlotContent(index); // 或 item.Data.itemName 等
///     // TODO 打开商品详情面板并传参
/// };
/// </code>
/// </summary>
public class VendingMachineUI : MonoBehaviour
{
    [Tooltip("手动绑定格子（可留空：留空时自动收集子物体的 SlotUI）")]
    [SerializeField] private List<SlotUI> _slotImages = new List<SlotUI>();

    [Tooltip("售货机数据控制器（场景 3D 售货机物体上的 VendingMachineController），由搭建工具自动绑定")]
    [SerializeField] private VendingMachineController _controller;

    /// <summary>
    /// 格子点击事件（参数：被点击格子的索引）。供商品详情面板等后续功能订阅。
    /// 空格子也能触发，由订阅方自行判断是否有商品。
    /// </summary>
    public event System.Action<int> OnSlotClicked;

    /// <summary>
    /// 格子列表（惰性初始化：未手动绑定时自动从子物体收集）
    /// </summary>
    public IReadOnlyList<SlotUI> Slots
    {
        get
        {
            EnsureInitialized();
            return _slotImages;
        }
    }

    /// <summary>绑定的售货机控制器（无则返回 null）</summary>
    public VendingMachineController Controller => _controller;

    /// <summary>当前容器数据（未绑定/未生成时为 null）</summary>
    public IItemContainer Container => _controller != null ? _controller.Container : null;

    /// <summary>
    /// 读取指定格子的商品信息（点击事件配套接口，供详情面板/购买逻辑使用）
    /// </summary>
    public ItemInstance GetSlotContent(int index)
    {
        if (_controller == null || _controller.Data == null) return null;
        return _controller.Data.GetSlotContent(index);
    }

    /// <summary>
    /// 添加格子点击监听（OnSlotClicked 的便捷包装，供后续详情面板接线）
    /// </summary>
    public void AddSlotClickListener(System.Action<int> listener)
    {
        OnSlotClicked += listener;
    }

    private bool _isBound = false;
    private bool _slotsReady = false;

    /// <summary>实际订阅过事件的容器（防止控制器重建容器后解绑错对象；OnSlotChanged 事件在 VendingMachineData 具体类上，不在 IItemContainer 接口）</summary>
    private VendingMachineData _boundData;

    /// <summary>已挂过点击监听的格子（避免重复挂载 EventTrigger 条目）</summary>
    private readonly HashSet<SlotUI> _clickBound = new HashSet<SlotUI>();

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnEnable()
    {
        TryBind();

        if (!_isBound)
        {
            StartCoroutine(WaitForController());
        }
    }

    /// <summary>
    /// 收集格子（幂等）：未手动绑定时自动从子物体获取
    /// </summary>
    public void EnsureInitialized()
    {
        if (_slotsReady) return;
        _slotsReady = true;

        if (_slotImages == null || _slotImages.Count == 0)
        {
            _slotImages = new List<SlotUI>();
            var children = GetComponentsInChildren<SlotUI>(true);
            if (children != null)
                _slotImages.AddRange(children);
        }
    }

    /// <summary>
    /// 设置数据控制器并重新绑定
    /// </summary>
    public void BindController(VendingMachineController controller)
    {
        Unbind();
        _controller = controller;
        TryBind();

        if (!_isBound)
        {
            StartCoroutine(WaitForController());
        }
    }

    private void OnDisable()
    {
        Unbind();
        StopAllCoroutines();
    }

    /// <summary>
    /// 尝试绑定售货机数据
    /// </summary>
    private void TryBind()
    {
        if (_isBound) return;

        var data = _controller != null ? _controller.Data : null;
        if (data == null) return;

        EnsureInitialized();
        BindClickHandlers();

        _boundData = data;
        data.OnSlotChanged += RefreshSlot;

        if (_slotImages.Count != data.SlotCount)
        {
            Debug.LogWarning($"[VendingMachineUI] 格子数量({_slotImages.Count})与容器容量({data.SlotCount})不一致，请运行「Tools/售货机/一键搭建售货机UI」重新生成格子");
        }

        _isBound = true;

        RefreshUI();
    }

    /// <summary>
    /// 解除绑定
    /// </summary>
    private void Unbind()
    {
        if (!_isBound) return;

        if (_boundData != null)
        {
            _boundData.OnSlotChanged -= RefreshSlot;
            _boundData = null;
        }

        _isBound = false;
    }

    /// <summary>
    /// 等待售货机控制器就绪（最多等待3秒）
    /// </summary>
    private IEnumerator WaitForController()
    {
        float timeout = 3f;
        float elapsed = 0f;

        while (!_isBound && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
            TryBind();
        }

        if (!_isBound)
        {
            Debug.LogError("[VendingMachineUI] 等待售货机控制器超时！" +
                           "请确认场景 3D 售货机物体上存在 VendingMachineController，且 VendingMachineUI 的 _controller 已绑定。");
        }
    }

    /// <summary>
    /// 为每个格子挂载 PointerClick 事件 → OnSlotClicked(index)。
    /// 事件挂到格子根物体上（EventTrigger，向上冒泡，点图标也能触发）。
    /// </summary>
    private void BindClickHandlers()
    {
        for (int i = 0; i < _slotImages.Count; i++)
        {
            var slot = _slotImages[i];
            if (slot == null || _clickBound.Contains(slot)) continue;
            _clickBound.Add(slot);

            int index = i;
            EventTrigger trigger = slot.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = slot.gameObject.AddComponent<EventTrigger>();

            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener(_ => OnSlotClicked?.Invoke(index));
            trigger.triggers.Add(entry);
        }
    }

    /// <summary>
    /// 单格刷新（供数据层 OnSlotChanged 调用）
    /// </summary>
    private void RefreshSlot(int index)
    {
        if (index < 0 || index >= _slotImages.Count) return;

        var item = Container != null ? Container.GetItem(index) : null;
        _slotImages[index].SetItem(item);
    }

    /// <summary>
    /// 全量刷新（UI生命周期使用，如面板每次打开）
    /// </summary>
    public void RefreshUI()
    {
        if (Container == null) return;
        for (int i = 0; i < _slotImages.Count; i++)
        {
            RefreshSlot(i);
        }
    }
}