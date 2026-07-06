using NUnit.Framework;
using ProjectEmber.Rendering;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class RuntimeMeshRendererTests
    {
        private GameObject gameObject;
        private RuntimeMeshRenderer renderer;
        private VectorSpriteData data;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("Runtime mesh renderer test");
            renderer = gameObject.AddComponent<RuntimeMeshRenderer>();
            data = ScriptableObject.CreateInstance<VectorSpriteData>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(data);
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void BuildMeshFromVectorDataCreatesColoredTriangleMesh()
        {
            data.Layers.Add(new VectorLayer(
                new[]
                {
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0f),
                    new Vector2(0f, 1f)
                },
                Color.red));

            renderer.BuildMeshFromVectorData(data);

            Assert.NotNull(renderer.Mesh);
            Assert.AreEqual(3, renderer.Mesh.vertexCount);
            Assert.AreEqual(3, renderer.Mesh.triangles.Length);
            CollectionAssert.AreEqual(new[] { Color.red, Color.red, Color.red }, renderer.Mesh.colors);
        }

        [Test]
        public void BuildMeshFromVectorDataTriangulatesConcavePolygon()
        {
            data.Layers.Add(new VectorLayer(
                new[]
                {
                    new Vector2(-1f, -1f),
                    new Vector2(1f, -1f),
                    new Vector2(0f, 0f),
                    new Vector2(1f, 1f),
                    new Vector2(-1f, 1f)
                },
                Color.white));

            renderer.BuildMeshFromVectorData(data);

            Assert.AreEqual(5, renderer.Mesh.vertexCount);
            Assert.AreEqual(9, renderer.Mesh.triangles.Length);
        }

        [Test]
        public void BuildMeshFromVectorDataSkipsOpenLayers()
        {
            data.Layers.Add(new VectorLayer(new[] { Vector2.zero, Vector2.right, Vector2.up }, Color.white)
            {
                CloseLoop = false
            });

            renderer.BuildMeshFromVectorData(data);

            Assert.AreEqual(0, renderer.Mesh.vertexCount);
            Assert.AreEqual(0, renderer.Mesh.triangles.Length);
        }
    }
}
