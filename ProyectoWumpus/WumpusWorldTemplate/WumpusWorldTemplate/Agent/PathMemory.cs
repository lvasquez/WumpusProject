using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Griddy2D;
using Microsoft.Xna.Framework;

namespace WumpusWorldTemplate
{
    public class PathMemory
    {

            private List<Tile> _visitedTiles;

            private GridData _gridData;

            public static PathMemory NewFromInitialTile(Tile initialTile, GridData gridData)
            {
                return new PathMemory(initialTile, gridData);
            }

            public PathMemory(Tile initialTile, GridData gridData)
            {
                _visitedTiles = new List<Tile>();

                _gridData = gridData;

                RegisterTile(initialTile);
            }

            public void RegisterTile(Tile initialTile)
            {
                if (_visitedTiles.Contains(initialTile))
                    return;

                _visitedTiles.Add(initialTile);
            }

            public Path ReturnPathToStart(Tile currentTile, Tile goalTile)
            {                
                Grid grid = Grid.NewGrid(_gridData, new PathGridFactory(_gridData, new PathFactory(_gridData, _visitedTiles)), null);
                return Pathfinder.Instance.GetPath(currentTile, goalTile, grid);
            }

    }

    public class PathFactory : ICollectionFactory
    {
            private Tile[,] _tiles;

            PropertyCollection _properties;

            public string Name
            {
                get { return string.Empty; }
            }

            public Tile[,] Tiles
            {
                get { return _tiles; }
            }
 
            public PathFactory(GridData gridData, List<Tile> visitedTiles)
            {
                _properties = PropertyCollection.NewEmpty();
                _properties.RegisterProperty("cost", "9999");
                _properties.RegisterProperty("passable", "false");

                GenerateTiles(gridData);
                ReplaceBlankTilesWithVisited(gridData, visitedTiles);
            }
        
            private void GenerateTiles(GridData gridData)
            {
                _tiles = new Tile[gridData.NumberOfColumns, gridData.NumberOfRows];

                for (int rows = 0; rows < gridData.NumberOfRows; rows++)
                {
                    for (int columns = 0; columns < gridData.NumberOfColumns; columns++)
                    {
                        Vector2 tilePosition = new Vector2(columns, rows) * gridData.CellSize;
                        string name = (rows + "-" + columns).ToString();
                        _tiles[rows, columns] = Tile.NewTile(_properties, null, tilePosition, name);
                    }
                }
            }

            private void ReplaceBlankTilesWithVisited(GridData gridData, List<Tile> visitedTiles)
            {
                visitedTiles.ForEach(o => o.Properties.RegisterProperty("passable", "true"));

                for (int rows = 0; rows < gridData.NumberOfRows; rows++)
                {
                    for (int columns = 0; columns < gridData.NumberOfColumns; columns++)
                    {
                        Tile tile = GetMatchingTile(visitedTiles, rows, columns);

                        if (tile != null)
                        {
                            tile.Properties.RegisterProperty("cost", "1");
                            _tiles[rows, columns] = tile;
                        }
                    }
                }
            }

            private Tile GetMatchingTile(List<Tile> visitedTiles, int rows, int columns)
            {
                foreach (var tile in visitedTiles)
                {
                    int tileRow = int.Parse(tile.Properties.GetString("coordinate").Split('-').First());
                    int tileColumn = int.Parse(tile.Properties.GetString("coordinate").Split('-').Last());

                    if (tileRow == rows && tileColumn == columns)
                        return tile;
                }

                return null;
            }

    }

    public class PathGridFactory : IGridFactory
    {

            private Dictionary<string, TileCollection> _tileCollections;

            public Dictionary<string, TileCollection> TileCollections
            {
                get { return _tileCollections; }
            }


            public PathGridFactory(GridData gridData, PathFactory pathFactory)
            {
                _tileCollections = new Dictionary<string, TileCollection>();
                _tileCollections.Add("Path", TileCollection.NewFromFactory(pathFactory, gridData));
            }            
    }

}
