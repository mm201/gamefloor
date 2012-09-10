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
        /// <summary>
        /// Render
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="reverse">If true, this should draw below other things already in the buffer.</param>
        void Render(IGraphicsContext context, bool reverse);

        /// <summary>
        /// Depth ordering. Higher priorities are on top.
        /// </summary>
        Priority DrawOrder { get; }

        /// <summary>
        /// If true, this renderable is able to draw below other things already in the buffer.
        /// </summary>
        bool Reversible { get; }

        /// <summary>
        /// Not to be confused with DrawOrder. This means the renderable must be drawn first to avoid errors.
        /// More than one such renderable in a framebuffer may cause graphics errors.
        /// </summary>
        bool DrawFirst { get; }
    }
}
