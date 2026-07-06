using ProjectEmber.Gameplay;
using ProjectEmber.ProceduralAssets;
using ProjectEmber.Shared;
using ProjectEmber.Simulation;
using ProjectEmber.UI;
using ProjectEmber.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectEmber.Bootstrap
{
    public sealed class ProjectEmberDemoBootstrap : MonoBehaviour
    {
        private const int WorldSeed = 20260706;

        private InventorySystem inventory;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureDemoBootstrap()
        {
            if (FindFirstObjectByType<ProjectEmberDemoBootstrap>() != null)
            {
                return;
            }

            var systems = GameObject.Find("Systems") ?? new GameObject("Systems");
            systems.AddComponent<ProjectEmberDemoBootstrap>();
        }

        private void Start()
        {
            inventory = new InventorySystem(8);
            inventory.TryAddItem(ItemType.Axe, 1);
            inventory.TryAddItem(ItemType.Pickaxe, 1);
            inventory.TryAddItem(ItemType.Sword, 1);
            inventory.TryAddItem(ItemType.Berries, 5);

            var player = SetupPlayer();
            SetupWorld(player.transform);
            SetupCamera(player.transform);
            SetupUi();
            SetupSimulation();
        }

        private GameObject SetupPlayer()
        {
            var player = GameObject.Find("Player") ?? new GameObject("Player");
            player.tag = "Player";
            player.transform.position = Vector3.zero;

            var character = player.GetComponent<ProceduralCharacter>() ?? player.AddComponent<ProceduralCharacter>();
            character.RandomizeAppearance(WorldSeed);

            if (player.GetComponent<Rigidbody2D>() == null)
            {
                player.AddComponent<Rigidbody2D>();
            }

            var movement = player.GetComponent<PlayerTopDownController>() ?? player.AddComponent<PlayerTopDownController>();
            var bobbing = player.GetComponent<ProceduralWalkBobbing>() ?? player.AddComponent<ProceduralWalkBobbing>();
            var gather = player.GetComponent<ToolGatheringController>() ?? player.AddComponent<ToolGatheringController>();
            gather.Initialize(inventory);
            return player;
        }

        private static void SetupWorld(Transform player)
        {
            var world = GameObject.Find("World") ?? new GameObject("World");
            var chunkManager = world.GetComponent<ChunkManager>() ?? world.AddComponent<ChunkManager>();
            chunkManager.Initialize(player, WorldSeed, 1);
        }

        private static void SetupCamera(Transform player)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            if (camera.GetComponent<AudioListener>() == null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }

            camera.orthographic = true;
            camera.orthographicSize = 7f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            var follow = camera.GetComponent<CameraFollow2D>() ?? camera.gameObject.AddComponent<CameraFollow2D>();
            follow.SetTarget(player);
        }

        private void SetupUi()
        {
            var uiRoot = GameObject.Find("UI");
            if (uiRoot == null || uiRoot.GetComponent<Canvas>() == null)
            {
                if (uiRoot != null)
                {
                    Destroy(uiRoot);
                }

                uiRoot = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            }

            var canvas = uiRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var canvasScaler = uiRoot.GetComponent<CanvasScaler>() ?? uiRoot.AddComponent<CanvasScaler>();
            var graphicRaycaster = uiRoot.GetComponent<GraphicRaycaster>() ?? uiRoot.AddComponent<GraphicRaycaster>();

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            var hotbar = new GameObject("Hotbar", typeof(RectTransform));
            hotbar.transform.SetParent(uiRoot.transform, false);
            var hotbarRect = hotbar.GetComponent<RectTransform>();
            hotbarRect.anchorMin = new Vector2(0.5f, 0f);
            hotbarRect.anchorMax = new Vector2(0.5f, 0f);
            hotbarRect.pivot = new Vector2(0.5f, 0f);
            hotbarRect.anchoredPosition = new Vector2(0f, 20f);
            hotbarRect.sizeDelta = new Vector2(420f, 72f);

            var layout = hotbar.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var items = new[] { ItemType.Axe, ItemType.Pickaxe, ItemType.Sword, ItemType.Logs, ItemType.Berries };
            for (var i = 0; i < items.Length; i++)
            {
                CreateHotbarSlot(hotbar.transform, items[i]);
            }
        }

        private static void CreateHotbarSlot(Transform parent, ItemType type)
        {
            var slot = new GameObject($"{type} Slot", typeof(RectTransform));
            slot.transform.SetParent(parent, false);
            var rect = slot.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(64f, 64f);
            slot.AddComponent<CanvasRenderer>();
            var background = slot.AddComponent<Image>();
            background.color = new Color(0.08f, 0.07f, 0.06f, 0.78f);

            var iconObject = new GameObject($"{type} Icon", typeof(RectTransform));
            iconObject.transform.SetParent(slot.transform, false);
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(6f, 6f);
            iconRect.offsetMax = new Vector2(-6f, -6f);
            iconObject.AddComponent<CanvasRenderer>();
            iconObject.AddComponent<UIVectorIconDisplay>().SetIcon(type);
        }

        private static void SetupSimulation()
        {
            var systems = GameObject.Find("Systems") ?? new GameObject("Systems");
            var simulation = systems.GetComponent<TimeSimulationEngine>() ?? systems.AddComponent<TimeSimulationEngine>();
        }
    }
}
