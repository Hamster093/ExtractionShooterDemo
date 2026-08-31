/****************************************************
    文件：UIController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-28 16:15:34
	功能：UI控制器 管理所有UI
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 该类全局唯一，切场景不销毁
/// </summary>
public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [SerializeField] private GameObject crosshair; // 准心UI图片
    [SerializeField] private GameObject weaponSprit1; // 1号武器选中特效
    [SerializeField] private GameObject weaponSprit2; // 2号武器选中特效
    [SerializeField] private GameObject meleeWeapon; // 近战武器选中特效

    [Header("弹药UI设置")]
    [SerializeField] private Text ammoText; // 弹药文本
    [SerializeField] private Text ReserveAmmoText; // 弹药文本

    private WeaponBase _currenWeapon;

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
    /// 绑定武器并监听弹药变化（由PlayerController切枪/初始化时调用）
    /// </summary>
    public void BindWeapon(WeaponBase weapon)
    {
        // 1. 解绑旧武器
        if (_currenWeapon != null)
            _currenWeapon.OnAmmoChanged -= UpdateAmmoDisplay;

        // 2. 绑定新武器
        _currenWeapon = weapon;

        if (_currenWeapon != null)
        {
            _currenWeapon.OnAmmoChanged += UpdateAmmoDisplay;
            _currenWeapon.OnReserveAmmoChanged += UpdateHUDReserveText;
            // 立即刷新一次初始状态
            UpdateAmmoDisplay(_currenWeapon.CurrentAmmo, _currenWeapon.MaxAmmo);
            UpdateHUDReserveText(_currenWeapon.ReserveAmmo);
        }

    }
    /// 
    /// <summary>
    /// 弹药UI更新回调
    /// </summary>
    private void UpdateAmmoDisplay(int current, int max)
    {
        if (ammoText != null)
        {
            ammoText.text = $"{current} / {max}";
        }
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

    void OnDestroy()
    {
        if (_currenWeapon != null)
        { 
            _currenWeapon.OnAmmoChanged -= UpdateAmmoDisplay;
            _currenWeapon.OnReserveAmmoChanged -= UpdateHUDReserveText;     
        }

        if (Instance == this) Instance = null;
    }
    private void UpdateHUDReserveText(int reserve)
    {
        if (ReserveAmmoText != null)
        {
            ReserveAmmoText.text = "剩余弹药：" + reserve.ToString();
        }
    }
    /// <summary>
    /// 武器栏位选中特效开关
    /// </summary>
    /// <param name="index"></param>
    private void SelectedWeaponUI(int index)
    {
        weaponSprit1.SetActive(false);
        weaponSprit2.SetActive(false);
        meleeWeapon.SetActive(false);

        switch (index)
        {
            case 1: weaponSprit1.SetActive(true); break;
            case 2: weaponSprit2.SetActive(true); break;
            default: meleeWeapon.SetActive(true); break;
        }
    }
}
    
