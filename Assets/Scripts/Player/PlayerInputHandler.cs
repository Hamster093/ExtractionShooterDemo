/****************************************************
    文件：PlayerInputHandler.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 11:46:35
	功能：玩家输入事件
*****************************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputHandler : MonoBehaviour 
{
    #region 语义化输入事件
    /// <summary>
    /// 玩家移动事件
    /// </summary>
    public event Action<Vector2> OnMove;
    /// <summary>
    /// 人物视角追随鼠标事件
    /// </summary>
    public event Action<Vector3> OnAimTarget;
    /// <summary>
    /// 冲刺/翻滚触发事件
    /// </summary>
    public event Action<bool> OnSprintChanged;
    /// <summary>
    /// 跳跃触发事件
    /// </summary>
    public event Action OnJumpTriggered;
    /// <summary>
    /// 按下瞬间攻击触发一次
    /// </summary>
    public event Action OnAttackStarted;
    /// <summary>
    /// 按住期间持续触发（全自动射击）
    /// </summary>
    public event Action OnAttackHeld;
    /// <summary>
    /// 松开瞬间（用来停止连射）
    /// </summary>
    public event Action OnAttackCanceled;

    public event Action OnInteract;
    #endregion

    /// <summary>
    /// 玩家操作映射
    /// </summary>
    private PlayerControls _playerActions;
    /// <summary>
    /// 射击的携程
    /// </summary>
    private Coroutine _attackCoroutine;
    [Header("连射设置")]
    [SerializeField][Tooltip("连射间隔")] private float _attackRate = 0.1f;
    /// <summary>
    /// 主摄像机
    /// </summary>
    private Camera _mainCamera;
    /// <summary>
    /// 最后瞄准位置
    /// </summary>
    private Vector3 _lastAimTarget;

    private void Awake()
    {
        _playerActions = new PlayerControls();
        _mainCamera = Camera.main;

        // 绑定移动
        _playerActions.Player.Move.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        _playerActions.Player.Move.canceled += ctx => OnMove?.Invoke(Vector2.zero);

        // 绑定冲刺（按住为 true，松开为 false）
        _playerActions.Player.Sprint.performed += ctx => OnSprintChanged?.Invoke(true);
        _playerActions.Player.Sprint.canceled += ctx => OnSprintChanged?.Invoke(false);

        // 绑定跳跃（触发一次）
        _playerActions.Player.Jump.performed += ctx => OnJumpTriggered?.Invoke();

        // Attack 动作绑定
        _playerActions.Player.Attack.started += ctx =>
        {
            OnAttackStarted?.Invoke();          // 按下瞬间触发
            //全自动携程                                    
            if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
            _attackCoroutine = StartCoroutine(AttackRoutine());
        };
        _playerActions.Player.Attack.canceled += ctx =>
        {
            OnAttackCanceled?.Invoke();         // 松开瞬间触发
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
        };
        // 交互（按钮，触发一次）
        _playerActions.Player.Interact.performed += _ => OnInteract?.Invoke();
    }

    private void Update()
    {
        // 读取鼠标屏幕位置
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 worldTarget = GetWorldPointOnGround(mouseScreen);
        //待机检测
        if (worldTarget != _lastAimTarget)
        {
            _lastAimTarget = worldTarget;
            OnAimTarget?.Invoke(worldTarget);
        }
    }
    /// <summary>
    /// 将屏幕坐标转换为世界坐标
    /// </summary>
    /// <param name="screenPos"></param>
    /// <returns></returns>
    private Vector3 GetWorldPointOnGround(Vector2 screenPos)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);
        else
            return ray.GetPoint(100f);
    }

    /// <summary>
    /// 攻击协助 实现全自动逻辑
    /// </summary>
    /// <returns></returns>
    private IEnumerator AttackRoutine()
    {
        OnAttackHeld?.Invoke();

        while (true)
        {
            yield return new WaitForSeconds(_attackRate);
            OnAttackHeld?.Invoke();
        }
    }

    private void OnEnable() => _playerActions.Enable();
    private void OnDisable()
    {
        _playerActions.Disable();
        if (_attackCoroutine != null)
            StopCoroutine(_attackCoroutine);
    }
}