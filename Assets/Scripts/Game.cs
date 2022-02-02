using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace AssemblyCSharp.Assets.Scripts
{
    public class PathNode
    {
        public Vector3Int pos;
        public List<Vector3Int> path;
    }

    public class Game : MonoBehaviour
    {
        public Vector3Int GetGridXY(Vector3 worldPos)
        {
            var grid = GameObject.FindWithTag("grid").GetComponent<Grid>();
            return grid.WorldToCell(worldPos);
        }

        public Vector3 GetWorldXYZ(Vector3Int cellPos)
        {
            var grid = GameObject.FindWithTag("grid").GetComponent<Grid>();
            return grid.CellToWorld(cellPos);
        }

        public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
        {
            // Handle equality as being greater. Note: this will break Remove(key) or
            // IndexOfKey(key) since the comparer never returns 0 to signal key equality
            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1;
                else
                    return result;
            }
        }

        // A-star, pick first sorted by distance to target
        public List<Vector3Int> FindPath(Vector3Int sourceXYZ, Vector3Int targetXYZ)
        {
            if (sourceXYZ == targetXYZ) return null;

            var sourceXY = new Vector3Int(sourceXYZ.x, sourceXYZ.y, 0);
            var targetXY = new Vector3Int(targetXYZ.x, targetXYZ.y, 0);

            var orderInLayer = sourceXYZ.z;
            var belowTilemap = GameObject.FindGameObjectsWithTag("tilemap")
                .FirstOrDefault(i => i.GetComponent<TilemapRenderer>().sortingOrder == orderInLayer - 1);
            var sameTilemap = GameObject.FindGameObjectsWithTag("tilemap")
                .FirstOrDefault(i => i.GetComponent<TilemapRenderer>().sortingOrder == orderInLayer);

            {
                var belowPos = new Vector3Int(targetXYZ.x - 1, targetXYZ.y - 1, 0); // the grid below is offset slightly
                if ((belowTilemap == null || belowTilemap.GetComponent<Tilemap>().GetTile(belowPos) == null)
                  || (sameTilemap != null && sameTilemap.GetComponent<Tilemap>().GetTile(targetXY) != null))
                {
                    return null;
                }
            }

            var grid = GameObject.FindWithTag("grid").GetComponent<Grid>();
            var list = new SortedList<float, PathNode>(new DuplicateKeyComparer<float>());
            var visited = new HashSet<Vector3Int>();

            {
                var neighboors = new List<Vector3Int>()
                {
                    new Vector3Int(sourceXYZ.x - 1, sourceXYZ.y, sourceXYZ.z),
                    new Vector3Int(sourceXYZ.x + 1, sourceXYZ.y, sourceXYZ.z),
                    new Vector3Int(sourceXYZ.x, sourceXYZ.y - 1, sourceXYZ.z),
                    new Vector3Int(sourceXYZ.x, sourceXYZ.y + 1, sourceXYZ.z),
                };

                foreach (var neighboor in neighboors)
                {
                    list.Add(Vector3Int.Distance(neighboor, targetXYZ), new PathNode()
                    {
                        pos = neighboor,
                        path = new List<Vector3Int>() { sourceXYZ }
                    });
                }
            }

            while (list.Count > 0)
            {
                // infinite loop guard first line
                if (visited.Count() > 100) return null;

                // dequeue and mark as visited
                var node = list.First().Value;
                list.RemoveAt(0);
                visited.Add(node.pos);

                // check tilemaps
                var belowPos = new Vector3Int(node.pos.x - 1, node.pos.y - 1, 0); // the grid below is offset slightly
                var nodePosXY = new Vector3Int(node.pos.x, node.pos.y, 0);

                if ((belowTilemap == null || belowTilemap.GetComponent<Tilemap>().GetTile(belowPos) == null)
                    || (sameTilemap != null && sameTilemap.GetComponent<Tilemap>().GetTile(nodePosXY) != null))
                {
                    continue;
                }

                if (node.pos == targetXYZ)
                {
                    // found
                    var foundPath = new List<Vector3Int>();
                    foundPath.AddRange(node.path);
                    foundPath.Add(targetXYZ);
                    return foundPath;
                }

                // add all possible next positions and sort them
                var neighboors = new List<Vector3Int>()
                {
                    new Vector3Int(node.pos.x - 1, node.pos.y, node.pos.z),
                    new Vector3Int(node.pos.x + 1, node.pos.y, node.pos.z),
                    new Vector3Int(node.pos.x, node.pos.y - 1, node.pos.z),
                    new Vector3Int(node.pos.x, node.pos.y + 1, node.pos.z),
                };

                var newPath = new List<Vector3Int>();
                newPath.AddRange(node.path);
                newPath.Add(node.pos);

                foreach (var neighboor in neighboors)
                {
                    // do not add already visited
                    if (visited.Contains(neighboor)) continue;

                    list.Add(Vector3Int.Distance(neighboor, targetXYZ), new PathNode()
                    {
                        pos = neighboor,
                        path = newPath,
                    });
                }
            }

            return null;
        }
    }
}