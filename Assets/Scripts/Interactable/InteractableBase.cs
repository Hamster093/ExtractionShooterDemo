/****************************************************
    文件：InteractableBase.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-14 17:16:50
	功能：Nothing
*****************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// 可交互物体基类
/// 负责：范围检测、交互按钮显隐、F键/点击统一触发
/// 子类只需重写 OnInteract() 实现具体业务逻辑
/// </summary>
public abstract class InteractableBase : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] protected GameObject _buttonRoot;   // 按钮根节点（控制显隐）
    [SerializeField] protected Button _interactButton;   // 交互按钮

    [Header("识别设置")]
    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private Key _interactKey = Key.F;

    /// <summary>
    /// 玩家当前是否在交互范围内
    /// </summary>
    public bool PlayerNearby { get; private set; }

    protected virtual void Awake()
    {
        if (_interactButton != null)
            _interactButton.onClick.AddListener(HandleInteract);

        SetButtonVisible(false);
    }

    protected virtual void OnDestroy()
    {
        // 防止对象销毁时回调残留
        if (_interactButton != null)
            _interactButton.onClick.RemoveListener(HandleInteract);
    }

    private void Update()
    {
        if (PlayerNearby
            && Keyboard.current != null
            && Keyboard.current[_interactKey].wasPressedThisFrame)
        {
            HandleInteract();
        }
    }

    #region 范围检测

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

    #endregion

    #region 内部方法

    private void SetButtonVisible(bool visible)
    {
        if (_buttonRoot != null)
            _buttonRoot.SetActive(visible);
    }

    /// <summary>
    /// 统一入口
    /// </summary>
    private void HandleInteract()
    {
        OnInteract();
    }

    #endregion

    /// <summary>
    /// 【核心】子类重写此方法实现具体交互逻辑
    /// </summary>
    protected abstract void OnInteract();
}