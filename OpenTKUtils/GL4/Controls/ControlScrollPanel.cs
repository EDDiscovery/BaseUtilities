using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    // Scroll panel
    // must not be a child of GLForm as it needs a bitmap to paint into

    public class GLVerticalScrollPanel : GLPanel
    {
        public GLVerticalScrollPanel(string name, Rectangle location, Color back) : base(name, location, back)
        {
        }

        public GLVerticalScrollPanel() : this("VSP?", DefaultWindowRectangle, DefaultBackColor)
        {
        }

        public int ScrollRange { get { return (LevelBitmap != null) ? (LevelBitmap.Height - Height) : 0; } }
        public int ScrollPos { get { return scrollpos; } set { SetScrollPos(value); } }
        private int scrollpos = 0;

        // Width/Height is size of the control without scrolling
        // we layout the children within that area.
        // but if we have areas outside that, the bitmap is expanded to cover it

        public override void PerformRecursiveLayout()
        {
            base.PerformRecursiveLayout();               // layout the children

            bool needbitmap = false;

            if (children.Count > 0)
            {
                Rectangle r = ChildArea();
                int childheight = r.Bottom;

                needbitmap = childheight > Height;

                if (needbitmap)
                {
                    if (LevelBitmap == null )
                    {
                        System.Diagnostics.Debug.WriteLine("Make SP bitmap " + Width + "," + childheight);
                        SetLevelBitmap(Width, childheight);
                    }
                    else if ( childheight != LevelBitmap.Height || LevelBitmap.Width != Width) // if height is different, or width is different
                    {
                        SetLevelBitmap(Width, childheight);
                        System.Diagnostics.Debug.WriteLine("Make SP bitmap " + Width + "," + childheight);
                    }
                }
            }

            if ( !needbitmap && LevelBitmap != null)
            {
                SetLevelBitmap(0,0);
            }
        }

        public override void CheckBitmapAfterLayout()       // do nothing, we do not resize bitmap just because our client size has changed
        {
        }

        private void SetScrollPos(int value)
        {
            if (LevelBitmap != null)
            {
                int maxsp = LevelBitmap.Height - Height;
                scrollpos = Math.Max(0, Math.Min(value, maxsp));
                System.Diagnostics.Debug.WriteLine("ScrollPanel scrolled to " + scrollpos);
                Invalidate();
            }
        }

        // only will be called if we have a bitmap defined..

        protected override void PaintParent(Rectangle parentarea, Graphics parentgr)
        {
            System.Diagnostics.Debug.WriteLine("Scroll panel {0} parea {1} Bitmap {2}", Name, parentarea, LevelBitmap.Size);

            parentgr.DrawImage(LevelBitmap, parentarea.Left, parentarea.Top, new Rectangle(0, scrollpos, Width, Height), GraphicsUnit.Pixel);
        }
    }
}

