using NUnit.Framework;
using ProjectEmber.Gameplay;
using ProjectEmber.Save;
using ProjectEmber.Simulation;
using ProjectEmber.Shared;
using ProjectEmber.World;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class SaveManagerTests
    {
        private GameObject systemsObject;
        private TimeSimulationEngine timeEngine;

        [SetUp]
        public void SetUp()
        {
            systemsObject = new GameObject("Systems");
            timeEngine = systemsObject.AddComponent<TimeSimulationEngine>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(systemsObject);
        }

        [Test]
        public void CompressedSaveRoundTripsCoreState()
        {
            var inventory = new InventorySystem(4);
            inventory.TryAddItem(ItemType.Axe, 1);
            inventory.TryAddItem(ItemType.Logs, 3);

            var registry = new WorldRegistry();
            registry.RecordTileDelta(new Vector2Int(2, 3), 5, new TileData(TileType.Dirt, 2, 1001));

            var profile = SaveManager.CreateProfile(new Vector3(4f, 5f, 0f), inventory, timeEngine, registry);
            var payload = SaveManager.ToCompressedJson(profile);
            var restored = SaveManager.FromCompressedJson(payload);

            Assert.AreEqual(profile.PlayerPosition, restored.PlayerPosition);
            Assert.AreEqual(profile.Hour, restored.Hour);
            Assert.AreEqual(profile.Minute, restored.Minute);
            Assert.AreEqual(profile.InventorySlots.Length, restored.InventorySlots.Length);
            Assert.AreEqual(ItemType.Axe, restored.InventorySlots[0].Type);
            Assert.AreEqual(1, restored.ChunkDeltas.Length);
            Assert.AreEqual(new Vector2Int(2, 3), restored.ChunkDeltas[0].ChunkCoordinates);
        }
    }
}