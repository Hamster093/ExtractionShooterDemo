/****************************************************
    文件：ItemRegistry.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-01 15:59:21
	功能：物品注册表
*****************************************************/

using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public static class ItemRegistry
{
    private static readonly Dictionary<int, ItemData> _items = new();
    private static bool _isInitialized = false;//是否已初始化

    /// <summary>
    /// 已注册物品总数（方便Editor窗口快速查看）
    /// </summary>
    public static int Count => _items.Count;

    /// <summary>
    /// 从 XML 文件初始化注册表（在游戏启动时调用一次）
    /// </summary>
    /// <param name="xmlPath">XML 文件路径，Assets/Resources/XML/Items.xml</param>
    public static void InitializeFromXml(string xmlPath)
    {
        if (_isInitialized)
        {
            Debug.LogWarning("[ItemRegistry] 已初始化，跳过重复加载");
            return;
        }

        if (!File.Exists(xmlPath))
        {
            Debug.LogError($"[ItemRegistry] XML 文件不存在: {xmlPath}");
            return;
        }

        try
        {
            var serializer = new XmlSerializer(typeof(ItemDataList));
            using var stream = new FileStream(xmlPath, FileMode.Open);
            var dataList = (ItemDataList)serializer.Deserialize(stream);

            _items.Clear();
            foreach (var item in dataList.Items)
            {
                if (_items.ContainsKey(item.id))
                    Debug.LogError($"[ItemRegistry] ⚠️ 重复ID: {item.id} ({item.itemName})");
                else
                    _items[item.id] = item;
            }

            _isInitialized = true;
            Debug.Log($"[ItemRegistry] ✅ 成功加载 {_items.Count} 个物品");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ItemRegistry] ❌ XML 解析失败: {e.Message}");
        }
    }

    /// <summary>
    /// 从 XML 字符串内容初始化
    /// </summary>
    public static void InitializeFromXmlContent(string xmlContent)
    {
        if (_isInitialized)
        {
            Debug.LogWarning("[ItemRegistry] 已初始化，跳过重复加载");
            return;
        }

        try
        {
            var serializer = new XmlSerializer(typeof(ItemDataList));
            using var reader = new System.IO.StringReader(xmlContent);
            var dataList = (ItemDataList)serializer.Deserialize(reader);

            _items.Clear();
            foreach (var item in dataList.Items)
            {
                if (_items.ContainsKey(item.id))
                    Debug.LogError($"[ItemRegistry] ⚠️ 重复ID: {item.id} ({item.itemName})");
                else
                    _items[item.id] = item;
            }

            _isInitialized = true;
            Debug.Log($"[ItemRegistry] ✅ 成功加载 {_items.Count} 个物品");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ItemRegistry] ❌ XML 解析失败: {e.Message}");
        }
    }

    /// <summary>
    /// 运行时安全获取物品
    /// </summary>
    public static ItemData Get(int id)
    {
        if (!_isInitialized)
            Debug.LogError("[ItemRegistry] 未初始化！请先调用 InitializeFromXml()");

        if (_items.TryGetValue(id, out var item)) return item;

        Debug.LogError($"[ItemRegistry] 未知物品ID: {id}");
        return null;
    }

    /// <summary>
    /// 检查物品是否存在
    ///</summary>
    public static bool Contains(int id) => _items.ContainsKey(id);

    /// <summary>
    /// 获取所有物品（只读）
    ///</summary>
    public static IReadOnlyDictionary<int, ItemData> All => _items;
}