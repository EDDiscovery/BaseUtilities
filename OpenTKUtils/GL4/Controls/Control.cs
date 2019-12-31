using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public abstract class GLBaseControl
    {
        // co-ords are in parent control positions
        
        public int Left { get { return window.Left; } set { SetPos(value, window.Top, window.Width, window.Height); } }
        public int Right { get { return window.Right; } set { SetPos(window.Left, window.Top, value - window.Left, window.Height); } }
        public int Top { get { return window.Top; } set { SetPos(window.Left, value, window.Width, window.Height); } }
        public int Bottom { get { return window.Bottom; } set { SetPos(window.Left, window.Top, window.Width, value - window.Top); } }
        public int Width { get { return window.Width; } set { SetPos(window.Left, window.Top, value, window.Height); } }
        public int Height { get { return window.Height; } set { SetPos(window.Left, window.Top, window.Width, value); } }
        public Rectangle Position { get { return window; } set { SetPos(value.Left, value.Top, value.Width, value.Height); } }
        public Point Location { get { return new Point(window.Left, window.Top); } set { SetPos(value.X, value.Y, window.Width, window.Height); } }
        public Size Size { get { return new Size(window.Width, window.Height); } set { SetPos(window.Left, window.Top, value.Width, value.Height); } }
        public Rectangle ClientRectangle { get { return window; } }

        public enum DockingType { None, Left, Right, Top, Bottom, Fill };
        public DockingType Dock { get { return docktype; } set { docktype = value; } } // parent?.PerformLayout(); } }
        public float DockPercent { get; set; } = 0.0f;        // % in 0-1 terms used to dock on left,top,right,bottom.  0 means just use width/height

        public GLBaseControl Parent { get { return parent; } }
        public Font Font { get { return fnt ?? parent?.fnt; } set { fnt = value; } }

        public string Name { get; set; } = "?";

        public Color BackColor { get; set; } = Color.Transparent;

        public bool NeedRedraw { get; set; } = true;        

        // imp

        protected void SetPos(int left, int top, int width, int height, bool performlayout = true)
        {
            window = new Rectangle(left, top, width, height);
            //if ( performlayout )
            //    parent?.PerformLayout();        // go up one and perform layout on all its children, since we are part of it.
        }

        public Bitmap GetBitmap() { return levelbmp ?? parent.GetBitmap(); }
//        protected GLBaseControl FindTopControl() { return parent is GLForm ? this : parent.FindTopControl(); }
      //  protected GLBaseControl FindForm() { return this is GLForm ? this : parent.FindForm(); }

        protected Bitmap levelbmp;       // set if the level has a new bitmap.  Controls under Form always does. Other ones may if they scroll

        private Rectangle window;       // total area owned, in parent co-ords
        private DockingType docktype = DockingType.None;
        private GLBaseControl parent;       // its parent, null if top of top
        protected List<GLBaseControl> children;   // its children
        private Font fnt;

        public GLBaseControl(GLBaseControl p = null)
        {
            parent = p;
            children = new List<GLBaseControl>();
            window = new Rectangle(0, 0, 100, 100);
        }

        public virtual void PerformLayout()
        {
            Rectangle area = new Rectangle(0, 0, Width, Height);

            foreach (var c in children)
            {
                c.AdjustSize(area);
            }

            foreach (var c in children)
            {
                area = c.Layout(area);
                c.PerformLayout();
            }
        }

        protected virtual Rectangle Layout(Rectangle area)
        {
            int ws = DockPercent>0 ? ((int)(area.Width * DockPercent)) : window.Width;
            ws = Math.Min(ws, area.Width);
            int hs = DockPercent>0 ? ((int)(area.Height * DockPercent)) : window.Height;
            hs = Math.Min(hs, area.Height);

            Rectangle oldwindow = window;
            Rectangle areaout = area;

            if ( docktype == DockingType.Fill )
            {
                window = area;
                areaout = new Rectangle(0, 0, 0, 0);
            }
            else if (docktype == DockingType.Left)
            {
                window = new Rectangle(area.Left, area.Top, ws, area.Height);
                areaout = new Rectangle(area.Left+ws, area.Top, area.Width - ws, area.Height);
            }
            else if (docktype == DockingType.Right)
            {
                window = new Rectangle(area.Right-ws, area.Top, ws, area.Height);
                areaout = new Rectangle(window.Left, area.Top, area.Width - window.Width, area.Height);
            }
            else if (docktype == DockingType.Top)
            {
                window = new Rectangle(area.Left, area.Top, area.Width, hs);
                areaout =  new Rectangle(window.Left, area.Top + hs, area.Width, area.Height - hs);
            }
            else if (docktype == DockingType.Bottom)
            {
                window = new Rectangle(area.Left, area.Bottom-hs, area.Width, hs);
                areaout =  new Rectangle(window.Left, area.Top, area.Width, area.Height - hs);
            }

            System.Diagnostics.Debug.WriteLine("{0} in {1} out {2} dock {3} win {4}", Name, area, areaout , Dock, window);
            if ( oldwindow != window )
            {
                NeedRedraw = true;      // set flag to say need redraw
            }

            return areaout;
        }

        public virtual void Redraw(Bitmap usebmp, Rectangle area )
        {
            if (usebmp == null || levelbmp != null)             // if no bitmap, or we have a level bmp
            {
                if (levelbmp == null)                           // if we are a level where we have no basebmp, make one
                    levelbmp = new Bitmap(Width, Height);       // occurs for controls directly under form

                usebmp = levelbmp;
                area = new Rectangle(0, 0, Width, Height);      // restate area in terms of bitmap
            }

            System.Diagnostics.Debug.WriteLine("Redraw {0} to {1}", Name, area);

            if (BackColor != Color.Transparent)
            {
                using (Graphics gr = Graphics.FromImage(usebmp))
                {
                    gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    using (Brush b = new SolidBrush(BackColor))
                        gr.FillRectangle(b, area);
                }

            }

            foreach (var c in children)
            {
                // child, redraw using this bitmap, in this area of the bitmap
                c.Redraw(usebmp,new Rectangle(area.Left + c.Left, area.Top+c.Top,c.Width,c.Height));
            }

            Paint(usebmp, area);
            NeedRedraw = false;
        }

        public virtual void Paint(Bitmap bmp, Rectangle area)
        {
            System.Diagnostics.Debug.WriteLine("Paint {0} to {1}", Name, area);
        }

        public virtual void AdjustSize(Rectangle area)
        {
        }

        public void Add(GLBaseControl other)
        {
            children.Add(other);
            this.NeedRedraw |= other.NeedRedraw;
        }

        public void Remove(GLBaseControl other)
        {
            children.Remove(other);
            this.NeedRedraw |= other.NeedRedraw;
        }

    }
}
