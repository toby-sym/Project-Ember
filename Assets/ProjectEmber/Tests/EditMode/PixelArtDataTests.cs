using System.Collections.Generic;
using NUnit.Framework;
using ProjectEmber.Rendering;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class PixelArtDataTests
    {
        [Test]
        public void FrameDurationSetterClampsToMinimum()
        {
            var frame = new PixelArtFrame("idle");

            frame.Duration = 0.0001f;

            Assert.AreEqual(0.01f, frame.Duration);
        }

        [Test]
        public void FrameDurationSetterKeepsValidValues()
        {
            var frame = new PixelArtFrame("idle") { Duration = 0.5f };

            Assert.AreEqual(0.5f, frame.Duration);
            Assert.AreEqual("idle", frame.SpriteName);
        }

        [Test]
        public void AddFrameWithoutDurationUsesFrameRate()
        {
            var animation = new PixelArtAnimation("walk", frameRate: 10f);

            animation.AddFrame("walk_0");

            Assert.AreEqual(1, animation.Frames.Count);
            Assert.AreEqual(0.1f, animation.Frames[0].Duration, 0.0001f);
        }

        [Test]
        public void FrameRateSetterClampsToMinimum()
        {
            var animation = new PixelArtAnimation("walk") { FrameRate = -5f };

            Assert.AreEqual(0.1f, animation.FrameRate);
        }

        [Test]
        public void AnimationFramesSetterCoercesNullToEmptyList()
        {
            var animation = new PixelArtAnimation("walk") { Frames = null };

            Assert.IsNotNull(animation.Frames);
            Assert.IsEmpty(animation.Frames);
        }

        [Test]
        public void CharacterDataResolvesAnimationsByName()
        {
            var character = ScriptableObject.CreateInstance<PixelArtCharacterData>();
            try
            {
                var walk = new PixelArtAnimation("walk");
                character.Animations = new List<PixelArtAnimation> { walk };
                character.DefaultAnimation = "walk";

                Assert.AreSame(walk, character.GetAnimation("walk"));
                Assert.IsNull(character.GetAnimation("missing"));
                Assert.IsNull(character.GetAnimation(string.Empty));
                Assert.AreSame(walk, character.GetDefaultAnimation());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(character);
            }
        }

        [Test]
        public void TreeDataFoliageSpreadRadiusClampsNegativeToZero()
        {
            var tree = ScriptableObject.CreateInstance<PixelArtTreeData>();
            try
            {
                tree.FoliageSpreadRadius = -3f;

                Assert.AreEqual(0f, tree.FoliageSpreadRadius);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tree);
            }
        }

        [Test]
        public void TileDataGetRandomVariantReturnsNullWhenNoVariants()
        {
            var tile = ScriptableObject.CreateInstance<PixelArtTileData>();
            try
            {
                tile.VariantSprites = new List<string>();

                Assert.IsNull(tile.GetRandomVariant(new System.Random(1)));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tile);
            }
        }

        [Test]
        public void TileDataGetRandomVariantReturnsExistingVariant()
        {
            var tile = ScriptableObject.CreateInstance<PixelArtTileData>();
            try
            {
                var variants = new List<string> { "a", "b", "c" };
                tile.VariantSprites = variants;

                var variant = tile.GetRandomVariant(new System.Random(1));

                Assert.Contains(variant, variants);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tile);
            }
        }
    }
}
