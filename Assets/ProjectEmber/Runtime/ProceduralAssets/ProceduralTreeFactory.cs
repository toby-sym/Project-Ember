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
            return CreateTree(name, seed, parent, true);
        }

        public static GameObject CreateTree(string name, int seed, Transform parent, bool usePixelArt)
        {
            var tree = new GameObject(name);
            if (parent != null)
            {
                tree.transform.SetParent(parent, false);
            }

            if (usePixelArt)
            {
                CreatePixelArtTree(tree, seed);
            }
            else
            {
                CreateGeometricTree(tree, seed);
            }

            var collider = tree.AddComponent<CircleCollider2D>();
            collider.radius = 0.75f;
            collider.offset = new Vector2(0f, 0.35f);
            return tree;
        }

        private static void CreatePixelArtTree(GameObject tree, int seed)
        {
            var random = new System.Random(seed);
            var barkColor = BarkPalette[random.Next(BarkPalette.Length)];
            var leafColors = new Color[3];
            for (var i = 0; i < 3; i++)
            {
                leafColors[i] = LeafPalette[random.Next(LeafPalette.Length)];
            }

            var meshRenderer = tree.AddComponent<RuntimeMeshRenderer>();
            meshRenderer.UsePixelArt = true;

            var data = ScriptableObject.CreateInstance<VectorSpriteData>();
            var layer = ProceduralShapeUtility.GenerateBoxPolygon(1.5f, 2f);
            layer.Color = Color.white;
            data.Layers.Add(layer);

            meshRenderer.BuildMeshFromVectorData(data, seed);

            // Generate and apply pixel art texture
            var treeTexture = ProceduralPixelArtGenerator.GenerateTreeTexture(barkColor, leafColors, seed, 32);
            if (tree.GetComponent<MeshRenderer>() != null)
            {
                tree.GetComponent<MeshRenderer>().material.mainTexture = treeTexture;
            }

            DisposeTemporaryData(data);
        }

        private static void CreateGeometricTree(GameObject tree, int seed)
        {
            var renderer = tree.AddComponent<RuntimeMeshRenderer>();
            renderer.UsePixelArt = false;
            var data = CreateTreeSpriteData(seed);
            renderer.BuildMeshFromVectorData(data);
            DisposeTemporaryData(data);
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
