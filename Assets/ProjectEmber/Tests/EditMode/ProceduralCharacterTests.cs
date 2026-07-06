using NUnit.Framework;
using ProjectEmber.ProceduralAssets;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class ProceduralCharacterTests
    {
        private GameObject gameObject;
        private ProceduralCharacter character;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("Procedural character test");
            character = gameObject.AddComponent<ProceduralCharacter>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void RebuildCreatesExpectedBodyParts()
        {
            character.Rebuild();

            Assert.NotNull(gameObject.transform.Find("Head"));
            Assert.NotNull(gameObject.transform.Find("Torso"));
            Assert.NotNull(gameObject.transform.Find("LeftArm"));
            Assert.NotNull(gameObject.transform.Find("RightArm"));
            Assert.NotNull(gameObject.transform.Find("LeftLeg"));
            Assert.NotNull(gameObject.transform.Find("RightLeg"));
        }

        [Test]
        public void RandomizeAppearanceIsDeterministicForSeed()
        {
            character.RandomizeAppearance(99);
            var hair = character.HairColor;
            var clothing = character.ClothingColor;
            var skin = character.SkinTone;

            character.RandomizeAppearance(99);

            Assert.AreEqual(hair, character.HairColor);
            Assert.AreEqual(clothing, character.ClothingColor);
            Assert.AreEqual(skin, character.SkinTone);
        }
    }
}
