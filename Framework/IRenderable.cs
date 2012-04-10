using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using Gamefloor.Support;

namespace Gamefloor.Framework
{
    public interface IRenderable
    {
        void Render(IGraphicsContext context);
        Priority DrawOrder { get; }
    }
}
