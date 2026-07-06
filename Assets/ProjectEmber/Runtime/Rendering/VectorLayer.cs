using System;
using UnityEngine;

namespace ProjectEmber.Rendering
{
    [Serializable]
    public sealed class VectorLayer
    {
        [SerializeField] private Vector2[] points = Array.Empty<Vector2>();
        [SerializeField] private Color color = Color.white;
        [SerializeField] private int sortingOrderWithinSprite;
        [SerializeField] private bool closeLoop = true;
        [SerializeField] private bool applyVertexJitter;
        [SerializeField, Min(0f)] private float vertexJitterRadius;
        [SerializeField] private int vertexJitterSeed;

        public Vector2[] Points
        {
            get => points;
            set => points = value ?? Array.Empty<Vector2>();
        }

        public Color Color
        {
            get => color;
            set => color = value;
        }

        public int SortingOrderWithinSprite
        {
            get => sortingOrderWithinSprite;
            set => sortingOrderWithinSprite = value;
        }

        public bool CloseLoop
        {
            get => closeLoop;
            set => closeLoop = value;
        }

        public bool ApplyVertexJitter
        {
            get => applyVertexJitter;
            set => applyVertexJitter = value;
        }

        public float VertexJitterRadius
        {
            get => vertexJitterRadius;
            set => vertexJitterRadius = Mathf.Max(0f, value);
        }

        public int VertexJitterSeed
        {
            get => vertexJitterSeed;
            set => vertexJitterSeed = value;
        }

        public VectorLayer()
        {
        }

        public VectorLayer(Vector2[] points, Color color, int sortingOrderWithinSprite = 0, bool closeLoop = true)
        {
            Points = points;
            Color = color;
            SortingOrderWithinSprite = sortingOrderWithinSprite;
            CloseLoop = closeLoop;
        }

        public Vector2[] GetRenderPoints()
        {
            var renderPoints = new Vector2[points.Length];
            Array.Copy(points, renderPoints, points.Length);

            if (!applyVertexJitter || vertexJitterRadius <= 0f)
            {
                return renderPoints;
            }

            var random = new System.Random(vertexJitterSeed);
            for (var i = 0; i < renderPoints.Length; i++)
            {
                var angle = random.NextDouble() * Mathf.PI * 2f;
                var radius = Math.Sqrt(random.NextDouble()) * vertexJitterRadius;
                renderPoints[i] += new Vector2(
                    (float)(Math.Cos(angle) * radius),
                    (float)(Math.Sin(angle) * radius));
            }

            return renderPoints;
        }
    }
}
