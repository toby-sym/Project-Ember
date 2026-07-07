using ProjectEmber.ProceduralAssets;
using ProjectEmber.Rendering;
using ProjectEmber.Shared;
using UnityEngine;

namespace ProjectEmber.World
{
    [DisallowMultipleComponent]
    public sealed class WorldResourceDrop : MonoBehaviour
    {
        [SerializeField] private ItemType itemType;
        [SerializeField] private int quantity = 1;
        [SerializeField] private float pickupRadius = 0.75f;
        [SerializeField] private float bobSpeed = 4f;
        [SerializeField] private float bobHeight = 0.12f;

        private IItemInventory inventory;
        private Vector3 startPosition;
        private float phase;

        public void Initialize(ItemType type, int amount, IItemInventory targetInventory)
        {
            itemType = type;
            quantity = Mathf.Max(1, amount);
            inventory = targetInventory;
            startPosition = transform.position;

            var meshRenderer = gameObject.GetComponent<RuntimeMeshRenderer>() ?? gameObject.AddComponent<RuntimeMeshRenderer>();
            meshRenderer.UsePixelArt = true;

            var data = ScriptableObject.CreateInstance<VectorSpriteData>();
            var quad = ProceduralShapeUtility.GenerateBoxPolygon(1f, 1f);
            quad.Color = Color.white;
            data.Layers.Add(quad);
            meshRenderer.BuildMeshFromVectorData(data, (int)type);

            var renderer = gameObject.GetComponent<MeshRenderer>();
            var itemTexture = ProceduralPixelArtGenerator.GenerateItemTexture(type, (int)type);
            if (renderer != null && itemTexture != null)
            {
                renderer.material.mainTexture = itemTexture;
            }

            data.DisposeTemporary();
        }

        private void Update()
        {
            phase += Time.deltaTime * bobSpeed;
            transform.position = startPosition + new Vector3(0f, Mathf.Sin(phase) * bobHeight, 0f);

            var player = GameObject.Find("Player");
            if (player == null)
            {
                return;
            }

            if (Vector3.Distance(player.transform.position, transform.position) <= pickupRadius)
            {
                if (inventory == null)
                {
                    Debug.LogWarning($"[WorldResourceDrop] No inventory assigned on '{gameObject.name}'; {quantity}x {itemType} lost on pickup.");
                }
                else
                {
                    if (!inventory.TryAddItem(itemType, quantity))
                    {
                        Debug.LogWarning($"[WorldResourceDrop] Failed to add {quantity}x {itemType} to inventory (full?). Item lost.");
                    }
                }

                Destroy(gameObject);
            }
        }
    }
}