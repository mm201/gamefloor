using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace Gamefloor.Sprites
{
    public abstract class RotozoomSprite : CustomSprite
    {
        // todo: implement own matrix as a combination of rotation, translation, and scale transforms.
        #region CustomSprite implementation
        public override Matrix4 Matrix
        {
            get
            {
                // todo: implement for real
                return Matrix4.Identity;
            }
        }
        #endregion
    }
}
