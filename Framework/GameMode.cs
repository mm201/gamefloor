using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics;
using Gamefloor.Support;

namespace Gamefloor.Framework
{
    /// <summary>
    /// A GameMode assumes full control when it launches and only returns when the mode's
    /// execution concludes.
    /// </summary>
    public class GameMode : IRenderable, IGameComponent
    {
        #region Lifecycle
        private Game m_game;

        public GameMode(Game game)
        {
            m_game = game;
        }

        public Game Game
        {
            get
            {
                return m_game;
            }
        }

        public void Run()
        {
            m_game.Renderables.Add(this);
            m_game.Components.Add(this);
            Begin();
            m_game.Renderables.Remove(this);
            m_game.Components.Remove(this);
        }

        protected virtual void Begin()
        {
            // override this and place the entire lifecycle of this mode within
        }
        #endregion

        #region IRenderable implementation
        public virtual void Render(IGraphicsContext context, bool reverse)
        {
        }

        public virtual Priority DrawOrder
        {
            get
            {
                return new Priority(0, 0, 0, 0);
            }
        }

        public virtual bool Reversible
        {
            get
            {
                return false;
            }
        }

        public virtual bool DrawFirst
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region IGameComponent implementation
        public virtual void Update(bool RenderableFrame)
        {
        }

        public virtual Priority ProcessOrder
        {
            get
            {
                return new Priority(0, 0, 0, 0);
            }
        }
        #endregion
    }
}
