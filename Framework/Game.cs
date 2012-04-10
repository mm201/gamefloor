using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using Gamefloor.Support;
using System.ComponentModel;

namespace Gamefloor.Framework
{
    public class Game
    {
        #region Lifecycle
        public Game(int width, int height, String title, GameWindowFlags options, GraphicsMode mode, DisplayDevice device)
        {
            // window thread
            m_window = new NativeWindow(width, height, title, options, mode, device);
            m_renderables = new List<IRenderable>();
            m_components = new List<IGameComponent>();
            m_context = new GraphicsContext(mode, m_window.WindowInfo);

            m_window.Closing += window_Exiting;
            SetupWindow();
            m_window.Visible = true;
        }

        public Game(int width, int height, String title) : this(width, height, title, GameWindowFlags.Default, GraphicsMode.Default, DisplayDevice.Default)
        {
        }

        public void Run()
        {
            // window thread == game thread
            Run(false);
        }

        private void Run(bool async)
        {
            // game thread
            m_running = true;
            m_async = async;
            try
            {
                Begin();
            }
            catch (GamefloorExitingException)
            {
            }
            m_running = false;
        }

        protected virtual void Begin()
        {
            // game thread
            // override this and place your entire game lifecycle within.
        }

        public void RunAsync()
        {
            if (m_running) throw new InvalidOperationException("Game is already running");

            m_thread = new Thread(delegate()
            {
                Run(true);
            });

            m_running = true;
            m_thread.Start();
        }

        private NativeWindow m_window;
        protected NativeWindow Window
        {
            get
            {
                return m_window;
            }
        }

        protected virtual void SetupWindow()
        {
            // window thread
            // initial call to set window things like icon, ...
        }

        private void window_Exiting(object sender, CancelEventArgs e)
        {
            // window thread
            e.Cancel = Exit();
        }

        // raised in game thread
        protected event EventHandler<CancelEventArgs> Exiting;

        public bool Exit()
        {
            // window thread
            m_exiting = true;
            while (m_running && m_exiting)
            {
                // hang the main thread while the game thread decides whether to exit or not
            }

            bool result = m_exiting;
            m_exiting = false;
            return result;
        }

        private bool m_exiting = false;

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

        public void NextFrame()
        {
            if (m_exiting)
            {
                CancelEventArgs e = new CancelEventArgs();
                if (Exiting != null) Exiting(this, e);

                if (m_exiting = !e.Cancel) // single = to assign m_exiting and finish the exit
                    throw new GamefloorExitingException(); // plzzzz don't catch this or your game will hang. Use the event to cancel exiting
            }

            UpdateComponents();
            Render();

            Context.SwapBuffers();

            // keep windows message loop going always
            if (!m_async) m_window.ProcessEvents();
        }

        public void Wait(int frames)
        {
            for (int x = 0; x < frames; x++) NextFrame();
        }

        public void ProcessEvents()
        {
            m_window.ProcessEvents();
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

        private IGraphicsContext m_context;
        protected IGraphicsContext Context
        {
            get
            {
                return m_context;
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

        private bool m_loaded = false;

        private void Render()
        {
            AssertHelper.Assert(m_renderables != null, "Game.m_renderables is null.");
            if (m_renderables == null) return;

            m_renderables.Sort((x, y) => (x.DrawOrder.CompareTo(y.DrawOrder)));
            m_context.MakeCurrent(m_window.WindowInfo);
            if (!m_loaded)
            {
                m_context.LoadAll();
                m_loaded = true;
            }
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
