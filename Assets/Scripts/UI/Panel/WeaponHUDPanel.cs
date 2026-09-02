/****************************************************
    文件：WeaponHUDPanel.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-03 20:28:12
	功能：Nothing
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

public class WeaponHUDPanel : BaseUIPanel
{
    [SerializeField] private GameObject crosshair; // 准心UI图片
    [SerializeField] private GameObject weaponSprit1; // 1号武器选中特效
    [SerializeField] private GameObject weaponSprit2; // 2号武器选中特效
    [SerializeField] private GameObject meleeWeapon; // 近战武器选中特效

    [Header("弹药UI设置")]
    [SerializeField] private Text ammoText; // 弹药文本
    [SerializeField] private Text ReserveAmmoText; // 弹药文本

    public override UIPriority Priority => UIPriority.HUD;

    private void OnEnable()
    {
        PlayerEvents.Instance.OnWeaponChanged += OnWeaponChanged;
        PlayerEvents.Instance.OnHasWeaponChanged += UpdateHoldState;
        PlayerEvents.Instance.OnCurrentAmmoChanged += UpdateAmmoDisplay;
        PlayerEvents.Instance.OnReserveAmmoChanged += UpdateHUDReserveText;
    }

    private void OnDisable()
    {
        PlayerEvents.Instance.OnWeaponChanged -= OnWeaponChanged;
        PlayerEvents.Instance.OnHasWeaponChanged -= UpdateHoldState;
        PlayerEvents.Instance.OnCurrentAmmoChanged -= UpdateAmmoDisplay;
        PlayerEvents.Instance.OnReserveAmmoChanged -= UpdateHUDReserveText;
    }

    public override void OnOpen()
    {
        base.OnOpen();
    }

    public override void OnClose()
    {
        base.OnClose();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <param name="weapon"></param>
    private void OnWeaponChanged(int slotIndex, WeaponBase weapon)
    {
        SelectedWeaponUI(slotIndex + 1);
    }


    private void UpdateHoldState(bool hasWeapon)
    {
        if (crosshair != null) crosshair.SetActive(hasWeapon);
    }

    /// 
    /// <summary>
    /// 弹药UI更新回调
    /// </summary>
    private void UpdateAmmoDisplay(int current, int max)
    {
        if (ammoText != null) ammoText.text = $"{current} / {max}";
    }

    void OnDestroy()
    {

    }
    private void UpdateHUDReserveText(int reserve)
    {
        if (ReserveAmmoText != null) ReserveAmmoText.text = $"剩余弹药：{reserve}";
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