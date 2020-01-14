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
        public GLVerticalScrollPanel()
        {
        }

        public GLVerticalScrollPanel(string name, Rectangle location, Color back) : base(name, location, back)
        {
        }

        public int ScrollRange { get { return (levelbmp != null) ? (levelbmp.Height - Height) : 0; } }
        public int ScrollPos { get { return scrollpos; } set { SetScrollPos(value); } }
        private int scrollpos = 0;

        // Width/Height is size of the control without scrolling
        // we layout the children within that area.
        // but if we have areas outside that, the bitmap is expanded to cover it

        public override void PerformLayout()
        {
            base.PerformLayout();               // layout the children

            bool needbitmap = false;

            if (children.Count > 0)
            {
                Rectangle r = ChildArea();
                int childheight = r.Bottom;

                needbitmap = childheight > Height;

                if (needbitmap)
                {
                    if (levelbmp == null )
                    {
                        System.Diagnostics.Debug.WriteLine("Make SP bitmap " + Width + "," + childheight);
                        levelbmp = new Bitmap(Width, childheight);
                    }
                    else if ( childheight != levelbmp.Height || levelbmp.Width != Width) // if height is different, or width is different
                    {
                        levelbmp.Dispose();
                        levelbmp = new Bitmap(Width, childheight);
                        System.Diagnostics.Debug.WriteLine("Make SP bitmap " + Width + "," + childheight);
                    }
                }
            }

            if ( !needbitmap && levelbmp != null)
            {
                levelbmp.Dispose();
                levelbmp = null;
            }
        }

        private void SetScrollPos(int value)
        {
            if (levelbmp != null)
            {
                int maxsp = levelbmp.Height - Height;
                scrollpos = Math.Max(0, Math.Min(value, maxsp));
                System.Diagnostics.Debug.WriteLine("ScrollPanel scrolled to " + scrollpos);
                Invalidate();
            }
        }

        // only will be called if we have a bitmap defined..

        public override void PaintParent(Rectangle parentarea, Graphics parentgr)
        {
            System.Diagnostics.Debug.WriteLine("Scroll panel {0} parea {1} Bitmap {2}", Name, parentarea, levelbmp.Size);

            parentgr.DrawImage(levelbmp, parentarea.Left, parentarea.Top, new Rectangle(0, scrollpos, Width, Height), GraphicsUnit.Pixel);
        }
    }
}

