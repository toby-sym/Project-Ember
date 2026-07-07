using System.Collections.Generic;
using UnityEngine;

namespace ProjectEmber.Rendering
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class RuntimeMeshRenderer : MonoBehaviour
    {
        private const string MeshName = "Project Ember Runtime Pixel Art Mesh";
        private const string PixelArtShaderName = "ProjectEmber/Pixel Art";
        private const string VertexColorShaderName = "ProjectEmber/Unlit Vertex Color";
        private static Material cachedPixelArtMaterial;
        private static Material cachedVertexColorMaterial;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh runtimeMesh;
        private Texture2D pixelArtTexture;
        private bool usePixelArt = true;

        public bool UsePixelArt
        {
            get => usePixelArt;
            set => usePixelArt = value;
        }

        public Mesh Mesh => runtimeMesh;

        private void Awake()
        {
            EnsureComponents();
        }

        public void BuildMeshFromVectorData(VectorSpriteData data)
        {
            BuildMeshFromVectorData(data, 0);
        }

        public void BuildMeshFromVectorData(VectorSpriteData data, int seed)
        {
            EnsureComponents();

            if (usePixelArt)
            {
                BuildPixelArtMesh(data, seed);
            }
            else
            {
                BuildVectorMesh(data);
            }
        }

        private void BuildPixelArtMesh(VectorSpriteData data, int seed)
        {
            EnsurePixelArtMaterial();
            EnsureMesh();

            if (data == null || data.Layers.Count == 0)
            {
                return;
            }

            // Generate pixel art texture from vector data
            pixelArtTexture = ProceduralPixelArtGenerator.GeneratePixelArtTexture(
                data.Layers.ToArray(), 
                64);

            meshRenderer.sharedMaterial.mainTexture = pixelArtTexture;
            meshRenderer.sharedMaterial.SetFloat("_PixelSize", 32.0f);
            meshRenderer.sharedMaterial.SetFloat("_WorldPixelSize", 32.0f);
            meshRenderer.sharedMaterial.SetFloat("_DitherStrength", 0.5f);

            // Create simple quad mesh for texture display (2x2 unit tiles)
            runtimeMesh.Clear();
            var vertices = new List<Vector3>
            {
                new Vector3(-1.0f, -1.0f, 0f),
                new Vector3(1.0f, -1.0f, 0f),
                new Vector3(1.0f, 1.0f, 0f),
                new Vector3(-1.0f, 1.0f, 0f)
            };

            var uvs = new List<Vector2>
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };

            var triangles = new List<int>
            {
                0, 2, 1,
                0, 3, 2
            };

            runtimeMesh.SetVertices(vertices);
            runtimeMesh.SetUVs(0, uvs);
            runtimeMesh.SetTriangles(triangles, 0);
            runtimeMesh.RecalculateBounds();
            runtimeMesh.RecalculateNormals();

            Debug.Log($"Built pixel art mesh on {gameObject.name}");
        }

        private void BuildVectorMesh(VectorSpriteData data)
        {
            EnsureVertexColorMaterial();
            EnsureMesh();

            runtimeMesh.Clear();

            if (data == null || data.Layers.Count == 0)
            {
                return;
            }

            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var triangles = new List<int>();

            foreach (var tessellated in VectorSpriteTessellator.Tessellate(data))
            {
                var layer = tessellated.Layer;
                var points = tessellated.Points;
                var layerTriangles = tessellated.Triangles;

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

            Debug.Log($"Built vector mesh with {vertices.Count} vertices on {gameObject.name}");
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

        private void EnsurePixelArtMaterial()
        {
            if (cachedPixelArtMaterial == null)
            {
                var shader = Shader.Find(PixelArtShaderName);
                if (shader == null)
                {
                    Debug.LogWarning($"[RuntimeMeshRenderer] Shader '{PixelArtShaderName}' not found, falling back to 'Sprites/Default'.");
                    shader = Shader.Find("Sprites/Default");
                }

                if (shader == null)
                {
                    Debug.LogError("[RuntimeMeshRenderer] No fallback shader available; material will not be created.");
                    return;
                }

                cachedPixelArtMaterial = new Material(shader)
                {
                    name = "Project Ember Pixel Art Material"
                };
            }

            meshRenderer.sharedMaterial = cachedPixelArtMaterial;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            meshRenderer.receiveShadows = true;
        }

        private void EnsureVertexColorMaterial()
        {
            if (cachedVertexColorMaterial == null)
            {
                var shader = Shader.Find(VertexColorShaderName);
                if (shader == null)
                {
                    Debug.LogWarning($"[RuntimeMeshRenderer] Shader '{VertexColorShaderName}' not found, falling back to 'Sprites/Default'.");
                    shader = Shader.Find("Sprites/Default");
                }

                if (shader == null)
                {
                    Debug.LogError("[RuntimeMeshRenderer] No fallback shader available; material will not be created.");
                    return;
                }

                cachedVertexColorMaterial = new Material(shader)
                {
                    name = "Project Ember Runtime Vertex Color Material"
                };
            }

            meshRenderer.sharedMaterial = cachedVertexColorMaterial;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
        }

        private void OnDestroy()
        {
            if (pixelArtTexture != null)
            {
                Destroy(pixelArtTexture);
            }
        }
    }
}
