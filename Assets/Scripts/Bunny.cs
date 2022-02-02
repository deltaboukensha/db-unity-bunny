using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;

namespace AssemblyCSharp.Assets.Scripts
{
    public class Bunny : GameActor
    {
        public GameObject draggableTarget;
        private int food = 0;

        // Start is called before the first frame update
        new void Start()
        {
            base.Start();

            this.speed = 2.0f;

            draggableTarget = Instantiate(draggableTarget, draggableTarget.transform.position, Quaternion.identity);
            draggableTarget.GetComponent<DraggableTarget>().parent = this.gameObject;

            StartCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            while (true)
            {
                // infinite loop guard first line
                yield return new WaitForSeconds(0.100f);

                this.gridXYZ = this.GetGridXYZ();

                var carrots = GameObject.FindGameObjectsWithTag("carrot");
                if (carrots == null || !carrots.Any())
                {
                    continue;
                };

                foreach (var carrot in carrots)
                {
                    var carrotActor = carrot.GetComponent<GameActor>();
                    var distance = Vector3Int.Distance(carrotActor.gridXYZ, this.gridXYZ);
                    if (distance > 0.1) continue;

                    food += 1;
                    carrotActor.IsDying(true);
                }

                var snakes = GameObject.FindGameObjectsWithTag("snake");
                if (snakes == null || !snakes.Any())
                {
                    continue;
                };

                this.IsFighting(false);

                foreach (var snake in snakes)
                {
                    var snakeActor = snake.GetComponent<GameActor>();
                    var distance = Vector3Int.Distance(snakeActor.gridXYZ, this.gridXYZ);
                    if (distance > 2.0) continue;

                    var attacker = snakeActor;
                    var defender = this;

                    var defenderDelta = attacker.transform.position - defender.transform.position;
                    defender.GetComponent<Animator>().SetFloat("deltaX", defenderDelta.x);
                    defender.GetComponent<Animator>().SetFloat("deltaY", defenderDelta.y);
                    defender.GetComponent<SpriteRenderer>().flipX = defenderDelta.x < 0;
                    defender.IsFighting(true);
                }
            }
        }

        public void SetTargetPosition(Vector3Int pos)
        {
            var path = game.FindPath(this.gridXYZ, pos);
            if (path == null) return;

            this.targetPath = path;
            DrawGizmoPath(path);
        }
    }
}