using System.Collections.Generic;
using ProjectEmber.Rendering;
using UnityEngine;

namespace ProjectEmber.ProceduralAssets
{
    public static class ProceduralShapeUtility
    {
        public static VectorLayer GenerateCirclePolygon(float radius, int segments)
        {
            radius = Mathf.Max(0f, radius);
            segments = Mathf.Max(3, segments);

            var points = new Vector2[segments];
            for (var i = 0; i < segments; i++)
            {
                var angle = (Mathf.PI * 2f * i) / segments;
                points[i] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            }

            return new VectorLayer(points, Color.white);
        }

        public static VectorLayer GenerateBoxPolygon(float width, float height)
        {
            var halfWidth = Mathf.Max(0f, width) * 0.5f;
            var halfHeight = Mathf.Max(0f, height) * 0.5f;
            return new VectorLayer(
                new[]
                {
                    new Vector2(-halfWidth, -halfHeight),
                    new Vector2(halfWidth, -halfHeight),
                    new Vector2(halfWidth, halfHeight),
                    new Vector2(-halfWidth, halfHeight)
                },
                Color.white);
        }

        public static VectorLayer GenerateCapsulePolygon(float width, float height)
        {
            width = Mathf.Max(0f, width);
            height = Mathf.Max(0f, height);

            if (Mathf.Approximately(width, 0f) || Mathf.Approximately(height, 0f))
            {
                return GenerateBoxPolygon(width, height);
            }

            var radius = width * 0.5f;
            var straightHeight = Mathf.Max(0f, height - width);
            var halfStraightHeight = straightHeight * 0.5f;
            var points = new List<Vector2>();
            const int halfCircleSegments = 8;

            for (var i = 0; i <= halfCircleSegments; i++)
            {
                var t = i / (float)halfCircleSegments;
                var angle = Mathf.Lerp(0f, Mathf.PI, t);
                points.Add(new Vector2(Mathf.Cos(angle) * radius, halfStraightHeight + Mathf.Sin(angle) * radius));
            }

            for (var i = 0; i <= halfCircleSegments; i++)
            {
                var t = i / (float)halfCircleSegments;
                var angle = Mathf.Lerp(Mathf.PI, Mathf.PI * 2f, t);
                points.Add(new Vector2(Mathf.Cos(angle) * radius, -halfStraightHeight + Mathf.Sin(angle) * radius));
            }

            return new VectorLayer(points.ToArray(), Color.white);
        }
    }
}
