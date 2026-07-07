using System;
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
                ChunkDeltas = registry != null ? CaptureChunkDeltas(registry) : Array.Empty<SaveChunkDelta>()
            };
        }

        public static SaveChunkDelta[] CaptureChunkDeltas(WorldRegistry registry)
        {
            if (registry == null)
            {
                Debug.LogError("[SaveManager] CaptureChunkDeltas called with null registry.");
                return Array.Empty<SaveChunkDelta>();
            }

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
            if (profile == null)
            {
                Debug.LogError("[SaveManager] ToCompressedJson called with null profile.");
                return null;
            }

            var json = JsonUtility.ToJson(profile);
            var bytes = Encoding.UTF8.GetBytes(json);
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
            {
                gzip.Write(bytes, 0, bytes.Length);
            }

            return Convert.ToBase64String(output.ToArray());
        }

        public static SaveProfile FromCompressedJson(string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                Debug.LogError("[SaveManager] FromCompressedJson called with null or empty payload.");
                return null;
            }

            try
            {
                var bytes = Convert.FromBase64String(payload);
                using var input = new MemoryStream(bytes);
                using var gzip = new GZipStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();
                gzip.CopyTo(output);
                var json = Encoding.UTF8.GetString(output.ToArray());
                var profile = JsonUtility.FromJson<SaveProfile>(json);
                if (profile == null)
                {
                    Debug.LogError("[SaveManager] Deserialized profile is null; save data may be corrupted.");
                }
                return profile;
            }
            catch (FormatException e)
            {
                Debug.LogError($"[SaveManager] Failed to decode Base64 payload: {e.Message}");
                return null;
            }
            catch (InvalidDataException e)
            {
                Debug.LogError($"[SaveManager] Failed to decompress save data (corrupt or truncated): {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Unexpected error deserializing save profile: {e.Message}");
                return null;
            }
        }

        public static bool WriteToDisk(string fileName, SaveProfile profile)
        {
            if (profile == null)
            {
                Debug.LogError("[SaveManager] WriteToDisk called with null profile.");
                return false;
            }

            var resolvedName = string.IsNullOrWhiteSpace(fileName) ? DefaultSaveFileName : fileName;
            var path = Path.Combine(Application.persistentDataPath, resolvedName);

            try
            {
                var compressed = ToCompressedJson(profile);
                if (compressed == null)
                {
                    return false;
                }
                File.WriteAllText(path, compressed);
                return true;
            }
            catch (IOException e)
            {
                Debug.LogError($"[SaveManager] Failed to write save file '{path}': {e.Message}");
                return false;
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.LogError($"[SaveManager] Permission denied writing save file '{path}': {e.Message}");
                return false;
            }
        }

        public static SaveProfile ReadFromDisk(string fileName)
        {
            var resolvedName = string.IsNullOrWhiteSpace(fileName) ? DefaultSaveFileName : fileName;
            var path = Path.Combine(Application.persistentDataPath, resolvedName);

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                var contents = File.ReadAllText(path);
                return FromCompressedJson(contents);
            }
            catch (IOException e)
            {
                Debug.LogError($"[SaveManager] Failed to read save file '{path}': {e.Message}");
                return null;
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.LogError($"[SaveManager] Permission denied reading save file '{path}': {e.Message}");
                return null;
            }
        }
    }
}
