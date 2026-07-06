using NUnit.Framework;
using ProjectEmber.ProceduralAssets;
using ProjectEmber.Shared;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class VectorItemIconFactoryTests
    {
        [TestCase(ItemType.Axe)]
        [TestCase(ItemType.Pickaxe)]
        [TestCase(ItemType.Sword)]
        [TestCase(ItemType.Logs)]
        [TestCase(ItemType.Berries)]
        public void CreateIconReturnsRenderableLayers(ItemType itemType)
        {
            var icon = VectorItemIconFactory.CreateIcon(itemType);

            Assert.NotNull(icon);
            Assert.Greater(icon.Layers.Count, 0);
            Assert.IsTrue(icon.Layers.TrueForAll(layer => layer.Points.Length >= 3));

            Object.DestroyImmediate(icon);
        }
    }
}
