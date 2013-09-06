using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics;
using Gamefloor.Framework;

namespace Gamefloor.Sprites
{
    public class SpriteCollection : CustomSprite
    {
        #region Construction
        public SpriteCollection()
        {
            Initialize(null);
        }

        public SpriteCollection(IGraphicsContext context)
        {
            Initialize(context);
        }

        public SpriteCollection(Game game) : this(game.Context)
        {
        }

        private void Initialize(IGraphicsContext context)
        {
        }
        #endregion

        #region Collection management
        public void Add(CustomSprite sprite)
        {
            throw new NotImplementedException();
        }

        public void Remove(CustomSprite customSprite)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region CustomSprite implementation
        public override OpenTK.Matrix4 Matrix
        {
            get { throw new NotImplementedException(); }
        }

        public override HitResult HitTest(System.Drawing.PointF location)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
