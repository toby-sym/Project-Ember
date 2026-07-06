using System.IO;
using System.IO.Compression;
using System.Text;
using ProjectEmber.Gameplay;
using ProjectEmber.Simulation;
using ProjectEmber.World;
using UnityEngine;

namespace ProjectEmber.Save
{
    public static class SaveManager
    {
        private const string DefaultSaveFileName = "project-ember-save.json";

        public static SaveProfile CreateProfile(Vector3 playerPosition, InventorySystem inventory, TimeSimulationEngine time, WorldRegistry registry)
        {
            return new SaveProfile
            {
                PlayerPosition = new Vector2(playerPosition.x, playerPosition.y),
                Minute = time != null ? time.Minute : 0,
                Hour = time != null ? time.Hour : 8,
                Day = time != null ? time.Day : 1,
                Season = time != null ? time.Season : 0,
                InventorySlots = inventory != null ? inventory.Slots : null,
                ChunkDeltas = registry != null ? CaptureChunkDeltas(registry) : System.Array.Empty<SaveChunkDelta>()
            };
        }

        public static SaveChunkDelta[] CaptureChunkDeltas(WorldRegistry registry)
        {
            var deltas = new System.Collections.Generic.List<SaveChunkDelta>();
            foreach (var pair in registry.TileDeltas)
            {
                var tiles = new SaveTileDelta[pair.Value.Count];
                var index = 0;
                foreach (var tile in pair.Value)
                {
                    tiles[index++] = new SaveTileDelta
                    {
                        TileIndex = tile.Key,
                        BaseType = tile.Value.BaseType,
                        Durability = tile.Value.Durability,
                        OccupantId = tile.Value.OccupantId
                    };
                }

                deltas.Add(new SaveChunkDelta
                {
                    ChunkCoordinates = pair.Key,
                    Tiles = tiles
                });
            }

            return deltas.ToArray();
        }

        public static string ToCompressedJson(SaveProfile profile)
        {
            var json = JsonUtility.ToJson(profile);
            var bytes = Encoding.UTF8.GetBytes(json);
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
            {
                gzip.Write(bytes, 0, bytes.Length);
            }

            return System.Convert.ToBase64String(output.ToArray());
        }

        public static SaveProfile FromCompressedJson(string payload)
        {
            var bytes = System.Convert.FromBase64String(payload);
            using var input = new MemoryStream(bytes);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return JsonUtility.FromJson<SaveProfile>(Encoding.UTF8.GetString(output.ToArray()));
        }

        public static void WriteToDisk(string fileName, SaveProfile profile)
        {
            File.WriteAllText(Path.Combine(Application.persistentDataPath, string.IsNullOrWhiteSpace(fileName) ? DefaultSaveFileName : fileName), ToCompressedJson(profile));
        }

        public static SaveProfile ReadFromDisk(string fileName)
        {
            var path = Path.Combine(Application.persistentDataPath, string.IsNullOrWhiteSpace(fileName) ? DefaultSaveFileName : fileName);
            if (!File.Exists(path))
            {
                return null;
            }

            return FromCompressedJson(File.ReadAllText(path));
        }
    }
}
