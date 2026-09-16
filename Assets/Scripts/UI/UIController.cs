/****************************************************
    文件：UIController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-28 16:15:34
	功能：UI控制器 管理所有UI
*****************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum UIPriority
{
    Hotbar = -10,   // 热键栏
    Game = 0,      // 游戏主界面
    Backpack = 10, // 背包
    Loot = 20,     // 战利品
    Warehouse = 25,// 仓库
    VendingMachine = 26, // 售货机
    Shop = 30,     // 商店
    Dialog = 40,   // 对话框
    Pause = 100,    // 暂停菜单
    HUD = 101       //弹药系统
}

/// <summary>
/// 本场景唯一：切场景时旧实例随场景销毁（OnDestroy 置空 Instance），
/// 新场景实例自动接管并绑定当前场景面板。注意不要与 InventoryService 挂在同一物体上
/// （InventoryService 的 DontDestroyOnLoad 会把本组件也拖进常驻区，导致旧场景面板引用断链）
/// </summary>
public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    private readonly List<IUIPanel> _panelStack = new List<IUIPanel>();

    [SerializeField] private GameObject crosshair; // 准心UI图片
    [SerializeField] private GameObject weaponSprit1; // 1号武器选中特效
    [SerializeField] private GameObject weaponSprit2; // 2号武器选中特效
    [SerializeField] private GameObject meleeWeapon; // 近战武器选中特效
    [SerializeField] private BackpackPanel BackpackPanel; // 背包面板
    [SerializeField] private LootPanel LootPanel; // 战利品面板
    [SerializeField] private WarehousePanel WarehousePanel; // 仓库面板
    [SerializeField] private VendingMachinePanel VendingMachinePanel; // 售货机面板

    [Header("弹药UI设置")]
    [SerializeField] private Text ammoText; // 弹药文本
    [SerializeField] private Text ReserveAmmoText; // 弹药文本

    [Header("背包UI")]
    public BackpackUI backpackUI; 


    void Awake()
    {
        // 本场景唯一：切场景时旧实例随场景销毁（OnDestroy 置空 Instance），
        // 新场景实例自动接管并绑定当前场景面板
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("已经存在UIController类");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 游戏运行时默认隐藏系统鼠标，并开启准心
        HideCursor();
        if (crosshair != null) crosshair.SetActive(true);
    }

    
    /// <summary>
    /// 打开面板
    /// </summary>
    public void OpenPanel(IUIPanel panel)
    {
        if (_panelStack.Contains(panel)) return;

        _panelStack.Add(panel);
        panel.OnOpen();
        //刷新鼠标状态
        UpdateCursorState();

        CheckAndBroadcastBlockState();
    }
    /// <summary>
    /// 关闭指定面板
    /// </summary>
    public void ClosePanel(IUIPanel panel)
    {
        if (!_panelStack.Contains(panel)) return;

        panel.OnClose();
        _panelStack.Remove(panel);

        UpdateCursorState();

        CheckAndBroadcastBlockState();
    }

    /// <summary>
    ///  ESC 统一入口：只处理栈顶面板
    /// </summary>
    public void HandleEscapeKey()
    {
        if (_panelStack.Count == 0)
        {
            TogglePauseMenu();
            return;
        }

        // 按 Priority 降序找第一个
        var topPanel = _panelStack.OrderByDescending(p => p.Priority).FirstOrDefault();
        topPanel.OnEscapePressed();
    }

    /// <summary>
    /// 当前面板是否需要鼠标指针
    /// 遍历当前所有打开的面板，只要有一个需要鼠标，就显示鼠标
    /// </summary>
    private void UpdateCursorState()
    {
        bool shouldShowCursor = false;
        foreach (var panel in _panelStack)
        {
            if (panel is BaseUIPanel basePanel && basePanel.RequireCursor)
            {
                shouldShowCursor = true;
                break;
            }
        }

        if (shouldShowCursor)
            ShowCursor();
        else
            HideCursor();
    }

    /// <summary>
    /// 切换暂停菜单
    /// </summary>
    private void TogglePauseMenu()
    {
        // 暂停菜单本身也作为一个 IUIPanel 纳入栈管理
    }

    void OnDestroy()
    {

        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// 隐藏系统鼠标指针
    /// 同时将鼠标限制在窗口内，防止拖到屏幕外面
    /// </summary>
    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        // 鼠标隐藏时显示准心（两者互斥）
        if (crosshair != null) crosshair.SetActive(true);
    }

    /// <summary>
    /// 显示系统鼠标指针
    /// </summary>
    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // 鼠标显示时隐藏准心（两者互斥）
        if (crosshair != null) crosshair.SetActive(false);
    }

    /// <summary>
    /// 打开背包
    /// </summary>
    public void OpenBackpack()
    {
        bool isBackpackOpen = _panelStack.Any(panel => panel.Priority == UIPriority.Backpack);  
        if (isBackpackOpen)
        {
            ClosePanel(BackpackPanel);            
        }
        else
        {
            OpenPanel(BackpackPanel);
        }
    }
    /// <summary>
    /// 打开战利品
    /// </summary>
    public void OpenLoot(ItemContainer lootContainer = null)
    {
        if (LootPanel == null) { Debug.LogError("[UIController] 未绑定 LootPanel"); return; }

        if (lootContainer != null)
            LootPanel.SetPendingContainer(lootContainer);

        OpenPanel(LootPanel);
    }

    /// <summary>
    /// 打开/关闭仓库（打开时联动打开背包，关闭时联动关闭背包）
    /// </summary>
    public void OpenWarehouse()
    {
        if (WarehousePanel == null) { Debug.LogError("[UIController] 未绑定 WarehousePanel"); return; }

        bool isOpen = _panelStack.Any(panel => panel.Priority == UIPriority.Warehouse);
        if (isOpen)
        {
            ClosePanel(WarehousePanel);
        }
        else
        {
            OpenPanel(WarehousePanel);
        }
    }

    /// <summary>
    /// 打开/关闭售货机面板（由 VendingMachineInteractable 按 F 触发；独立开关，不联动其他面板）
    /// </summary>
    public void OpenVendingMachine()
    {
        if (VendingMachinePanel == null) { Debug.LogError("[UIController] 未绑定 VendingMachinePanel"); return; }

        bool isOpen = _panelStack.Any(panel => panel.Priority == UIPriority.VendingMachine);
        if (isOpen)
        {
            ClosePanel(VendingMachinePanel);
        }
        else
        {
            OpenPanel(VendingMachinePanel);
        }
    }

    /// <summary>
    /// 根据当前面板栈，决定是否阻塞游戏操作（开火/移动等）
    /// </summary>
    private void CheckAndBroadcastBlockState()
    {
        // 只要存在任何需要鼠标光标（RequireCursor=true）的面板，就禁用开火
        bool shouldBlock = _panelStack.Any(p => p is BaseUIPanel basePanel && basePanel.RequireCursor);
        // 广播
        PlayerEvents.Instance.TriggerGameplayBlocked(shouldBlock);

        // 面板打开时相机聚焦回玩家正上方（缓动归位），关闭后恢复鼠标偏移
        var camFollow = Camera.main != null ? Camera.main.GetComponent<TopDownCameraFollow>() : null;
        if (camFollow != null)
            camFollow.SetFocusMode(shouldBlock);
    }
}
    
