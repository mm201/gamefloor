using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using Gamefloor.Support;
using System.ComponentModel;
using OpenTK.Graphics.OpenGL;

namespace Gamefloor.Framework
{
    public class Game
    {
        #region Lifecycle
        public Game(int width, int height, String title)
        {
            DisplayDevice device = DisplayDevice.Default;
            int x = device.Bounds.Left + (device.Bounds.Width - width) / 2;
            int y = device.Bounds.Top + (device.Bounds.Height - height) / 2;
            m_vsync = true;
            m_fps = 60;

            Init(x, y, width, height, title, GameWindowFlags.Default, GraphicsMode.Default, device);
        }

        public Game(int x, int y, int width, int height, String title, bool vsync, int fps, bool fullscreen, GraphicsMode mode, DisplayDevice display)
        {
            m_vsync = true;// vsync;
            m_fps = 60;// fps;

            Init(x, y, width, height, title, fullscreen ? GameWindowFlags.Fullscreen : GameWindowFlags.Default, mode, display ?? DisplayDevice.Default);
            m_window.WindowState = WindowState.Fullscreen;
        }

        private void Init(int x, int y, int width, int height, String title, GameWindowFlags options, GraphicsMode mode, DisplayDevice display)
        {
            // window thread
            m_window = new NativeWindow(x, y, width, height, title, options, mode, display);
            m_renderables = new List<IRenderable>();
            m_components = new List<IGameComponent>();
            m_context = new GraphicsContext(mode, m_window.WindowInfo);

            m_window.Closing += window_Exiting;
            SetupWindow();
            m_window.Visible = true;
        }

        public void Run()
        {
            // window thread == game thread
            m_thread = Thread.CurrentThread;
            Run(false);
        }

        private void Run(bool async)
        {
            // game thread
            m_running = true;
            m_async = async;

            m_context.MakeCurrent(m_window.WindowInfo);
            m_context.LoadAll();
            m_context.VSync = m_vsync;

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
        public NativeWindow Window
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
            if (this.Thread == Thread.CurrentThread) PrepareToExit();
            else
            {
                while (m_running && m_exiting)
                {
                    // hang the main thread while the game thread decides whether to exit or not
                }
            }

            bool result = m_exiting;
            m_exiting = false;
            return result;
        }

        private void PrepareToExit()
        {
            CancelEventArgs e = new CancelEventArgs();
            if (Exiting != null) Exiting(this, e);

            if (m_exiting = !e.Cancel) // single = to assign m_exiting and finish the exit
                throw new GamefloorExitingException(); // plzzzz don't catch this or your game will hang. Use the event to cancel exiting
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
                return m_thread;
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
                //m_fps = value;
            }
        }

        private bool m_has_input = false;
        public void WaitForInput()
        {
            if (m_has_input) return;
            // todo: low latency stuff goes here.
            // for now, this is safe doing nothing

            m_has_input = true;
        }

        public void NextFrame()
        {
            if (m_exiting) PrepareToExit();

            // todo: figure out some voodoo to make multiple updates happen per render when fps > refresh rate
            // and multiple renders (or idle time, preferably) happen when fps < refresh rate.
            // at present, refresh rate overrides fps unconditionally.
            WaitForInput();
            UpdateComponents();
            Render();

            Context.SwapBuffers();
            // todo: replace prev input with cur input once input stuff is done
            m_has_input = false;

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

        private bool m_vsync;
        public bool Vsync
        {
            get
            {
                return m_vsync;
            }
            set
            {
                //m_vsync = value;
                //if (m_context != null) m_context.VSync = value;
            }
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
        public IGraphicsContext Context
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

        public void SetViewport(ViewportSizing sizing, double scale)
        {
            GL.Viewport(0, 0, Window.Width, Window.Height);
            GL.LoadIdentity();
            double x, y, w, h;
            switch (sizing)
            {
                    // todo: consideration to sampling origin stuff.
                case ViewportSizing.Pixels:
                default:
                    x = 0.0d;
                    y = 0.0d;
                    w = Window.Width / scale;
                    h = Window.Height / scale;
                    break;
                case ViewportSizing.Unit:
                    x = 0.0d;
                    y = 0.0d;
                    w = scale;
                    h = scale;
                    break;
                case ViewportSizing.GreaterUnit:
                    if (Window.Width > Window.Height)
                    {
                        x = 0.0d;
                        y = 0.0d;
                        w = 1.0d / scale;
                        h = Window.Height / scale / Window.Width;
                    }
                    else
                    {
                        x = 0.0d;
                        y = 0.0d;
                        w = Window.Width / scale / Window.Height;
                        h = 1.0d / scale;
                    }
                    break;
                case ViewportSizing.LesserUnit:
                    if (Window.Width > Window.Height)
                    {
                        x = 0.0d;
                        y = 0.0d;
                        w = Window.Width / scale / Window.Height;
                        h = 1.0d / scale;
                    }
                    else
                    {
                        x = 0.0d;
                        y = 0.0d;
                        w = 1.0d / scale;
                        h = Window.Height / scale / Window.Width;
                    }
                    break;
            }
            GL.Ortho(x, w - x, h - y, y, DEPTH_MIN, DEPTH_MAX);
        }

        public void SetViewport(ViewportSizing sizing)
        {
            SetViewport(sizing, 1.0f);
        }

        private const double DEPTH_MIN = -1.0d;
        private const double DEPTH_MAX = 1.0d;

        #endregion
    }

    public enum TimingModes
    {
        Unlimited = 0,
        Vsync = 1,
        Fixed = 2
    }

    public enum ViewportSizing
    {
        /// <summary>
        /// Vertex coordinates correspond to screen pixels
        /// </summary>
        Pixels,
        /// <summary>
        /// Vertex coordinates are on the 0, 1 range.
        /// </summary>
        Unit,
        /// <summary>
        /// The larger axis is on the 0, 1 range, the smaller axis is scaled to fixed aspect ratio.
        /// </summary>
        GreaterUnit,
        /// <summary>
        /// The smaller axis is on the 0, 1 range, the larger axis is scaled to fixed aspect ratio
        /// </summary>
        LesserUnit
    }
}
