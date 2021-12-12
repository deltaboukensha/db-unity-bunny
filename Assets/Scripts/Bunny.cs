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

        // Start is called before the first frame update
        new void Start()
        {
            base.Start();

            draggableTarget = Instantiate(draggableTarget, draggableTarget.transform.position, Quaternion.identity);
            draggableTarget.GetComponent<DraggableTarget>().parent = this.gameObject;

            game.QueueCard(new SpawnCard()
            {
                actor = this,
                pos = this.GetGridXYZ(),
            });
        }

        public void SetTargetPosition(Vector3Int cellPosition)
        {
            var grid = GameObject.FindWithTag("grid").GetComponent<Grid>();
            //todo figure out bug where world position is not always the same cell position
            var currentCellPosition = grid.WorldToCell(new Vector3(this.transform.position.x, this.transform.position.y + 0.25f, this.transform.position.z));
            cellPosition.z = currentCellPosition.z = GetComponent<SpriteRenderer>().sortingOrder;
            var path = game.FindPath(currentCellPosition, cellPosition);

            if (path == null) return;

            DrawGizmoPath(path);
            game.QueueCard(game.PathToCard(path, this));
        }
    }
}