using System;
using ProjectEmber.Shared;

namespace ProjectEmber.Gameplay
{
    public sealed class InventorySystem : IItemInventory
    {
        private readonly InventorySlot[] slots;

        public event Action OnInventoryChanged;

        public int SlotCount => slots.Length;
        public InventorySlot[] Slots => slots;

        public InventorySystem(int slotCount)
        {
            slots = new InventorySlot[Math.Max(1, slotCount)];
        }

        public bool TryAddItem(ItemType type, int quantity)
        {
            if (quantity <= 0)
            {
                return false;
            }

            for (var i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty && slots[i].Type == type)
                {
                    slots[i].Quantity += quantity;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }

            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty)
                {
                    slots[i] = new InventorySlot(type, quantity);
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }

            return false;
        }

        public bool RemoveItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length || slots[slotIndex].IsEmpty)
            {
                return false;
            }

            slots[slotIndex] = default;
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool HasItem(ItemType type)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty && slots[i].Type == type)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
