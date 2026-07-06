using ProjectEmber.Shared;
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
            if (!Input.GetMouseButtonDown(0) || worldCamera == null)
            {
                return;
            }

            var mouse = worldCamera.ScreenToWorldPoint(Input.mousePosition);
            mouse.z = 0f;
            if (Vector3.Distance(transform.position, mouse) > range)
            {
                return;
            }

            var hit = Physics2D.OverlapCircle(mouse, 0.35f);
            if (hit == null || !hit.name.Contains("Tree"))
            {
                return;
            }

            Destroy(hit.gameObject);
            inventory?.TryAddItem(ItemType.Logs, 2);
        }
    }
}
