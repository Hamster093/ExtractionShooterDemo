/****************************************************
    文件：NewMonoBehaviourScript.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：#DATE#
	功能：Nothing
*****************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteractable : InteractableBase
{
    [Header("场景")]
    [Tooltip("点击出发时加载的游戏场景名称")]
    [SerializeField] private string gameSceneName = "";

    private bool _isOpen = false;

    protected override void OnInteract()
    {
        _isOpen = !_isOpen;

        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[MainMenuPanel] 未填写游戏场景名称，请在 Inspector 的 gameSceneName 字段中填写");
            return;
        }
        Debug.Log($"[MainMenuPanel] 加载游戏场景：{gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }
}