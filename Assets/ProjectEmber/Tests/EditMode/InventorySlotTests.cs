using NUnit.Framework;
using ProjectEmber.Gameplay;
using ProjectEmber.Shared;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class InventorySlotTests
    {
        [Test]
        public void ConstructorStoresTypeAndQuantity()
        {
            var slot = new InventorySlot(ItemType.Logs, 5);

            Assert.AreEqual(ItemType.Logs, slot.Type);
            Assert.AreEqual(5, slot.Quantity);
            Assert.IsFalse(slot.IsEmpty);
        }

        [Test]
        public void DefaultSlotIsEmpty()
        {
            var slot = default(InventorySlot);

            Assert.IsTrue(slot.IsEmpty);
        }

        [Test]
        public void NonPositiveQuantityIsConsideredEmpty()
        {
            Assert.IsTrue(new InventorySlot(ItemType.Axe, 0).IsEmpty);
            Assert.IsTrue(new InventorySlot(ItemType.Axe, -3).IsEmpty);
        }
    }
}
