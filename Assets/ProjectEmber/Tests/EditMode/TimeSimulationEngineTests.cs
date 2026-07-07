using NUnit.Framework;
using ProjectEmber.Simulation;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class TimeSimulationEngineTests
    {
        private GameObject host;
        private TimeSimulationEngine engine;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("TimeSimulation");
            engine = host.AddComponent<TimeSimulationEngine>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host);
        }

        [Test]
        public void StartsAtMorningOnDayOne()
        {
            Assert.AreEqual(0, engine.Minute);
            Assert.AreEqual(8, engine.Hour);
            Assert.AreEqual(1, engine.Day);
            Assert.AreEqual(0, engine.Season);
        }

        [Test]
        public void NormalizedDayTimeReflectsHourAndMinute()
        {
            var expected = (8f * 60f + 0f) / 1440f;

            Assert.AreEqual(expected, engine.NormalizedDayTime, 0.0001f);
        }
    }
}
