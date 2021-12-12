using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace AssemblyCSharp.Assets.Scripts
{
    public abstract class GameCard
    {
        public GameCard chain = null;
        public GameActor actor = null;
    }

    public class SpawnCard : GameCard
    {
        public Vector3Int pos;

        public override string ToString()
        {
            return "spawn: " + actor.tag + " pos: " + pos + " card: " + this.GetHashCode();
        }
    }

    public class MoveCard : GameCard
    {
        public Vector3Int newPos;
        public Vector3Int oldPos;

        public override string ToString()
        {
            return "move: " + actor.tag + " oldPos: " + oldPos + " newPos: " + newPos + " card: " + this.GetHashCode() + " chain: " + this.chain?.GetHashCode();
        }
    }

    public class PathStartCard : GameCard
    {

        public override string ToString()
        {
            return "path start: " + actor.tag + " card: " + this.GetHashCode();
        }
    }
    public class PathFinishCard : GameCard
    {
        public override string ToString()
        {
            return "path finish: " + actor.tag + " card: " + this.GetHashCode();
        }
    }

    public class FightCard : GameCard
    {
        public GameActor attacker;
        public GameActor defender;

        public override string ToString()
        {
            return "fight: " + attacker.tag + " vs " + defender.tag + " card: " + this.GetHashCode();
        }
    }

    public class SeekCard : GameCard
    {
        public Dictionary<Vector3Int, GameActor> gameBoard;

        public override string ToString()
        {
            return "seek by:" + actor.tag + " card: " + this.GetHashCode();
        }
    }

    public class DamageCard : GameCard
    {
        public GameActor attacker;
        public GameActor defender;
        public int damage;

        public override string ToString()
        {
            return "damage: " + damage + " from: " + attacker.tag + " to: " + defender.tag + " card: " + this.GetHashCode();
        }
    }

    public class DeathCard : GameCard
    {
    }
}
