/****************************************************
    文件：GamePlayer.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-01 16:06:08
	功能：游戏启动器（物品注册表初始化）
	说明：初始化在场景加载前（BeforeSceneLoad）的静态入口执行，
	      保证早于场景内所有 MonoBehaviour 的 Awake（如 VendingMachineController 等）。
*****************************************************/

using UnityEngine;

/// <summary>
/// 游戏启动器：在场景加载前初始化 ItemRegistry（读取 Resources/XML/Items.xml）。
/// 使用 RuntimeInitializeOnLoadMethod(BeforeSceneLoad) 提高初始化权重，
/// 避免场景中其他脚本在 Awake 阶段访问 ItemRegistry 时尚未初始化。
/// 本类保留 MonoBehaviour 仅为兼容场景中已挂载的组件引用。
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitItemRegistry()
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