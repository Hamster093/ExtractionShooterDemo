/****************************************************
    文件：SliderLookAtCamera.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-09 12:07:46
	功能：血条面板
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceHealthBar : MonoBehaviour
{
    [Header("引用设置")]
    public Transform target; // 拖入角色（敌人）
    public RectTransform healthBar; // 血条的RectTransform
    public Slider healthSlider;
    private CharacterHealth _characterHealth;

    [Header("参数")]
    public float barHeight = 1f; // 血条与角色的距离（沿相机屏幕上方方向）

    private void Awake()
    {
        // 自动从目标身上获取 CharacterHealth 组件
        if (target != null)
            _characterHealth = target.GetComponent<CharacterHealth>();

        if (_characterHealth == null)
            Debug.LogWarning($"[WorldSpaceHealthBar] 目标 {target?.name} 上没有 CharacterHealth 组件！");
    }

    private void Start()
    {
        // ⭐ 订阅事件
        if (_characterHealth != null)
        {
            _characterHealth.OnDamaged += UpdateHealthUI;
            _characterHealth.OnHealthChanged += UpdateHealthUI;

            // 初始化时同步一次当前血量
            UpdateHealthUI(_characterHealth.CurrentHealth,
                           target.GetComponent<CharacterStats>().MaxHealth);
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
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            gameObject.SetActive(false);
            //todo 归还对象池
            return;
        }

        // 偏移方向用"相机屏幕的上方"（camera.up），而非世界Y轴：
        Vector3 headPos = target.position + Camera.main.transform.up * barHeight;

        // 将头顶的世界坐标转为屏幕坐标
        Vector3 screenPos = Camera.main.WorldToScreenPoint(headPos);
        // 直接赋值给UI，UI永远朝向摄像机
        healthBar.position = screenPos;
    }
}