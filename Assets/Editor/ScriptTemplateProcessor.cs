using UnityEditor;
using System.IO;
using System;
using UnityEngine;

public class ScriptTemplateProcessor : AssetModificationProcessor
{
    public static void OnWillCreateAsset(string path)
    {
        if (!path.EndsWith(".cs")) return;

        // 等待文件创建完成后处理
        EditorApplication.delayCall += () =>
        {
            string fullPath = Path.Combine(Application.dataPath, "..", path);
            if (!File.Exists(fullPath)) return;

            string content = File.ReadAllText(fullPath);
            // 用当前时间替换所有无法识别的日期占位符
            content = content.Replace("#DATE#", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            content = content.Replace("#CREATIONDATE#", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            content = content.Replace("#CREATION_DATE#", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            File.WriteAllText(fullPath, content);
            AssetDatabase.Refresh();
        };
    }
}