using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace Gamefloor.Graphics
{
    /// <summary>
    /// A texture which is to be stored inside an atlas.
    /// </summary>
    public class LoadingTexture
    {
        public LoadingTexture(String name, Bitmap bitmap, PaddingFill padding_fill, TexelPosition texel_position, Color padding_colour)
        {
            Init(name, bitmap, padding_fill, texel_position, padding_colour);
        }

        public LoadingTexture(String name, Bitmap bitmap, PaddingFill padding_fill, TexelPosition texel_position)
            : this(name, bitmap, padding_fill, texel_position, Color.Transparent)
        {
        }

        public LoadingTexture(String name, Bitmap bitmap)
            : this(name, bitmap, PaddingFill.Clamp, TexelPosition.Middle)
        {
        }

        public LoadingTexture(String filename, PaddingFill padding_fill, TexelPosition texel_position, Color padding_colour)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException(new FileNotFoundException().Message, filename);
            Init(Path.GetFileName(filename), new Bitmap(filename), padding_fill, texel_position, padding_colour);
        }

        public LoadingTexture(String filename, PaddingFill padding_fill, TexelPosition texel_position)
            : this(filename, padding_fill, texel_position, Color.Transparent)
        {
        }

        public LoadingTexture(String filename)
            : this(filename, PaddingFill.Clamp, TexelPosition.Middle)
        {
        }

        private void Init(String name, Bitmap bitmap, PaddingFill padding_fill, TexelPosition texel_position, Color padding_colour)
        {
            Name = name;
            this.Bitmap = bitmap;
            this.PaddingFill = padding_fill;
            this.TexelPosition = texel_position;
            PaddingColour = padding_colour;
        }

        public String Name { get; set; }
        public Bitmap Bitmap { get; set; }
        public PaddingFill PaddingFill { get; set; }
        public TexelPosition TexelPosition { get; set; }
        public Color PaddingColour { get; set; }
    }
}
