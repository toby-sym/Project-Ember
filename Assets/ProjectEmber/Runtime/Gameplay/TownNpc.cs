using System.Collections.Generic;
using ProjectEmber.ProceduralAssets;
using ProjectEmber.Simulation;
using ProjectEmber.World;
using UnityEngine;

namespace ProjectEmber.Gameplay
{
    /// <summary>
    /// A townsperson that walks to a market tile during the day and returns home
    /// at night, following A* paths from <see cref="GridPathfinder"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TownNpc : MonoBehaviour
    {
        private const int MarketOpenHour = 8;
        private const int MarketCloseHour = 20;

        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float arrivalThreshold = 0.08f;
        [SerializeField] private float repathCooldown = 1.5f;

        private ChunkManager chunkManager;
        private TimeSimulationEngine time;
        private GridPathfinder pathfinder;
        private Vector2Int homeTile;
        private Vector2Int marketTile;

        private readonly List<Vector2Int> path = new();
        private int pathIndex;
        private Vector2Int currentTarget;
        private bool hasTarget;
        private float repathTimer;

        public Vector2Int HomeTile => homeTile;
        public Vector2Int MarketTile => marketTile;

        public void Initialize(ChunkManager manager, TimeSimulationEngine timeEngine, Vector2Int home, Vector2Int market, int seed)
        {
            chunkManager = manager;
            time = timeEngine;
            homeTile = home;
            marketTile = market;

            if (chunkManager != null)
            {
                pathfinder = new GridPathfinder(chunkManager.IsWorldTileWalkable);
            }

            var character = GetComponent<ProceduralCharacter>() ?? gameObject.AddComponent<ProceduralCharacter>();
            character.RandomizeAppearance(seed);

            transform.position = new Vector3(home.x, home.y, transform.position.z);
        }

        private void Update()
        {
            if (chunkManager == null || pathfinder == null)
            {
                return;
            }

            var desired = ResolveDesiredTile();
            if (!hasTarget || desired != currentTarget)
            {
                RequestPath(desired);
            }

            FollowPath();

            if (repathTimer > 0f)
            {
                repathTimer -= Time.deltaTime;
            }
        }

        private Vector2Int ResolveDesiredTile()
        {
            if (time == null)
            {
                return marketTile;
            }

            var isDaytime = time.Hour >= MarketOpenHour && time.Hour < MarketCloseHour;
            return isDaytime ? marketTile : homeTile;
        }

        private void RequestPath(Vector2Int target)
        {
            currentTarget = target;
            hasTarget = true;

            // When the previous search yielded nothing walkable, throttle retries.
            if (path.Count == 0 && repathTimer > 0f)
            {
                return;
            }

            var found = pathfinder.FindPath(CurrentTile(), target);
            path.Clear();
            path.AddRange(found);
            pathIndex = 0;
            repathTimer = repathCooldown;
        }

        private void FollowPath()
        {
            if (pathIndex >= path.Count)
            {
                return;
            }

            var next = path[pathIndex];
            var targetPosition = new Vector3(next.x, next.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) <= arrivalThreshold)
            {
                pathIndex++;
            }
        }

        private Vector2Int CurrentTile()
        {
            return new Vector2Int(
                Mathf.RoundToInt(transform.position.x),
                Mathf.RoundToInt(transform.position.y));
        }
    }
}
