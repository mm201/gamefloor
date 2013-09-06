using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using Gamefloor.Support;
using System.ComponentModel;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

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
            m_vsync = vsync;
            m_fps = fps;

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
            m_window.WindowStateChanged += window_WindowStateChanged;
            SetupWindow();
            m_window.Visible = true;
        }

        private GraphicsMode PreferredMode()
        {
            return GraphicsMode.Default;
        }

        /// <summary>
        /// Runs the game on the current thread.
        /// </summary>
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
            ObtainDeviceTiming();
            m_context.VSync = m_vsync;
            m_stopwatch = Stopwatch.StartNew();
            ResetStopwatch();

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

        /// <summary>
        /// Runs the game on a separate thread from the window.
        /// This mode seems to cause choppy performance.
        /// </summary>
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

        private void window_WindowStateChanged(object sender, EventArgs e)
        {
            // window thread

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
        /// <summary>
        /// If true, the game/render loop is running in a separate thread from the main window thread.
        /// </summary>
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
                m_fps = value;
                ResetStopwatch();
            }
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
                if (m_refresh_rate == 0) return;
                m_vsync = value;
                if (m_context != null) m_context.VSync = value;
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

        /// <summary>
        /// Waits for one updatable cycle
        /// </summary>
        public void NextFrame()
        {
            // todo: figure out some voodoo to make multiple updates happen per render when fps > refresh rate
            // and multiple renders (or idle time, preferably) happen when fps < refresh rate.
            // at present, refresh rate overrides fps unconditionally.

            // todo: if frames are late (system lag), figure out a way to skip them.
            // (skip if lost time is less than, say, 1 second)

            NextFrameImmediate(true);
            // idle after the frame has reached the screen to reduce input lag
            HandleTiming();
        }

        private void NextFrameImmediate(bool render)
        {
            if (m_exiting) PrepareToExit();

            WaitForInput();
            UpdateComponents(); // todo: add Update Without Input event

            if (render)
            {
                Render();
                Context.SwapBuffers();
            }
            // todo: replace prev input with cur input once input stuff is done
            m_has_input = false;

            // keep windows message loop going always
            if (!m_async) m_window.ProcessEvents();
        }

        public void Wait(int frames)
        {
            // todo: error condition if fps is unlimited
            for (int x = 0; x < frames; x++) NextFrame();
        }

        // number of the last frame for whom an Update was performed
        private long m_last_frame_number = 0;

        // number of seconds tolerance when it'll skip ahead instead of assuming
        // the program was suspended and picking up where it left off.
        private const float FRAMESKIP_TIME_MAX = 1.0f;
        // if there's only this much time before we're due for the next frame, stop processing window events
        private const float STOP_PROCESSING_EVENTS_TIME = 0.001f;

        private void HandleTiming()
        {
            // seconds since last frame
            float seconds = (m_stopwatch.ElapsedTicks - FrameToTicks(m_last_frame_number)) / (float)(Stopwatch.Frequency);
            if (seconds > FRAMESKIP_TIME_MAX)
            {
                ResetStopwatch();
                return;
            }
            long remaining_ticks = FrameToTicks(m_last_frame_number + 1) - m_stopwatch.ElapsedTicks;
            if (remaining_ticks <= 0)
            {
                m_last_frame_number++;
                return;
            }

            long target_ticks = FrameToTicks(m_last_frame_number + 1);
            long process_event_ticks = target_ticks - (long)(Stopwatch.Frequency * STOP_PROCESSING_EVENTS_TIME);
            while (m_stopwatch.ElapsedTicks < target_ticks)
            {
                if (m_async && m_stopwatch.ElapsedTicks < process_event_ticks) m_window.ProcessEvents();
            }
            m_last_frame_number++;
        }

        /// <summary>
        /// Restart timing, at the cost of slight inaccuracy.
        /// </summary>
        private void ResetStopwatch()
        {
            if (m_stopwatch == null) return;
            m_last_frame_number = -1;
            m_stopwatch.Reset();
            m_stopwatch.Start();
        }

        private long FrameToTicks(long frame)
        {
            return frame * Stopwatch.Frequency / m_fps;
        }

        private long TicksToFrame(long ticks)
        {
            return ticks * m_fps / Stopwatch.Frequency;
        }

        private int m_refresh_rate = -1;
        private Stopwatch m_stopwatch;

        private void ObtainDeviceTiming()
        {
            m_refresh_rate = ResolutionHelper.GetCurrentRefreshRate();
            if (m_refresh_rate == 0)
            {
                m_vsync = false;
                if (m_context != null) m_context.VSync = false;
            }
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

            // todo: cache this somehow by detecting changes in the m_renderables collection
            List<IRenderable> renderables = new List<IRenderable>(m_renderables);
            renderables.Sort((x, y) => (x.DrawOrder.CompareTo(y.DrawOrder)));
            m_context.MakeCurrent(m_window.WindowInfo);
            if (!m_loaded)
            {
                m_context.LoadAll();
                m_loaded = true;
            }
            foreach (IRenderable r in renderables)
            {
                r.Render(Context, false);
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
