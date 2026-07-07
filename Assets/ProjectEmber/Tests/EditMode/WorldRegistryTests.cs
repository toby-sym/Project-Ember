using NUnit.Framework;
using ProjectEmber.World;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class WorldRegistryTests
    {
        [Test]
        public void RecordTileDeltaStoresAndAppliesChunkChanges()
        {
            var registry = new WorldRegistry();
            var generator = new WorldGenerator(123);
            var chunkCoords = new Vector2Int(1, 2);
            var chunk = registry.GetOrCreateChunk(chunkCoords, generator);
            var tileIndex = WorldChunk.ToIndex(4, 6);
            var tile = new TileData(TileType.DeepStone, 9, 42);

            registry.ApplyTileDelta(chunkCoords, tileIndex, tile);

            Assert.IsTrue(registry.TryGetTileDelta(chunkCoords, tileIndex, out var recorded));
            Assert.AreEqual(tile.BaseType, recorded.BaseType);
            Assert.AreEqual(tile.Durability, recorded.Durability);
            Assert.AreEqual(tile.OccupantId, recorded.OccupantId);

            Assert.AreEqual(tile.BaseType, chunk.GetTile(4, 6).BaseType);
            Assert.AreEqual(tile.Durability, chunk.GetTile(4, 6).Durability);
            Assert.AreEqual(tile.OccupantId, chunk.GetTile(4, 6).OccupantId);
        }

        [Test]
        public void RecordedDeltasReapplyWhenChunkIsGenerated()
        {
            var registry = new WorldRegistry();
            var generator = new WorldGenerator(456);
            var chunkCoords = new Vector2Int(-3, 5);
            var tileIndex = WorldChunk.ToIndex(7, 2);
            var tile = new TileData(TileType.Dirt, 0, 0);

            // Record the delta before the chunk has ever been generated, then
            // generate it: the delta should be baked into the fresh chunk.
            registry.RecordTileDelta(chunkCoords, tileIndex, tile);
            var chunk = registry.GetOrCreateChunk(chunkCoords, generator);

            Assert.AreEqual(tile.BaseType, chunk.GetTile(7, 2).BaseType);
            Assert.AreEqual(tile.Durability, chunk.GetTile(7, 2).Durability);
            Assert.AreEqual(tile.OccupantId, chunk.GetTile(7, 2).OccupantId);
        }
    }
}