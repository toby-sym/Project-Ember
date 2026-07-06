using NUnit.Framework;
using ProjectEmber.Rendering;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class VectorLayerTests
    {
        [Test]
        public void ConstructorStoresCoreLayerData()
        {
            var points = new[]
            {
                new Vector2(-1f, -1f),
                new Vector2(1f, -1f),
                new Vector2(0f, 1f)
            };

            var layer = new VectorLayer(points, Color.red, 4, false);

            Assert.AreSame(points, layer.Points);
            Assert.AreEqual(Color.red, layer.Color);
            Assert.AreEqual(4, layer.SortingOrderWithinSprite);
            Assert.IsFalse(layer.CloseLoop);
        }

        [Test]
        public void GetRenderPointsReturnsCopyWhenJitterIsDisabled()
        {
            var layer = new VectorLayer(new[] { Vector2.zero, Vector2.right }, Color.white);

            var renderPoints = layer.GetRenderPoints();

            Assert.AreEqual(layer.Points, renderPoints);
            Assert.AreNotSame(layer.Points, renderPoints);
        }

        [Test]
        public void GetRenderPointsAppliesDeterministicJitterWithinRadius()
        {
            var layer = new VectorLayer(new[] { Vector2.zero, Vector2.right, Vector2.up }, Color.white)
            {
                ApplyVertexJitter = true,
                VertexJitterRadius = 0.25f,
                VertexJitterSeed = 12345
            };

            var firstPass = layer.GetRenderPoints();
            var secondPass = layer.GetRenderPoints();

            Assert.AreEqual(firstPass, secondPass);
            for (var i = 0; i < layer.Points.Length; i++)
            {
                Assert.LessOrEqual(Vector2.Distance(layer.Points[i], firstPass[i]), layer.VertexJitterRadius + 0.0001f);
            }
        }

        [Test]
        public void NegativeJitterRadiusIsClampedToZero()
        {
            var layer = new VectorLayer
            {
                VertexJitterRadius = -10f
            };

            Assert.AreEqual(0f, layer.VertexJitterRadius);
        }
    }
}
