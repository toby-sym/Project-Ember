using NUnit.Framework;
using ProjectEmber.World;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class TileDataTests
    {
        [Test]
        public void ConstructorDefaultsOccupantToZero()
        {
            var tile = new TileData(TileType.Grass, 2);

            Assert.AreEqual(TileType.Grass, tile.BaseType);
            Assert.AreEqual(2, tile.Durability);
            Assert.AreEqual(0, tile.OccupantId);
            Assert.IsFalse(tile.HasOccupant);
        }

        [Test]
        public void HasOccupantIsTrueForNonZeroOccupantId()
        {
            var tile = new TileData(TileType.Dirt, 3, 1042);

            Assert.IsTrue(tile.HasOccupant);
            Assert.AreEqual(1042, tile.OccupantId);
        }

        [Test]
        public void DefaultTileHasNoOccupant()
        {
            var tile = default(TileData);

            Assert.IsFalse(tile.HasOccupant);
        }
    }
}
