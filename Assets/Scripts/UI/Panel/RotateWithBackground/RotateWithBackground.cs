/****************************************************
    文件：RotateWithBackground.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-29 13:40:29
	功能：搜索时自动旋转
*****************************************************/

using UnityEngine;


public class RotateWithBackground : MonoBehaviour
{
    [Header("引用设置")]
    [Tooltip("场景中的背景图GameObject")]
    public GameObject backgroundImage;

    [Header("旋转参数")]
    [Tooltip("绕Z轴旋转速度（度/秒）")]
    public float rotateSpeed = 90f;

    private void Update()
    {
        // 核心逻辑：仅当背景图存在且处于激活状态时才旋转
        if (backgroundImage != null && backgroundImage.activeInHierarchy)
        {
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
        }
        // 背景图隐藏后自动停止，无需额外else处理
    }
}