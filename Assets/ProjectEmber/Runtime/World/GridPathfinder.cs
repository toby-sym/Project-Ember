using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectEmber.World
{
    /// <summary>
    /// Allocation-conscious A* over integer world tile coordinates. Internal
    /// buffers are reused between calls so repeated queries (e.g. from NPCs)
    /// avoid per-search garbage. Walkability is resolved through a delegate,
    /// typically <see cref="ChunkManager.IsWorldTileWalkable"/>.
    /// </summary>
    public sealed class GridPathfinder
    {
        private static readonly Vector2Int[] Neighbours =
        {
            new(1, 0),
            new(-1, 0),
            new(0, 1),
            new(0, -1)
        };

        private readonly Func<Vector2Int, bool> isWalkable;
        private readonly int maxExpandedNodes;

        private readonly Dictionary<Vector2Int, int> gScore = new();
        private readonly Dictionary<Vector2Int, int> fScore = new();
        private readonly Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        private readonly HashSet<Vector2Int> closed = new();
        private readonly List<Vector2Int> openSet = new();
        private readonly List<Vector2Int> path = new();

        public GridPathfinder(Func<Vector2Int, bool> walkabilityQuery, int maxExpandedNodes = 4096)
        {
            isWalkable = walkabilityQuery ?? throw new ArgumentNullException(nameof(walkabilityQuery));
            this.maxExpandedNodes = Mathf.Max(1, maxExpandedNodes);
        }

        /// <summary>
        /// Finds a walkable path from <paramref name="start"/> to <paramref name="goal"/>.
        /// The returned list is a shared, reused buffer valid until the next call;
        /// callers that need to retain it must copy the contents. An empty list
        /// means no path was found.
        /// </summary>
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
        {
            path.Clear();

            if (start == goal)
            {
                path.Add(start);
                return path;
            }

            if (!isWalkable(goal))
            {
                return path;
            }

            gScore.Clear();
            fScore.Clear();
            cameFrom.Clear();
            closed.Clear();
            openSet.Clear();

            gScore[start] = 0;
            fScore[start] = Heuristic(start, goal);
            openSet.Add(start);

            var expanded = 0;
            while (openSet.Count > 0 && expanded < maxExpandedNodes)
            {
                var current = PopLowestF();
                if (current == goal)
                {
                    return ReconstructPath(current);
                }

                closed.Add(current);
                expanded++;

                var currentG = gScore[current];
                for (var i = 0; i < Neighbours.Length; i++)
                {
                    var neighbour = current + Neighbours[i];
                    if (closed.Contains(neighbour) || !isWalkable(neighbour))
                    {
                        continue;
                    }

                    var tentativeG = currentG + 1;
                    if (gScore.TryGetValue(neighbour, out var existingG) && tentativeG >= existingG)
                    {
                        continue;
                    }

                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentativeG;
                    fScore[neighbour] = tentativeG + Heuristic(neighbour, goal);
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }

            return path;
        }

        private Vector2Int PopLowestF()
        {
            var bestIndex = 0;
            var bestF = fScore[openSet[0]];
            for (var i = 1; i < openSet.Count; i++)
            {
                var f = fScore[openSet[i]];
                if (f < bestF)
                {
                    bestF = f;
                    bestIndex = i;
                }
            }

            var node = openSet[bestIndex];
            openSet[bestIndex] = openSet[openSet.Count - 1];
            openSet.RemoveAt(openSet.Count - 1);
            return node;
        }

        private List<Vector2Int> ReconstructPath(Vector2Int goal)
        {
            path.Clear();
            var current = goal;
            path.Add(current);
            while (cameFrom.TryGetValue(current, out var previous))
            {
                current = previous;
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        private static int Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}
