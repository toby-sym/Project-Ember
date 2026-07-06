using System;

namespace ProjectEmber.World
{
    [Serializable]
    public struct TileData
    {
        public TileType BaseType;
        public int Durability;
        public int OccupantId;

        public bool HasOccupant => OccupantId != 0;

        public TileData(TileType baseType, int durability, int occupantId = 0)
        {
            BaseType = baseType;
            Durability = durability;
            OccupantId = occupantId;
        }
    }
}
