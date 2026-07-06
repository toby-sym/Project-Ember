using ProjectEmber.Shared;
using ProjectEmber.World;
using UnityEngine;

namespace ProjectEmber.Gameplay
{
    public sealed class ToolGatheringController : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private float range = 2.25f;

        private InventorySystem inventory;

        public void Initialize(InventorySystem inventorySystem)
        {
            inventory = inventorySystem;
        }

        private void Awake()
        {
            worldCamera ??= Camera.main;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0) || worldCamera == null || inventory == null)
            {
                return;
            }

            if (!inventory.HasItem(ItemType.Axe))
            {
                return;
            }

            var mouse = worldCamera.ScreenToWorldPoint(Input.mousePosition);
            mouse.z = 0f;
            if (Vector3.Distance(transform.position, mouse) > range)
            {
                return;
            }

            var direction = mouse - transform.position;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            var hit = Physics2D.Raycast(transform.position, direction.normalized, range);
            if (hit.collider == null)
            {
                return;
            }

            var tree = hit.collider.GetComponentInParent<HarvestableTree>();
            if (tree == null)
            {
                return;
            }

            tree.TryDamage(1, inventory);
        }
    }
}
