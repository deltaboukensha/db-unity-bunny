using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System;

namespace AssemblyCSharp.Assets.Scripts
{
    public class DraggableTarget : GameActor
    {
        public GameObject parent;
        private bool dragging = false;

        // Start is called before the first frame update
        new void Start()
        {
            base.Start();
            this.GetComponent<SpriteRenderer>().enabled = false;
        }

        // Update is called once per frame
        new void Update()
        {
            if (!dragging)
            {
                this.transform.position = parent.transform.position;
            }
        }

        void OnMouseDrag()
        {
            var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            p.z = 0;
            this.transform.position = game.GetWorldXYZ(game.GetGridXY(p));
        }

        void OnMouseDown()
        {
            dragging = true;
            this.GetComponent<SpriteRenderer>().enabled = true;
        }

        void OnMouseUp()
        {
            var gridXY = game.GetGridXY(this.transform.position);
            parent.GetComponent<Bunny>().SetTargetPosition(new Vector3Int()
            {
                x = gridXY.x,
                y = gridXY.y,
                z = 1,
            });
            dragging = false;
            this.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}