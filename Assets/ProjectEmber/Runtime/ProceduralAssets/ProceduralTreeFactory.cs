using ProjectEmber.Rendering;
using UnityEngine;

namespace ProjectEmber.ProceduralAssets
{
    public static class ProceduralTreeFactory
    {
        private static readonly Color[] BarkPalette =
        {
            new(0.24f, 0.12f, 0.06f),
            new(0.34f, 0.18f, 0.08f),
            new(0.18f, 0.09f, 0.045f)
        };

        private static readonly Color[] LeafPalette =
        {
            new(0.95f, 0.38f, 0.08f),
            new(0.78f, 0.22f, 0.06f),
            new(0.62f, 0.15f, 0.08f),
            new(0.98f, 0.58f, 0.12f)
        };

        public static GameObject CreateTree(string name, int seed, Transform parent = null)
        {
            var tree = new GameObject(name);
            if (parent != null)
            {
                tree.transform.SetParent(parent, false);
            }

            var renderer = tree.AddComponent<RuntimeMeshRenderer>();
            var data = CreateTreeSpriteData(seed);
            renderer.BuildMeshFromVectorData(data);
            DisposeTemporaryData(data);
            return tree;
        }

        public static VectorSpriteData CreateTreeSpriteData(int seed)
        {
            var random = new System.Random(seed);
            var data = ScriptableObject.CreateInstance<VectorSpriteData>();
            data.Layers.Add(CreateTrunkLayer(random));

            var leafCount = random.Next(6, 11);
            for (var i = 0; i < leafCount; i++)
            {
                var center = new Vector2(
                    RandomRange(random, -0.65f, 0.65f),
                    RandomRange(random, 1.0f, 1.85f));
                var radius = RandomRange(random, 0.42f, 0.74f);
                var layer = CreateOrganicBlob(center, radius, random.Next(9, 15), LeafPalette[random.Next(LeafPalette.Length)]);
                layer.SortingOrderWithinSprite = 10 + i;
                layer.ApplyVertexJitter = true;
                layer.VertexJitterRadius = 0.04f;
                layer.VertexJitterSeed = seed + i * 31;
                data.Layers.Add(layer);
            }

            return data;
        }

        private static VectorLayer CreateTrunkLayer(System.Random random)
        {
            var baseWidth = RandomRange(random, 0.32f, 0.48f);
            var crownWidth = RandomRange(random, 0.18f, 0.3f);
            var height = RandomRange(random, 1.35f, 1.75f);
            var lean = RandomRange(random, -0.12f, 0.12f);
            var points = new[]
            {
                new Vector2(-baseWidth, -0.9f),
                new Vector2(baseWidth, -0.9f),
                new Vector2(crownWidth + lean, height * 0.18f),
                new Vector2(crownWidth * 0.65f + lean, height * 0.6f),
                new Vector2(-crownWidth * 0.65f + lean, height * 0.58f),
                new Vector2(-crownWidth + lean, height * 0.15f)
            };

            var layer = new VectorLayer(points, BarkPalette[random.Next(BarkPalette.Length)], 0);
            layer.ApplyVertexJitter = true;
            layer.VertexJitterRadius = 0.035f;
            layer.VertexJitterSeed = random.Next();
            return layer;
        }

        private static VectorLayer CreateOrganicBlob(Vector2 center, float radius, int segments, Color color)
        {
            var points = new Vector2[segments];
            for (var i = 0; i < segments; i++)
            {
                var angle = Mathf.PI * 2f * i / segments;
                var squash = 0.78f + Mathf.Sin(angle * 2f + center.x) * 0.12f;
                points[i] = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius * squash);
            }

            return new VectorLayer(points, color);
        }

        private static float RandomRange(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        private static void DisposeTemporaryData(VectorSpriteData data)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(data);
            }
            else
            {
                Object.DestroyImmediate(data);
            }
        }
    }
}
