using NUnit.Framework;
using ProjectEmber.ProceduralAssets;
using ProjectEmber.Rendering;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class ProceduralTreeFactoryTests
    {
        [Test]
        public void CreateTreeSpriteDataIncludesTrunkAndLeafLayers()
        {
            var data = ProceduralTreeFactory.CreateTreeSpriteData(42);

            Assert.GreaterOrEqual(data.Layers.Count, 7);
            Assert.AreEqual(0, data.Layers[0].SortingOrderWithinSprite);
            Assert.IsTrue(data.Layers[0].ApplyVertexJitter);

            Object.DestroyImmediate(data);
        }

        [Test]
        public void SameSeedProducesSameTreePoints()
        {
            var first = ProceduralTreeFactory.CreateTreeSpriteData(123);
            var second = ProceduralTreeFactory.CreateTreeSpriteData(123);

            Assert.AreEqual(first.Layers.Count, second.Layers.Count);
            CollectionAssert.AreEqual(first.Layers[0].Points, second.Layers[0].Points);

            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }

        [Test]
        public void CreateTreeAddsRuntimeMeshRenderer()
        {
            var tree = ProceduralTreeFactory.CreateTree("Test Tree", 7);

            Assert.NotNull(tree.GetComponent<RuntimeMeshRenderer>());

            Object.DestroyImmediate(tree);
        }
    }
}
