using NUnit.Framework;
using ProjectEmber.Shared;
using ProjectEmber.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class UIVectorIconDisplayTests
    {
        private GameObject gameObject;
        private UIVectorIconDisplay display;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("UI vector icon test", typeof(RectTransform));
            gameObject.transform.SetParent(new GameObject("Canvas", typeof(Canvas)).transform);
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(64f, 64f);
            display = gameObject.AddComponent<UIVectorIconDisplay>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject.transform.parent.gameObject);
        }

        [Test]
        public void SetIconGeneratesPixelArtTexture()
        {
            display.SetIcon(ItemType.Axe);

            Assert.AreEqual(ItemType.Axe, display.ItemType);
            Assert.NotNull(display.IconTexture);
            Assert.AreEqual(FilterMode.Point, display.IconTexture.filterMode);
            Assert.AreSame(display.IconTexture, display.mainTexture);
        }
    }
}
