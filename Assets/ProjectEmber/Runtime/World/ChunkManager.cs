using System.Collections.Generic;
using ProjectEmber.ProceduralAssets;
using ProjectEmber.Rendering;
using UnityEngine;

namespace ProjectEmber.World
{
    public sealed class ChunkManager : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private int seed = 20260706;
        [SerializeField] private int visibilityRadius = 1;

        private readonly Dictionary<Vector2Int, GameObject> chunkObjects = new();
        private readonly Queue<GameObject> pool = new();
        private WorldGenerator generator;
        private WorldRegistry registry;
        private Vector2Int currentPlayerChunk = new(int.MinValue, int.MinValue);

        public WorldRegistry Registry => registry;
        public WorldGenerator Generator => generator;

        public bool TryGetLoadedChunk(Vector2Int chunkCoordinates, out GameObject chunkObject)
        {
            return chunkObjects.TryGetValue(chunkCoordinates, out chunkObject);
        }

        public void Initialize(Transform playerTransform, int worldSeed, int radius)
        {
            player = playerTransform;
            seed = worldSeed;
            visibilityRadius = Mathf.Max(0, radius);
            generator = new WorldGenerator(seed);
            registry = new WorldRegistry();
            RefreshVisibleChunks(true);
        }

        private void Awake()
        {
            generator ??= new WorldGenerator(seed);
            registry ??= new WorldRegistry();
        }

        private void Update()
        {
            RefreshVisibleChunks(false);
        }

        private void RefreshVisibleChunks(bool force)
        {
            if (player == null)
            {
                return;
            }

            var playerChunk = WorldToChunk(player.position);
            if (!force && playerChunk == currentPlayerChunk)
            {
                return;
            }

            currentPlayerChunk = playerChunk;
            var needed = new HashSet<Vector2Int>();
            for (var y = -visibilityRadius; y <= visibilityRadius; y++)
            {
                for (var x = -visibilityRadius; x <= visibilityRadius; x++)
                {
                    var coords = playerChunk + new Vector2Int(x, y);
                    needed.Add(coords);
                    if (!chunkObjects.ContainsKey(coords))
                    {
                        CreateChunkObject(coords);
                    }
                }
            }

            var toRecycle = new List<Vector2Int>();
            foreach (var pair in chunkObjects)
            {
                if (!needed.Contains(pair.Key))
                {
                    toRecycle.Add(pair.Key);
                }
            }

            for (var i = 0; i < toRecycle.Count; i++)
            {
                var coords = toRecycle[i];
                var chunkObject = chunkObjects[coords];
                chunkObjects.Remove(coords);
                chunkObject.SetActive(false);
                pool.Enqueue(chunkObject);
            }
        }

        private void CreateChunkObject(Vector2Int coords)
        {
            var chunk = registry.GetOrCreateChunk(coords, generator);
            var chunkObject = pool.Count > 0 ? pool.Dequeue() : new GameObject("Chunk");
            chunkObject.name = $"Chunk {coords.x}, {coords.y}";
            chunkObject.transform.SetParent(transform, false);
            chunkObject.transform.position = new Vector3(coords.x * WorldChunk.Size, coords.y * WorldChunk.Size, 0f);
            chunkObject.SetActive(true);
            chunkObjects.Add(coords, chunkObject);

            for (var i = chunkObject.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(chunkObject.transform.GetChild(i).gameObject);
            }

            PopulateChunkVisuals(chunk, chunkObject.transform);
        }

        private void PopulateChunkVisuals(WorldChunk chunk, Transform parent)
        {
            for (var y = 0; y < WorldChunk.Size; y += 4)
            {
                for (var x = 0; x < WorldChunk.Size; x += 4)
                {
                    CreateGroundPatch(chunk, parent, x, y);
                }
            }

            for (var y = 0; y < WorldChunk.Size; y += 2)
            {
                for (var x = 0; x < WorldChunk.Size; x += 2)
                {
                    var tile = chunk.GetTile(x, y);
                    if (tile.HasOccupant)
                    {
                        var tree = ProceduralTreeFactory.CreateTree($"Tree {tile.OccupantId}", tile.OccupantId, parent);
                        tree.transform.localPosition = new Vector3(x, y, 0f);
                        var harvestable = tree.AddComponent<HarvestableTree>();
                        harvestable.Initialize(this, coords, new Vector2Int(x, y), tile.OccupantId, tile.Durability);
                    }
                }
            }
        }

        public void HarvestTree(Vector2Int chunkCoordinates, Vector2Int localTile, int newDurability, int occupantId)
        {
            if (!registry.TryGetChunk(chunkCoordinates, out var chunk))
            {
                return;
            }

            var tile = chunk.GetTile(localTile.x, localTile.y);
            tile.Durability = Mathf.Max(0, newDurability);
            tile.OccupantId = occupantId;
            chunk.SetTile(localTile.x, localTile.y, tile);
            registry.RecordTileDelta(chunkCoordinates, WorldChunk.ToIndex(localTile.x, localTile.y), tile);

            if (chunkObjects.TryGetValue(chunkCoordinates, out var chunkObject))
            {
                RebuildChunkVisuals(chunk, chunkObject.transform);
            }
        }

        private void RebuildChunkVisuals(WorldChunk chunk, Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }

            PopulateChunkVisuals(chunk, parent);
        }

        private static void CreateGroundPatch(WorldChunk chunk, Transform parent, int x, int y)
        {
            var tile = chunk.GetTile(x, y);
            var patch = new GameObject($"Ground {x}, {y}");
            patch.transform.SetParent(parent, false);
            patch.transform.localPosition = new Vector3(x + 1.5f, y + 1.5f, 0.25f);

            var layer = ProceduralShapeUtility.GenerateBoxPolygon(4f, 4f);
            layer.Color = ColorForTile(tile.BaseType);
            var data = ScriptableObject.CreateInstance<VectorSpriteData>();
            data.Layers.Add(layer);
            patch.AddComponent<RuntimeMeshRenderer>().BuildMeshFromVectorData(data);
            Destroy(data);
        }

        private static Color ColorForTile(TileType type)
        {
            return type switch
            {
                TileType.Water => new Color(0.12f, 0.34f, 0.58f),
                TileType.DeepStone => new Color(0.25f, 0.25f, 0.28f),
                TileType.Dirt => new Color(0.34f, 0.22f, 0.12f),
                _ => new Color(0.24f, 0.42f, 0.18f)
            };
        }

        public static Vector2Int WorldToChunk(Vector3 worldPosition)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPosition.x / WorldChunk.Size),
                Mathf.FloorToInt(worldPosition.y / WorldChunk.Size));
        }
    }
}
