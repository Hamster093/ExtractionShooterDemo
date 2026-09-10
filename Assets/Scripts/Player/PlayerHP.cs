/****************************************************
    文件：SliderLookAtCamera.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-09 12:07:46
	功能：血条面板
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    [Header("引用设置")]
    public Image healthSlider;
    public CharacterHealth _characterHealth;
    public Text text;

    private void Awake()
    {
    }

    private void Start()
    {
        // ⭐ 订阅事件
        if (_characterHealth != null)
        {
            _characterHealth.OnDamaged += UpdateHealthUI;
            _characterHealth.OnHealthChanged += UpdateHealthUI;

            // 初始化时同步一次当前血量
            UpdateHealthUI(_characterHealth.GetComponent<CharacterStats>().MaxHealth,
                           _characterHealth.GetComponent<CharacterStats>().MaxHealth);
        }
    }

    private void OnDisable()
    {
        // 取消订阅
        if (_characterHealth != null)
        {
            _characterHealth.OnDamaged -= UpdateHealthUI;
            _characterHealth.OnHealthChanged -= UpdateHealthUI;
        }
    }

    private void UpdateHealthUI(int current, int max)
    {
        UpdateHealthUI(current, max, 0);
    }

    /// <summary>
    /// 统一处理血量更新的回调
    /// </summary>
    private void UpdateHealthUI(int current, int max, int damage)
    {
        if (healthSlider != null && max > 0)
        {
           float sum= (current * 1.0f) / (max * 1.0f);
            healthSlider.fillAmount = sum;
        }
        if (text!=null)
        {
            text.text = current + "/" + max;
        }
    }

}