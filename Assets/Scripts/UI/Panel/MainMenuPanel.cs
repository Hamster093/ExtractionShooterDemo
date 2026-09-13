/****************************************************
    文件：MainMenuPanel.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-12 17:50:00
	功能：主菜单面板（开始/继续游戏 + 退出游戏）
*****************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 主菜单面板：
/// 1. 顶部显示当前登录账号欢迎语
/// 2. 开始游戏按钮（有存档时显示"继续游戏"）
/// 3. 退出游戏按钮
/// </summary>
public class MainMenuPanel : MonoBehaviour
{
    [Header("欢迎语")]
    [SerializeField] private Text welcomeText;          // 顶部欢迎文本（显示当前登录账号，可空）

    [Header("按钮")]
    [SerializeField] private Button startButton;      // 开始/继续游戏按钮
    [SerializeField] private Button quitButton;       // 退出游戏按钮
    [SerializeField] private Text startButtonText;    // 开始按钮上的文字（用于切换"开始游戏/继续游戏"）

    [Header("场景")]
    [Tooltip("点击开始/继续游戏时加载的游戏场景名称，留空则点击时给出提示")]
    [SerializeField] private string gameSceneName = "";

    /// <summary>
    /// 存档标记的 PlayerPrefs 键（占位）
    /// TODO: 项目目前还没有存档系统，做真实存档时改成从存档系统读取
    /// </summary>
    private const string SaveKey = "GameSave";

    private void Awake()
    {
        // 运行时绑定按钮点击（Editor 一键搭建工具只负责填充字段引用）
        if (startButton != null) startButton.onClick.AddListener(OnStartGameClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnEnable()
    {
        // 每次面板显示时都刷新（比如从游戏返回主菜单后也能正确显示）
        RefreshStartButton();
        RefreshWelcome();
    }

    /// <summary>
    /// 根据是否有存档，刷新开始按钮文字
    /// </summary>
    private void RefreshStartButton()
    {
        if (startButtonText == null) return;
        startButtonText.text = HasSaveData() ? "继续游戏" : "开始游戏";
    }

    /// <summary>
    /// 刷新顶部欢迎语（显示当前登录账号）
    /// </summary>
    private void RefreshWelcome()
    {
        if (welcomeText == null) return;
        welcomeText.text = GameService.CurrentPlayer != null
            ? $"欢迎，{GameService.CurrentPlayer.name}"
            : "";
    }

    /// <summary>
    /// 是否有存档（占位实现）
    /// 目前项目暂无存档系统，先用 PlayerPrefs 是否有存档标记判断；
    /// 以后接入真实存档系统后，替换成真实的存档检测逻辑即可。
    /// </summary>
    private bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    /// <summary>
    /// 开始/继续游戏按钮点击回调：按名称加载游戏场景
    /// </summary>
    public void OnStartGameClicked()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[MainMenuPanel] 未填写游戏场景名称，请在 Inspector 的 gameSceneName 字段中填写");
            return;
        }
        Debug.Log($"[MainMenuPanel] 加载游戏场景：{gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// 退出游戏按钮点击回调
    /// </summary>
    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        Debug.Log("[MainMenuPanel] 编辑器模式下不会真正退出，请直接停止运行");
#else
        Application.Quit();
#endif
    }
}
