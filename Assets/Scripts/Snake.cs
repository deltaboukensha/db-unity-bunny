using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace AssemblyCSharp.Assets.Scripts
{
    public class Snake : GameActor
    {
        // Start is called before the first frame update
        new void Start()
        {
            base.Start();

            this.speed = 1.0f;
            StartCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            while (true)
            {
                // infinite loop guard first line
                yield return new WaitForSeconds(1.0f);

                this.gridXYZ = this.GetGridXYZ();

                var bunny = GameObject.FindWithTag("bunny");
                if (bunny == null)
                {
                    this.fightInProgress = false;
                    GetComponent<Animator>().SetBool("fightInProgress", fightInProgress);
                    continue;
                };

                var bunnyActor = bunny.GetComponent<GameActor>();

                var gridDistance = Vector3Int.Distance(bunnyActor.gridXYZ, this.gridXYZ);
                if (gridDistance > 5.0f)
                {
                    this.IsFighting(false);
                    continue;
                }

                var path = game.FindPath(this.gridXYZ, bunnyActor.gridXYZ);
                Debug.Log(path);
                if (path == null)
                {
                    this.IsFighting(false);

                    path = game.FindPath(this.gridXYZ, new Vector3Int(
                        gridXYZ.x + UnityEngine.Random.Range(-5, +5),
                        gridXYZ.y + UnityEngine.Random.Range(-5, +5),
                        gridXYZ.z
                    ));

                    Debug.Log(string.Join(",", path));
                    continue;
                }
                else
                {
                    path.RemoveAt(path.Count() - 1);
                }

                DrawGizmoPath(path);
                this.targetPath = path;

                if (gridDistance < 2.0f)
                {
                    var attacker = this;
                    var defender = bunnyActor;

                    var attackerDelta = defender.transform.position - attacker.transform.position;
                    attacker.GetComponent<Animator>().SetFloat("deltaX", attackerDelta.x);
                    attacker.GetComponent<Animator>().SetFloat("deltaY", attackerDelta.y);
                    attacker.GetComponent<SpriteRenderer>().flipX = attackerDelta.x < 0;
                    attacker.IsFighting(true);
                }
            }
        }
    }
}