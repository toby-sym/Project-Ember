using System;
using ProjectEmber.Gameplay;
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
    }
}
