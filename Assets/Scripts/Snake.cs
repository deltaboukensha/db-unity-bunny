using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace AssemblyCSharp.Assets.Scripts
{
    public class Snake : GameActor
    {
        // Start is called before the first frame update
        new void Start()
        {
            base.Start();

            game.QueueCard(new SpawnCard()
            {
                actor = this,
                pos = this.GetGridXYZ(),
            });

            StartCoroutine(Coroutine());
        }

        public override void PlayCard(SeekCard card)
        {
            var board = card.gameBoard;
            var p = this.GetGridXYZ();

            for (var y = p.y - 5; y < p.y + 5; y++)
            {
                for (var x = p.x - 5; x < p.x + 5; x++)
                {
                    var b = new Vector3Int(x, y, p.z);

                    if(!board.ContainsKey(b)) continue;

                    var occupant = board[b];

                    if(occupant is Bunny)
                    {
                        var path = game.FindPath(this.GetGridXYZ(), occupant.GetGridXYZ());

                        if (path == null) continue;

                        DrawGizmoPath(path);
                        game.QueueCard(game.PathToCard(path, this));
                        return;
                    }
                }
            }

            if (pathInProgress) return;

            // else random move
            {
                var oldPos = this.GetGridXYZ();
                var newPos = new Vector3Int()
                {
                    x = oldPos.x + UnityEngine.Random.Range(-5, +5),
                    y = oldPos.y + UnityEngine.Random.Range(-5, +5),
                    z = oldPos.z,
                };
                var path = game.FindPath(oldPos, newPos);

                if (path == null) return;

                DrawGizmoPath(path);
                game.QueueCard(game.PathToCard(path, this));
            }
        }

        IEnumerator Coroutine()
        {
            while (true)
            {
                // infinite loop guard first line
                yield return new WaitForSeconds(1.0f);

                game.QueueCard(new SeekCard()
                {
                    actor = this,
                });

                continue;

                if (pathInProgress) continue;

                var p = this.transform.position;
                var oldPos = game.GetGridXY(new Vector3()
                {
                    x = p.x,
                    y = p.y + 0.25f,
                    z = p.z,
                });
                oldPos.z = GetComponent<SpriteRenderer>().sortingOrder;
                var newPos = new Vector3Int()
                {
                    x = Mathf.RoundToInt(oldPos.x + UnityEngine.Random.Range(-5, +5)),
                    y = Mathf.RoundToInt(oldPos.y + UnityEngine.Random.Range(-5, +5)),
                    z = oldPos.z,
                };

                if (newPos == oldPos) continue;

                var path = game.FindPath(oldPos, newPos);

                if (path == null) continue;

                DrawGizmoPath(path);
                game.QueueCard(game.PathToCard(path, this));
            }
        }
    }
}