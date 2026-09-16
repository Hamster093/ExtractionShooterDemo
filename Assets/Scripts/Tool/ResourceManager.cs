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
    /// 按 iconKey 从 Assets/Resources/UI/ 下加载 Sprite。
    /// 物品图标统一以 "icon_" 前缀命名（见 Resources/XML/Items.xml 的 iconKey 字段），
    /// 非 icon_ 前缀的调用会被视为错误并警告，避免再出现"用名称/文件名当路径"的隐式约定。
    /// </summary>
    /// <param name="iconKey">物品 iconKey（不含扩展名，不含 "UI/" 前缀，通常形如 icon_xxx）</param>
    /// <returns>加载成功返回 Sprite，失败返回 null</returns>
    public static Sprite LoadUISpriteByIconKey(string iconKey)
    {
        if (string.IsNullOrEmpty(iconKey))
        {
            Debug.LogWarning("[ResourceManager] LoadUISpriteByIconKey: 传入的 iconKey 为空");
            return null;
        }

        // 用 iconKey 加载时应带 icon_ 前缀；裸文件名/名称调用说明用法不对，直接报错提示
        if (!iconKey.StartsWith("icon_", System.StringComparison.Ordinal))
        {
            Debug.LogError($"[ResourceManager] LoadUISpriteByIconKey: 「{iconKey}」不是合法的 iconKey（缺少 icon_ 前缀），" +
                           "物品图标请用 Items.xml 中的 iconKey 字段加载");
            return null;
        }

        string fullPath = UI_SPRITE_PATH + iconKey;
        Sprite sprite = Resources.Load<Sprite>(fullPath);

        if (sprite == null)
        {
            Debug.LogError($"[ResourceManager] 未找到 Sprite: {fullPath}");
        }

        return sprite;
    }
}