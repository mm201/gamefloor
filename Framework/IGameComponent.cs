using System;
using System.Collections.Generic;
using System.Text;
using Gamefloor.Support;

namespace Gamefloor.Framework
{
    public interface IGameComponent
    {
        void Update(bool RenderableFrame);
        Priority ProcessOrder { get; }
    }
}
