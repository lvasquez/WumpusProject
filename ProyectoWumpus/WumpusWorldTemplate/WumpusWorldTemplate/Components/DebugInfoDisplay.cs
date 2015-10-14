using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Griddy2D;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WumpusWorldTemplate
{
    public class DebugInfoDisplay : IDraws, IUpdates
    {
            private const string BREEZY = "breezy";

            private const string SMELLY = "smelly";

            private const string GlITTER = "glitter";

            private Grid _grid;

            private bool _visible;

            private Dictionary<string, Label> _debugLabels;

            public bool Visible
            {
                get { return _visible; }
                set { _visible = value; }
            }

            public DebugInfoDisplay(Grid grid, SpriteFont gameFont)
            {
                _grid = grid;

                _debugLabels = new Dictionary<string, Label>();
                _debugLabels.Add("currentTile", new Label("Current Tile", string.Empty, new Vector2(0, 0), gameFont));
                _debugLabels.Add(BREEZY, new Label(BREEZY, string.Empty, new Vector2(0, 20), gameFont));
                _debugLabels.Add(SMELLY, new Label(SMELLY, string.Empty, new Vector2(0, 40), gameFont));
                _debugLabels.Add(GlITTER, new Label(GlITTER, string.Empty, new Vector2(0, 60), gameFont));
            }


            public void Update(GameTime gameTime)
            {
                if (_grid.Intersects(Main.Input.MousePosition))
                    UpdateDebugInfo(Main.Input.MousePosition);
            }


            private void UpdateDebugInfo(Vector2 mousePosition)
            {
                _debugLabels["currentTile"].Text = _grid["world"].GetTile(mousePosition).Name;
                _debugLabels[BREEZY].Text = _grid["world"].GetTile(mousePosition).Properties.GetString(BREEZY);
                _debugLabels["glitter"].Text = _grid["world"].GetTile(mousePosition).Properties.GetString(GlITTER);
                _debugLabels[SMELLY].Text = _grid["world"].GetTile(mousePosition).Properties.GetString(SMELLY);
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                if(_visible)
                    foreach (var label in _debugLabels.Values)
                        label.Draw(spriteBatch);
            }
    }
}
