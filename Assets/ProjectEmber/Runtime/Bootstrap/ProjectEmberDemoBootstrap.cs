using ProjectEmber.Gameplay;
using ProjectEmber.ProceduralAssets;
using ProjectEmber.Save;
using ProjectEmber.Shared;
using ProjectEmber.Simulation;
using ProjectEmber.UI;
using ProjectEmber.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace ProjectEmber.Bootstrap
{
    public sealed class ProjectEmberDemoBootstrap : MonoBehaviour
    {
        private const int WorldSeed = 20260706;
        private const int TownNpcCount = 3;
        private const string SaveFileName = "project-ember-save.json";
        private const KeyCode SaveHotkey = KeyCode.F5;

        private InventorySystem inventory;
        private Transform playerTransform;
        private ChunkManager chunkManager;
        private TimeSimulationEngine simulation;

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
            var _ = Rendering.PixelArtSpriteManager.Instance;
            
            inventory = new InventorySystem(8);
            inventory.TryAddItem(ItemType.Axe, 1);
            inventory.TryAddItem(ItemType.Pickaxe, 1);
            inventory.TryAddItem(ItemType.Sword, 1);
            inventory.TryAddItem(ItemType.Berries, 5);

            var player = SetupPlayer();
            chunkManager = SetupWorld(player.transform);
            SetupCamera(player.transform);
            SetupUi();
            simulation = SetupSimulation();
            playerTransform = player.transform;

            LoadSavedProfile(player.transform);
            SpawnTownNpcs(player.transform);

            Debug.Log($"Bootstrap complete. Player at {player.transform.position}, Camera at {Camera.main.transform.position}");
        }

        private void Update()
        {
            if (Input.GetKeyDown(SaveHotkey))
            {
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        private void LoadSavedProfile(Transform player)
        {
            var profile = SaveManager.ReadFromDisk(SaveFileName);
            if (profile == null)
            {
                return;
            }

            SaveManager.ApplyProfile(profile, player, simulation, inventory, chunkManager);
            Debug.Log($"[Bootstrap] Loaded save: player {profile.PlayerPosition}, {profile.Hour:00}:{profile.Minute:00}, {profile.ChunkDeltas?.Length ?? 0} chunk delta(s).");
        }

        private void SaveGame()
        {
            if (playerTransform == null || chunkManager == null)
            {
                return;
            }

            var profile = SaveManager.CreateProfile(
                playerTransform.position, inventory, simulation, chunkManager.Registry);
            if (SaveManager.WriteToDisk(SaveFileName, profile))
            {
                Debug.Log("[Bootstrap] Game saved.");
            }
        }

        private void SpawnTownNpcs(Transform player)
        {
            if (chunkManager == null)
            {
                return;
            }

            var origin = new Vector2Int(
                Mathf.RoundToInt(player.position.x),
                Mathf.RoundToInt(player.position.y));

            for (var i = 0; i < TownNpcCount; i++)
            {
                var home = FindNearestWalkable(origin + new Vector2Int(-4 - i, 3 + i));
                var market = FindNearestWalkable(origin + new Vector2Int(5 + i, -4 - i));

                var npcObject = new GameObject($"Town NPC {i}");
                npcObject.transform.position = new Vector3(home.x, home.y, 0f);
                var npc = npcObject.AddComponent<TownNpc>();
                npc.Initialize(chunkManager, simulation, home, market, WorldSeed + i * 7919);
            }
        }

        private Vector2Int FindNearestWalkable(Vector2Int origin)
        {
            if (chunkManager.IsWorldTileWalkable(origin))
            {
                return origin;
            }

            for (var radius = 1; radius <= 12; radius++)
            {
                for (var dy = -radius; dy <= radius; dy++)
                {
                    for (var dx = -radius; dx <= radius; dx++)
                    {
                        if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius)
                        {
                            continue;
                        }

                        var candidate = origin + new Vector2Int(dx, dy);
                        if (chunkManager.IsWorldTileWalkable(candidate))
                        {
                            return candidate;
                        }
                    }
                }
            }

            return origin;
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

        private static ChunkManager SetupWorld(Transform player)
        {
            var world = GameObject.Find("World") ?? new GameObject("World");
            var manager = world.GetComponent<ChunkManager>() ?? world.AddComponent<ChunkManager>();
            manager.Initialize(player, WorldSeed, 1);
            return manager;
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
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.12f, 0.12f, 1f);
            
            if (camera.GetComponent<UniversalAdditionalCameraData>() == null)
            {
                camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }
            
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

        private static TimeSimulationEngine SetupSimulation()
        {
            var systems = GameObject.Find("Systems") ?? new GameObject("Systems");
            var engine = systems.GetComponent<TimeSimulationEngine>() ?? systems.AddComponent<TimeSimulationEngine>();

            var lightObject = GameObject.Find("Global Light 2D") ?? new GameObject("Global Light 2D");
            var globalLight = lightObject.GetComponent<Light2D>() ?? lightObject.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            engine.GlobalLight = globalLight;

            return engine;
        }
    }
}
