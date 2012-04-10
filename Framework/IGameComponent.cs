using System;
using System.Collections.Generic;
using System.Text;
using Gamefloor.Support;

namespace Gamefloor.Framework
{
    interface IGameComponent
    {
        public void Update(bool RenderableFrame);
        public Priority ProcessOrder { get; }
    }
}
