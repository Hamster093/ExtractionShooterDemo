/****************************************************
    文件：BaseUIPanel.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-03 15:35:43
	功能：面板基类
*****************************************************/

using UnityEngine;

/// <summary>
/// 所有UI面板的基类，封装通用的显隐和鼠标逻辑
/// </summary>
public abstract class BaseUIPanel : MonoBehaviour, IUIPanel
{
    [Header("该面板打开时是否需要显示鼠标")]
    [SerializeField] protected bool requireCursor = true;

    public bool RequireCursor => requireCursor;

    public abstract UIPriority Priority { get; }

    /// <summary>
    /// 默认打开逻辑：激活物体
    /// </summary>
    public virtual void OnOpen()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 默认关闭逻辑：隐藏物体
    /// </summary>
    public virtual void OnClose()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 默认ESC行为：关闭自己
    /// </summary>
    public virtual void OnEscapePressed()
    {
        UIController.Instance.ClosePanel(this);
    }
}