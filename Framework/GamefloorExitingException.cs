using System;
using System.Collections.Generic;
using System.Text;
using Gamefloor.Support;

namespace Gamefloor.Framework
{
    class GamefloorExitingException : GamefloorException
    {
        public GamefloorExitingException()
            : base("Gamefloor exiting")
        {
        }
    }
}
