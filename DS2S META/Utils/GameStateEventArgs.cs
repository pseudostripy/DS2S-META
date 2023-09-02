using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public class GameStateEventArgs : EventArgs
    {
        public int OldGameState; // for extra checks
        public int GameState; // new gamestate after transition

        public GameStateEventArgs(object? sender, int oldGameState, int newGameState)
        {
            OldGameState = oldGameState;
            GameState = newGameState;
        }
    }
}
