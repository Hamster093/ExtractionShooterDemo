/****************************************************
    文件：ExtractionEndPanel.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-17
	功能：撤离结算面板（撤离倒计时结束弹出，点击确认回 Concealment 场景）
	说明：确认后经 SceneLoader.LoadSceneWithPlayerState 保存玩家状态（血量/装备栏/弹匣/激活栏位）
	      并重载 Concealment 场景；背包/仓库由常驻 InventoryService 自动保留，无需额外处理。
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 撤离结算面板：由撤离点（ExtractionPoint）倒计时结束后经 UIController.OpenExtractionEnd 打开。
/// 点击【确认】→ SceneLoader.LoadSceneWithPlayerState("Concealment")：
/// 先保存玩家状态再加载场景，新场景 Player 生成后由 PlayerStateData.RestoreEquipment 自动恢复武器栏。
/// ESC 不关闭本面板（撤离结算必须点确认，避免误按丢失结算流程）。
/// </summary>
public class ExtractionEndPanel : BaseUIPanel
{
    [Header("引用（由 ExtractionUIBuilder 自动绑定）")]
    [Tooltip("确认按钮：点击保存状态并回到 Concealment 场景")]
    [SerializeField] private Button confirmButton;

    public override UIPriority Priority => UIPriority.GameEnd;

    private void Awake()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(HandleConfirm);
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(HandleConfirm);
    }

    /// <summary>
    /// ESC 不关闭撤离结算面板（只能点确认按钮结束）
    /// </summary>
    public override void OnEscapePressed()
    {
        // 空实现：撤离结算必须通过确认按钮结束
    }

    /// <summary>
    /// 确认撤离：保存玩家状态（血量/装备栏/弹匣/激活栏位）并重载 Concealment 场景。
    /// 背包/仓库数据由常驻 InventoryService（DontDestroyOnLoad）自动保留。
    /// </summary>
    private void HandleConfirm()
    {
        SceneLoader.LoadSceneWithPlayerState("Concealment");
    }
}
