/****************************************************
    文件：LoginPanel.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-12 18:32:00
	功能：账号登录/注册面板（提示信息统一走 Toast 通知）
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 账号登录/注册面板：
/// 1. 登录按钮：校验账号密码（DBMsg.Login），失败弹出 Toast"账号或者密码不正确"
/// 2. 注册按钮：DBMsg.Register 插入新账号，已存在弹 Toast"用户名已存在"，成功弹"注册成功"
/// 3. 本地游玩按钮：跳过登录/注册直接进入游戏场景（不连接数据库、不实现存档）
/// 4. 登录成功后调用 GameService.InitGame 初始化全局数据，并打开主菜单
/// </summary>
public class LoginPanel : MonoBehaviour
{
    [Header("输入")]
    [SerializeField] private InputField accountInput;   // 账号输入框
    [SerializeField] private InputField passwordInput;  // 密码输入框

    [Header("按钮")]
    [SerializeField] private Button loginButton;        // 登录按钮
    [SerializeField] private Button registerButton;     // 注册按钮
    [SerializeField] private Button localPlayButton;    // 本地游玩按钮（跳过登录直接进游戏，不实现存档）

    [Header("跳转目标")]
    [SerializeField] private GameObject mainMenuPanel;  // 登录成功后的主菜单面板
    [Tooltip("本地游玩直接进入的游戏场景名（须已加入 Build Settings）")]
    [SerializeField] private string localPlaySceneName = "Concealment";

    /// <summary>
    /// 数据库是否已初始化（防止重复 Init 建立多个连接）
    /// </summary>
    private static bool s_dbInited = false;

    private void Awake()
    {
        if (loginButton != null) loginButton.onClick.AddListener(OnLoginClicked);
        if (registerButton != null) registerButton.onClick.AddListener(OnRegisterClicked);
        if (localPlayButton != null) localPlayButton.onClick.AddListener(OnLocalPlayClicked);
        if (passwordInput != null) passwordInput.contentType = InputField.ContentType.Password;
    }

    private void OnApplicationQuit()
    {
        // 应用退出时关闭数据库连接
        DBMsg.Instance.Close();
    }

    /// <summary>
    /// 登录按钮点击回调
    /// </summary>
    public void OnLoginClicked()
    {
        string acct = accountInput != null ? accountInput.text.Trim() : "";
        string pass = passwordInput != null ? passwordInput.text.Trim() : "";

        if (string.IsNullOrEmpty(acct) || string.IsNullOrEmpty(pass))
        {
            ToastManager.ShowMessage("请输入账号和密码");
            return;
        }

        EnsureDbReady();

        // 登录只校验，不会自动创建账号；失败统一提示账号或密码不正确
        PlayerData playerData = DBMsg.Instance.Login(acct, pass);

        if (playerData != null)
        {
            OnLoginSuccess(playerData);
        }
        else
        {
            ToastManager.ShowMessage("账号或者密码不正确");
        }
    }

    /// <summary>
    /// 注册按钮点击回调
    /// </summary>
    public void OnRegisterClicked()
    {
        string acct = accountInput != null ? accountInput.text.Trim() : "";
        string pass = passwordInput != null ? passwordInput.text.Trim() : "";

        if (string.IsNullOrEmpty(acct) || string.IsNullOrEmpty(pass))
        {
            ToastManager.ShowMessage("请输入账号和密码");
            return;
        }

        EnsureDbReady();

        // 注册：-2 账号已存在；>0 成功（返回新账号 id）；-1 插入失败
        int result = DBMsg.Instance.Register(acct, pass);

        if (result == -2)
        {
            ToastManager.ShowMessage("用户名已存在");
        }
        else if (result > 0)
        {
            ToastManager.ShowMessage("注册成功");
        }
        else
        {
            ToastManager.ShowMessage("注册失败，请检查数据库连接");
        }
    }

    /// <summary>
    /// 本地游玩按钮点击回调：跳过登录/注册，直接进入游戏场景。
    /// 不连接数据库、不设置 CurrentPlayer（存档按钮会提示"当前未登录，无法存档"），
    /// 进入前清空上次会话残留的跨场景状态（PlayerStateData），保证全新开局。
    /// </summary>
    public void OnLocalPlayClicked()
    {
        if (string.IsNullOrEmpty(localPlaySceneName))
        {
            ToastManager.ShowMessage("未配置本地游玩场景名");
            return;
        }

        // 清空上次会话残留的玩家状态（血量/装备栏/弹匣等），避免串档；
        // 注意不能省：StartNewGame 本身不清（登录读档依赖它保留暂存数据）
        PlayerStateData.Clear();

        Debug.Log($"[LoginPanel] 本地游玩：直接进入场景 {localPlaySceneName}");
        SceneLoader.StartNewGame(localPlaySceneName);
    }

    /// <summary>
    /// 登录成功后的扩展点（虚方法，可重写）。
    /// 登录成功后：初始化全局数据 → 从数据库读取该玩家存档（背包/仓库/装备栏/血量等）。
    /// 只有"主菜单登录"会走到这里；直接从游戏场景（如 Concealment）Play 不会触发读档。
    /// </summary>
    protected virtual void OnLoginSuccess(PlayerData playerData)
    {
        // 初始化全局玩家数据（GameService 中已有 TODO：从数据库加载更多数据）
        GameService.InitGame(playerData);

        // 先清空上次会话的跨场景状态（避免残留），再读档：
        // LoadGame 会把读档数据暂存到 PlayerStateData，进游戏场景后由 PlayerController.Start 灌入容器
        PlayerStateData.Clear();
        SaveGameService.LoadGame(playerData.id);

        // 打开主菜单
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 确保数据库连接已初始化（只连接一次）
    /// </summary>
    private void EnsureDbReady()
    {
        if (s_dbInited) return;
        DBMsg.Instance.Init();
        s_dbInited = true;
    }
}
