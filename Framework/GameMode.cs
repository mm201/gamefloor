using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics;
using Gamefloor.Support;

namespace Gamefloor.Framework
{
    class GameMode : IRenderable
    {
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
            Begin();
            m_game.Renderables.Remove(this);
        }

        protected virtual void Begin()
        {
        }

        public virtual void Render(IGraphicsContext context)
        {
        }

        public virtual Priority DrawOrder
        {
            get
            {
                return new Priority(0, 0, 0, 0);
            }
        }
    }
}
