using UnityEngine;

namespace ProjectEmber.World
{
    public sealed class WorldGenerator
    {
        private readonly int seed;

        public WorldGenerator(int seed)
        {
            this.seed = seed;
        }

        public WorldChunk GenerateChunk(Vector2Int chunkCoordinates)
        {
            var chunk = new WorldChunk(chunkCoordinates);
            for (var y = 0; y < WorldChunk.Size; y++)
            {
                for (var x = 0; x < WorldChunk.Size; x++)
                {
                    var world = new Vector2Int(
                        chunkCoordinates.x * WorldChunk.Size + x,
                        chunkCoordinates.y * WorldChunk.Size + y);
                    chunk.SetTile(x, y, CreateTile(world));
                }
            }

            return chunk;
        }

        public BiomeType GetBiome(Vector2Int worldTile)
        {
            var elevation = Noise(worldTile, 0.035f, 11);
            var moisture = Noise(worldTile, 0.055f, 29);
            var treeDensity = Noise(worldTile, 0.09f, 47);

            if (moisture > 0.62f && elevation < 0.58f)
            {
                return BiomeType.River;
            }

            if (elevation > 0.72f)
            {
                return BiomeType.RockyCaveMouth;
            }

            return treeDensity > 0.48f ? BiomeType.DenseForest : BiomeType.OpenGrassValley;
        }

        private TileData CreateTile(Vector2Int worldTile)
        {
            var biome = GetBiome(worldTile);
            return biome switch
            {
                BiomeType.River => new TileData(TileType.Water, 1),
                BiomeType.RockyCaveMouth => new TileData(TileType.DeepStone, 5),
                BiomeType.OpenGrassValley => new TileData(TileType.Grass, 2),
                _ => new TileData(TileType.Dirt, 3, OccupantIdForTree(worldTile))
            };
        }

        private int OccupantIdForTree(Vector2Int worldTile)
        {
            var treeRoll = Noise(worldTile, 0.21f, 101);
            return treeRoll > 0.63f ? 1000 + Mathf.Abs(worldTile.x * 73856093 ^ worldTile.y * 19349663) : 0;
        }

        private float Noise(Vector2Int point, float scale, int salt)
        {
            var x = (point.x + seed + salt * 13) * scale;
            var y = (point.y - seed + salt * 17) * scale;
            return Mathf.PerlinNoise(x, y);
        }
    }
}
