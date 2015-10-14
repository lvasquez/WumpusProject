using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WumpusWorldTemplate
{
    public abstract class State<T>
    {

            public event Action<T, State<T>> OnRequestNextState;

            public abstract void OnInitalize(T owner);

            public abstract void OnUpdate(T owner, GameTime gameTime);

            public abstract void OnEnd( T owner);

            protected void InvokeStateChange(T owner, State<T> newState)
            {
                OnRequestNextState(owner, newState);
            }

    }
}
