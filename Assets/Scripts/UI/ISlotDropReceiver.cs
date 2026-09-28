/// <summary>
/// 槽位放置接管接口：某个格子实现本接口后，DragManager 会在执行"默认移动/交换"之前
/// 先询问它，返回 true 表示本次放置已被该格子完全接管（DragManager 不再搬运物品）。
/// 目前仅快捷栏格（HotbarBindSlotHandler）实现：拖入只建立绑定、不移动源物品。
/// </summary>
public interface ISlotDropReceiver
{
    /// <summary>
    /// 尝试接管一次放置。返回 true = 已处理（无论成功与否都不要走默认移动逻辑）。
    /// </summary>
    /// <param name="sourceContainer">拖拽源容器</param>
    /// <param name="sourceIndex">拖拽源索引</param>
    bool TryReceiveDrop(IItemContainer sourceContainer, int sourceIndex);
}
