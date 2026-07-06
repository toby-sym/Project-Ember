using System;
using ProjectEmber.Shared;

namespace ProjectEmber.Gameplay
{
    [Serializable]
    public struct InventorySlot
    {
        public ItemType Type;
        public int Quantity;

        public bool IsEmpty => Quantity <= 0;

        public InventorySlot(ItemType type, int quantity)
        {
            Type = type;
            Quantity = quantity;
        }
    }
}
