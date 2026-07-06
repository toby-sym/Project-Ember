using NUnit.Framework;
using ProjectEmber.ProceduralAssets;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class ProceduralShapeUtilityTests
    {
        [Test]
        public void GenerateCirclePolygonReturnsRequestedSegmentCount()
        {
            var layer = ProceduralShapeUtility.GenerateCirclePolygon(2f, 12);

            Assert.AreEqual(12, layer.Points.Length);
            Assert.IsTrue(layer.CloseLoop);
            Assert.AreEqual(new Vector2(2f, 0f), layer.Points[0]);
        }

        [Test]
        public void GenerateBoxPolygonReturnsCenteredClockwiseCorners()
        {
            var layer = ProceduralShapeUtility.GenerateBoxPolygon(4f, 2f);

            CollectionAssert.AreEqual(
                new[]
                {
                    new Vector2(-2f, -1f),
                    new Vector2(2f, -1f),
                    new Vector2(2f, 1f),
                    new Vector2(-2f, 1f)
                },
                layer.Points);
        }

        [Test]
        public void GenerateCapsulePolygonReturnsClosedRoundedShape()
        {
            var layer = ProceduralShapeUtility.GenerateCapsulePolygon(1f, 3f);

            Assert.Greater(layer.Points.Length, 8);
            Assert.IsTrue(layer.CloseLoop);
        }
    }
}
