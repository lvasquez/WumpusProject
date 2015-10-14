using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Griddy2D;
using System.IO;

namespace WumpusWorldTemplate
{
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont gameFont;
        InputManager input;

        public static InputManager Input;

        private Grid _grid;

        private DebugInfoDisplay _debugInfo;

        private Agent _agent;

        private Agent _agent2;

        public static SpriteFont GameFont;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);

          
            input = new InputManager(this);
            Input = input;

            Content.RootDirectory = "Content";

            this.graphics.PreferredBackBufferHeight = 500;
            this.graphics.PreferredBackBufferWidth = 500;

            this.IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            gameFont = Content.Load<SpriteFont>("gameFont");
            GameFont = gameFont;

            SetUpGriddy();

            SetUpWumpusWorld();
        }

        void SetUpGriddy()
        {
            Stream gridDataStream = new FileStream("Content/Maps/wumpusWorld.tmx", FileMode.Open, FileAccess.Read);
            Stream tileBankStream = new FileStream("Content/Maps/tileBank.xml", FileMode.Open, FileAccess.Read);

            GridData gridData = GridData.NewFromStreamAndWorldPosition(gridDataStream, new Vector2(0, 0));
            TileBank tileBank = TileBank.CreateFromSerializedData(tileBankStream, Content);

            gridDataStream.Position = 0;
            SerializedGridFactory gridFactory = SerializedGridFactory.NewFromData(gridDataStream, gridData, tileBank);
            

            _grid = Grid.NewGrid(gridData, gridFactory, DefaultGridDrawer.NewFromGridData(gridData,Content));
            _grid.RenderDrawer = true;
            //_grid.Visible = true;

            
            
        }

        void SetUpWumpusWorld()
        {
        
            var wumpusTiles = _grid.GetLayer("world").GetAllMatchingTiles(o => o.Name == "wumpus");
            wumpusTiles.ForEach(p => p.GetCardinalNeighbors().ForEach(n => n.Properties.RegisterProperty("smelly", "true")));
          
            var pitTiles = _grid.GetLayer("world").GetAllMatchingTiles(o => o.Name == "pit");
            pitTiles.ForEach(p => p.GetCardinalNeighbors().ForEach(n => n.Properties.RegisterProperty("breezy", "true")));

            //var archer2 = _grid.GetLayer("world").GetAllMatchingTiles(o => o.Name == "archer2");
            //archer2.ForEach(p => p.GetCardinalNeighbors().ForEach(n => n.Properties.RegisterProperty("smelly", "true")));

            _debugInfo = new DebugInfoDisplay(_grid, gameFont);
            _debugInfo.Visible = true;

            var startTile = _grid["world"].GetTile(o => o.Name == "archer");
            if (startTile == null)
                throw new InvalidDataException("The Map did not include an Agent start tile!!!");

            _agent = Agent.NewAgent(_grid, Content.Load<Texture2D>("archer"));
            _agent2 = Agent.NewAgent2(_grid, Content.Load<Texture2D>("archer2"));

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            _debugInfo.Update(gameTime);
            _agent.Update(gameTime);
            _agent2.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            RenderDrawables(spriteBatch);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void RenderDrawables(SpriteBatch spriteBatch)
        {
            _grid.Draw(spriteBatch);
            _agent.Draw(spriteBatch);
            _agent2.Draw(spriteBatch);
            //_debugInfo.Draw(spriteBatch);
        }
    }
}
