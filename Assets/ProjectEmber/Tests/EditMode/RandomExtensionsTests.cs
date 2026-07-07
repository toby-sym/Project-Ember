using NUnit.Framework;
using ProjectEmber.Shared;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class RandomExtensionsTests
    {
        [Test]
        public void NextFloatStaysWithinRequestedRange()
        {
            var random = new System.Random(7);

            for (var i = 0; i < 256; i++)
            {
                var value = random.NextFloat(-2f, 3f);
                Assert.GreaterOrEqual(value, -2f);
                Assert.LessOrEqual(value, 3f);
            }
        }

        [Test]
        public void NextFloatIsDeterministicForSameSeed()
        {
            var first = new System.Random(99);
            var second = new System.Random(99);

            for (var i = 0; i < 32; i++)
            {
                Assert.AreEqual(first.NextFloat(0f, 1f), second.NextFloat(0f, 1f));
            }
        }

        [Test]
        public void NextColorChannelsStayWithinRange()
        {
            var random = new System.Random(3);

            for (var i = 0; i < 64; i++)
            {
                var color = random.NextColor(0.2f, 0.8f);
                Assert.GreaterOrEqual(color.r, 0.2f);
                Assert.LessOrEqual(color.r, 0.8f);
                Assert.GreaterOrEqual(color.g, 0.2f);
                Assert.LessOrEqual(color.g, 0.8f);
                Assert.GreaterOrEqual(color.b, 0.2f);
                Assert.LessOrEqual(color.b, 0.8f);
            }
        }

        [Test]
        public void NextColorIsDeterministicForSameSeed()
        {
            var first = new System.Random(21);
            var second = new System.Random(21);

            Assert.AreEqual(first.NextColor(0f, 1f), second.NextColor(0f, 1f));
        }
    }
}
