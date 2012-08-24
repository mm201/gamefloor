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
            Bitmap b;
            List<TextureInfo> elements;

            InnerBuild(out b, out elements);
            return new TextureAtlas(b, game, elements);
        }

        public Bitmap GetBitmap()
        {
            Bitmap b;
            List<TextureInfo> elements;

            InnerBuild(out b, out elements);
            return b;
        }

        private void InnerBuild(out Bitmap b, out List<TextureInfo> elements)
        {
            // really naive algorithm for now: just stack all of them horizontally

            int max_height = 0;
            int width = 0;
            foreach (LoadingTexture l in m_textures)
            {
                max_height = Math.Max(max_height, l.Bitmap.Height);
                width += l.Bitmap.Width;
            }

            b = new Bitmap(width, max_height);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(b);
            g.Clear(Color.Transparent);

            int progress = 0;
            elements = new List<TextureInfo>(m_textures.Count);

            foreach (LoadingTexture l in m_textures)
            {
                g.DrawImage(l.Bitmap, new Rectangle(progress, 0, l.Bitmap.Width, l.Bitmap.Height));
                elements.Add(new TextureInfo(l.Name, (double)progress / width, 0, (double)(progress + l.Bitmap.Width) / width, (double)(l.Bitmap.Height) / (double)max_height, l.Bitmap.Width, l.Bitmap.Height, l.PaddingFill, l.TexelPosition, l.PaddingColour));
                progress += l.Bitmap.Width;
            }
        }

        private void MakeAdjacentBorder(System.Drawing.Graphics dest, Bitmap src, int x, int y)
        {
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

        private struct PlacingTexture
        {
            public PlacingTexture(LoadingTexture loading_texture, int x, int y)
            {
                this.LoadingTexture = loading_texture;
                X = x;
                Y = y;
                CorrectedWidth = loading_texture.PaddingFill == PaddingFill.None ? loading_texture.Bitmap.Width : loading_texture.Bitmap.Width + 2;
                CorrectedHeight = loading_texture.PaddingFill == PaddingFill.None ? loading_texture.Bitmap.Height : loading_texture.Bitmap.Height + 2;
                Area = CorrectedWidth * CorrectedHeight;
            }

            public PlacingTexture(LoadingTexture loading_texture)
                : this(loading_texture, 0, 0)
            {
            }

            public LoadingTexture LoadingTexture;
            public int X, Y, CorrectedWidth, CorrectedHeight;
            public int Area;
        }
    }
}
