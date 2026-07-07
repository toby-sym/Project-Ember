using NUnit.Framework;
using ProjectEmber.World;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class WorldGeneratorTests
    {
        [Test]
        public void GenerateChunkPopulatesEveryTile()
        {
            var generator = new WorldGenerator(7);

            var chunk = generator.GenerateChunk(new Vector2Int(0, 0));

            Assert.AreEqual(new Vector2Int(0, 0), chunk.Coordinates);
            Assert.AreEqual(WorldChunk.Size * WorldChunk.Size, chunk.GetTiles().Length);
            foreach (var tile in chunk.GetTiles())
            {
                Assert.Greater(tile.Durability, 0);
            }
        }

        [Test]
        public void GenerationIsDeterministicForTheSameSeed()
        {
            var coords = new Vector2Int(3, -2);
            var first = new WorldGenerator(2024).GenerateChunk(coords).GetTiles();
            var second = new WorldGenerator(2024).GenerateChunk(coords).GetTiles();

            Assert.AreEqual(first.Length, second.Length);
            for (var i = 0; i < first.Length; i++)
            {
                Assert.AreEqual(first[i].BaseType, second[i].BaseType);
                Assert.AreEqual(first[i].Durability, second[i].Durability);
                Assert.AreEqual(first[i].OccupantId, second[i].OccupantId);
            }
        }

        [Test]
        public void BiomeLookupIsDeterministicForTheSameSeed()
        {
            var generator = new WorldGenerator(99);
            var tile = new Vector2Int(12, 34);

            Assert.AreEqual(generator.GetBiome(tile), generator.GetBiome(tile));
        }

        [Test]
        public void TileBaseTypeMatchesGeneratedBiome()
        {
            var generator = new WorldGenerator(555);

            for (var y = 0; y < WorldChunk.Size; y++)
            {
                for (var x = 0; x < WorldChunk.Size; x++)
                {
                    var worldTile = new Vector2Int(x, y);
                    var biome = generator.GetBiome(worldTile);
                    var tile = generator.GenerateChunk(Vector2Int.zero).GetTile(x, y);

                    var expected = biome switch
                    {
                        BiomeType.River => TileType.Water,
                        BiomeType.RockyCaveMouth => TileType.DeepStone,
                        BiomeType.OpenGrassValley => TileType.Grass,
                        _ => TileType.Dirt
                    };

                    Assert.AreEqual(expected, tile.BaseType);
                }
            }
        }

        [Test]
        public void OnlyDenseForestTilesCanCarryTreeOccupants()
        {
            var generator = new WorldGenerator(321);
            var chunk = generator.GenerateChunk(Vector2Int.zero);

            for (var y = 0; y < WorldChunk.Size; y++)
            {
                for (var x = 0; x < WorldChunk.Size; x++)
                {
                    var tile = chunk.GetTile(x, y);
                    if (tile.HasOccupant)
                    {
                        Assert.AreEqual(TileType.Dirt, tile.BaseType);
                        Assert.GreaterOrEqual(tile.OccupantId, 1000);
                    }
                }
            }
        }
    }
}
