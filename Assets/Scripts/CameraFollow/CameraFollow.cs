/****************************************************
    文件：CameraFollow.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-28 15:47:46
	功能：相机跟随
*****************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownCameraFollow : MonoBehaviour
{
    [Header("目标设置")]
    [SerializeField] private Transform target;
    [SerializeField] private float cameraHeight = 15f;

    [Header("基础跟随参数")]
    [SerializeField] private float deadZone = 0.5f;
    [SerializeField] private float smoothTime = 0.15f;   // 平滑阻尼（输出全程滤波，恢复跟手）
    [SerializeField] private float maxSpeed = 20f;       // 最大跟随速度（不再用于掩盖阶梯，恢复响应）
    [SerializeField] private float cameraZOffset = 7f;

    [Header("鼠标偏移参数")]
    [SerializeField] private float mouseInfluenceRadius = 5f;   // 鼠标影响的最大世界距离
    [SerializeField] private float mouseOffsetMultiplier = 0.3f; // 偏移强度系数 (0~1)
    [SerializeField] private float mouseSmoothTime = 0.2f;       // 鼠标偏移自身的平滑时间

    [Header("聚焦动画")]
    [SerializeField] private float focusSmoothTime = 0.1f;  // 聚焦时鼠标偏移归零的缓动时长
    private bool _focusMode;                                // true=相机回到玩家正上方（面板打开时）

    // 基础跟随变量
    private Vector2 velocity;
    private Vector2 smoothPos;

    // 鼠标偏移变量
    private Vector2 currentMouseOffset;
    private Vector2 mouseOffsetVelocity;
    private Plane groundPlane; // 用于精确的鼠标转世界坐标

    /// <summary>
    /// 切换聚焦模式：开启后相机在 focusSmoothTime 内缓动回到玩家正上方（无鼠标偏移）
    /// </summary>
    public void SetFocusMode(bool enabled)
    {
        _focusMode = enabled;
    }


    private void Start()
    {
        
        if (target != null)
        {
            smoothPos = new Vector2(target.position.x, target.position.z);
            transform.position = new Vector3(smoothPos.x, cameraHeight, smoothPos.y-7);
        }
        // 初始化地面平面，Y=0 为角色所在高度
        groundPlane = new Plane(Vector3.up, Vector3.zero);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector2 targetPos = new Vector2(target.position.x, target.position.z);

        // ==================== 相机基础平滑跟随 ====================
        // 计算平滑位置与目标的差值（用于死区判断）
        Vector2 delta = targetPos - smoothPos;
        float deltaMag = delta.magnitude;

        // ⭐ 死区通过"改变 SmoothDamp 的目标"实现，而非在输出层钳制：
        //   - 玩家在死区内 → 目标=相机自身位置（相机停住，不抖动）
        //   - 玩家走出死区 → 目标=玩家位置（SmoothDamp 全程平滑追赶）
        // 输出永远来自 SmoothDamp 的平滑状态，原始 targetPos（物理阶梯）不会直接透传。
        Vector2 dampTarget = (deltaMag > deadZone) ? targetPos : smoothPos;
        smoothPos = Vector2.SmoothDamp(smoothPos, dampTarget, ref velocity, smoothTime, maxSpeed);

        // 输出即平滑位置（无阶梯透传）
        Vector2 baseRenderPos = smoothPos;

        // ==================== 2. 鼠标偏移计算 ====================
        Vector2 targetMouseOffset;

        if (_focusMode)
        {
            // 聚焦模式：偏移目标归零，缓动回到玩家正上方（相机初始位置）
            targetMouseOffset = Vector2.zero;
        }
        else
        {
            Vector2 rawMouseOffset = GetMouseWorldOffset(target.position);

            // 对原始偏移进行钳制，限制最大影响范围
            float offsetMag = rawMouseOffset.magnitude;
            if (offsetMag > mouseInfluenceRadius)
            {
                rawMouseOffset = rawMouseOffset.normalized * mouseInfluenceRadius;
            }

            targetMouseOffset = rawMouseOffset * mouseOffsetMultiplier;
        }

        // 乘以系数后，再做一次平滑，避免鼠标抖动传递到相机
        float mouseSmooth = _focusMode ? focusSmoothTime : mouseSmoothTime;
        currentMouseOffset = Vector2.SmoothDamp(
            currentMouseOffset,
            targetMouseOffset,
            ref mouseOffsetVelocity,
            mouseSmooth
        );

        // ====================  合成最终位置 ====================
        Vector2 finalPos = baseRenderPos + currentMouseOffset;
        transform.position = new Vector3(finalPos.x, cameraHeight, finalPos.y - cameraZOffset);
    }

    /// <summary>
    /// 将鼠标屏幕位置转换为相对于玩家的世界平面偏移
    /// </summary>
    private Vector2 GetMouseWorldOffset(Vector3 playerWorldPos)
    {
        Vector2 mouseScreenPos = Mouse.current != null? Mouse.current.position.ReadValue(): Vector2.zero;

        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);

        // 动态更新平面高度为玩家当前Y值，确保斜视角下转换准确
        groundPlane.SetNormalAndPosition(Vector3.up, new Vector3(0, playerWorldPos.y, 0));

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            return new Vector2(
                worldPoint.x - playerWorldPos.x,
                worldPoint.z - playerWorldPos.z
            );
        }

        return Vector2.zero;
    }
}