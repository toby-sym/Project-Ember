using NUnit.Framework;
using ProjectEmber.Rendering;
using ProjectEmber.Shared;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class ProceduralPixelArtItemTests
    {
        [TestCase(ItemType.Axe)]
        [TestCase(ItemType.Pickaxe)]
        [TestCase(ItemType.Sword)]
        [TestCase(ItemType.Logs)]
        [TestCase(ItemType.Berries)]
        public void GenerateItemTextureProducesPointFilteredTexture(ItemType itemType)
        {
            var texture = ProceduralPixelArtGenerator.GenerateItemTexture(itemType, 1234, 32);

            Assert.NotNull(texture);
            Assert.AreEqual(32, texture.width);
            Assert.AreEqual(32, texture.height);
            Assert.AreEqual(FilterMode.Point, texture.filterMode);

            var opaquePixels = 0;
            foreach (var pixel in texture.GetPixels())
            {
                if (pixel.a > 0.5f)
                {
                    opaquePixels++;
                }
            }

            Assert.Greater(opaquePixels, 0, "Item texture should draw at least some opaque pixels.");

            Object.DestroyImmediate(texture);
        }
    }
}
