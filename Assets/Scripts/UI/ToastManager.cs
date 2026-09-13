/****************************************************
    文件：ToastManager.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-12 18:32:00
	功能：Toast 通知管理器（队列按序显示，3 秒后隐藏）
*****************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Toast 通知管理器（单例）：
/// 1. 调用 ToastManager.ShowMessage("内容") 发送消息，消息进入待发送队列
/// 2. 每帧检测队列：没有在显示的消息时，取出队首消息显示
/// 3. 每条消息显示 3 秒后隐藏通知界面，再显示下一条（按加入顺序）
/// 4. 通知界面常驻所有界面之上（由编辑器工具放在 Canvas 最上层）
/// </summary>
public class ToastManager : MonoBehaviour
{
    public static ToastManager Instance { get; private set; }

    [Header("通知界面")]
    [SerializeField] private GameObject toastPanel; // 通知界面（默认隐藏）
    [SerializeField] private Text messageText;      // 通知文本

    /// <summary>
    /// 待发送消息队列（先进先出，按加入顺序显示）
    /// </summary>
    private readonly Queue<string> _queue = new Queue<string>();

    /// <summary>
    /// 当前是否正在显示消息
    /// </summary>
    private bool _isShowing = false;

    /// <summary>
    /// 当前消息剩余显示时间
    /// </summary>
    private float _hideTimer = 0f;

    /// <summary>
    /// 每条消息显示时长（秒）
    /// </summary>
    private const float ShowDuration = 3f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // 正在显示中：倒计时，结束后隐藏通知界面
        if (_isShowing)
        {
            _hideTimer -= Time.deltaTime;
            if (_hideTimer <= 0f)
            {
                _isShowing = false;
                if (toastPanel != null) toastPanel.SetActive(false);
            }
            return;
        }

        // 空闲且有消息：取出队首显示
        if (_queue.Count > 0)
        {
            string msg = _queue.Dequeue();
            if (messageText != null) messageText.text = msg;
            if (toastPanel != null) toastPanel.SetActive(true);

            _isShowing = true;
            _hideTimer = ShowDuration;
        }
    }

    /// <summary>
    /// 发送一条通知消息（入队，按顺序显示 3 秒）
    /// </summary>
    public static void ShowMessage(string message)
    {
        if (Instance == null)
        {
            Debug.LogWarning("[ToastManager] 场景中不存在 ToastManager，消息未显示：" + message);
            return;
        }
        Instance._queue.Enqueue(message);
    }
}
