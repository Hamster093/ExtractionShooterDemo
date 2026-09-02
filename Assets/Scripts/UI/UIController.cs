/****************************************************
    文件：UIController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-28 16:15:34
	功能：UI控制器 管理所有UI
*****************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum UIPriority
{
    Hotbar = -10,   // 热键栏
    Game = 0,      // 游戏主界面
    Backpack = 10, // 背包
    Loot = 20,     // 战利品
    Shop = 30,     // 商店
    Dialog = 40,   // 对话框
    Pause = 100,    // 暂停菜单
    HUD = 101       //弹药系统
}

/// <summary>
/// 该类全局唯一，切场景不销毁
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

    [Header("弹药UI设置")]
    [SerializeField] private Text ammoText; // 弹药文本
    [SerializeField] private Text ReserveAmmoText; // 弹药文本

    [Header("背包UI")]
    public BackpackUI backpackUI; 


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
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
    }

    /// <summary>
    /// 显示系统鼠标指针
    /// </summary>
    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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
    public void OpenLoot()
    {
        OpenPanel(LootPanel);
    }
}
    
