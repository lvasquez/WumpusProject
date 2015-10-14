using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Griddy2D;
using Microsoft.Xna.Framework;

namespace WumpusWorldTemplate
{
    public class IStateMachine<T> : IUpdates
    {

            private State<T> _currentState;
            private T _owner;

            public bool IsCompleted
            {
                get { return _currentState == null; }
            }

            protected void Initalize(T owner, State<T> initialState)
            {
                ValidateData(owner, initialState);

                _owner = owner;
                _currentState = initialState;

                _currentState.OnRequestNextState += HandleStateChangeRequest;
                _currentState.OnInitalize(_owner);
            }

            protected IStateMachine() { }

            private void ValidateData(T owner, State<T> initialState)
            {
                if (owner == null)
                    throw new ArgumentNullException("Cannot have a null Owner");
                if (initialState == null)
                    throw new ArgumentNullException("InitialState cannot be null");
            }

            public virtual void Update(GameTime gameTime)
            {
                if (!IsCompleted)
                    _currentState.OnUpdate(_owner, gameTime);
            }

            private void HandleStateChangeRequest(T owner, State<T> nextState)
            {
                _currentState.OnEnd(_owner);

                _currentState = nextState;

                if (!IsCompleted)
                {
                    _currentState.OnInitalize(_owner);
                    _currentState.OnRequestNextState += HandleStateChangeRequest;
                }
            }
                    
    }
}
