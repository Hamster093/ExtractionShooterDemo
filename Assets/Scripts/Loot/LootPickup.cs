/****************************************************
    文件：LootPickup.cs
    作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-31 17:20:00
    功能：战利品拾取物（玩家靠近时在物体旁显示交互按钮，支持F键/鼠标点击触发）
*****************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// 战利品拾取物
/// 玩家进入触发范围后在物体旁显示“战利品”按钮，离开后隐藏
/// 交互方式：鼠标点击按钮，或玩家在范围内按下 F 键（两者都触发同一个点击事件）
/// </summary>
public class LootPickup : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private GameObject _buttonRoot;   // 按钮根节点（控制显隐）
    [SerializeField] private Button _lootButton;      // 战利品按钮

    [Header("识别设置")]
    [SerializeField] private string _playerTag = "Player"; // 玩家Tag
    [SerializeField] private Key _interactKey = Key.F;    // 交互快捷键

    /// <summary>
    /// 玩家当前是否在交互范围内
    /// </summary>
    public bool PlayerNearby { get; private set; }

    private void Awake()
    {
        if (_lootButton != null)
            _lootButton.onClick.AddListener(OnLootButtonClicked);

        SetButtonVisible(false);
    }

    private void Update()
    {
        // F 键联动：仅在玩家处于范围内时生效，触发与按钮点击相同的事件
        if (PlayerNearby && Keyboard.current != null && Keyboard.current[_interactKey].wasPressedThisFrame)
            _lootButton?.onClick.Invoke();

        //todo 打开背包界面和战利品界面
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_playerTag))
        {
            PlayerNearby = true;
            SetButtonVisible(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_playerTag))
        {
            PlayerNearby = false;
            SetButtonVisible(false);
        }
    }

    /// <summary>
    /// 控制按钮显隐
    /// </summary>
    private void SetButtonVisible(bool visible)
    {
        if (_buttonRoot != null)
            _buttonRoot.SetActive(visible);
    }

    /// <summary>
    /// 按钮点击回调
    /// </summary>
    private void OnLootButtonClicked()
    {
        Debug.Log("[LootPickup] 玩家打开了战利品：" + gameObject.name);
        UIController.Instance.OpenLoot();
    }
}