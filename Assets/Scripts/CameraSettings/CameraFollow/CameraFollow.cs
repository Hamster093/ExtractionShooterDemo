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
    [SerializeField] private float followSpeed = 100f;      // 跟随速度（越大越跟手，解决阶梯卡顿）
    [SerializeField] private float cameraZOffset = 7f;

    [Header("鼠标偏移参数")]
    [SerializeField] private float mouseInfluenceRadius = 5f;   // 鼠标影响的最大世界距离
    [SerializeField] private float mouseOffsetMultiplier = 0.3f; // 偏移强度系数 (0~1)
    [SerializeField] private float mouseSmoothTime = 0.1f;       // 正常时鼠标偏移平滑时间

    [Header("聚焦（取消鼠标偏移）")]
    [SerializeField] private float focusSmoothTime = 0.3f;  // 聚焦时鼠标偏移归零的平滑时间
    private bool _focusMode;                                // true = 取消鼠标偏移

    // 基础跟随变量
    private Vector2 smoothPos;

    // 鼠标偏移变量
    private Vector2 currentMouseOffset;
    private Plane groundPlane;

    /// <summary>
    /// 设置聚焦模式：true 时取消鼠标偏移，false 时恢复
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
            transform.position = new Vector3(smoothPos.x, cameraHeight, smoothPos.y - cameraZOffset);
        }

        groundPlane = new Plane(Vector3.up, Vector3.zero);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector2 targetPos = new Vector2(target.position.x, target.position.z);

        // ==================== 1. 基础跟随（始终使用 followSpeed） ====================
        float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
        smoothPos = Vector2.Lerp(smoothPos, targetPos, t);

        Vector2 baseRenderPos = smoothPos;

        // ==================== 2. 鼠标偏移 ====================
        Vector2 targetMouseOffset;

        if (_focusMode)
        {
            // 聚焦：鼠标偏移目标为零
            targetMouseOffset = Vector2.zero;
        }
        else
        {
            Vector2 rawMouseOffset = GetMouseWorldOffset(target.position);

            float offsetMag = rawMouseOffset.magnitude;
            if (offsetMag > mouseInfluenceRadius)
            {
                rawMouseOffset = rawMouseOffset.normalized * mouseInfluenceRadius;
            }

            targetMouseOffset = rawMouseOffset * mouseOffsetMultiplier;
        }

        // 鼠标偏移平滑：聚焦时用 focusSmoothTime，正常时用 mouseSmoothTime
        float currentMouseSmooth = _focusMode ? focusSmoothTime : mouseSmoothTime;
        float mouseT = 1f - Mathf.Exp(-(1f / currentMouseSmooth) * Time.deltaTime);
        currentMouseOffset = Vector2.Lerp(currentMouseOffset, targetMouseOffset, mouseT);

        // ==================== 3. 合成最终位置 ====================
        Vector2 finalPos = baseRenderPos + currentMouseOffset;
        transform.position = new Vector3(finalPos.x, cameraHeight, finalPos.y - cameraZOffset);
    }

    private Vector2 GetMouseWorldOffset(Vector3 playerWorldPos)
    {
        Vector2 mouseScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);

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