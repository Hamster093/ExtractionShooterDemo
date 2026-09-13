/****************************************************
    文件：WeaponFactory.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-04 18:31:49
	功能：武器工厂类
*****************************************************/

using UnityEngine;

public static class WeaponFactory
{
    public static WeaponBase CreateWeapon(int itemID)
    {
        // 示例：从 Resources 加载预制体，路径按需调整
        string path = $"Weapons/Weapon_{itemID}";
        WeaponBase prefab = Resources.Load<WeaponBase>(path);
        if (prefab == null)
        {
            Debug.LogWarning($"未找到武器预制体：{path}");
            return null;
        }
        return Object.Instantiate(prefab);
    }
}