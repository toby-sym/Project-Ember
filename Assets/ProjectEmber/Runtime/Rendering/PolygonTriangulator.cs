using System.Collections.Generic;
using UnityEngine;

namespace ProjectEmber.Rendering
{
    public static class PolygonTriangulator
    {
        public static List<int> Triangulate(IReadOnlyList<Vector2> points)
        {
            var triangles = new List<int>();
            if (points == null || points.Count < 3)
            {
                return triangles;
            }

            var indices = new List<int>(points.Count);
            if (SignedArea(points) > 0f)
            {
                for (var i = 0; i < points.Count; i++)
                {
                    indices.Add(i);
                }
            }
            else
            {
                for (var i = points.Count - 1; i >= 0; i--)
                {
                    indices.Add(i);
                }
            }

            var guard = points.Count * points.Count;
            while (indices.Count > 3 && guard-- > 0)
            {
                var earFound = false;
                for (var i = 0; i < indices.Count; i++)
                {
                    var previousIndex = indices[(i + indices.Count - 1) % indices.Count];
                    var currentIndex = indices[i];
                    var nextIndex = indices[(i + 1) % indices.Count];

                    if (!IsEar(points, indices, previousIndex, currentIndex, nextIndex))
                    {
                        continue;
                    }

                    triangles.Add(previousIndex);
                    triangles.Add(currentIndex);
                    triangles.Add(nextIndex);
                    indices.RemoveAt(i);
                    earFound = true;
                    break;
                }

                if (!earFound)
                {
                    return new List<int>();
                }
            }

            if (indices.Count == 3)
            {
                triangles.Add(indices[0]);
                triangles.Add(indices[1]);
                triangles.Add(indices[2]);
            }

            return triangles;
        }

        private static float SignedArea(IReadOnlyList<Vector2> points)
        {
            var area = 0f;
            for (var i = 0; i < points.Count; i++)
            {
                var next = (i + 1) % points.Count;
                area += points[i].x * points[next].y - points[next].x * points[i].y;
            }

            return area * 0.5f;
        }

        private static bool IsEar(
            IReadOnlyList<Vector2> points,
            IReadOnlyList<int> polygonIndices,
            int previousIndex,
            int currentIndex,
            int nextIndex)
        {
            var previous = points[previousIndex];
            var current = points[currentIndex];
            var next = points[nextIndex];

            if (Cross(current - previous, next - current) <= Mathf.Epsilon)
            {
                return false;
            }

            for (var i = 0; i < polygonIndices.Count; i++)
            {
                var pointIndex = polygonIndices[i];
                if (pointIndex == previousIndex || pointIndex == currentIndex || pointIndex == nextIndex)
                {
                    continue;
                }

                if (IsPointInTriangle(points[pointIndex], previous, current, next))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsPointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            var ab = Cross(b - a, point - a);
            var bc = Cross(c - b, point - b);
            var ca = Cross(a - c, point - c);

            return ab >= -Mathf.Epsilon && bc >= -Mathf.Epsilon && ca >= -Mathf.Epsilon;
        }

        private static float Cross(Vector2 left, Vector2 right)
        {
            return left.x * right.y - left.y * right.x;
        }
    }
}
