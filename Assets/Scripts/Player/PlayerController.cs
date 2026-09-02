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
    [SerializeField] private PlayerWeaponSlots _weaponSlots;  //武器栏位
    private WeaponBase _lastActiveWeapon;//最后激活的武器

    public StateMachine _stateMachine;

    // 缓存输入
    public Vector3 _moveDirection;  //移动方向
    public bool _jumpPressed;
    public bool _attackPressed;
    public bool _isSprinting;

    public bool IsGrounded { get; private set; }

    /// <summary>
    /// 获取当前激活的武器
    /// </summary>
    public WeaponBase CurrentWeapon => _weaponSlots != null ? _weaponSlots.ActiveWeapon : null;

    private void Awake()
    {
        _stateMachine = new StateMachine();

        if (_weaponSlots == null)
        {
            Debug.LogError("PlayerWeaponSlots 未在 Inspector 中赋值！", this);
            enabled = false; // 禁用脚本避免后续连环报错
            return;
        }

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
    private void Start()
    {
        InitializeWeapon(CurrentWeapon);
    }

    /// <summary>
    /// 拾取武器并装备到指定栏位
    /// </summary>
    /// <param name="weapon">拾取到的武器实例</param>
    /// <param name="targetSlot">目标栏位索引 (0=主武器, 1=副武器, 2=近战)</param>
    public void PickupWeapon(WeaponBase weapon, int targetSlot)
    {
        if (weapon == null || _weaponSlots == null) return;

        // 1. 将武器记录到栏位系统（内部会自动触发OnSlotChanged如果设为当前栏位）
        _weaponSlots.SetWeapon(targetSlot, weapon);

        // 2. 初始化武器
        weapon.Initialize(_animDriver, gameObject);

        // 3. 如果装备的就是当前激活栏位，额外刷新UI
        if (targetSlot == _weaponSlots.ActiveSlotIndex)
        {
            PlayerEvents.Instance.TriggerWeaponChanged(targetSlot, weapon);
        }

        Debug.Log($"[PlayerController] 拾取武器 {weapon.name} 到栏位 {targetSlot}");
    }

    /// <summary>
    /// 切换到指定栏位（可由输入系统调用）
    /// </summary>
    public void SwitchWeaponSlot(int slotIndex)
    {
        _weaponSlots?.SwitchTo(slotIndex);
    }

    /// <summary>
    /// 初始化单个武器实例
    /// </summary>
    private void InitializeWeapon(WeaponBase weapon)
    {
        if (weapon == null) return;

        weapon.Initialize(_animDriver, gameObject);
        weapon.SetAimTargetProvider(() => _inputHandler._lastAimTarget);
    }

    /// <summary>
    /// 栏位切换回调：处理新武器的初始化和UI绑定
    /// </summary>
    private void OnWeaponSlotChanged(int slotIndex, WeaponBase weapon)
    {

        if (_lastActiveWeapon != null && _lastActiveWeapon != weapon)
        {
            _lastActiveWeapon.CancelReload();
            _lastActiveWeapon.CancelFire();
        }
        // 解绑旧武器的弹药监听
        if (_lastActiveWeapon != null)
        {
            _lastActiveWeapon.OnAmmoChanged -= HandleAmmoChanged;
            _lastActiveWeapon.OnReserveAmmoChanged -= HandleReserveAmmoChanged;
        }

        InitializeWeapon(weapon);
        PlayerEvents.Instance.TriggerWeaponChanged(slotIndex, weapon);//武器变更事件

        // 绑定新武器的弹药监听，并立即同步一次初始值
        if (weapon != null)
        {
            weapon.OnAmmoChanged += HandleAmmoChanged;
            weapon.OnReserveAmmoChanged += HandleReserveAmmoChanged;
            HandleAmmoChanged(weapon.CurrentAmmo, weapon.MaxAmmo);
            HandleReserveAmmoChanged(weapon.ReserveAmmo);
        }
        else
        {
            // 切到空栏位时清零显示
            PlayerEvents.Instance.TriggerCurrentAmmoChanged(0, 0);
            PlayerEvents.Instance.TriggerReserveAmmoChanged(0);
        }
        _lastActiveWeapon = weapon;

    }

    private void Update()
    {
        CurrentWeapon?.Tick(Time.deltaTime);
        // 简单的地面检测 未验证高度值是否合适
        IsGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
    private void OnEnable()
    {
        if (_inputHandler == null)
        {
            Debug.LogWarning("PlayerInputHandler 未赋值！");
            return;
        }
        _inputHandler.OnMove += HandleMove;
        _inputHandler.OnJumpTriggered += HandleJump;
        _inputHandler.OnSprintChanged += HandleSprintFlag;
        _inputHandler.OnAimTarget += PlayerAt;

        _inputHandler.OnRollTriggered += HandleRoll;
        _inputHandler.OnSprintStarted += HandleSprintStart;
        _inputHandler.OnSprintEnded += HandleSprintEnd;

        _inputHandler.OnAttackStarted += HandleAttackIntent;
        _inputHandler.OnAttackCanceled += HandleAttackCancel;
        _inputHandler.OnReloadTriggered += HandleReload;
        // 武器栏位切换事件
        if (_weaponSlots != null)
            _weaponSlots.OnSlotChanged += OnWeaponSlotChanged;

        _inputHandler.OnSwitchSlot += HandleSwitchSlot;
        _inputHandler.MeleeWeapon += HandMeleeWeapon;

        _weaponSlots.OnHasWeaponChanged += OnHasWeaponChanged;
        // 初始化时同步持枪动画
        OnHasWeaponChanged(_weaponSlots.HasAnyWeapon);
    }

    
    private void OnDisable()
    {
        // 取消订阅
        if (_inputHandler != null)
        {
            _inputHandler.OnMove -= HandleMove;
            _inputHandler.OnJumpTriggered -= HandleJump;
            _inputHandler.OnSprintChanged -= HandleSprintFlag;
            _inputHandler.OnAimTarget -= PlayerAt;

            _inputHandler.OnRollTriggered -= HandleRoll;
            _inputHandler.OnSprintStarted -= HandleSprintStart;
            _inputHandler.OnSprintEnded -= HandleSprintEnd;

            _inputHandler.OnAttackStarted -= HandleAttackIntent;
            _inputHandler.OnAttackCanceled -= HandleAttackCancel;

            _inputHandler.OnReloadTriggered -= HandleReload;

            if (_weaponSlots != null)
                _weaponSlots.OnSlotChanged -= OnWeaponSlotChanged;
            _inputHandler.OnSwitchSlot -= HandleSwitchSlot;
            _inputHandler.MeleeWeapon -= HandMeleeWeapon;

            _weaponSlots.OnHasWeaponChanged -= OnHasWeaponChanged;

            // 清理当前武器状态
            if (_lastActiveWeapon != null)
            {
                _lastActiveWeapon.OnAmmoChanged -= HandleAmmoChanged;
                _lastActiveWeapon.OnReserveAmmoChanged -= HandleReserveAmmoChanged;
                _lastActiveWeapon.CancelReload();
                _lastActiveWeapon.CancelFire();
                _lastActiveWeapon = null;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;

        //状态机
        _stateMachine.Tick(Time.fixedDeltaTime);

        // 保持当前垂直速度，叠加重力
        float newVertical = _rb.linearVelocity.y + _config.gravity * Time.fixedDeltaTime;
        // 限制最大下落速度
        if (newVertical < -50f) newVertical = -50f;

        //写入刚体速度
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, newVertical, _rb.linearVelocity.z);

    }

    #region 输入事件处理
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
        if (!IsGrounded) return;

        if (_stateMachine.CurrentState is not RollState)
        {
            //todo 在地面中才能翻滚
            _stateMachine.ChangeState<RollState>();
        }     
    }

    // 标记方法，不触发任何状态切换
    private void HandleSprintFlag(bool isSprinting)
    {
        _isSprinting = isSprinting;
    }

    private void HandleSprintEnd()
    {
        _isSprinting = false;
        // 只有当前处于 SprintState 时才响应结束
        if (_stateMachine.CurrentState is SprintState)
        {
            _stateMachine.ChangeState<MoveState>();
        }
    }

    // <summary>
    /// 换弹处理
    /// </summary>
    private void HandleReload()
    {
        // 翻滚/跳跃等状态下不允许换弹
        if (_stateMachine.CurrentState is not (IdleState or MoveState)) return;
        CurrentWeapon?.TryReload();
    }

    private void HandleSprintStart()
    {
        if (_stateMachine.CurrentState is MoveState or IdleState)
        {
            _stateMachine.ChangeState<SprintState>();
        }
    }
    /// <summary>
    /// 玩家想开枪
    /// </summary>
    private void HandleAttackIntent()
    {
        CurrentWeapon?.RequestFire(transform.forward);
    }

    /// <summary>
    /// 玩家停止开枪
    /// </summary>
    private void HandleAttackCancel()
    {
        CurrentWeapon?.CancelFire();
    }
    /// <summary>
    /// 按键处理方法（1-8）
    /// </summary>
    /// <param name="slotIndex"></param>
    private void HandleSwitchSlot(int slotIndex)
    {
        if (slotIndex<3)
        {
            // 翻滚/跳跃等状态下不允许切枪
            if (_stateMachine.CurrentState is RollState or JumpState) return;

            SwitchWeaponSlot(slotIndex-1);
            Debug.Log($"[PlayerController] 切换到栏位 {slotIndex}");
        }
        else
        {
            //todo 触发物品栏3-8
            Debug.Log($"使用物品栏 {slotIndex}号位置的物品");
        }
    }
    /// <summary>
    /// 切换近战武器槽位
    /// </summary>
    private void HandMeleeWeapon()
    {
        if (_stateMachine.CurrentState is RollState or JumpState) return;

        SwitchWeaponSlot((int)WeaponSlot.Melee);
        Debug.Log($"[PlayerController] 切换到近战武器");
    }

    #endregion

    /// <summary>
    /// 武器状态改变时回调
    /// </summary>
    /// <param name="hasWeapon"></param>
    private void OnHasWeaponChanged(bool hasWeapon)
    {
        float targetWeight = hasWeapon ? 1f : 0f;
        _animDriver.Animator.SetLayerWeight(AnimParams.HOLD_GUN_LAYER_INDEX, targetWeight);
        PlayerEvents.Instance.TriggerHasWeaponChanged(hasWeapon);
    }

    /// <summary>
    /// 视角跟随鼠标开关（用于翻滚
    /// </summary>
    /// <param name="enable"></param>
    public void AimFollowEnabled(bool enable)
    {
        _inputHandler.IsAimFollowEnabled = enable;
    }

    // 转发事件
    private void HandleAmmoChanged(int current, int max)
        => PlayerEvents.Instance.TriggerCurrentAmmoChanged(current, max);

    private void HandleReserveAmmoChanged(int reserve)
        => PlayerEvents.Instance.TriggerReserveAmmoChanged(reserve);


}
public enum WeaponSlot { Primary = 0, Secondary = 1, Melee = 2 }