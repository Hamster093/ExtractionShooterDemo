/****************************************************
    文件：VisionManager.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-20 15:37:49
	功能：玩家视锥管理器
*****************************************************/

using System.Collections.Generic;
using UnityEngine;

public class VisionManager : MonoBehaviour 
{
    private VisionCone vision;
    [SerializeField] private float checkInterval = 0.1f; // 每 0.1 秒检测一次，省性能

    private readonly List<EnemyController> _enemiesInRange = new List<EnemyController>();
    private float _timer;

    private void Awake()
    {
        if (vision == null)
            vision = GetComponent<VisionCone>();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < checkInterval) return;
        _timer = 0f;

        CheckEnemiesVisibility();
    }

    public void CheckEnemiesVisibility()
    {
        // 倒序遍历，方便处理列表中可能被销毁的对象
        for (int i = _enemiesInRange.Count - 1; i >= 0; i--)
        {
            var enemy = _enemiesInRange[i];

            // 敌人被销毁或已禁用，移除
            if (enemy == null)
            {
                _enemiesInRange.RemoveAt(i);
                continue;
            }

            bool visible = vision.CanSee(enemy.transform);
            enemy.SetVisible(visible);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.GetComponentInParent<EnemyController>();
        if (enemy != null && !_enemiesInRange.Contains(enemy))
        {
            _enemiesInRange.Add(enemy);
            enemy.SetVisible(vision.CanSee(enemy.transform));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var enemy = other.GetComponentInParent<EnemyController>();
        if (enemy != null)
        {
            _enemiesInRange.Remove(enemy);
            // 离开范围时强制隐藏，避免残留
            enemy.SetVisible(false);
        }
    }
}