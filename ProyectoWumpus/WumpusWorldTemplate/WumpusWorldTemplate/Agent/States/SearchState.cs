using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace WumpusWorldTemplate
{
    public class SearchState : State<Agent>
    {
        public override void OnInitalize(Agent owner)
        {
            
            
        }

        public override void OnUpdate(Agent owner, GameTime gameTime)
        {

            if (Main.Input.IsKeyPressed(Keys.Q))
            {
                if (!owner.TryToKillTheWumpus())
                {
                    owner.ChooseNextTile();
                    owner.MoveToBestTile();
                }

                EvaluateCurrentTile(owner);
                EvaluateCurrentState(owner);
            }
            
        }
        
        private void EvaluateCurrentTile(Agent owner)
        {
            if (owner.IsDead)
                throw new NotImplementedException();
            if (owner.FoundGold)
            {
                owner.GrabGold();
                InvokeStateChange(owner, new RetreatState());
            }
        }

        private void EvaluateCurrentState(Agent owner)
        {
           if(owner.HaveAllSafeTilesBeenVisited())
           {
               InvokeStateChange(owner, new RetreatState());
           }
        }

        public override void OnEnd(Agent owner)
        {
            
        }
    }
}
