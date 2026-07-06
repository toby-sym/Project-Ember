using ProjectEmber.Gameplay;
using ProjectEmber.Shared;
using UnityEngine;

namespace ProjectEmber.World
{
    [DisallowMultipleComponent]
    public sealed class HarvestableTree : MonoBehaviour
    {
        [SerializeField] private int health = 3;
        [SerializeField] private int woodYield = 2;

        private ChunkManager chunkManager;
        private Vector2Int chunkCoordinates;
        private Vector2Int localTile;
        private int occupantId;
        private bool harvested;

        public void Initialize(ChunkManager manager, Vector2Int chunkCoords, Vector2Int localTileCoords, int treeOccupantId, int startingHealth)
        {
            chunkManager = manager;
            chunkCoordinates = chunkCoords;
            localTile = localTileCoords;
            occupantId = treeOccupantId;
            health = Mathf.Max(1, startingHealth);
        }

        public bool TryDamage(int amount, InventorySystem inventory)
        {
            if (harvested)
            {
                return false;
            }

            health -= Mathf.Max(1, amount);
            if (health > 0)
            {
                return false;
            }

            Harvest(inventory);
            return true;
        }

        private void Harvest(InventorySystem inventory)
        {
            harvested = true;
            chunkManager?.HarvestTree(chunkCoordinates, localTile, 0, 0);
            SpawnDrop(inventory);
            Destroy(gameObject);
        }

        private void SpawnDrop(InventorySystem inventory)
        {
            var drop = new GameObject($"Logs Drop {occupantId}");
            drop.transform.position = transform.position + new Vector3(0f, 0.5f, 0f);
            var resourceDrop = drop.AddComponent<WorldResourceDrop>();
            resourceDrop.Initialize(ItemType.Logs, woodYield, inventory);
        }
    }
}