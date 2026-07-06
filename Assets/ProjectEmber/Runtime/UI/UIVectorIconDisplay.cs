using System.Collections.Generic;
using ProjectEmber.ProceduralAssets;
using ProjectEmber.Rendering;
using ProjectEmber.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEmber.UI
{
    public sealed class UIVectorIconDisplay : MaskableGraphic
    {
        [SerializeField] private ItemType itemType;
        [SerializeField] private VectorSpriteData vectorData;
        [SerializeField, Range(0f, 0.45f)] private float padding = 0.12f;

        public ItemType ItemType => itemType;
        public VectorSpriteData VectorData => vectorData;

        public void SetIcon(ItemType type)
        {
            itemType = type;
            vectorData = VectorItemIconFactory.CreateIcon(type);
            SetVerticesDirty();
        }

        public void SetVectorData(VectorSpriteData data)
        {
            vectorData = data;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            if (vectorData == null || vectorData.Layers.Count == 0)
            {
                return;
            }

            var bounds = CalculateBounds(vectorData);
            var boundsSize = (Vector2)bounds.size;
            var boundsCenter = (Vector2)bounds.center;
            if (boundsSize.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            var rect = GetPixelAdjustedRect();
            var usableWidth = rect.width * (1f - padding * 2f);
            var usableHeight = rect.height * (1f - padding * 2f);
            var scale = Mathf.Min(usableWidth / boundsSize.x, usableHeight / boundsSize.y);
            var sortedLayers = new List<VectorLayer>(vectorData.Layers);
            sortedLayers.Sort((left, right) => left.SortingOrderWithinSprite.CompareTo(right.SortingOrderWithinSprite));

            foreach (var layer in sortedLayers)
            {
                if (layer == null || !layer.CloseLoop)
                {
                    continue;
                }

                var points = layer.GetRenderPoints();
                var triangles = PolygonTriangulator.Triangulate(points);
                if (points.Length < 3 || triangles.Count == 0)
                {
                    continue;
                }

                var vertexOffset = vertexHelper.currentVertCount;
                for (var i = 0; i < points.Length; i++)
                {
                    var local = (points[i] - boundsCenter) * scale;
                    vertexHelper.AddVert(new UIVertex
                    {
                        position = new Vector3(local.x, local.y, 0f),
                        color = layer.Color * color,
                        uv0 = Vector2.zero
                    });
                }

                for (var i = 0; i < triangles.Count; i += 3)
                {
                    vertexHelper.AddTriangle(vertexOffset + triangles[i], vertexOffset + triangles[i + 1], vertexOffset + triangles[i + 2]);
                }
            }
        }

        private static Bounds CalculateBounds(VectorSpriteData data)
        {
            var hasPoint = false;
            var min = Vector2.zero;
            var max = Vector2.zero;

            for (var layerIndex = 0; layerIndex < data.Layers.Count; layerIndex++)
            {
                var layer = data.Layers[layerIndex];
                if (layer == null)
                {
                    continue;
                }

                var points = layer.Points;
                for (var pointIndex = 0; pointIndex < points.Length; pointIndex++)
                {
                    if (!hasPoint)
                    {
                        min = points[pointIndex];
                        max = points[pointIndex];
                        hasPoint = true;
                        continue;
                    }

                    min = Vector2.Min(min, points[pointIndex]);
                    max = Vector2.Max(max, points[pointIndex]);
                }
            }

            var bounds = new Bounds();
            if (hasPoint)
            {
                bounds.SetMinMax(min, max);
            }

            return bounds;
        }
    }
}
