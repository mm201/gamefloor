using System;
using System.Collections.Generic;
using System.Text;
using Gamefloor.Graphics;
using System.Drawing;
using Gamefloor.Framework;

namespace Gamefloor.Graphics
{
    public class TextureAtlas : Texture2D
    {
        public TextureAtlas(Bitmap data, Game game, List<TextureInfo> elements)
            : base(data, game)
        {
            m_elements = new Dictionary<string, TextureInfo>();
            foreach (TextureInfo i in elements)
            {
                m_elements.Add(i.Name, i);
            }
        }

        private Dictionary<String, TextureInfo> m_elements;

        public TextureInfo this[String key]
        {
            get
            {
                return m_elements[key];
            }
        }

    }

    /// <summary>
    /// What do we fill the padding region of a sprite with?
    /// </summary>
    public enum PaddingFill
    {
        /// <summary>
        /// Repeat what's on the opposite
        /// </summary>
        Wrap,
        /// <summary>
        /// Repeat the adjacent texel
        /// </summary>
        Clamp,
        /// <summary>
        /// Fill the padding region with solid colour
        /// </summary>
        Solid,
        /// <summary>
        /// Don't add a padding (spurious contents may be seen at the edges of interpolated textures)
        /// </summary>
        None
    }

    /// <summary>
    /// How a sprite's reported texel coordinates relate to the underlying texture's position and size
    /// </summary>
    public enum TexelPosition
    {
        /// <summary>
        /// Expand so that no border is visible
        /// </summary>
        Stretch,
        /// <summary>
        /// Shrink so that the entire border is visible
        /// </summary>
        Shrink,
        /// <summary>
        /// The entire top and left borders are visible, but none of the bottom or right. Texels scale 1:1.
        /// </summary>
        TopLeft,
        /// <summary>
        /// Half of the border on each side is visible. Texels scale 1:1.
        /// </summary>
        Middle
    }
}
