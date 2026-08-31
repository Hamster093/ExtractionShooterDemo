/****************************************************
    文件：PlayerInputHandler.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 11:46:35
	功能：玩家输入事件
*****************************************************/

using System;
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
    public event Action<bool> OnSprintChanged; //仅用与通知表现层
    /// <summary>
    /// 跳跃触发事件
    /// </summary>
    public event Action OnJumpTriggered;
    /// <summary>
    /// 按下瞬间攻击触发一次
    /// </summary>
    public event Action OnAttackStarted;
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
    /// <summary>
    /// 换弹触发事件
    /// </summary>
    public event Action OnReloadTriggered;
    #endregion

    /// <summary>
    /// 玩家操作映射
    /// </summary>
    private PlayerControls _playerActions;
    /// <summary>
    /// 主摄像机
    /// </summary>
    private Camera _mainCamera;
    /// <summary>
    /// 最后瞄准位置
    /// </summary>
    public Vector3 _lastAimTarget;
    /// <summary>
    /// 视角跟随开关
    /// </summary>
    public bool IsAimFollowEnabled = true;

    /// <summary>
    /// 武器栏位切换事件，参数为槽位索引 (0-7)
    /// </summary>
    public event Action<int> OnSwitchSlot;
    /// <summary>
    /// V键切换近战武器
    /// </summary>
    public event Action MeleeWeapon;

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
    private bool _isHolding;
    /// <summary>
    /// 是否已经处理
    /// </summary>
    private bool _actionResolved;
    #endregion

    

    private void Awake()
    {
        _playerActions = new PlayerControls();
        _mainCamera = Camera.main;

        // 绑定移动
        _playerActions.Player.Move.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        _playerActions.Player.Move.canceled += ctx => OnMove?.Invoke(Vector2.zero);

        // 绑定跳跃（触发一次）
        _playerActions.Player.Jump.performed += ctx => OnJumpTriggered?.Invoke();

        // Attack 动作绑定改为纯状态通知
        _playerActions.Player.Attack.started += _ => OnAttackStarted?.Invoke();
        _playerActions.Player.Attack.canceled += _ => OnAttackCanceled?.Invoke();
        // 交互（按钮，触发一次）
        _playerActions.Player.Interact.performed += _ => OnInteract?.Invoke();
        // 换弹 
        _playerActions.Player.Reload.performed += ctx => OnReloadTriggered?.Invoke();

        // 冲刺/翻滚
        _playerActions.Player.Sprint.started += OnSprintPressed;
        _playerActions.Player.Sprint.canceled += OnSprintReleased;

        // 武器栏位切换 (1-8键)
        // SwitchSlot 是 Value 动作且绑定按键(KeyControl, float)，不能 ReadValue<int>，
        // 改为从触发回调的按键名(如 "1"、"2")解析槽位索引
        _playerActions.Player.SwitchSlot.performed += ctx =>
        {
            if (ctx.control != null && int.TryParse(ctx.control.name, out int slotIndex))
                OnSwitchSlot?.Invoke(slotIndex);
        };

        _playerActions.Player.MeleeWeapon.performed += _ => MeleeWeapon?.Invoke();
    }
    /// <summary>
    /// 启用输入映射
    /// </summary>
    private void OnEnable()
    {
        if (_playerActions != null)
            _playerActions.Enable();
    }

    /// <summary>
    /// 禁用输入映射（防止后台继续响应）
    /// </summary>
    private void OnDisable()
    {
        if (_playerActions != null)
            _playerActions.Disable();
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
        // 判断当前是否有按下
        if (_isHolding && !_actionResolved)
        {
            if (Time.time - _pressTime >= _tapWindow)
            {
                OnSprintStarted?.Invoke();
                _actionResolved = true;
            }
        }
        #endregion
    }
    



    //    #region 判断翻滚还是冲刺
    //    //判断当前是否有按下
    //    var keyboard = Keyboard.current;
    //    if (keyboard == null) return;

    //    bool sDown = keyboard.leftShiftKey.wasPressedThisFrame;  //按下
    //    bool sUp = keyboard.leftShiftKey.wasReleasedThisFrame;   //松开
    //    bool sHeld = keyboard.leftShiftKey.isPressed;            //按住

    //    if (sDown)
    //    {
    //        _pressTime = Time.time;
    //        _isPressed = true;
    //        _actionResolved = false;
    //    }

    //    if (_isPressed && !_actionResolved && sHeld)
    //    {
    //        if (Time.time - _pressTime >= _tapWindow)
    //        {
    //            //冲刺事件
    //            OnSprintStarted?.Invoke();
    //            _actionResolved = true;
    //        }
    //    }

    //    if (sUp)
    //    {
    //        if (!_actionResolved)
    //        {
    //            //触发翻滚事件
    //            OnRollTriggered?.Invoke();
    //        }
    //        else
    //        {
    //            //冲刺结束事件
    //            OnSprintEnded?.Invoke();
    //        }
    //        _isPressed = false;
    //        _actionResolved = false;
    //    }
    //}
    //#endregion

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
    /// 按下瞬间：记录时间，重置标记
    /// </summary>
    private void OnSprintPressed(InputAction.CallbackContext ctx)
    {
        _pressTime = Time.time;
        _isHolding = true;
        _actionResolved = false;

        // 通知UI层
        OnSprintChanged?.Invoke(true);
    }

    /// <summary>
    /// 根据持有时长决定是翻滚还是结束冲刺
    /// </summary>
    private void OnSprintReleased(InputAction.CallbackContext ctx)
    {
        OnSprintChanged?.Invoke(false);

        if (!_actionResolved)
        {
            // 未超过 tapWindow → 短按 → 翻滚
            OnRollTriggered?.Invoke();
        }
        else
        {
            // 已超过 tapWindow → 长按后松开 → 冲刺结束
            OnSprintEnded?.Invoke();
        }

        _isHolding = false;
        _actionResolved = false;
    }

}