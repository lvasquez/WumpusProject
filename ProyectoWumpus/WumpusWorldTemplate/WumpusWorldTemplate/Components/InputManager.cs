using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WumpusWorldTemplate
{
    public class InputManager : GameComponent
    {
        #region members
            /// <summary>
            /// Used to poll mouse
            /// </summary>
            MouseState _mouseState;
            MouseState _previousMouseState;

            /// <summary>
            /// Used to poll keyboard
            /// </summary>
            KeyboardState _keyState;
            KeyboardState _previousKeyState;

            /// <summary>
            /// The current mouse position
            /// </summary>
            Vector2 _mousePosition; 

            /// <summary>
            /// Binds an action to a key
            /// </summary>
            Dictionary<Keys, Action> _keyPressEvents = new Dictionary<Keys, Action>();

        #endregion members

        #region properties
            /// <summary>
            /// Registers an action with a key 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="action"></param>
            public void BindEvent(Keys key, Action action)
            {
                _keyPressEvents[key] += action;
            }

            /// <summary>
            /// Unbind an action from a key
            /// </summary>
            /// <param name="key"></param>
            /// <param name="action"></param>
            public void UnbindEvent(Keys key, Action action)
            {
                if (_keyPressEvents.ContainsKey(key))
                    _keyPressEvents[key] -= action;
            }

            /// <summary>
            /// Returns the current mouse position
            /// </summary>
            public Vector2 MousePosition
            {
                get { return new Vector2(_mouseState.X, _mouseState.Y); }
            }

        #endregion properties

        #region constructors
            /// <summary>
            /// Creates the input manager
            /// </summary>
            /// <param name="parentGame"></param>
            public InputManager(Game parentGame)
                : base(parentGame)
            {
                ConstructKeyPressEvents();
                parentGame.Components.Add(this);
            }

            /// <summary>
            /// Create an entry for every key, giving each a blank event
            /// </summary>
            void ConstructKeyPressEvents()
            {
                foreach (Keys key in Enum.GetValues(typeof(Keys)))
                {
                    _keyPressEvents.Add(key, () => { });
                }

            }

        #endregion construcors

        #region methods
        /// <summary>
        /// Updates the mouse / key states
        /// polls keyboard for keypresses, if occured fires the event bound to that key 
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            _mouseState = Mouse.GetState();
            _keyState = Keyboard.GetState();

            foreach (Keys key in _keyPressEvents.Keys)
            {
                if (_keyState.IsKeyDown(key) && _previousKeyState.IsKeyUp(key))
                    if (_keyPressEvents[key] != null)
                        _keyPressEvents[key]();
            }

            _previousKeyState = _keyState;
            _previousMouseState = _mouseState;

            base.Update(gameTime);
        }

        /// <summary>
        /// Manually poll a keyboard. Note you should probably bind to an event instead, that is the point of this input manager! 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsKeyPressed(Keys key)
        {
            _keyState = Keyboard.GetState();
            return _keyState.IsKeyDown(key) && _previousKeyState.IsKeyUp(key);
        }



        #endregion methods
    }
}
