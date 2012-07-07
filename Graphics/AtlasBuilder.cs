using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Gamefloor.Framework;

namespace Gamefloor.Graphics
{
    public class AtlasBuilder
    {
        public AtlasBuilder()
            : this(null)
        {
        }

        public AtlasBuilder(Game game)
        {
            Init();
            Game = game;
        }

        private void Init()
        {
            m_textures = new List<LoadingTexture>();
        }

        public TextureAtlas Build()
        {
            return Build(Game);
        }

        public Game Game { get; set; }

        public TextureAtlas Build(Game game)
        {
            // really naive algorithm for now: just stack all of them horizontally

            int max_height = 0;
            int width = 0;
            foreach (LoadingTexture l in m_textures)
            {
                max_height = Math.Max(max_height, l.Bitmap.Height);
                width += l.Bitmap.Width;
            }

            Bitmap b = new Bitmap(width, max_height);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(b);
            g.Clear(Color.Transparent);

            int progress = 0;
            List<TextureInfo> elements = new List<TextureInfo>(m_textures.Count);

            foreach (LoadingTexture l in m_textures)
            {
                g.DrawImage(l.Bitmap, new Rectangle(progress, 0, l.Bitmap.Width, l.Bitmap.Height));
                elements.Add(new TextureInfo(l.Name, (double)progress / width, 0, (double)(progress + l.Bitmap.Width) / width, 1.0d, l.Bitmap.Width, l.Bitmap.Height, l.PaddingFill, l.TexelPosition, l.PaddingColour));
                progress += l.Bitmap.Width;
            }

            return new TextureAtlas(b, game, elements);
        }

        private List<LoadingTexture> m_textures;

        public bool RequirePot { get; set; }
        public int MinWidth { get; set; }
        public int MinHeight { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }

        public List<LoadingTexture> Textures
        {
            get
            {
                return m_textures;
            }
        }
    }
}
