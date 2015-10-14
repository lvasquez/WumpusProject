using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Griddy2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WumpusWorldTemplate
{
    class MapEvaluator : IDraws
    {
            private const int SAFE_COST = 1;

            private const int POTENTIAL_DANGER_COST = 2;

            private const int DANGEROUS_COST = 3;

            private const int GOLD_COST = 0;
 
            private TileCollection _memoryBank;

            private Dictionary<string, Label> _debugLabels;

            //private Dictionary<string, Label> _debugLabelsCurrent;

            private Grid _world;

            private const int WUMPUS_CONFIDENCE_THRESHOLD = 30;

            private bool _wumpusKilled = false;

            public Tile LikelyWumpusTile
            {
                get
                {
                    var orderedTiles = _memoryBank.GetAllMatchingTiles(o => o.Properties.PropertyExists("WumpusProbability"));

                    if (orderedTiles.Count() > 0)
                    {
                        var potentialWumpusTiles = orderedTiles.OrderBy(o => o.Properties.GetInt("WumpusProbability"));

                        if (potentialWumpusTiles.Last().Properties.GetInt("WumpusProbability") >= WUMPUS_CONFIDENCE_THRESHOLD)
                            return potentialWumpusTiles.Last();
                    }

                    return null;
                }
            }


            public static MapEvaluator NewFromWorld(Grid grid, Vector2 startPosition)
            {
                return new MapEvaluator(grid,startPosition);
            }


            private MapEvaluator(Grid grid, Vector2 startPosition)
            {
                _memoryBank = TileCollection.NewFromFactory(new MapEvaluatorFactory(grid.GridData), grid.GridData);
                _world = grid;
                //CreateDebugLabels();
                EvaluateTile(grid["world"].GetTile(startPosition));
            }

            
            private void CreateDebugLabels()
            {
                _debugLabels = new Dictionary<string,Label>();
                
               var allTiles = _memoryBank.GetAllMatchingTiles(o => true);
               allTiles.ForEach(o =>
                   {
                       Label newLabel = new Label("Cost", o.Properties.GetInt("cost").ToString(), o.Position, Main.GameFont);
                       _debugLabels.Add(o.Name, newLabel);

                   });                  
            }
               

            public void EvaluateTile(Tile tile)
            {
                Tile tileInMemory = _memoryBank.GetTile(tile.Position);

                if (!tileInMemory.Visited())
                {
                    AssignCostToCurrentTile(tile);
                    EstimateNeighbors(tile);
                    ResolveFalseRatings();
                }
            }


            private void EstimateNeighbors(Tile tile)
            {
                Tile tileInMemory = _memoryBank.GetTile(tile.Position);
                var neighbors = tile.GetCardinalNeighbors();

                foreach (var neighbor in neighbors)
                {
                    
                    if (tile.IsBreezy() || tile.IsSmelly())
                    {
                        Tile neighborTile = _memoryBank.GetTile(neighbor.Position);

                   
                        if (!neighborTile.Visited())
                        {
                            neighborTile.SetCost(DANGEROUS_COST);
                            UpdateDebugText(neighborTile.Name, neighborTile.Cost().ToString());
                        }
                    }                    
                }                
            }


            private void ResolveFalseRatings()
            {
                var allTiles = _memoryBank.GetAllMatchingTiles(o => true);

                foreach (var unknownTile in allTiles)
                {
                    if (!unknownTile.Visited())
                    {
                        
                        var visitedNeighbors = unknownTile.GetCardinalNeighbors().Where(o => o.Visited());
                        
                        if (visitedNeighbors.Count() > 0)                        
                            EstimateUnknownTilesDangers(unknownTile, visitedNeighbors);        
                    }
                }
            }


            private void EstimateUnknownTilesDangers(Tile unknownTile, IEnumerable<Tile> visitedNeighbors)
            {   
                var nonDangerousTiles = visitedNeighbors.Where(o => o.Cost() == SAFE_COST || o.Cost() == GOLD_COST);
            
                if (nonDangerousTiles.Count() > 0)
                {
                    unknownTile.SetCost(SAFE_COST);
                    UpdateDebugText(unknownTile.Name, unknownTile.Cost().ToString());
                }
                else
                {
                    unknownTile.SetCost(DANGEROUS_COST);
                    UpdateDebugText(unknownTile.Name, unknownTile.Cost().ToString());
                }

                CheckForInconsistentDangers(unknownTile, visitedNeighbors);
                MarkTheWumpus();
            }


            private void MarkTheWumpus()
            {
                var allTiles = _memoryBank.GetAllMatchingTiles(o => true);
                var dangerousTiles = allTiles.Where(o => !o.IsSafe());

                foreach (var tile in dangerousTiles)
                {
                    var visitedNeighbors = tile.GetCardinalNeighbors().Where(o => o.Visited() == true);
                    var smellyNeighbors = visitedNeighbors.Where(o => _world["world"].GetTile(o.Position).IsSmelly());

                    if (smellyNeighbors.Count() == visitedNeighbors.Count())
                    {
                        tile.Properties.RegisterProperty("WumpusProbability",(10 * smellyNeighbors.Count()).ToString());
                        UpdateDebugText(tile.Name, tile.Cost().ToString());
                    }
                }
            }


            private void CheckForInconsistentDangers(Tile unknownTile, IEnumerable<Tile> visitedNeighbors)
            {
                var dangerousNeighbors = visitedNeighbors.Where(o => o.Cost() == POTENTIAL_DANGER_COST);
                
                var smellyNeighbors = dangerousNeighbors.Where(o => _world["world"].GetTile(o.Position).IsSmelly());
                var breezyNeighbors = dangerousNeighbors.Where(o => _world["world"].GetTile(o.Position).IsBreezy());

                if (dangerousNeighbors.Count() >= 2 && smellyNeighbors.Count() > 0 && breezyNeighbors.Count() > 0)
                {
                    if ((smellyNeighbors.Count() != dangerousNeighbors.Count() && breezyNeighbors.Count() == dangerousNeighbors.Count()))
                        return;
                    if ((breezyNeighbors.Count() != dangerousNeighbors.Count() && smellyNeighbors.Count() == dangerousNeighbors.Count()))
                        return;

                    unknownTile.SetCost(SAFE_COST);
                    UpdateDebugText(unknownTile.Name, unknownTile.Cost().ToString());

                }
            }


            private void AssignCostToCurrentTile(Tile tile)
            {
                Tile tileInMemory = _memoryBank.GetTile(tile.Position);

                if (tile.IsBreezy() || (tile.IsSmelly() && !_wumpusKilled))
                    tileInMemory.SetCost(POTENTIAL_DANGER_COST);
                else if (tile.IsPit() || (tile.IsWumpus() && !_wumpusKilled))
                    tileInMemory.SetCost(DANGEROUS_COST);
                else if (tile.IsGlitter())
                    tileInMemory.SetCost(GOLD_COST);
                else
                    tileInMemory.SetCost(SAFE_COST);

                tileInMemory.SetVisited(true);
                tile.SetVisited(true);

                UpdateDebugText(tileInMemory.Name, tileInMemory.Cost().ToString());
                //_debugLabels[tileInMemory.Name].Color = Color.Blue;
            }
            

            private void UpdateDebugText(string tileName, string value)
            {
                //_debugLabels[tileName].Text = value;
              
            }


            public void Draw(SpriteBatch spriteBatch)
            {
                /*
                foreach (var item in _debugLabels.Values)
                    item.Draw(spriteBatch);
                */
            }


            public int GetTilesCost(Tile tile)
            {
                Tile tileInMemory = _memoryBank.GetTile(tile.Position);
                EvaluateTile(tile);
                return tileInMemory.Cost();
            }


            public int GetTilesNeighborCost(Tile tile, NeighborDirections neighborDirection)
            {
                Tile tileInMemory = _memoryBank.GetTile(tile.Position);
                return tileInMemory.GetNeighborTile(neighborDirection).Cost();
            }


            public List<Tile> GetTilesNeighbors(Tile tile)
            {
                Tile tileInMemory = _memoryBank.GetTile(tile.Position);
                return tileInMemory.GetCardinalNeighbors();
            }


            public List<Tile> GetAllTraversableUnvisitedTiles()
            {
                var allTiles = _memoryBank.GetAllMatchingTiles(o => true);

                List<Tile> unvisitedTiles = new List<Tile>();

                foreach (var unknownTile in allTiles)
                {
                    if (!unknownTile.Visited())
                    {
                        var visitedNeighbors = unknownTile.GetCardinalNeighbors().Where(o => o.Visited());

                        if (visitedNeighbors.Count() > 0)
                            unvisitedTiles.Add(unknownTile);
                    }
                }

                return unvisitedTiles;
            }


            public void WumpusKilled(Tile tile)
            {
                Tile tileInMemory = _memoryBank.GetTile(tile.Position);

                if (!tile.IsBreezy())
                {
                    _wumpusKilled = true;
                    tileInMemory.SetCost(SAFE_COST);
                    var wumpusNeighbors = tileInMemory.GetCardinalNeighbors();

                    foreach (var neighbor in wumpusNeighbors)
                    {
                        if (_world["world"].GetTile(neighbor.Position).IsBreezy())
                            continue;

                        neighbor.SetCost(SAFE_COST);
                        UpdateDebugText(neighbor.Name, neighbor.Cost().ToString());
                    }

                    UpdateDebugText(tileInMemory.Name, tileInMemory.Cost().ToString());
                }
            }
    }

    class MapEvaluatorFactory : ICollectionFactory
    {
 
            Tile[,] _tiles;

            PropertyCollection _properties;

            public string Name
            {
                get { return "Memory Bank"; }
            }

            public Tile[,] Tiles
            {
                get { return _tiles; }
            }

            public MapEvaluatorFactory(GridData gridData)
            {
                _properties = PropertyCollection.NewEmpty();
                _properties.RegisterProperty("cost", "1");
                _properties.RegisterProperty("visited", "false");
                                
                GenerateTiles(gridData);
            }

            private void GenerateTiles(GridData gridData)
            {
                _tiles = new Tile[gridData.NumberOfColumns, gridData.NumberOfRows];

                for (int rows = 0; rows < gridData.NumberOfRows; rows++)
                {
                    for (int columns = 0; columns < gridData.NumberOfColumns; columns++)
                    {
                        Vector2 tilePosition = new Vector2(columns,rows) * gridData.CellSize;
                        string name = (rows + "-" + columns).ToString();
                        _tiles[rows, columns] = Tile.NewTile(_properties, null, tilePosition,name);
                    }
                }
            }

    }
}
