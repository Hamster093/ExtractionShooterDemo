/****************************************************
    文件：TitleScreenPanel.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-12 17:50:00
	功能：开始界面（标题图 + 游戏名称），点击任意处跳转登录界面
*****************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 开始界面面板：
/// 1. 全屏标题图（图内自带"点击屏幕继续"提示）
/// 2. 图片中央叠加一个游戏名称文本（可在 Inspector 修改 gameTitle）
/// 3. 玩家点击任意位置后隐藏自己并打开下一个面板（登录界面）
/// </summary>
public class TitleScreenPanel : MonoBehaviour, IPointerClickHandler
{
    [Header("游戏名称")]
    [Tooltip("图片中央的游戏名称文本组件")]
    [SerializeField] private Text gameTitleText;
    [Tooltip("游戏名称内容，修改后运行时会自动写入 gameTitleText")]
    [SerializeField] private string gameTitle = "逃离鸭科夫";

    [Header("跳转目标")]
    [Tooltip("点击后打开的下一个面板（登录界面）")]
    [SerializeField] private GameObject nextPanel;

    private void Awake()
    {
        // 把游戏名称写入中央文本组件
        if (gameTitleText != null && !string.IsNullOrEmpty(gameTitle))
        {
            gameTitleText.text = gameTitle;
        }
    }

    /// <summary>
    /// 点击屏幕回调
    /// </summary>
    public void OnClickContinue()
    {
        if (nextPanel != null)
        {
            nextPanel.SetActive(true);
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 事件系统点击回调：整张标题图即点击区域
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickContinue();
    }
}
