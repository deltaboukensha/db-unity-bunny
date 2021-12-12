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
        public static Game main;
        private List<GameCard> gameCards = new List<GameCard>();
        private List<GameActor> gameActors = new List<GameActor>();

        IEnumerator Coroutine()
        {
            while (true)
            {
                // infinite loop guard first line
                yield return new WaitForSeconds(1.2f);

                ResolveCards();
            }
        }

        void Start()
        {
            main = this;
            StartCoroutine(Coroutine());
        }

        void Update()
        {
        }

        public void QueueCard(GameCard gameCard)
        {
            Debug.Log("queue: " + gameCard);

            gameCards.Add(gameCard);
        }

        private void ResolveCards()
        {
            var gameBoard = new Dictionary<Vector3Int, GameActor>();

            foreach (var gameActor in gameActors)
            {
                // todo figure out how to handle overlapping positions
                if (gameBoard.ContainsKey(gameActor.GetGridXYZ())) continue;

                gameBoard.Add(gameActor.GetGridXYZ(), gameActor);
            }

            while (gameCards.Count > 0)
            {
                // infinite loop guard first line
                var card = gameCards.First();
                gameCards.RemoveAt(0);

                if (card is SpawnCard)
                {
                    var c = card as SpawnCard;
                    gameActors.Add(c.actor);
                }
                else if (card is MoveCard)
                {
                    var c = card as MoveCard;

                    if (gameBoard.ContainsKey(c.newPos))
                    {
                        var f = new FightCard()
                        {
                            attacker = c.actor,
                            defender = gameBoard[c.newPos],
                            chain = c,
                        };
                        this.QueueCard(f);
                    }
                    else
                    {
                        c.actor.PlayCard(c);
                    }
                }
                else if (card is FightCard)
                {
                    var c = card as FightCard;
                    c.attacker.PlayCard(c);
                    c.defender.PlayCard(c);
                }
                else if (card is PathStartCard)
                {
                    card.actor.PlayCard(card as PathStartCard);
                }
                else if (card is PathFinishCard)
                {
                    card.actor.PlayCard(card as PathFinishCard);
                }
                else if(card is SeekCard)
                {
                    var c = card as SeekCard;
                    c.gameBoard = gameBoard;
                    c.actor.PlayCard(c);
                }
                else
                {
                    throw new System.Exception("unsupported card");
                }
            }
        }

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
                if (visited.Count() > 100) break;

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

        public GameCard PathToCard(List<Vector3Int> path, GameActor actor)
        {
            var start = new PathStartCard()
            {
                actor = actor,
            };

            var finish = new PathFinishCard()
            {
                actor = actor,
            };

            var moves = path.Select(p => new MoveCard()
            {
                oldPos = p,
                actor = actor,
            }).ToArray();

            for (var i = 0; i < moves.Count() - 1; i++)
            {
                moves[i].newPos = moves[i + 1].oldPos;
            }

            var list = new List<GameCard>();
            list.Add(start);
            list.AddRange(moves.Take(moves.Length - 1));
            list.Add(finish);

            return MakeCardChain(list);
        }

        public GameCard MakeCardChain(List<GameCard> list)
        {
            for (var i = 0; i < list.Count() - 1; i++)
            {
                list[i].chain = list[i + 1];
            }

            return list.First();
        }
    }
}