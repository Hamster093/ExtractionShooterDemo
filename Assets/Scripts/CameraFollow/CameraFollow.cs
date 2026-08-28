/****************************************************
    文件：CameraFollow.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-28 15:47:46
	功能：相机跟随
*****************************************************/

using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float cameraHeight = 15f;
    [SerializeField] private float deadZone = 0.5f;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private float maxSpeed = 20f;

    private Vector2 velocity;
    private Vector2 smoothPos;

    private void Start()
    {
        if (target != null)
        {
            smoothPos = new Vector2(target.position.x, target.position.z);
            transform.position = new Vector3(smoothPos.x, cameraHeight, smoothPos.y);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector2 targetPos = new Vector2(target.position.x, target.position.z);

        // 1. SmoothDamp 永远只追真实目标，保证绝对平滑无抖动
        smoothPos = Vector2.SmoothDamp(smoothPos, targetPos, ref velocity, smoothTime, maxSpeed);

        // 2. 计算平滑位置与目标的差值
        Vector2 delta = targetPos - smoothPos;
        float deltaMag = delta.magnitude;

        // 3. ⭐ 核心：用纯数学钳制实现死区，不影响 smoothPos 和 velocity
        // 如果距离 < deadZone，相机就在 smoothPos（自然平滑）
        // 如果距离 > deadZone，相机被推到死区边缘
        Vector2 renderPos = (deltaMag > deadZone)
            ? targetPos - delta * (deadZone / deltaMag)
            : smoothPos;

        transform.position = new Vector3(renderPos.x, cameraHeight, renderPos.y);
    }
}