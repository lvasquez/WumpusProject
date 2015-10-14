using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Griddy2D;

namespace WumpusWorldTemplate
{
    public static class HelperMethods
    {
        public static Random Rand = new Random();

        public const string PIT = "pit";
        public const string BREEZY = "breezy";
        public const string WUMPUS = "wumpus";
        public const string SMELLY = "smelly";
        public const string GOLD = "gold";

        public static bool IsPit(this Tile tile)
        {
            return tile.Name == PIT;
        }

        public static bool IsBreezy(this Tile tile)
        {
            return tile.Properties.GetBool(BREEZY) == true;
        }

        public static bool IsWumpus(this Tile tile)
        {
            return tile.Name == WUMPUS;
        }

        public static bool IsSmelly(this Tile tile)
        {
            return tile.Properties.GetBool(SMELLY) == true;
        }

        public static bool IsGlitter(this Tile tile)
        {
            return tile.Name == GOLD;
        }

        public static bool Visited(this Tile tile)
        {
            return tile.Properties.GetBool("visited") == true;
        }

        public static int Cost(this Tile tile)
        {
            return tile.Properties.GetInt("cost");
        }

        public static void SetCost(this Tile tile, int cost)
        {
            tile.Properties.RegisterProperty("cost", cost.ToString());
        }

        public static void SetVisited(this Tile tile, bool visited)
        {
            tile.Properties.RegisterProperty("visited", visited.ToString());
        }

        public static bool IsSafe(this Tile tile)
        {
            return tile.Cost() == 1 || tile.Cost() == 2;
        }
    }
}
