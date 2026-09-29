/****************************************************
    文件：LootPickup.cs
    作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-08-31 17:20:00
    功能：战利品拾取物（玩家靠近时在物体旁显示交互按钮，支持F键/鼠标点击触发）
*****************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// 战利品拾取物
/// 玩家进入触发范围后在物体旁显示“战利品”按钮，离开后隐藏
/// 交互方式：鼠标点击按钮，或玩家在范围内按下 F 键（两者都触发同一个点击事件）
/// </summary>
public class LootPickup : InteractableBase
{
    [Header("销毁")]
    [Tooltip("全部搜完后、面板关闭多久，箱子自动销毁（秒）。<=0 表示永不销毁")]
    [SerializeField] private float _destroyDelay = 5f;

    private ItemContainer _container;
    private bool _dataSent = false;//是否已经发送数据到UI
    private Coroutine _recycleRoutine;


    public void SetContainer(ItemContainer container)
    {
        _container = container;
        _dataSent = false;
    }

  
    protected override void OnInteract()
    {
        if (!_dataSent)
        {
            UIController.Instance.OpenLoot(_container);
            _dataSent = true;
        }
        else
        {
            UIController.Instance.OpenLoot();
        }

        // 告诉 LootPanel 当前打开的是哪个箱子，方便关闭时回调
        LootPanel.SetCurrentPickup(this);

        // 打开瞬间取消待销毁（防止玩家正在看的时候箱子没了）
        CancelRecycle();
    }

    public void OnPanelClosed()
    {
        if (_destroyDelay <= 0f) return;              // 未开启销毁
        if (_container == null) return;
        if (!_container.HasSearched) return;          // 没搜完，留着下次来搜
        if (_recycleRoutine != null) return;          // 已在倒计时

        _recycleRoutine = StartCoroutine(RecycleAfterDelay());
    }

    private void CancelRecycle()
    {
        if (_recycleRoutine != null)
        {
            StopCoroutine(_recycleRoutine);
            _recycleRoutine = null;
        }
    }

    private IEnumerator RecycleAfterDelay()
    {
        yield return new WaitForSeconds(_destroyDelay);
        _recycleRoutine = null;

        // 如果这期间又被打开，取消销毁（保险，虽然 OnInteract 已处理）
        // 简易判断：如果 LootPanel 当前指向自己，说明还开着
        if (LootPanel.CurrentPickup == this) yield break;

        Destroy(gameObject);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // 避免 LootPanel 持有已销毁的引用
        LootPanel.ClearCurrentPickup(this);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 可选：在 Scene 里画个圈，方便看销毁范围
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
#endif
}