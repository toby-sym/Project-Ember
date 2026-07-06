namespace ProjectEmber.Shared
{
    public interface IItemInventory
    {
        bool TryAddItem(ItemType type, int quantity);
        bool HasItem(ItemType type);
    }
}