using System;
using ProjectEmber.Gameplay;
using ProjectEmber.World;
using UnityEngine;

namespace ProjectEmber.Save
{
    [Serializable]
    public sealed class SaveProfile
    {
        public Vector2 PlayerPosition;
        public int Minute;
        public int Hour;
        public int Day;
        public int Season;
        public InventorySlot[] InventorySlots;
        public SaveChunkDelta[] ChunkDeltas;
    }

    [Serializable]
    public sealed class SaveChunkDelta
    {
        public Vector2Int ChunkCoordinates;
        public SaveTileDelta[] Tiles;
    }

    [Serializable]
    public sealed class SaveTileDelta
    {
        public int TileIndex;
        public TileType BaseType;
        public int Durability;
        public int OccupantId;
    }
}
