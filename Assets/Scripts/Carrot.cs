using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssemblyCSharp.Assets.Scripts
{
    public class Carrot : GameActor
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
        }
    }
}