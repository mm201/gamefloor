using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics;
using Gamefloor.Support;

namespace Gamefloor.Framework
{
    /// <summary>
    /// A GameComponent runs asyncronously and must be started and stopped.
    /// Execution of the active Mode continues while the GameComponent is running.
    /// </summary>
    public class GameComponent: IRenderable, IGameComponent
    {
        #region Lifecycle
        private Game m_game;

        public GameComponent(Game game)
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
        #endregion

        #region State
        public void Show()
        {
            Game.Renderables.Add(this);
        }

        public void Hide()
        {
            Game.Renderables.Remove(this);
        }

        public bool Visible
        {
            get
            {
                return Game.Renderables.Contains(this);
            }
            set
            {
                if (value) Show();
                else Hide();
            }
        }

        public void Start()
        {
            Game.Components.Add(this);
        }

        public void Stop()
        {
            Game.Components.Remove(this);
        }

        public bool Running
        {
            get
            {
                return Game.Components.Contains(this);
            }
            set
            {
                if (value) Start();
                else Stop();
            }
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
