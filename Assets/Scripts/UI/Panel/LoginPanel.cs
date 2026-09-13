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
/// 3. 登录成功后调用 GameService.InitGame 初始化全局数据，并打开主菜单
/// </summary>
public class LoginPanel : MonoBehaviour
{
    [Header("输入")]
    [SerializeField] private InputField accountInput;   // 账号输入框
    [SerializeField] private InputField passwordInput;  // 密码输入框

    [Header("按钮")]
    [SerializeField] private Button loginButton;        // 登录按钮
    [SerializeField] private Button registerButton;     // 注册按钮

    [Header("跳转目标")]
    [SerializeField] private GameObject mainMenuPanel;  // 登录成功后的主菜单面板

    /// <summary>
    /// 数据库是否已初始化（防止重复 Init 建立多个连接）
    /// </summary>
    private static bool s_dbInited = false;

    private void Awake()
    {
        if (loginButton != null) loginButton.onClick.AddListener(OnLoginClicked);
        if (registerButton != null) registerButton.onClick.AddListener(OnRegisterClicked);
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
    /// 登录成功后的扩展点（虚方法，可重写）。
    /// TODO(扩展接口)：玩家的存档数据以后会写在数据库里，目前还没确定要读取哪些字段。
    /// 未来在这里加载该玩家的存档（背包、武器、位置、血量等），例如：
    ///     var saveData = SaveSystem.LoadPlayer(playerData.id);
    /// 相关的字段读取也可在 DBMsg.Login 的 TOADD 处补充。
    /// </summary>
    protected virtual void OnLoginSuccess(PlayerData playerData)
    {
        // 初始化全局玩家数据（GameService 中已有 TODO：从数据库加载更多数据）
        GameService.InitGame(playerData);

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
