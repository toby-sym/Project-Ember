using UnityEngine;

namespace ProjectEmber.World
{
    public sealed class WorldChunk
    {
        public const int Size = 16;

        private readonly TileData[] tiles = new TileData[Size * Size];

        public Vector2Int Coordinates { get; }

        public WorldChunk(Vector2Int coordinates)
        {
            Coordinates = coordinates;
        }

        public TileData GetTile(int x, int y)
        {
            return tiles[ToIndex(x, y)];
        }

        public void SetTile(int x, int y, TileData tile)
        {
            tiles[ToIndex(x, y)] = tile;
        }

        public TileData[] GetTiles()
        {
            return tiles;
        }

        public static int ToIndex(int x, int y)
        {
            return y * Size + x;
        }
    }
}
