using ProjectEmber.Rendering;
using ProjectEmber.Shared;
using UnityEngine;

namespace ProjectEmber.ProceduralAssets
{
    public static class VectorItemIconFactory
    {
        private static readonly Color Wood = new(0.46f, 0.25f, 0.1f);
        private static readonly Color DarkWood = new(0.28f, 0.14f, 0.06f);
        private static readonly Color Metal = new(0.72f, 0.76f, 0.78f);
        private static readonly Color DarkMetal = new(0.42f, 0.46f, 0.5f);
        private static readonly Color BerryRed = new(0.72f, 0.05f, 0.08f);
        private static readonly Color LeafGreen = new(0.16f, 0.45f, 0.18f);

        public static VectorSpriteData CreateIcon(ItemType type)
        {
            return type switch
            {
                ItemType.Axe => CreateAxe(),
                ItemType.Pickaxe => CreatePickaxe(),
                ItemType.Sword => CreateSword(),
                ItemType.Logs => CreateLogs(),
                ItemType.Berries => CreateBerries(),
                _ => CreateLogs()
            };
        }

        private static VectorSpriteData CreateAxe()
        {
            var data = ScriptableObject.CreateInstance<VectorSpriteData>();
            data.Layers.Add(RectLayer(new Vector2(-0.08f, -0.45f), new Vector2(0.08f, 0.52f), Wood, 0, -28f));
            data.Layers.Add(new VectorLayer(
                new[]
                {
                    new Vector2(-0.08f, 0.25f),
                    new Vector2(0.52f, 0.42f),
                    new Vector2(0.42f, 0.02f),
                    new Vector2(0.04f, -0.02f)
                },
                Metal,
                1));
            data.Layers.Add(new VectorLayer(new[] { new Vector2(0.4f, 0.38f), new Vector2(0.56f, 0.28f), new Vector2(0.42f, 0.02f) }, DarkMetal, 2));
            return data;
        }

        private static VectorSpriteData CreatePickaxe()
        {
            var data = ScriptableObject.CreateInstance<VectorSpriteData>();
            data.Layers.Add(RectLayer(new Vector2(-0.06f, -0.5f), new Vector2(0.06f, 0.48f), Wood, 0, -18f));
            data.Layers.Add(new VectorLayer(
                new[]
                {
                    new Vector2(-0.62f, 0.3f),
                    new Vector2(-0.16f, 0.44f),
                    new Vector2(0.62f, 0.32f),
                    new Vector2(0.14f, 0.18f)
                },
                Metal,
                1));
            return data;
        }

        private static VectorSpriteData CreateSword()
        {
            var data = ScriptableObject.CreateInstance<VectorSpriteData>();
            data.Layers.Add(new VectorLayer(
                new[]
                {
                    new Vector2(0f, 0.64f),
                    new Vector2(0.13f, -0.1f),
                    new Vector2(0f, -0.28f),
                    new Vector2(-0.13f, -0.1f)
                },
                Metal,
                1));
            data.Layers.Add(RectLayer(new Vector2(-0.36f, -0.34f), new Vector2(0.36f, -0.22f), DarkMetal, 2));
            data.Layers.Add(RectLayer(new Vector2(-0.06f, -0.62f), new Vector2(0.06f, -0.3f), Wood, 0));
            return data;
        }

        private static VectorSpriteData CreateLogs()
        {
            var data = ScriptableObject.CreateInstance<VectorSpriteData>();
            data.Layers.Add(RectLayer(new Vector2(-0.58f, -0.22f), new Vector2(0.5f, 0.02f), Wood, 0, -8f));
            data.Layers.Add(RectLayer(new Vector2(-0.42f, 0.04f), new Vector2(0.58f, 0.28f), DarkWood, 1, 7f));
            data.Layers.Add(ProceduralShapeUtility.GenerateCirclePolygon(0.13f, 14));
            data.Layers[^1].Color = new Color(0.66f, 0.39f, 0.16f);
            data.Layers[^1].SortingOrderWithinSprite = 2;
            data.Layers[^1].Points = Offset(data.Layers[^1].Points, new Vector2(0.47f, 0.17f));
            return data;
        }

        private static VectorSpriteData CreateBerries()
        {
            var data = ScriptableObject.CreateInstance<VectorSpriteData>();
            data.Layers.Add(new VectorLayer(new[] { new Vector2(-0.12f, 0.1f), new Vector2(0.08f, 0.35f), new Vector2(0.23f, 0.05f) }, LeafGreen, 0));

            var centers = new[] { new Vector2(-0.18f, -0.08f), new Vector2(0.12f, -0.05f), new Vector2(-0.02f, 0.2f) };
            for (var i = 0; i < centers.Length; i++)
            {
                var berry = ProceduralShapeUtility.GenerateCirclePolygon(0.2f, 16);
                berry.Points = Offset(berry.Points, centers[i]);
                berry.Color = BerryRed;
                berry.SortingOrderWithinSprite = 1 + i;
                data.Layers.Add(berry);
            }

            return data;
        }

        private static VectorLayer RectLayer(Vector2 min, Vector2 max, Color color, int order, float rotationDegrees = 0f)
        {
            var center = (min + max) * 0.5f;
            var points = new[]
            {
                new Vector2(min.x, min.y),
                new Vector2(max.x, min.y),
                new Vector2(max.x, max.y),
                new Vector2(min.x, max.y)
            };

            if (!Mathf.Approximately(rotationDegrees, 0f))
            {
                var rotation = Quaternion.Euler(0f, 0f, rotationDegrees);
                for (var i = 0; i < points.Length; i++)
                {
                    points[i] = center + (Vector2)(rotation * (points[i] - center));
                }
            }

            return new VectorLayer(points, color, order);
        }

        private static Vector2[] Offset(Vector2[] points, Vector2 offset)
        {
            var result = new Vector2[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                result[i] = points[i] + offset;
            }

            return result;
        }
    }
}
