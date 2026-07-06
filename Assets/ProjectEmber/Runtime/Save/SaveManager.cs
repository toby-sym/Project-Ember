using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace ProjectEmber.Save
{
    public static class SaveManager
    {
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
            File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName), ToCompressedJson(profile));
        }
    }
}
