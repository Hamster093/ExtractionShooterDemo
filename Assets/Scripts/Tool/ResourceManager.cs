/****************************************************
    文件：ResourceManager.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-01 17:11:13
	功能：加载资源管理类
*****************************************************/

using UnityEngine;

public static class ResourceManager
{
    private const string UI_SPRITE_PATH = "UI/";

    /// <summary>
    /// 从 Assets/Resources/UI/ 下加载 Sprite
    /// </summary>
    /// <param name="spriteName">图片名称（不含扩展名，不含 "UI/" 前缀）</param>
    /// <returns>加载成功返回 Sprite，失败返回 null</returns>
    public static Sprite LoadUISprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            Debug.LogWarning("[ResourceManager] LoadUISprite: 传入的名称为空");
            return null;
        }

        string fullPath = UI_SPRITE_PATH + spriteName;
        Sprite sprite = Resources.Load<Sprite>(fullPath);

        if (sprite == null)
        {
            Debug.LogError($"[ResourceManager] 未找到 Sprite: {fullPath}");
        }

        return sprite;
    }
}