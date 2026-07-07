using System.Collections.Generic;
using UnityEngine;

namespace ProjectEmber.Rendering
{
    public static class VectorSpriteTessellator
    {
        public readonly struct TessellatedLayer
        {
            public readonly VectorLayer Layer;
            public readonly Vector2[] Points;
            public readonly List<int> Triangles;

            public TessellatedLayer(VectorLayer layer, Vector2[] points, List<int> triangles)
            {
                Layer = layer;
                Points = points;
                Triangles = triangles;
            }
        }

        public static List<TessellatedLayer> Tessellate(VectorSpriteData data)
        {
            var result = new List<TessellatedLayer>();
            if (data == null || data.Layers.Count == 0)
            {
                return result;
            }

            var sortedLayers = new List<VectorLayer>(data.Layers);
            sortedLayers.Sort((left, right) => left.SortingOrderWithinSprite.CompareTo(right.SortingOrderWithinSprite));

            foreach (var layer in sortedLayers)
            {
                if (layer == null || !layer.CloseLoop)
                {
                    continue;
                }

                var points = layer.GetRenderPoints();
                if (points.Length < 3)
                {
                    continue;
                }

                var triangles = PolygonTriangulator.Triangulate(points);
                if (triangles.Count == 0)
                {
                    continue;
                }

                result.Add(new TessellatedLayer(layer, points, triangles));
            }

            return result;
        }
    }
}
