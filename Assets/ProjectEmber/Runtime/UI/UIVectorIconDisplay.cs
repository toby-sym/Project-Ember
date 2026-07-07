using ProjectEmber.Rendering;
using ProjectEmber.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEmber.UI
{
    /// <summary>
    /// Renders a procedurally generated pixel-art item icon into the Canvas by
    /// drawing a textured quad, replacing the earlier vector-polygon tessellation.
    /// </summary>
    public sealed class UIVectorIconDisplay : MaskableGraphic
    {
        [SerializeField] private ItemType itemType;
        [SerializeField, Range(0f, 0.45f)] private float padding = 0.12f;

        private Texture2D iconTexture;

        public ItemType ItemType => itemType;
        public Texture2D IconTexture => iconTexture;

        public override Texture mainTexture => iconTexture != null ? iconTexture : base.mainTexture;

        public void SetIcon(ItemType type)
        {
            itemType = type;
            RegenerateTexture();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (iconTexture == null)
            {
                RegenerateTexture();
            }
        }

        private void RegenerateTexture()
        {
            DestroyTexture();
            iconTexture = ProceduralPixelArtGenerator.GenerateItemTexture(itemType, itemType.GetHashCode());
            SetMaterialDirty();
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            if (iconTexture == null)
            {
                return;
            }

            var rect = GetPixelAdjustedRect();
            var inset = padding;
            var xMin = rect.xMin + rect.width * inset;
            var xMax = rect.xMax - rect.width * inset;
            var yMin = rect.yMin + rect.height * inset;
            var yMax = rect.yMax - rect.height * inset;

            var tint = color;
            vertexHelper.AddVert(new Vector3(xMin, yMin), tint, new Vector2(0f, 0f));
            vertexHelper.AddVert(new Vector3(xMin, yMax), tint, new Vector2(0f, 1f));
            vertexHelper.AddVert(new Vector3(xMax, yMax), tint, new Vector2(1f, 1f));
            vertexHelper.AddVert(new Vector3(xMax, yMin), tint, new Vector2(1f, 0f));

            vertexHelper.AddTriangle(0, 1, 2);
            vertexHelper.AddTriangle(2, 3, 0);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyTexture();
        }

        private void DestroyTexture()
        {
            if (iconTexture == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(iconTexture);
            }
            else
            {
                DestroyImmediate(iconTexture);
            }

            iconTexture = null;
        }
    }
}
