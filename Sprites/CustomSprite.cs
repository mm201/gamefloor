using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using Gamefloor.Framework;
using Gamefloor.Support;
using OpenTK.Graphics;

namespace Gamefloor.Sprites
{
    public abstract class CustomSprite : IRenderable
    {
        #region Transformations
        /// <summary>
        /// Transformation matrix of this object relative to its parent
        /// </summary>
        public virtual Matrix4 Matrix
        {
            get
            {
                return Matrix4.Identity;
            }
        }

        /// <summary>
        /// Transformation matrix of this object relative to the viewport
        /// </summary>
        public virtual Matrix4 CompositeMatrix
        {
            get
            {
                return (Parent == null) ? Matrix : Parent.CompositeMatrix * Matrix;
            }
        }

        /// <summary>
        /// In sprite coordinate space, the sprite extends from (0, 0) to (Size.Width, Size.Height)
        /// These are the coordinates passed onto click events.
        /// </summary>
        public virtual System.Drawing.SizeF Size
        {
            get
            {
                return new System.Drawing.SizeF(1f, 1f);
            }
            set { }
        }
        #endregion

        #region Parent
        public SpriteCollection Parent
        {
            get
            {
                return m_parent;
            }
            set
            {
                if (m_parent != null) m_parent.Remove(this);
                m_parent = value;
                if (value != null) value.Add(this);
            }
        }

        private SpriteCollection m_parent = null;

        #endregion

        #region Hit handling
        /// <summary>
        /// Provides info whether this object can handle mouse clicks or if an object below should be able to handle them
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public virtual HitResult HitTest(System.Drawing.PointF location)
        {
            // todo: touch scrollable views will need to do something funny
            // with click event bubbling and hit tests
            return HitResult.Clickable;
        }

        /// <summary>
        /// Raises the click event.
        /// If you override this, remember to call the base implementation or raise the OnClick event.
        /// </summary>
        /// <param name="location"></param>
        public virtual void Click(System.Drawing.PointF location)
        {
            if (OnClick != null) OnClick(this, new ClickEventArgs(location));
        }

        public event EventHandler<ClickEventArgs> OnClick;

        #endregion

        #region IRenderable implementation
        public virtual void Render(IGraphicsContext context, bool reverse)
        {
        }

        public virtual Priority DrawOrder
        {
            get
            {
                return new Priority(0, 0, 0, 0);
            }
        }

        public virtual bool Reversible
        {
            get
            {
                return false;
            }
        }

        public virtual bool DrawFirst
        {
            get
            {
                return false;
            }
        }
        #endregion
    }

    public enum HitResult
    {
        /// <summary>
        /// Does not handle clicks
        /// </summary>
        Transparent = 0,
        /// <summary>
        /// Does handle clicks
        /// </summary>
        Clickable = 1
    }

    public class ClickEventArgs : EventArgs
    {
        public readonly System.Drawing.PointF Location;

        public ClickEventArgs(System.Drawing.PointF location)
        {
            Location = location;
        }
    }
}
