using System.Collections.Generic;
using NUnit.Framework;
using ProjectEmber.Rendering;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class PolygonTriangulatorTests
    {
        [Test]
        public void ReturnsEmptyForNullInput()
        {
            Assert.IsEmpty(PolygonTriangulator.Triangulate(null));
        }

        [Test]
        public void ReturnsEmptyForFewerThanThreePoints()
        {
            var points = new List<Vector2> { Vector2.zero, Vector2.right };

            Assert.IsEmpty(PolygonTriangulator.Triangulate(points));
        }

        [Test]
        public void TriangleProducesSingleTriangle()
        {
            var points = new List<Vector2>
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f)
            };

            var triangles = PolygonTriangulator.Triangulate(points);

            Assert.AreEqual(3, triangles.Count);
            CollectionAssert.AllItemsAreInstancesOfType(triangles, typeof(int));
        }

        [Test]
        public void ConvexQuadProducesTwoTriangles()
        {
            var points = new List<Vector2>
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };

            var triangles = PolygonTriangulator.Triangulate(points);

            Assert.AreEqual(6, triangles.Count);
            foreach (var index in triangles)
            {
                Assert.GreaterOrEqual(index, 0);
                Assert.Less(index, points.Count);
            }
        }

        [Test]
        public void ClockwiseWindingIsTriangulatedLikeCounterClockwise()
        {
            var counterClockwise = new List<Vector2>
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };
            var clockwise = new List<Vector2>
            {
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f),
                new Vector2(0f, 0f)
            };

            Assert.AreEqual(
                PolygonTriangulator.Triangulate(counterClockwise).Count,
                PolygonTriangulator.Triangulate(clockwise).Count);
        }

        [Test]
        public void ConcavePolygonIsFullyTriangulated()
        {
            // Arrow / concave "notch" shape with five vertices.
            var points = new List<Vector2>
            {
                new Vector2(0f, 0f),
                new Vector2(2f, 0f),
                new Vector2(2f, 2f),
                new Vector2(1f, 1f),
                new Vector2(0f, 2f)
            };

            var triangles = PolygonTriangulator.Triangulate(points);

            // A simple polygon of n vertices triangulates into n-2 triangles.
            Assert.AreEqual((points.Count - 2) * 3, triangles.Count);
        }
    }
}
