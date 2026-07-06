using System.Collections.Generic;
using UnityEngine;

namespace ProjectEmber.World
{
    public sealed class WorldRegistry
    {
        private readonly Dictionary<Vector2Int, WorldChunk> activeChunks = new();

        public IReadOnlyDictionary<Vector2Int, WorldChunk> ActiveChunks => activeChunks;

        public WorldChunk GetOrCreateChunk(Vector2Int chunkCoordinates, WorldGenerator generator)
        {
            if (activeChunks.TryGetValue(chunkCoordinates, out var chunk))
            {
                return chunk;
            }

            chunk = generator.GenerateChunk(chunkCoordinates);
            activeChunks.Add(chunkCoordinates, chunk);
            return chunk;
        }

        public bool TryGetChunk(Vector2Int chunkCoordinates, out WorldChunk chunk)
        {
            return activeChunks.TryGetValue(chunkCoordinates, out chunk);
        }
    }
}
