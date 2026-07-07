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
            if (!IsInBounds(x, y))
            {
                Debug.LogError($"[WorldChunk] GetTile out of bounds: ({x}, {y}) in chunk {Coordinates}.");
                return default;
            }

            return tiles[ToIndex(x, y)];
        }

        public void SetTile(int x, int y, TileData tile)
        {
            if (!IsInBounds(x, y))
            {
                Debug.LogError($"[WorldChunk] SetTile out of bounds: ({x}, {y}) in chunk {Coordinates}.");
                return;
            }

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

        public static bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Size && y >= 0 && y < Size;
        }
    }
}
