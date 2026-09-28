/****************************************************
    文件：VisionCone.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-20 15:34:06
	功能：检查敌人是否在视线内
*****************************************************/

using UnityEngine;

public class VisionCone : MonoBehaviour
{
    [SerializeField] public float viewDistance = 15f;
    [SerializeField] public float fov = 90f;
    [SerializeField] public float nearRadius = 7f;       
    [SerializeField] public Transform eyePoint;
    [SerializeField] public LayerMask obstacleMask;

    public bool CanSee(Transform enemy)
    {
        Vector3 toEnemy = enemy.position - transform.position;
        toEnemy.y = 0f;                                   // 只算水平，防止高低差影响
        float sqrDist = toEnemy.sqrMagnitude;

        // 超出最大视距 → 直接看不见
        if (sqrDist > viewDistance * viewDistance) return false;

        // ⬇ 近身半径内 → 无视角度
        bool inNearRange = sqrDist <= nearRadius * nearRadius;

        if (!inNearRange)
        {
            // 角度检查只对锥形区域生效
            Vector3 dir = toEnemy.normalized;
            if (Vector3.Angle(transform.forward, dir) > fov * 0.5f)
                return false;
        }

        // 遮挡检查
        if (Physics.Linecast(eyePoint.position, enemy.position, obstacleMask))
            return false;

        return true;
    }
}