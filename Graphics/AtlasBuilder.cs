using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Gamefloor.Framework;
using Gamefloor.Support;

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
            List<PlacingTexture> placings = new List<PlacingTexture>(m_textures.Count);

            int width, height;
            placings = PlaceAllTextures(out width, out height);

            b = new Bitmap(width, height);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(b);
            g.Clear(Color.Transparent);
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            elements = new List<TextureInfo>(m_textures.Count);

            foreach (PlacingTexture p in placings)
            {
                DrawPlacingTexture(g, p);
                elements.Add(p.ToTextureInfo(width, height));
            }
        }

        private List<PlacingTexture> PlaceAllTextures(out int width, out int height)
        {
            List<PlacingTexture> result = new List<PlacingTexture>(m_textures.Count);
            long pixelcount = 0;
            int max_width = 0;
            int max_height = 0;

            foreach (LoadingTexture l in m_textures)
            {
                PlacingTexture p = new PlacingTexture(l);
                max_width = Math.Max(max_width, p.CorrectedWidth);
                max_height = Math.Max(max_height, p.CorrectedHeight);
                pixelcount += p.Area;
                result.Add(p);
            }

            int width_sum = 0;
            // same naive algorithm, new place
            for (int x = 0; x < result.Count; x++)
            {
                PlacingTexture p = result[x];
                p.X = width_sum;
                width_sum += p.CorrectedWidth;
                result[x] = p;
            }

            width = width_sum;
            height = max_height;

            return result;
        }

        private void DrawPlacingTexture(System.Drawing.Graphics g, PlacingTexture p)
        {
            bool draw_topleft_margin = p.LoadingTexture.PaddingFill != PaddingFill.None && p.LoadingTexture.TexelPosition != TexelPosition.Stretch && p.LoadingTexture.TexelPosition != TexelPosition.TopLeft;
            bool draw_bottomright_margin = p.LoadingTexture.PaddingFill != PaddingFill.None && p.LoadingTexture.TexelPosition != TexelPosition.Stretch;

            int offset_topleft = draw_topleft_margin ? 1 : 0;
            // draw the actual texture
            g.DrawImage(p.LoadingTexture.Bitmap, p.X + offset_topleft, p.Y + offset_topleft, p.LoadingTexture.Bitmap.Width, p.LoadingTexture.Bitmap.Height);

            switch (p.LoadingTexture.PaddingFill)
            {
                case PaddingFill.Wrap:
                    if (draw_topleft_margin)
                    {
                        // left
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X, p.Y + 1, 1, p.LoadingTexture.Bitmap.Height), new Rectangle(p.LoadingTexture.Bitmap.Width - 1, 0, 1, p.LoadingTexture.Bitmap.Height), GraphicsUnit.Pixel);
                        // top
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X + 1, p.Y, p.LoadingTexture.Bitmap.Width, 1), new Rectangle(0, p.LoadingTexture.Bitmap.Height - 1, p.LoadingTexture.Bitmap.Width, 1), GraphicsUnit.Pixel);
                        // topleft pixel
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X, p.Y, 1, 1), new Rectangle(p.LoadingTexture.Bitmap.Width - 1, p.LoadingTexture.Bitmap.Height - 1, 1, 1), GraphicsUnit.Pixel);

                        AssertHelper.Assert(draw_bottomright_margin);
                        // bottomleft pixel
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X, p.Y + p.LoadingTexture.Bitmap.Height + 1, 1, 1), new Rectangle(p.LoadingTexture.Bitmap.Width - 1, 0, 1, 1), GraphicsUnit.Pixel);
                        // topright pixel
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X + p.LoadingTexture.Bitmap.Width + 1, p.Y, 1, 1), new Rectangle(0, p.LoadingTexture.Bitmap.Height - 1, 1, 1), GraphicsUnit.Pixel);
                    }

                    if (draw_bottomright_margin)
                    {
                        // right
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X + p.LoadingTexture.Bitmap.Width + offset_topleft, p.Y + offset_topleft, 1, p.LoadingTexture.Bitmap.Height), new Rectangle(0, 0, 1, p.LoadingTexture.Bitmap.Height), GraphicsUnit.Pixel);
                        // bottom
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X + offset_topleft, p.Y + p.LoadingTexture.Bitmap.Height + offset_topleft, p.LoadingTexture.Bitmap.Width, 1), new Rectangle(0, 0, p.LoadingTexture.Bitmap.Width, 1), GraphicsUnit.Pixel);
                        // bottomright pixel
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X + p.LoadingTexture.Bitmap.Width + offset_topleft, p.Y + p.LoadingTexture.Bitmap.Height + offset_topleft, 1, 1), new Rectangle(0, 0, 1, 1), GraphicsUnit.Pixel);
                    }
                    break;
                case PaddingFill.Clamp:
                    if (draw_topleft_margin)
                    {
                        // left
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X, p.Y + 1, 1, p.LoadingTexture.Bitmap.Height), new Rectangle(0, 0, 1, p.LoadingTexture.Bitmap.Height), GraphicsUnit.Pixel);
                        // top
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X + 1, p.Y, p.LoadingTexture.Bitmap.Width, 1), new Rectangle(0, 0, p.LoadingTexture.Bitmap.Width, 1), GraphicsUnit.Pixel);
                        // topleft pixel
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X, p.Y, 1, 1), new Rectangle(0, 0, 1, 1), GraphicsUnit.Pixel);

                        AssertHelper.Assert(draw_bottomright_margin);
                        // bottomleft pixel
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X, p.Y + p.LoadingTexture.Bitmap.Height + 1, 1, 1), new Rectangle(0, p.LoadingTexture.Bitmap.Height - 1, 1, 1), GraphicsUnit.Pixel);
                        // topright pixel
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X + p.LoadingTexture.Bitmap.Width + 1, p.Y, 1, 1), new Rectangle(p.LoadingTexture.Bitmap.Width - 1, 0, 1, 1), GraphicsUnit.Pixel);
                    }

                    if (draw_bottomright_margin)
                    {
                        // right
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X + p.LoadingTexture.Bitmap.Width + offset_topleft, p.Y + offset_topleft, 1, p.LoadingTexture.Bitmap.Height), new Rectangle(p.LoadingTexture.Bitmap.Width - 1, 0, 1, p.LoadingTexture.Bitmap.Height), GraphicsUnit.Pixel);
                        // bottom
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X + offset_topleft, p.Y + p.LoadingTexture.Bitmap.Height + offset_topleft, p.LoadingTexture.Bitmap.Width, 1), new Rectangle(0, p.LoadingTexture.Bitmap.Height - 1, p.LoadingTexture.Bitmap.Width, 1), GraphicsUnit.Pixel);
                        // bottomright pixel
                        g.DrawImage(p.LoadingTexture.Bitmap, new Rectangle(p.X + p.LoadingTexture.Bitmap.Width + offset_topleft, p.Y + p.LoadingTexture.Bitmap.Height + offset_topleft, 1, 1), new Rectangle(p.LoadingTexture.Bitmap.Width - 1, p.LoadingTexture.Bitmap.Height - 1, 1, 1), GraphicsUnit.Pixel);
                    }
                    break;
                case PaddingFill.Solid:
                    Brush brush = new SolidBrush(p.LoadingTexture.PaddingColour);

                    if (draw_topleft_margin)
                    {
                        // left
                        g.FillRectangle(brush, new Rectangle(p.X, p.Y + 1, 1, p.LoadingTexture.Bitmap.Height));
                        // top
                        g.FillRectangle(brush, new Rectangle(p.X + 1, p.Y, p.LoadingTexture.Bitmap.Width, 1));
                        // topleft pixel
                        g.FillRectangle(brush, new Rectangle(p.X, p.Y, 1, 1));

                        AssertHelper.Assert(draw_bottomright_margin);
                        // bottomleft pixel
                        g.FillRectangle(brush, new Rectangle(p.X, p.Y + p.LoadingTexture.Bitmap.Height + 1, 1, 1));
                        // topright pixel
                        g.FillRectangle(brush, new Rectangle(p.X + p.LoadingTexture.Bitmap.Width + 1, p.Y, 1, 1));
                    }

                    if (draw_bottomright_margin)
                    {
                        // right
                        g.FillRectangle(brush, new Rectangle(p.X + p.LoadingTexture.Bitmap.Width + offset_topleft, p.Y + offset_topleft, 1, p.LoadingTexture.Bitmap.Height));
                        // bottom
                        g.FillRectangle(brush, new Rectangle(p.X + offset_topleft, p.Y + p.LoadingTexture.Bitmap.Height + offset_topleft, p.LoadingTexture.Bitmap.Width, 1));
                        // bottomright pixel
                        g.FillRectangle(brush, new Rectangle(p.X + p.LoadingTexture.Bitmap.Width + offset_topleft, p.Y + p.LoadingTexture.Bitmap.Height + offset_topleft, 1, 1));
                    }
                    break;
            }
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

                if (loading_texture.PaddingFill == PaddingFill.None || loading_texture.TexelPosition == TexelPosition.Shrink)
                {
                    CorrectedWidth = loading_texture.Bitmap.Width;
                    CorrectedHeight = loading_texture.Bitmap.Height;
                }
                else if (loading_texture.TexelPosition == TexelPosition.TopLeft)
                {
                    CorrectedWidth = loading_texture.Bitmap.Width + 1;
                    CorrectedHeight = loading_texture.Bitmap.Height + 1;
                }
                else
                {
                    CorrectedWidth = loading_texture.Bitmap.Width + 2;
                    CorrectedHeight = loading_texture.Bitmap.Height + 2;
                }

                Area = (long)CorrectedWidth * (long)CorrectedHeight;
            }

            public PlacingTexture(LoadingTexture loading_texture)
                : this(loading_texture, 0, 0)
            {
            }

            public LoadingTexture LoadingTexture;
            public int X, Y, CorrectedWidth, CorrectedHeight;
            public long Area;

            public TextureInfo ToTextureInfo(int atlas_width, int atlas_height)
            {
                bool draw_topleft_margin = this.LoadingTexture.PaddingFill != PaddingFill.None && this.LoadingTexture.TexelPosition != TexelPosition.Stretch && this.LoadingTexture.TexelPosition != TexelPosition.TopLeft;
                int margin_offset_topleft = draw_topleft_margin ? 1 : 0;

                double offset_topleft, offset_bottomright;
                switch (this.LoadingTexture.TexelPosition)
                {
                    // todo: code for texel origin other than center
                    case TexelPosition.Shrink:
                        offset_topleft = this.LoadingTexture.PaddingFill == PaddingFill.None ?
                                         -0.5 : 0.5;
                        offset_bottomright = this.LoadingTexture.PaddingFill == PaddingFill.None ?
                                             0.5 : 1.5;
                        break;
                    case TexelPosition.Stretch:
                        offset_topleft = 0.5;
                        offset_bottomright = -0.5;
                        break;
                    case TexelPosition.Middle:
                        offset_topleft = this.LoadingTexture.PaddingFill == PaddingFill.None ?
                                         0 : 1;
                        offset_bottomright = this.LoadingTexture.PaddingFill == PaddingFill.None ?
                                             0 : 1;
                        break;
                    case TexelPosition.TopLeft:
                        offset_topleft = 0.5;
                        offset_bottomright = 0.5;
                        break;
                    default:
                        throw new ArgumentException();
                }

                return new TextureInfo(this.LoadingTexture.Name,
                    (X + offset_topleft) / atlas_width, (Y + offset_topleft) / atlas_height,
                    (X + this.LoadingTexture.Bitmap.Width + offset_bottomright) / atlas_width, (Y + this.LoadingTexture.Bitmap.Height + offset_bottomright) / atlas_height,
                    this.LoadingTexture.Bitmap.Width, this.LoadingTexture.Bitmap.Height,
                    this.LoadingTexture.PaddingFill, this.LoadingTexture.TexelPosition, this.LoadingTexture.PaddingColour);
            }
        }
    }
}
