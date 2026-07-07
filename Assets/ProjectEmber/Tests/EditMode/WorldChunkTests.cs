using NUnit.Framework;
using ProjectEmber.World;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class WorldChunkTests
    {
        [Test]
        public void ToIndexMapsRowMajorCoordinates()
        {
            Assert.AreEqual(0, WorldChunk.ToIndex(0, 0));
            Assert.AreEqual(3, WorldChunk.ToIndex(3, 0));
            Assert.AreEqual(WorldChunk.Size, WorldChunk.ToIndex(0, 1));
            Assert.AreEqual(WorldChunk.Size + 5, WorldChunk.ToIndex(5, 1));
        }

        [Test]
        public void SetTileThenGetTileRoundTripsData()
        {
            var chunk = new WorldChunk(new Vector2Int(1, 1));
            var tile = new TileData(TileType.DeepStone, 4, 1234);

            chunk.SetTile(2, 3, tile);
            var read = chunk.GetTile(2, 3);

            Assert.AreEqual(tile.BaseType, read.BaseType);
            Assert.AreEqual(tile.Durability, read.Durability);
            Assert.AreEqual(tile.OccupantId, read.OccupantId);
        }

        [Test]
        public void GetTilesReturnsBackingArrayOfExpectedSize()
        {
            var chunk = new WorldChunk(Vector2Int.zero);

            Assert.AreEqual(WorldChunk.Size * WorldChunk.Size, chunk.GetTiles().Length);
        }

        [Test]
        public void CoordinatesAreExposedFromConstructor()
        {
            var coords = new Vector2Int(-4, 9);

            var chunk = new WorldChunk(coords);

            Assert.AreEqual(coords, chunk.Coordinates);
        }
    }
}
