/// <summary>
/// 任何可拖拽槽位的容器都必须实现此接口
/// </summary>
public interface ISlotOwner
{
    IItemContainer Container { get; }
    void RefreshSlot(int index);
}