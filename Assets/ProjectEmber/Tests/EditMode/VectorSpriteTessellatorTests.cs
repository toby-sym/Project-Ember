using NUnit.Framework;
using ProjectEmber.Rendering;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class VectorSpriteTessellatorTests
    {
        private static VectorLayer Triangle(Color color, int sortingOrder, bool closeLoop = true)
        {
            var points = new[]
            {
                new Vector2(-1f, -1f),
                new Vector2(1f, -1f),
                new Vector2(0f, 1f)
            };

            return new VectorLayer(points, color, sortingOrder, closeLoop);
        }

        [Test]
        public void TessellateSortsLayersBySortingOrder()
        {
            var data = ScriptableObject.CreateInstance<VectorSpriteData>();
            data.Layers.Add(Triangle(Color.red, 5));
            data.Layers.Add(Triangle(Color.green, 1));
            data.Layers.Add(Triangle(Color.blue, 3));

            var result = VectorSpriteTessellator.Tessellate(data);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0].Layer.SortingOrderWithinSprite);
            Assert.AreEqual(3, result[1].Layer.SortingOrderWithinSprite);
            Assert.AreEqual(5, result[2].Layer.SortingOrderWithinSprite);

            Object.DestroyImmediate(data);
        }

        [Test]
        public void TessellateSkipsOpenAndDegenerateLayers()
        {
            var data = ScriptableObject.CreateInstance<VectorSpriteData>();
            data.Layers.Add(Triangle(Color.white, 0));
            data.Layers.Add(Triangle(Color.white, 1, closeLoop: false));
            data.Layers.Add(new VectorLayer(new[] { Vector2.zero, Vector2.right }, Color.white, 2));

            var result = VectorSpriteTessellator.Tessellate(data);

            Assert.AreEqual(1, result.Count);
            Assert.Greater(result[0].Triangles.Count, 0);
            Assert.AreEqual(3, result[0].Points.Length);

            Object.DestroyImmediate(data);
        }

        [Test]
        public void TessellateReturnsEmptyForNullData()
        {
            Assert.AreEqual(0, VectorSpriteTessellator.Tessellate(null).Count);
        }
    }
}
