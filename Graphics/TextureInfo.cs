using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Gamefloor.Graphics
{
    /// <summary>
    /// Information about a processed sprite inside a sheet
    /// </summary>
    public struct TextureInfo
    {
        public TextureInfo(String name, double left, double top, double right, double bottom, int width, int height, PaddingFill padding_fill, TexelPosition texel_position, Color padding_colour)
        {
            Name = name;
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
            Width = width;
            Height = height;
            PaddingFill = padding_fill;
            TexelPosition = texel_position;
            PaddingColour = padding_colour;
        }

        /// <summary>
        /// Name (usually filename) of the sprite
        /// </summary>
        public String Name;
        /// <summary>
        /// Left texel coordinate on the [0, 1) range
        /// </summary>
        public double Left;
        /// <summary>
        /// Top texel coordinate on the [0, 1) range
        /// </summary>
        public double Top;
        /// <summary>
        /// Right texel coordinate on the [0, 1) range
        /// </summary>
        public double Right;
        /// <summary>
        /// Bottom texel coordinate on the [0, 1) range
        /// </summary>
        public double Bottom;
        /// <summary>
        /// Pixel width of the underlying texture.
        /// (Doesn't relate to Left/Top/Right/Bottom)
        /// </summary>
        public int Width;
        /// <summary>
        /// Pixel height of the underlying texture.
        /// (Doesn't relate to Left/Top/Right/Bottom)
        /// </summary>
        public int Height;
        /// <summary>
        /// Padding fill rule provided when the sheet was constructed
        /// </summary>
        public PaddingFill PaddingFill;
        /// <summary>
        /// Texel position rule provided when the sheet was constructed
        /// </summary>
        public TexelPosition TexelPosition;
        /// <summary>
        /// Padding fill colour provided when the sheet was constructed. Indeterminate if the rule doesn't call for a fill colour.
        /// </summary>
        public Color PaddingColour;
    }
}
