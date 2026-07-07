using NUnit.Framework;
using ProjectEmber.Gameplay;
using ProjectEmber.Shared;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class InventorySystemTests
    {
        [Test]
        public void ConstructorClampsSlotCountToAtLeastOne()
        {
            var inventory = new InventorySystem(0);

            Assert.AreEqual(1, inventory.SlotCount);
            Assert.AreEqual(1, inventory.Slots.Length);
        }

        [Test]
        public void TryAddItemPlacesItemInFirstEmptySlot()
        {
            var inventory = new InventorySystem(3);

            var added = inventory.TryAddItem(ItemType.Axe, 2);

            Assert.IsTrue(added);
            Assert.AreEqual(ItemType.Axe, inventory.Slots[0].Type);
            Assert.AreEqual(2, inventory.Slots[0].Quantity);
            Assert.IsTrue(inventory.Slots[1].IsEmpty);
        }

        [Test]
        public void TryAddItemStacksOntoExistingMatchingSlot()
        {
            var inventory = new InventorySystem(3);
            inventory.TryAddItem(ItemType.Logs, 3);

            var added = inventory.TryAddItem(ItemType.Logs, 4);

            Assert.IsTrue(added);
            Assert.AreEqual(7, inventory.Slots[0].Quantity);
            Assert.IsTrue(inventory.Slots[1].IsEmpty);
        }

        [Test]
        public void TryAddItemRejectsNonPositiveQuantity()
        {
            var inventory = new InventorySystem(3);

            Assert.IsFalse(inventory.TryAddItem(ItemType.Berries, 0));
            Assert.IsFalse(inventory.TryAddItem(ItemType.Berries, -5));
            Assert.IsTrue(inventory.Slots[0].IsEmpty);
        }

        [Test]
        public void TryAddItemReturnsFalseWhenInventoryIsFull()
        {
            var inventory = new InventorySystem(2);
            inventory.TryAddItem(ItemType.Axe, 1);
            inventory.TryAddItem(ItemType.Pickaxe, 1);

            var added = inventory.TryAddItem(ItemType.Sword, 1);

            Assert.IsFalse(added);
            Assert.IsFalse(inventory.HasItem(ItemType.Sword));
        }

        [Test]
        public void TryAddItemStacksIntoFullInventoryWhenTypeAlreadyPresent()
        {
            var inventory = new InventorySystem(1);
            inventory.TryAddItem(ItemType.Logs, 1);

            var added = inventory.TryAddItem(ItemType.Logs, 5);

            Assert.IsTrue(added);
            Assert.AreEqual(6, inventory.Slots[0].Quantity);
        }

        [Test]
        public void RemoveItemClearsPopulatedSlot()
        {
            var inventory = new InventorySystem(3);
            inventory.TryAddItem(ItemType.Axe, 1);

            var removed = inventory.RemoveItem(0);

            Assert.IsTrue(removed);
            Assert.IsTrue(inventory.Slots[0].IsEmpty);
            Assert.IsFalse(inventory.HasItem(ItemType.Axe));
        }

        [Test]
        public void RemoveItemReturnsFalseForEmptyOrOutOfRangeSlots()
        {
            var inventory = new InventorySystem(2);

            Assert.IsFalse(inventory.RemoveItem(0));
            Assert.IsFalse(inventory.RemoveItem(-1));
            Assert.IsFalse(inventory.RemoveItem(5));
        }

        [Test]
        public void HasItemReflectsInventoryContents()
        {
            var inventory = new InventorySystem(3);
            inventory.TryAddItem(ItemType.Pickaxe, 1);

            Assert.IsTrue(inventory.HasItem(ItemType.Pickaxe));
            Assert.IsFalse(inventory.HasItem(ItemType.Sword));
        }

        [Test]
        public void MutatingOperationsRaiseInventoryChangedEvent()
        {
            var inventory = new InventorySystem(2);
            var invocations = 0;
            inventory.OnInventoryChanged += () => invocations++;

            inventory.TryAddItem(ItemType.Axe, 1);
            inventory.TryAddItem(ItemType.Axe, 1);
            inventory.RemoveItem(0);

            Assert.AreEqual(3, invocations);
        }

        [Test]
        public void RestoreSlotsOverwritesContentsAndClearsExtraSlots()
        {
            var inventory = new InventorySystem(3);
            inventory.TryAddItem(ItemType.Axe, 1);

            inventory.RestoreSlots(new[]
            {
                new InventorySlot(ItemType.Logs, 4),
                new InventorySlot(ItemType.Berries, 2)
            });

            Assert.AreEqual(ItemType.Logs, inventory.Slots[0].Type);
            Assert.AreEqual(4, inventory.Slots[0].Quantity);
            Assert.AreEqual(ItemType.Berries, inventory.Slots[1].Type);
            Assert.IsTrue(inventory.Slots[2].IsEmpty);
        }

        [Test]
        public void RejectedOperationsDoNotRaiseInventoryChangedEvent()
        {
            var inventory = new InventorySystem(1);
            var invocations = 0;
            inventory.OnInventoryChanged += () => invocations++;

            inventory.TryAddItem(ItemType.Axe, 0);
            inventory.RemoveItem(0);

            Assert.AreEqual(0, invocations);
        }
    }
}
