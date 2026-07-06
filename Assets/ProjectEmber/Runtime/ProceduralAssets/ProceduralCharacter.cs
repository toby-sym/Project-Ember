using ProjectEmber.Rendering;
using UnityEngine;

namespace ProjectEmber.ProceduralAssets
{
    public sealed class ProceduralCharacter : MonoBehaviour
    {
        [SerializeField] private Color hairColor = new(0.16f, 0.09f, 0.04f);
        [SerializeField] private Color clothingColor = new(0.18f, 0.34f, 0.42f);
        [SerializeField] private Color skinTone = new(0.82f, 0.56f, 0.38f);

        public Color HairColor
        {
            get => hairColor;
            set => hairColor = value;
        }

        public Color ClothingColor
        {
            get => clothingColor;
            set => clothingColor = value;
        }

        public Color SkinTone
        {
            get => skinTone;
            set => skinTone = value;
        }

        public void Rebuild()
        {
            ClearChildren();
            AddPart("Torso", new Vector2(0f, 0.15f), ProceduralShapeUtility.GenerateCapsulePolygon(0.56f, 0.9f), clothingColor, 0);
            AddPart("Head", new Vector2(0f, 0.86f), ProceduralShapeUtility.GenerateCirclePolygon(0.28f, 18), skinTone, 10);
            AddPart("Hair", new Vector2(0f, 1.02f), ProceduralShapeUtility.GenerateCirclePolygon(0.3f, 14), hairColor, 11);
            AddPart("LeftArm", new Vector2(-0.45f, 0.15f), ProceduralShapeUtility.GenerateCapsulePolygon(0.18f, 0.72f), skinTone, -1, 18f);
            AddPart("RightArm", new Vector2(0.45f, 0.15f), ProceduralShapeUtility.GenerateCapsulePolygon(0.18f, 0.72f), skinTone, -1, -18f);
            AddPart("LeftLeg", new Vector2(-0.16f, -0.55f), ProceduralShapeUtility.GenerateCapsulePolygon(0.2f, 0.75f), clothingColor, -2);
            AddPart("RightLeg", new Vector2(0.16f, -0.55f), ProceduralShapeUtility.GenerateCapsulePolygon(0.2f, 0.75f), clothingColor, -2);
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

        private void Reset()
        {
            Rebuild();
        }

        private void AddPart(string partName, Vector2 localPosition, VectorLayer layer, Color color, int sortingOrder, float zRotation = 0f)
        {
            layer.Color = color;
            layer.SortingOrderWithinSprite = sortingOrder;

            var part = new GameObject(partName);
            part.transform.SetParent(transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);

            var data = ScriptableObject.CreateInstance<VectorSpriteData>();
            data.Layers.Add(layer);
            part.AddComponent<RuntimeMeshRenderer>().BuildMeshFromVectorData(data);
            DisposeTemporaryData(data);
        }

        private void ClearChildren()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
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
