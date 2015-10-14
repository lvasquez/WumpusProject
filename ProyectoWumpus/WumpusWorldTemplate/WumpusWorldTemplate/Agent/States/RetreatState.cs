using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Griddy2D;
using Microsoft.Xna.Framework;

namespace WumpusWorldTemplate
{
    class RetreatState : State<Agent>
    {
        private Path _returnPath;

        /// <summary>
        /// Used to move the cow automatically along the path
        /// </summary>
        float _elapsedTime = 0;
        /// <summary>
        /// How fast should the cow move along the path? 
        /// </summary>
        const float STEP_INTERVAL = 400f;

        public override void OnInitalize(Agent owner)
        {
            _returnPath = owner.PathBackToStart;
            owner.Returning = true;
        }

        public override void OnUpdate(Agent owner, GameTime gameTime)
        {
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_elapsedTime > STEP_INTERVAL)
            {
                MoveOnPath(owner);
                _elapsedTime = 0;
            }     
        }

        private void MoveOnPath(Agent owner)
        {
            if (_returnPath != null)
            {
                var result = _returnPath.GetNextTile();

                if (result != null)
                    owner.MoveToTile(result);
            }
        }

        public override void OnEnd(Agent owner)
        {
            
        }
    }
}
