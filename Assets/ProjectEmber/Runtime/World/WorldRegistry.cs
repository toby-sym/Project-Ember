using System.Collections.Generic;
using UnityEngine;

namespace ProjectEmber.World
{
    public sealed class WorldRegistry
    {
        private readonly Dictionary<Vector2Int, WorldChunk> activeChunks = new();
        private readonly Dictionary<Vector2Int, Dictionary<int, TileData>> tileDeltas = new();

        public IReadOnlyDictionary<Vector2Int, WorldChunk> ActiveChunks => activeChunks;
        public IReadOnlyDictionary<Vector2Int, Dictionary<int, TileData>> TileDeltas => tileDeltas;

        public WorldChunk GetOrCreateChunk(Vector2Int chunkCoordinates, WorldGenerator generator)
        {
            if (activeChunks.TryGetValue(chunkCoordinates, out var chunk))
            {
                return chunk;
            }

            if (generator == null)
            {
                Debug.LogError($"[WorldRegistry] Cannot generate chunk at {chunkCoordinates}: generator is null.");
                return null;
            }

            chunk = generator.GenerateChunk(chunkCoordinates);
            activeChunks.Add(chunkCoordinates, chunk);
            return chunk;
        }

        public void RecordTileDelta(Vector2Int chunkCoordinates, int tileIndex, TileData tileData)
        {
            if (!tileDeltas.TryGetValue(chunkCoordinates, out var chunkDeltas))
            {
                chunkDeltas = new Dictionary<int, TileData>();
                tileDeltas.Add(chunkCoordinates, chunkDeltas);
            }

            chunkDeltas[tileIndex] = tileData;
        }

        public bool TryGetTileDelta(Vector2Int chunkCoordinates, int tileIndex, out TileData tileData)
        {
            tileData = default;
            return tileDeltas.TryGetValue(chunkCoordinates, out var chunkDeltas) && chunkDeltas.TryGetValue(tileIndex, out tileData);
        }

        public void ApplyTileDelta(Vector2Int chunkCoordinates, int tileIndex, TileData tileData)
        {
            RecordTileDelta(chunkCoordinates, tileIndex, tileData);

            if (activeChunks.TryGetValue(chunkCoordinates, out var chunk))
            {
                var localX = tileIndex % WorldChunk.Size;
                var localY = tileIndex / WorldChunk.Size;
                chunk.SetTile(localX, localY, tileData);
            }
        }

        public bool TryGetChunk(Vector2Int chunkCoordinates, out WorldChunk chunk)
        {
            return activeChunks.TryGetValue(chunkCoordinates, out chunk);
        }
    }
}
