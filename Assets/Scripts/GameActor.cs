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
        protected bool pathInProgress = false;
        protected bool fightInProgress = false;
        protected Game game;
        protected int health;
        protected float speed = 1.0f;
        public Vector3Int gridXYZ;
        protected Vector3Int? targetGridXYZ;
        protected List<Vector3Int> targetPath;

        protected void Start()
        {
            game = GameObject.FindWithTag("GameController").GetComponent<Game>();
        }

        protected void Update()
        {
            this.gridXYZ = this.GetGridXYZ();

            if (this.targetPath == null) return;

            if (!this.targetPath.Any())
            {
                this.targetPath = null;
                return;
            }

            this.targetGridXYZ = this.targetPath.First();

            if (this.targetGridXYZ == null) return;

            var gridDistance = Vector3Int.Distance(this.targetGridXYZ.Value, this.gridXYZ);
            var targetWorldXYZ = game.GetWorldXYZ(this.targetGridXYZ.Value);
            var delta = targetWorldXYZ - transform.position;
            var change = delta.normalized * Time.deltaTime * this.speed;

            if (Math.Abs(delta.magnitude) < Math.Abs(change.magnitude))
            {
                // solves bug where change can overshoot target position
                transform.position = targetWorldXYZ;
            }
            else
            {
                transform.position += change;
            }

            if (gridDistance < 0.1f)
            {
                this.targetPath.RemoveAt(0);
            }
            else
            {
                GetComponent<Animator>().SetFloat("deltaX", delta.x);
                GetComponent<Animator>().SetFloat("deltaY", delta.y);
                GetComponent<SpriteRenderer>().flipX = delta.x < 0;
            }

            GetComponent<Animator>().SetFloat("velocity", Math.Abs(delta.magnitude));
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

        public void IsFighting(bool isFighting)
        {
            GetComponent<Animator>().SetBool("fightInProgress", isFighting);
        }

        public void IsDying(bool isDying)
        {
            Destroy(this.gameObject);
        }
    }
}
