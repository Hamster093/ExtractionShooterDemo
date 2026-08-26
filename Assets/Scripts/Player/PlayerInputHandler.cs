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
    /// <summary>
    /// 交互
    /// </summary>
    public event Action OnInteract;

    /// <summary>
    /// 翻滚触发
    /// </summary>
    public event Action OnRollTriggered;
    /// <summary>
    /// 冲刺开始
    /// </summary>
    public event Action OnSprintStarted;
    /// <summary>
    /// 冲刺结束
    /// </summary>
    public event Action OnSprintEnded;

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

    #region 冲刺/翻滚判定
    [Header("冲刺/翻滚的判定间隔")]
    [SerializeField] private float _tapWindow = 0.2f; // 超过这个时间判定为长按

    /// <summary>
    /// 按下的开始时间
    /// </summary>
    private float _pressTime;
    /// <summary>
    /// 按下状态
    /// </summary>
    private bool _isPressed;
    /// <summary>
    /// 是否已经处理
    /// </summary>
    private bool _actionResolved;
    #endregion

    /// <summary>
    /// 主摄像机
    /// </summary>
    private Camera _mainCamera;
    /// <summary>
    /// 最后瞄准位置
    /// </summary>
    private Vector3 _lastAimTarget;
    /// <summary>
    /// 视角跟随开关
    /// </summary>
    public bool IsAimFollowEnabled = true;

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
            #region 玩家视角跟随鼠标
        if (IsAimFollowEnabled)
        {
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 worldTarget = GetWorldPointOnGround(mouseScreen);
            //待机检测
            //if (worldTarget != _lastAimTarget)
            {
                _lastAimTarget = worldTarget;
                OnAimTarget?.Invoke(worldTarget);
            }
        }
            #endregion


        #region 判断翻滚还是冲刺
        //判断当前是否有按下
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool sDown = keyboard.leftShiftKey.wasPressedThisFrame;  //按下
        bool sUp = keyboard.leftShiftKey.wasReleasedThisFrame;   //松开
        bool sHeld = keyboard.leftShiftKey.isPressed;            //按住

        if (sDown)
        {
            _pressTime = Time.time;
            _isPressed = true;
            _actionResolved = false;
        }

        if (_isPressed && !_actionResolved && sHeld)
        {
            if (Time.time - _pressTime >= _tapWindow)
            {
                //冲刺事件
                OnSprintStarted?.Invoke();
                _actionResolved = true;
            }
        }

        if (sUp)
        {
            if (!_actionResolved)
            {
                //触发翻滚事件
                OnRollTriggered?.Invoke();
            }
            else
            {
                //冲刺结束事件
                OnSprintEnded?.Invoke();
            }
            _isPressed = false;
            _actionResolved = false;
        }
    }
    #endregion

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