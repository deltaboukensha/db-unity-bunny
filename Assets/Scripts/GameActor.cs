using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace AssemblyCSharp.Assets.Scripts
{
    public abstract class GameActor : MonoBehaviour
    {
        protected List<Vector3> gizmoPath = new List<Vector3>();
        protected MoveCard moveCard;
        protected bool pathInProgress = false;
        protected bool fightInProgress = false;
        protected Game game;
        protected int health;

        protected void Start()
        {
            game = GameObject.FindWithTag("GameController").GetComponent<Game>();
        }

        protected void Update()
        {
            if (moveCard != null)
            {
                var targetPosition = game.GetWorldXYZ(moveCard.newPos);
                var delta = targetPosition - transform.position;
                var change = delta.normalized * Time.deltaTime;

                if (Math.Abs(delta.magnitude) < Math.Abs(change.magnitude))
                {
                    // solves bug where change can overshoot target position
                    transform.position = targetPosition;
                }
                else
                {
                    transform.position += change;
                }

                if (delta.magnitude < 0.01f)
                {
                    if (moveCard.chain != null)
                    {
                        game.QueueCard(moveCard.chain);
                    }

                    moveCard = null;
                }
                else
                {
                    GetComponent<Animator>().SetFloat("velocity", Math.Abs(delta.magnitude));
                    GetComponent<Animator>().SetFloat("deltaX", delta.x);
                    GetComponent<Animator>().SetFloat("deltaY", delta.y);
                    GetComponent<SpriteRenderer>().flipX = delta.x < 0;
                }
            }
        }

        void OnDrawGizmos()
        {
            // draw bottom square
            var p = this.transform.position;
            Gizmos.DrawLine(p, p + new Vector3(-0.5f, -0.25f));
            Gizmos.DrawLine(p, p + new Vector3(+0.5f, -0.25f));
            Gizmos.DrawLine(p + new Vector3(0, -0.5f), p + new Vector3(-0.5f, -0.25f));
            Gizmos.DrawLine(p + new Vector3(0, -0.5f), p + new Vector3(+0.5f, -0.25f));

            // draw current path
            for (var i = 1; i < gizmoPath.Count(); i++)
            {
                Gizmos.DrawLine(gizmoPath[i - 1], gizmoPath[i]);
            }
        }

        public void DrawGizmoPath(List<Vector3Int> path)
        {
            gizmoPath = path.Select(p => game.GetWorldXYZ(p) + new Vector3(0, -0.25f)).ToList();
        }

        public Vector3Int GetGridXYZ()
        {
            var p = game.GetGridXY(this.transform.position);
            p.z = this.GetComponent<SpriteRenderer>().sortingOrder;
            return p;
        }

        public void PlayCard(MoveCard card)
        {
            this.moveCard = card;
            this.fightInProgress = false;
            GetComponent<Animator>().SetBool("fightInProgress", fightInProgress);
        }

        public void PlayCard(PathStartCard card)
        {
            this.pathInProgress = true;

            if (card.chain != null)
            {
                game.QueueCard(card.chain);
            }
        }

        public void PlayCard(PathFinishCard card)
        {
            this.pathInProgress = false;

            if (card.chain != null)
            {
                game.QueueCard(card.chain);
            }
        }

        public void PlayCard(FightCard fightCard)
        {
            this.fightInProgress = true;
            GetComponent<Animator>().SetBool("fightInProgress", fightInProgress);

            if (this == fightCard.attacker)
            {
                var delta = fightCard.defender.transform.position - fightCard.attacker.transform.position;
                GetComponent<Animator>().SetFloat("deltaX", delta.x);
                GetComponent<Animator>().SetFloat("deltaY", delta.y);
                GetComponent<SpriteRenderer>().flipX = delta.x < 0;
            }

            if(this == fightCard.defender)
            {
                var delta = fightCard.attacker.transform.position - fightCard.defender.transform.position;
                GetComponent<Animator>().SetFloat("deltaX", delta.x);
                GetComponent<Animator>().SetFloat("deltaY", delta.y);
                GetComponent<SpriteRenderer>().flipX = delta.x < 0;
            }
        }

        public virtual void PlayCard(SeekCard card)
        {
            throw new NotImplementedException();
        }

        public virtual void PlayCard(DamageCard damageCard)
        {
            damageCard.defender.health -= damageCard.damage;
            // spawn damage number

            if (damageCard.defender.health < 0)
            {
                //spawn death card
                game.QueueCard(new DeathCard()
                {
                    actor = damageCard.defender,
                    chain = damageCard.chain,
                });
            }
        }
    }
}
