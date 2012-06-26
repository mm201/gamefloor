using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using OpenTK.Graphics;
using System.IO;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using Gamefloor.Framework;
using OpenTK.Platform;

namespace Gamefloor.Graphics
{
    /// <summary>
    /// wraps an OpenGL texture object
    /// </summary>
    public class Texture2D : IDisposable
    {
        public Texture2D(Bitmap data, IGraphicsContext context, IWindowInfo info)
        {
            Initialize(data, context, info);
        }

        public Texture2D(Stream data, IGraphicsContext context, IWindowInfo info)
        {
            Initialize(new Bitmap(data), context, info);
        }

        public Texture2D(String filename, IGraphicsContext context, IWindowInfo info)
        {
            Stream s = File.OpenRead(filename);
            Initialize(new Bitmap(s), context, info);
        }

        public Texture2D(Bitmap data, Game game)
            : this(data, game.Context, game.Window.WindowInfo)
        {
        }

        public Texture2D(Stream data, Game game)
            : this(data, game.Context, game.Window.WindowInfo)
        {
        }

        public Texture2D(String filename, Game game)
            : this(filename, game.Context, game.Window.WindowInfo)
        {
        }

        private void Initialize(Bitmap data, IGraphicsContext context, IWindowInfo info)
        {
            OpenTK.Graphics.OpenGL.PixelFormat pf_external = PixelFormatToGl(data.PixelFormat);
            PixelInternalFormat pf_internal = PixelFormatToInternal(data.PixelFormat);

            IGraphicsContext old_context = GraphicsContext.CurrentContext;
            context.MakeCurrent(info);
            m_graphics_context = context;
            m_window_info = info;

            BitmapData bd = data.LockBits(new Rectangle(new Point(0, 0), data.Size), ImageLockMode.ReadOnly, data.PixelFormat);
            try
            {
                int old_texture;
                GL.GetInteger(GetPName.TextureBinding2D, out old_texture);

                m_texture_id = GL.GenTexture();
                GL.BindTexture(TEXTURE_TARGET, TextureId);
                GL.TexImage2D(TEXTURE_TARGET, 0, pf_internal, data.Width, data.Height, 0, pf_external, PixelType.UnsignedByte, bd.Scan0);

                // todo: optionally generate mipmaps if desirable
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.BindTexture(TEXTURE_TARGET, old_texture);
            }
            finally { data.UnlockBits(bd); }
            old_context.MakeCurrent(info);
        }

        private int m_texture_id;
        public int TextureId
        {
            get { return m_texture_id; }
        }

        private IGraphicsContext m_graphics_context;
        private IWindowInfo m_window_info;

        public const TextureTarget TEXTURE_TARGET = TextureTarget.Texture2D;

        public void Bind()
        {
            GL.BindTexture(TEXTURE_TARGET, TextureId);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                IGraphicsContext old_context = GraphicsContext.CurrentContext;
                m_graphics_context.MakeCurrent(m_window_info);
                GL.DeleteTexture(TextureId);
                old_context.MakeCurrent(m_window_info);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~Texture2D()
        {
            Dispose(false);
        }

        public static OpenTK.Graphics.OpenGL.PixelFormat PixelFormatToGl(System.Drawing.Imaging.PixelFormat format)
        {
            switch (format)
            {
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
                default:
                    // todo: add more of these as necessary
                    throw new NotImplementedException();
            }
        }

        public static OpenTK.Graphics.OpenGL.PixelInternalFormat PixelFormatToInternal(System.Drawing.Imaging.PixelFormat format)
        {
            switch (format)
            {
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgba;
                default:
                    // todo: add more of these as necessary
                    throw new NotImplementedException();
            }
        }
    }
}
