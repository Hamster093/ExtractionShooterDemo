/****************************************************
    文件：PlayerController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-25 13:39:18
	功能：玩家控制器
*****************************************************/

using System;
using UnityEngine;

public class PlayerController : MonoBehaviour 
{
    [Header("引用")]
    [SerializeField] private PlayerInputHandler _inputHandler;
    [SerializeField] private PlayerMovementConfig _config;     // 配置组件
    [SerializeField] private PlayerAnimatorDriver _animDriver;
    [SerializeField] public Rigidbody _rb;
    public StateMachine _stateMachine;

    // 缓存输入
    public Vector3 _moveDirection;  //移动方向
    public bool _isSprinting;
    public bool _jumpPressed;
    public bool _attackPressed;
    /// <summary>
    /// 角色状态
    /// </summary>


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _stateMachine = new StateMachine();

        // 使用自定义重力，禁用物理默认重力
        if (_config != null)
            _rb.useGravity = false;
        else
            Debug.LogWarning("未找到 PlayerMovementConfig，将使用默认值。");

        // 注册状态
        _stateMachine.RegisterState(new IdleState(this, _animDriver,_config));
        _stateMachine.RegisterState(new MoveState(this, _animDriver, _config));
        _stateMachine.RegisterState(new JumpState(this, _animDriver, _config));
        _stateMachine.RegisterState(new RollState(this, _animDriver, _config));
        _stateMachine.RegisterState(new SprintState(this, _animDriver, _config));

        // 设置初始状态
        _stateMachine.ChangeState<IdleState>();
    }

    private void OnEnable()
    {
        if (_inputHandler == null)
        {
            Debug.LogWarning("PlayerInputHandler 未赋值！");
            return;
        }
        _inputHandler.OnMove += HandleMove;
        _inputHandler.OnSprintChanged += HandleSprint;
        _inputHandler.OnJumpTriggered += HandleJump;
        _inputHandler.OnAimTarget += PlayerAt;

        _inputHandler.OnRollTriggered += HandleRoll;
        _inputHandler.OnSprintStarted += HandleSprintStart;
        _inputHandler.OnSprintEnded += HandleSprintEnd;
    }

    private void OnDisable()
    {
        // 取消订阅
        if (_inputHandler != null)
        {
            _inputHandler.OnMove -= HandleMove;
            _inputHandler.OnSprintChanged -= HandleSprint;
            _inputHandler.OnJumpTriggered -= HandleJump;
            _inputHandler.OnAimTarget -= PlayerAt;

            _inputHandler.OnRollTriggered -= HandleRoll;
            _inputHandler.OnSprintStarted -= HandleSprintStart;
            _inputHandler.OnSprintEnded -= HandleSprintEnd;
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;

        //状态机
        _stateMachine.Tick(Time.fixedDeltaTime);
        Debug.Log("当前状态是" + _stateMachine.CurrentState.ToString());


        // 保持当前垂直速度，叠加重力
        float newVertical = _rb.linearVelocity.y + _config.gravity * Time.fixedDeltaTime;
        // 限制最大下落速度
        if (newVertical < -50f) newVertical = -50f;

        //写入刚体速度
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, newVertical, _rb.linearVelocity.z);

    }

    #region 输入事件处理
    /// <summary>
    /// 玩家冲刺处理方法
    /// </summary>
    /// <param name="isSprinting"></param>
    private void HandleSprint(bool isSprinting)
    {
        _isSprinting = isSprinting;
    }
    /// <summary>
    /// 移动处理方法
    /// </summary>
    /// <param name="moveInput"></param>
    private void HandleMove(Vector2 moveInput)
    {
        Vector3 rawDirection = new Vector3(moveInput.x, 0, moveInput.y);
        if (rawDirection.sqrMagnitude > 1f)
            rawDirection.Normalize();
        _moveDirection = rawDirection;
    }

    /// <summary>
    /// 玩家看向鼠标位置处理方法
    /// </summary>
    /// <param name="vector"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void PlayerAt(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0; // 设置水平瞄准高度
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }
    /// <summary>
    /// 跳跃处理方法
    /// </summary>
    private void HandleJump()
    {
        //先不允许跳跃
        return;
        // 执行跳跃逻辑
        //if (_rb != null)
        //{
        //    Vector3 vel = _rb.linearVelocity;
        //    vel.y = _config.jumpForce;
        //    _rb.linearVelocity = vel;
        //    Debug.Log("跳跃执行！");
        //}
    }

    private void HandleRoll()
    {
        //todo 在地面中才能翻滚
        _stateMachine.ChangeState<RollState>();
    }

    private void HandleSprintStart()
    {
        // 只有当前处于 MoveState 时才允许冲刺
        if (_stateMachine.CurrentState is MoveState)
        {
            _stateMachine.ChangeState<SprintState>();
        }
    }

    private void HandleSprintEnd()
    {
        // 只有当前处于 SprintState 时才响应结束
        if (_stateMachine.CurrentState is SprintState)
        {
            _stateMachine.ChangeState<MoveState>();
        }
    }


    #endregion
    /// <summary>
    /// 视角跟随鼠标开关（用于翻滚
    /// </summary>
    /// <param name="enable"></param>
    public void AimFollowEnabled(bool enable)
    {
        _inputHandler.IsAimFollowEnabled = enable;
    }

}