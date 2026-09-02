public interface IItemContainer
{
    ItemInstance GetItem(int index);
    void SetItem(int index, ItemInstance item);
    int SlotCount { get; }
}