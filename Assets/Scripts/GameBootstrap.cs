/****************************************************
    文件：GamePlayer.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-01 16:06:08
	功能：Nothing
*****************************************************/

using System.IO;
using UnityEngine;


public class GameBootstrap : MonoBehaviour
{
    private void Awake()
    {
        // Resources.Load 自动从 Assets/Resources/ 下查找，不需要后缀
        var xmlAsset = Resources.Load<TextAsset>("XML/Items");

        if (xmlAsset == null)
        {
            Debug.LogError("[GameBootstrap] 未找到 XML/Items.xml，请确认文件位于 Assets/Resources/XML/ 目录下");
            return;
        }
        ItemRegistry.InitializeFromXmlContent(xmlAsset.text);
    }
}