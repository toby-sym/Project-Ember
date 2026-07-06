using ProjectEmber.Rendering;
using UnityEngine;

namespace ProjectEmber.ProceduralAssets
{
    public sealed class ProceduralCharacter : MonoBehaviour
    {
        [SerializeField] private PixelArtCharacterData characterData;
        [SerializeField] private bool usePixelArt = true;
        [SerializeField] private Color hairColor = new(0.16f, 0.09f, 0.04f);
        [SerializeField] private Color clothingColor = new(0.18f, 0.34f, 0.42f);
        [SerializeField] private Color skinTone = new(0.82f, 0.56f, 0.38f);
        
        private RuntimeMeshRenderer meshRenderer;
        private Texture2D characterTexture;

        public PixelArtCharacterData CharacterData
        {
            get => characterData;
            set => characterData = value;
        }

        public bool UsePixelArt
        {
            get => usePixelArt;
            set => usePixelArt = value;
        }

        private void Awake()
        {
            meshRenderer = GetComponent<RuntimeMeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<RuntimeMeshRenderer>();
            }
            meshRenderer.UsePixelArt = usePixelArt;
        }

        public void Rebuild()
        {
            meshRenderer.UsePixelArt = usePixelArt;
            
            if (usePixelArt)
            {
                BuildPixelArtCharacter();
            }
            else
            {
                BuildGeometricCharacter();
            }
        }

        public void RandomizeAppearance(int seed)
        {
            var random = new System.Random(seed);
            hairColor = RandomColor(random, 0.05f, 0.24f);
            clothingColor = RandomColor(random, 0.18f, 0.72f);
            skinTone = new Color(
                RandomRange(random, 0.55f, 0.92f),
                RandomRange(random, 0.36f, 0.68f),
                RandomRange(random, 0.24f, 0.5f));
            
            Rebuild();
        }

        private void BuildPixelArtCharacter()
        {
            var seed = hairColor.GetHashCode() + clothingColor.GetHashCode() + skinTone.GetHashCode();
            characterTexture = ProceduralPixelArtGenerator.GenerateCharacterTexture(
                hairColor, clothingColor, skinTone, seed, 32);
            
            // Create simple quad with character texture
            var data = ScriptableObject.CreateInstance<VectorSpriteData>();
            var layer = ProceduralShapeUtility.GenerateBoxPolygon(1f, 1.5f);
            layer.Color = Color.white;
            data.Layers.Add(layer);
            
            meshRenderer.BuildMeshFromVectorData(data, seed);
            
            // Apply the generated texture to the material
            if (meshRenderer.GetComponent<MeshRenderer>() != null && characterTexture != null)
            {
                meshRenderer.GetComponent<MeshRenderer>().material.mainTexture = characterTexture;
            }
            
            DisposeTemporaryData(data);
        }

        private void BuildGeometricCharacter()
        {
            var data = ScriptableObject.CreateInstance<VectorSpriteData>();
            
            data.Layers.Add(new VectorLayer(
                ProceduralShapeUtility.GenerateCapsulePolygon(0.56f, 0.9f).Points,
                clothingColor, 0));
            data.Layers.Add(new VectorLayer(
                ProceduralShapeUtility.GenerateCirclePolygon(0.28f, 18).Points,
                skinTone, 10));
            data.Layers.Add(new VectorLayer(
                ProceduralShapeUtility.GenerateCirclePolygon(0.3f, 14).Points,
                hairColor, 11));
            
            meshRenderer.BuildMeshFromVectorData(data);
            DisposeTemporaryData(data);
        }

        private void Reset()
        {
            Rebuild();
        }

        private void OnDestroy()
        {
            if (characterTexture != null)
            {
                Destroy(characterTexture);
            }
        }

        private static Color RandomColor(System.Random random, float min, float max)
        {
            return new Color(RandomRange(random, min, max), RandomRange(random, min, max), RandomRange(random, min, max));
        }

        private static float RandomRange(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        private static void DisposeTemporaryData(VectorSpriteData data)
        {
            if (Application.isPlaying)
            {
                Destroy(data);
            }
            else
            {
                DestroyImmediate(data);
            }
        }
    }
}
