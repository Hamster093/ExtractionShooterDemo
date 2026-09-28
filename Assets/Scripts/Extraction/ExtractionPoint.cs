/****************************************************
    文件：ExtractionPoint.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-17
	功能：撤离点（玩家站上自动触发撤离倒计时，结束弹出结算面板）
	说明：参考宝箱 LootPickup 的触发器模式（BoxCollider isTrigger + Player 标签检测），
	      区别是无需按键：玩家进入撤离区即自动开始倒计时（显示在 HUD 上），离开则取消。
*****************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 撤离点：玩家进入触发区自动开始撤离倒计时（默认 5 秒），倒计时显示在 HUD 上；
/// 期间离开触发区则取消倒计时。倒计时结束 → 打开撤离结算面板（ExtractionEndPanel），
/// 点击确认后回到 Concealment 场景（背包/仓库由常驻 InventoryService 保留，
/// 武器栏/血量/弹匣由 SceneLoader → PlayerStateData 保存恢复）。
/// 挂载要求：本物体上必须有 isTrigger 的 Collider（由 ExtractionUIBuilder 生成）。
/// </summary>
public class ExtractionPoint : MonoBehaviour
{
    [Header("识别设置")]
    [Tooltip("玩家标签（与 InteractableBase 一致）")]
    [SerializeField] private string _playerTag = "Player";

    [Header("倒计时")]
    [Tooltip("撤离倒计时时长（秒）")]
    [SerializeField] private float _countdownSeconds = 5f;

    [Header("引用（由 ExtractionUIBuilder 自动绑定）")]
    [Tooltip("HUD 上的撤离倒计时文本")]
    [SerializeField] private Text _countdownText;

    /// <summary>倒计时是否正在进行中</summary>
    public bool IsCountingDown => _countdownCoroutine != null;

    /// <summary>剩余秒数（未在倒计时时为 -1）</summary>
    public float RemainingSeconds { get; private set; } = -1f;

    private Coroutine _countdownCoroutine;
    private bool _extractionFinished; // 本次撤离已结算，禁止重复触发（结算面板只能点确认关闭）

    private void OnTriggerEnter(Collider other)
    {
        if (_extractionFinished) return;
        if (!other.CompareTag(_playerTag)) return;

        StartCountdown();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;

        // 玩家离开撤离区 → 取消倒计时
        CancelCountdown();
    }

    /// <summary>
    /// 开始撤离倒计时（已在倒计时中则忽略）
    /// </summary>
    private void StartCountdown()
    {
        if (_countdownCoroutine != null) return;
        _countdownCoroutine = StartCoroutine(CountdownRoutine());
    }

    /// <summary>
    /// 取消倒计时（离开触发区 / 倒计时结束）
    /// </summary>
    private void CancelCountdown()
    {
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }
        RemainingSeconds = -1f;
        HideCountdownText();
    }

    private IEnumerator CountdownRoutine()
    {
        float remaining = _countdownSeconds;
        ShowCountdownText();

        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            RemainingSeconds = Mathf.Max(remaining, 0f);

            if (_countdownText != null)
                _countdownText.text = $"撤离倒计时：{Mathf.CeilToInt(RemainingSeconds)}";

            yield return null;
        }

        _countdownCoroutine = null;
        RemainingSeconds = -1f;
        HideCountdownText();

        // 倒计时结束：弹出撤离结算面板（确认后由面板切回 Concealment 场景）
        _extractionFinished = true;
        if (UIController.Instance != null)
        {
            UIController.Instance.OpenExtractionEnd();
        }
        else
        {
            Debug.LogWarning("[ExtractionPoint] 场景中没有 UIController，无法打开撤离结算面板");
        }
    }

    private void ShowCountdownText()
    {
        if (_countdownText != null && !_countdownText.gameObject.activeSelf)
            _countdownText.gameObject.SetActive(true);
    }

    private void HideCountdownText()
    {
        if (_countdownText != null && _countdownText.gameObject.activeSelf)
            _countdownText.gameObject.SetActive(false);
    }
}
