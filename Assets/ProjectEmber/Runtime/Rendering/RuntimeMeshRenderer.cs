using System.Collections.Generic;
using UnityEngine;

namespace ProjectEmber.Rendering
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class RuntimeMeshRenderer : MonoBehaviour
    {
        private const string MeshName = "Project Ember Runtime Vector Mesh";
        private const string VertexColorShaderName = "ProjectEmber/Unlit Vertex Color";

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh runtimeMesh;

        public Mesh Mesh => runtimeMesh;

        private void Awake()
        {
            EnsureComponents();
            EnsureMaterial();
        }

        public void BuildMeshFromVectorData(VectorSpriteData data)
        {
            EnsureComponents();
            EnsureMaterial();
            EnsureMesh();

            runtimeMesh.Clear();

            if (data == null || data.Layers.Count == 0)
            {
                return;
            }

            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var triangles = new List<int>();
            var sortedLayers = new List<VectorLayer>(data.Layers);
            sortedLayers.Sort((left, right) => left.SortingOrderWithinSprite.CompareTo(right.SortingOrderWithinSprite));

            foreach (var layer in sortedLayers)
            {
                if (layer == null || !layer.CloseLoop)
                {
                    continue;
                }

                var points = layer.GetRenderPoints();
                if (points.Length < 3)
                {
                    continue;
                }

                var layerTriangles = PolygonTriangulator.Triangulate(points);
                if (layerTriangles.Count == 0)
                {
                    continue;
                }

                var vertexOffset = vertices.Count;
                var z = layer.SortingOrderWithinSprite * -0.001f;
                for (var i = 0; i < points.Length; i++)
                {
                    vertices.Add(new Vector3(points[i].x, points[i].y, z));
                    colors.Add(layer.Color);
                }

                for (var i = 0; i < layerTriangles.Count; i++)
                {
                    triangles.Add(vertexOffset + layerTriangles[i]);
                }
            }

            runtimeMesh.SetVertices(vertices);
            runtimeMesh.SetColors(colors);
            runtimeMesh.SetTriangles(triangles, 0);
            runtimeMesh.RecalculateBounds();
            runtimeMesh.RecalculateNormals();
        }

        private void EnsureComponents()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }
        }

        private void EnsureMesh()
        {
            if (runtimeMesh != null)
            {
                return;
            }

            runtimeMesh = new Mesh
            {
                name = MeshName
            };
            meshFilter.sharedMesh = runtimeMesh;
        }

        private void EnsureMaterial()
        {
            if (meshRenderer.sharedMaterial != null)
            {
                return;
            }

            var shader = Shader.Find(VertexColorShaderName) ?? Shader.Find("Sprites/Default");
            meshRenderer.sharedMaterial = new Material(shader)
            {
                name = "Project Ember Runtime Vertex Color Material"
            };
        }
    }
}
