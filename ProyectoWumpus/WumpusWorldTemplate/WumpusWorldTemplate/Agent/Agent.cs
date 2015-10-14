using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Griddy2D;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WumpusWorldTemplate
{
    public class Agent : IStateMachine<Agent>, IDraws
    {

        //Propiedades para la Clase

            private Grid _grid;

            private Sprite _sprite;

            private MapEvaluator _mapEvaluator;

            private Tile _nextTile;

            private Tile _startTile;

            private PathMemory _pathMemory;

            private int _arrowCount = 1;

            public bool IsDead
            {
                get
                {
                    return _mapEvaluator.GetTilesCost(CurrentTile) == 3;
                }
            }

            public bool FoundGold
            {
                get
                {
                    return CurrentTile.IsGlitter();
                }
            }
            
            public bool AtStartTile
            {
                get
                {
                    return CurrentTile == _startTile;
                }
            }

            public Path PathBackToStart
            {
                get { return _pathMemory.ReturnPathToStart(_grid["world"].GetTile(_sprite.Position), _startTile); }
            }

            public bool Returning
            {
                get;
                set;
            }

            private Tile CurrentTile
            {
                get { return _grid["world"].GetTile(_sprite.Position); }
            }


        // Metodos para la Clase

            public static Agent NewAgent(Grid grid, Texture2D texture)
            {
                var startTile = grid["world"].GetTile(o => o.Name == "archer");
                Vector2 startPosition = startTile.Position;

                return new Agent(grid,startPosition,texture);
            }

            public static Agent NewAgent2(Grid grid, Texture2D texture)
            { 
                var startTile = grid["world"].GetTile(o => o.Name == "archer2");
                //startTile.Properties.RegisterProperty("smelly", "true");

                //var wumpusTiles = _grid.GetLayer("world").GetAllMatchingTiles(o => o.Name == "wumpus");
                //wumpusTiles.ForEach(p => p.GetCardinalNeighbors().ForEach(n => n.Properties.RegisterProperty("smelly", "true")));

                Vector2 startPosition = startTile.Position;

                return new Agent(grid, startPosition, texture);
            }


            private Agent(Grid grid, Vector2 startPosition, Texture2D texture)
            {
                _sprite = new Sprite(startPosition, texture);
                _grid = grid;
                BindInput();

                _mapEvaluator = MapEvaluator.NewFromWorld(_grid, _sprite.Position);
                Initalize(this, new SearchState());

                _startTile = _grid["world"].GetTile(_sprite.Position);

                _pathMemory = PathMemory.NewFromInitialTile(_startTile, grid.GridData);
            }


            private void BindInput()
            {
                Main.Input.BindEvent(Keys.Right, () => { MoveToTile(NeighborDirections.East); });
                Main.Input.BindEvent(Keys.Left, () => { MoveToTile(NeighborDirections.West); });
                Main.Input.BindEvent(Keys.Up, () => { MoveToTile(NeighborDirections.North); });
                Main.Input.BindEvent(Keys.Down, () => { MoveToTile(NeighborDirections.South); });
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
            }

            private void MoveToTile(NeighborDirections direction)
            {
                Tile neighbor = _grid["world"].GetTile(_sprite.Position).GetNeighborTile(direction);
                MoveToTile(neighbor);                        
            }

            public void MoveToTile(Tile tile)
            {
                if (tile != null)
                {
                    _sprite.Position = tile.Position;

                    if(!Returning)
                        _pathMemory.RegisterTile(tile);
                }
            }

            public void ChooseNextTile()
            {
                Tile currentTile = _grid["world"].GetTile(_sprite.Position);

                _mapEvaluator.EvaluateTile(currentTile);

                var neighbors = _mapEvaluator.GetTilesNeighbors(currentTile);

                _nextTile = _grid["world"].GetTile(ChooseBestNeighbor(neighbors).Position);
            }

            private Tile ChooseBestNeighbor(List<Tile> potentialNeighbors)
            {
                var nonVisited = potentialNeighbors.Where(o => o.Visited() == false && o.Cost() == 1);
                if (nonVisited.Count() > 0)
                    return nonVisited.First();

                if (_mapEvaluator.GetAllTraversableUnvisitedTiles().Count > 0)
                    return GetTileClosestToUnvisited(potentialNeighbors);

                int lowestCost = potentialNeighbors.Min(o => o.Cost());
                var returnNeighbors = potentialNeighbors.Where(o => o.Cost() == lowestCost);
                return returnNeighbors.ElementAt(HelperMethods.Rand.Next(0, returnNeighbors.Count()));
            }

            private Tile GetTileClosestToUnvisited(List<Tile> potentialNeighbors)
            {
                var univistedTiles = _mapEvaluator.GetAllTraversableUnvisitedTiles().Where(o => o.Cost() <= 2);

                Tile neighborToReturn = null;
                float closestDistance = float.MaxValue;

                var safeNeighbors = potentialNeighbors.Where(o => o.IsSafe());

                foreach (var neighbor in safeNeighbors)
                {
                    float distance = Vector2.Distance(neighbor.Position, univistedTiles.First().Position);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        neighborToReturn = neighbor;
                    }
                }

                return neighborToReturn;
            }


            public void MoveToBestTile()
            {
                if (_nextTile != null)
                {
                    MoveToTile(_nextTile);                    
                }
            }

            public bool HaveAllSafeTilesBeenVisited()
            {
                var unvisitedTiles = _mapEvaluator.GetAllTraversableUnvisitedTiles();

                if (unvisitedTiles.Count > 0)
                {
                    var safeTiles = unvisitedTiles.Where(o => o.IsSafe());

                    return safeTiles.Count() == 0;
                }

                return false;
            }

            public void GrabGold()
            {
                CurrentTile.Visible = false;
            }

            public bool TryToKillTheWumpus()
            {
                if (_mapEvaluator.LikelyWumpusTile == null)
                    return false;

                if (_arrowCount <= 0)
                    return false;

                var test = _mapEvaluator.LikelyWumpusTile;

                foreach (var neighbor in CurrentTile.GetCardinalNeighbors())
                {
                    if (_mapEvaluator.LikelyWumpusTile.Position == neighbor.Position)
                        return KillTheWumpus(neighbor);
                }

                return false;
            }


            private bool KillTheWumpus(Tile tile)
            {
                if (tile.IsWumpus())
                {
                    if (_arrowCount > 0)
                    {
                        _arrowCount = 0;
                        tile.Visible = false;
                        _mapEvaluator.WumpusKilled(tile);
                        return true;
                    }
                }

                return false;
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                _sprite.Draw(spriteBatch);
                _mapEvaluator.Draw(spriteBatch);
            }

    }
}
