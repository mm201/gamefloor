using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using Gamefloor.Support;

namespace Gamefloor.Framework
{
    public class Game
    {
        #region Lifecycle

        public Game()
        {
            m_renderables = new List<IRenderable>();
        }

        public sealed void Run()
        {
            Run(false);
        }

        private void Run(bool async)
        {
            m_running = true;
            m_async = async;
            Begin();
            m_running = false;
        }

        protected virtual void Begin()
        {
            // override this and place your entire game lifecycle within.
        }

        public void RunAsync()
        {
            if (m_running) throw new InvalidOperationException("Game is already running");

            m_thread = new Thread(delegate()
            {
                Run(true);
            });

            m_thread.Start();
        }

        private bool m_running = false;
        public bool Running
        {
            get
            {
                return m_running;
            }
        }

        private bool m_async;
        protected bool Async
        {
            get
            {
                return m_async;
            }
        }

        private Thread m_thread = null;
        protected Thread Thread
        {
            get
            {
                return Thread.CurrentThread;
            }
        }

        #endregion

        #region Timing

        private int m_fps = 60;
        protected int TargetFps
        {
            get
            {
                return m_fps;
            }
            set
            {
                m_fps = value;
            }
        }

        public sealed void NextFrame()
        {
            UpdateComponents();
            Render();

            Context.SwapBuffers();
        }

        public sealed void Wait(int frames)
        {
            for (int x = 0; x < frames; x++) NextFrame();
        }

        #endregion

        #region Components

        private List<IGameComponent> m_components;
        public List<IGameComponent> Components
        {
            get
            {
                return m_components;
            }
        }

        private void UpdateComponents()
        {
            AssertHelper.Assert(m_components != null, "Game.m_components is null.");
            if (m_components == null) return;

            m_components.Sort((x, y) => (x.ProcessOrder.CompareTo(y.ProcessOrder)));
            foreach (IGameComponent c in m_components)
            {
                c.Update(true); // todo: renderable frame logic, part of fps limiting
            }
        }

        #endregion

        #region Graphics

        private GameWindow m_game_window;
        protected IGraphicsContext Context
        {
            get
            {
                return m_game_window.Context;
            }
        }

        private List<IRenderable> m_renderables;
        public List<IRenderable> Renderables
        {
            get
            {
                return m_renderables;
            }
        }

        private void Render()
        {
            AssertHelper.Assert(m_renderables != null, "Game.m_renderables is null.");
            if (m_renderables == null) return;

            m_renderables.Sort((x, y) => (x.DrawOrder.CompareTo(y.DrawOrder)));
            foreach (IRenderable r in m_renderables)
            {
                r.Render(Context);
            }
        }

        #endregion

        
    }

    public enum TimingModes
    {
        Unlimited = 0,
        Vsync = 1,
        Fixed = 2
    }
}
