using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLScrollPanel : GLPanel
    {
        public GLScrollPanel()
        {
        }

        public GLScrollPanel(string name, Rectangle location, Color back) : base(name, location, back)
        {
        }

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
                        levelbmp = new Bitmap(Width, childheight);
                    }
                    else if ( childheight > levelbmp.Height )
                    {
                        levelbmp.Dispose();
                        levelbmp = new Bitmap(Width, childheight);
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
                Invalidate();
            }
        }

        // only will be called if we have a bitmap defined..

        public override void PaintParent(Rectangle parentarea, Graphics parentgr)
        {
            parentgr.DrawImage(levelbmp, parentarea.Left, parentarea.Top, new Rectangle(0, scrollpos, Width, Height), GraphicsUnit.Pixel);
        }
    }
}

