using System.Collections.Generic;
using NUnit.Framework;
using ProjectEmber.World;
using UnityEngine;

namespace ProjectEmber.Tests.EditMode
{
    public sealed class GridPathfinderTests
    {
        [Test]
        public void FindsStraightPathOnOpenGrid()
        {
            var pathfinder = new GridPathfinder(_ => true);

            var path = pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(3, 0));

            Assert.AreEqual(new Vector2Int(0, 0), path[0]);
            Assert.AreEqual(new Vector2Int(3, 0), path[^1]);
            Assert.AreEqual(4, path.Count);
        }

        [Test]
        public void RoutesAroundBlockedTiles()
        {
            var blocked = new HashSet<Vector2Int>
            {
                new(1, 0),
                new(1, 1),
                new(1, -1)
            };
            var pathfinder = new GridPathfinder(tile => !blocked.Contains(tile));

            var path = pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(2, 0));

            Assert.Greater(path.Count, 0);
            Assert.AreEqual(new Vector2Int(2, 0), path[^1]);
            foreach (var step in path)
            {
                Assert.IsFalse(blocked.Contains(step));
            }
        }

        [Test]
        public void ReturnsEmptyWhenGoalUnreachable()
        {
            // Goal is walled off entirely.
            var walls = new HashSet<Vector2Int>
            {
                new(1, 0),
                new(3, 0),
                new(2, 1),
                new(2, -1)
            };
            var pathfinder = new GridPathfinder(tile => !walls.Contains(tile));

            var path = pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(2, 0));

            Assert.AreEqual(0, path.Count);
        }

        [Test]
        public void ReturnsSingleTileWhenStartEqualsGoal()
        {
            var pathfinder = new GridPathfinder(_ => true);

            var path = pathfinder.FindPath(new Vector2Int(5, 5), new Vector2Int(5, 5));

            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(new Vector2Int(5, 5), path[0]);
        }
    }
}
