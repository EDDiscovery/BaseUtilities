/*
 * Copyright © 2019 Robbyxp1 @ github.com
 * Part of the EDDiscovery Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

// Rules - no winforms in Control land except for ControlForm which intefaces with GLControl
 
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public struct Padding
    {
        public int Left; public int Top; public int Right; public int Bottom;
        public Padding(int left, int top, int right, int bottom) { Left = left; Top = top; Right = right; Bottom = bottom; }
        public Padding(int pad = 0) { Left = pad; Top = pad; Right = pad; Bottom = pad; }
        public int TotalWidth { get { return Left + Right; } }
        public int TotalHeight { get { return Top + Bottom; } }

        public static bool operator ==(Padding l, Padding r) { return l.Left == r.Left && l.Right == r.Right && l.Top == r.Top && l.Bottom == r.Bottom; }
        public static bool operator !=(Padding l, Padding r) { return !(l.Left == r.Left && l.Right == r.Right && l.Top == r.Top && l.Bottom == r.Bottom); }
        public override bool Equals(Object other) { return other is Padding && this == (Padding)other; }
        public override int GetHashCode() { return base.GetHashCode(); }
    };

    public struct Margin
    {
        public int Left; public int Top; public int Right; public int Bottom;
        public Margin(int left, int top, int right, int bottom) { Left = left; Top = top; Right = right; Bottom = bottom; }
        public Margin(int pad = 0) { Left = pad; Top = pad; Right = pad; Bottom = pad; }
        public int TotalWidth { get { return Left + Right; } }
        public int TotalHeight { get { return Top + Bottom; } }

        public static bool operator ==(Margin l, Margin r) { return l.Left == r.Left && l.Right == r.Right && l.Top == r.Top && l.Bottom == r.Bottom; }
        public static bool operator !=(Margin l, Margin r) { return !(l.Left == r.Left && l.Right == r.Right && l.Top == r.Top && l.Bottom == r.Bottom); }
        public override bool Equals(Object other) { return other is Margin && this == (Margin)other; }
        public override int GetHashCode() { return base.GetHashCode(); }
    };

    public enum CheckState { Unchecked, Checked, Indeterminate };

    public enum Appearance
    {
        Normal = 0,
        Button = 1
    }

    public abstract class GLBaseControl
    {
        // co-ords are in parent control positions

        public string Name { get; set; } = "?";

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

        public enum DockingType { None, Left, Right, Top, Bottom, Fill, Center };
        public DockingType Dock { get { return docktype; } set { if (docktype != value) { docktype = value; InvalidateLayoutParent(); } } }
        public float DockPercent { get { return dockpercent; } set { if (value != dockpercent) { dockpercent = value; InvalidateLayoutParent(); } } }        // % in 0-1 terms used to dock on left,top,right,bottom.  0 means just use width/height

        public GLBaseControl Parent { get { return parent; } }
        public Font Font { get { return fnt ?? parent.Font; } set { fnt = value; InvalidateLayoutParent(); } }

        public Color BackColor { get { return backcolor; } set { if (backcolor != value) { backcolor = value; Invalidate(); } } }
        public Color BorderColor { get { return bordercolor; } set { if (bordercolor != value) { bordercolor = value; Invalidate(); } } }
        public int BorderWidth { get { return borderwidth; } set { if (borderwidth != value) { borderwidth = value; InvalidateLayoutParent(); } } }
        public GL4.Controls.Padding Padding { get { return padding; } set { if (padding != value) { padding = value; InvalidateLayoutParent(); } } }
        public GL4.Controls.Margin Margin { get { return margin; } set { if (margin != value) { margin = value; InvalidateLayoutParent(); } } }

        public bool Enabled { get { return enabled; } set { if (enabled != value) { enabled = value; Invalidate(); } } }

        public bool AutoSize { get { return autosize; } set { if (autosize != value) { autosize = value; InvalidateLayoutParent(); } } }

        public virtual bool Focused { get { return false; } }

        public int Row { get { return row; } set { row = value; InvalidateLayoutParent(); } }
        public int Column { get { return column; } set { column = value; InvalidateLayoutParent(); } }

        public bool InvalidateOnEnterLeave { get; set; } = false;       // if set, invalidate on enter/leave to force a redraw
        public bool InvalidateOnMouseDownUp { get; set; } = false;      // if set, invalidate on mouse button down/up to force a redraw

        public bool Hover { get; set; } = false;            // mouse is over control
        public MouseEventArgs.MouseButtons MouseButtonsDown { get; set; } // set if mouse buttons down over control

        public GLForm FindForm() { return this is GLForm ? this as GLForm: parent?.FindForm(); }

        public Action<Object, MouseEventArgs> MouseDown { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseUp { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseMove { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseEnter { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseLeave { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseClick { get; set; } = null;
        public Action<Object, MouseEventArgs> MouseWheel { get; set; } = null;
        public Action<Object, KeyEventArgs> KeyDown { get; set; } = null;
        public Action<Object, KeyEventArgs> KeyUp { get; set; } = null;

        public Bitmap GetBitmap() { return levelbmp ?? parent.GetBitmap(); }

        public void Invalidate()
        {
            System.Diagnostics.Debug.WriteLine("Invalidate " + Name);
            NeedRedraw = true;
            var f = FindForm();
            if (f != null)
                f.RequestRender = true;
        }

        public void InvalidateLayoutParent()
        {
            System.Diagnostics.Debug.WriteLine("Invalidate Layout Parent " + Name);
            NeedRedraw = true;
            if (parent != null)
            {
                var f = FindForm();
                if (f != null)
                    f.RequestRender = true;
                System.Diagnostics.Debug.WriteLine(".. Redraw and layout on " + Parent.Name);
                parent.NeedRedraw = true;
                parent.PerformLayout();
            }
        }

        public void Add(GLBaseControl other)
        {
            other.parent = this;
            children.Add(other);

            if (this is GLForm) // if adding to a form, the child must have a bitmap
                other.levelbmp = new Bitmap(other.Width, other.Height);

            Invalidate();           // we are invalidated
            PerformLayout();        // reperform layout
        }

        public void Remove(GLBaseControl other)
        {
            if (other.levelbmp != null)
                other.levelbmp.Dispose();

            children.Remove(other);

            Invalidate();
            PerformLayout();        // reperform layout
        }

        public Point FormCoords()       // co-ordinates in the Form, not the screen
        {
            Point p = Location;
            GLBaseControl b = this;
            while (b.Parent != null)
            {
                p = new Point(p.X + b.parent.Left, p.Y + b.parent.Top);
                b = b.parent;
            }

            return p;
        }

        public GLBaseControl FindControlOver(Point p)       // p = form co-ords
        {
            if (p.X < Left || p.X > Right || p.Y < Top || p.Y > Bottom)
                return null;

            foreach (GLBaseControl c in children)
            {
                var r = c.FindControlOver(new Point(p.X - Left, p.Y - Top));   // find, converting co-ords into child co-ords
                if (r != null)
                    return r;
            }

            return this;
        }

        public void SuspendLayout()
        {
            SuspendLayoutSet = true;
        }

        public void ResumeLayout()
        {
            SuspendLayoutSet = false;
            if (NeedLayout)
            {
                System.Diagnostics.Debug.WriteLine("Resumed layout on " + Name);
                PerformLayout();
                NeedLayout = false;
            }
        }

        ////////////// imp

        protected void SetPos(int left, int top, int width, int height) // change window rectangle
        {
            Rectangle w = new Rectangle(left, top, width, height);

            if (w != window)        // if changed
            {
                window = w;

                NeedRedraw = true;      // we need a redraw
                System.Diagnostics.Debug.WriteLine("setpos need redraw on " + Name);
                parent?.Invalidate();   // parent is invalidated as well, and the whole form needs reendering

                if (!InAutosize)        // if not in autosize, then we need to perform a layout
                {
                    parent?.PerformLayout();     // go up one and perform layout on all its children, since we are part of it.
                }
            }
        }

        protected bool NeedRedraw { get; set; } = true;         // we need to redraw, therefore all children also redraw
        protected bool InAutosize { get; set; } = false;        // changing size in autosize 
        protected bool NeedLayout { get; set; } = false;        // need a layout after suspend layout was called
        protected bool SuspendLayoutSet { get; set; } = false;        // suspend layout is on
        protected bool enabled = true;
        protected Bitmap levelbmp;       // set if the level has a new bitmap.  Controls under Form always does. Other ones may if they scroll
        protected Rectangle window;       // total area owned, in parent co-ords
        private DockingType docktype { get; set; }  = DockingType.None;
        private float dockpercent { get; set; } = 0;
        private Color backcolor { get; set; } = Color.Transparent;
        private Color bordercolor { get; set; } = Color.Transparent;         // Margin - border - padding is common to all controls. Area left is control area to draw in
        private int borderwidth { get; set; } = 1;
        private GL4.Controls.Padding padding { get; set; }
        private GL4.Controls.Margin margin { get; set; }
        private bool autosize { get; set; }
        protected int column { get; set; } = 0;     // for table layouts
        protected int row { get; set; } = 0;        // for table layouts

        private GLBaseControl parent { get; set; } = null;       // its parent, null if top of top
        protected List<GLBaseControl> children;   // its children
        private Font fnt;

        public GLBaseControl(GLBaseControl p = null)
        {
            parent = p;
            children = new List<GLBaseControl>();
            window = new Rectangle(0, 0, 100, 100);
        }

        public virtual void PerformLayout()     // override for other layouts
        {
            if (SuspendLayoutSet)
            {
                NeedLayout = true;
                System.Diagnostics.Debug.WriteLine("Suspend layout on " + Name);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Perform layout on " + Name);

                foreach (var c in children)         // first let all children autosize
                    PerformSizeChildren(c);

                Rectangle area = AdjustByPaddingBorderMargin(new Rectangle(0, 0, Width, Height));

                foreach (var c in children)
                {
                    area = c.Layout(area);
                    c.PerformLayout();
                }
            }
        }

        protected Rectangle AdjustByPaddingBorderMargin(Rectangle area)
        {
            int bs = BorderColor != Color.Transparent ? BorderWidth : 0;
            return new Rectangle(area.Left + Margin.Left + Padding.Left + bs,
                                    area.Top + Margin.Top + Padding.Top + bs,
                                    area.Width - Margin.TotalWidth - Padding.TotalWidth - bs * 2,
                                    area.Height - Margin.TotalHeight - Padding.TotalHeight - bs * 2);
        }

        public virtual Rectangle Layout(Rectangle area)      // layout yourself inside the area, return area left.
        {
            int ws = DockPercent>0 ? ((int)(area.Width * DockPercent)) : window.Width;
            ws = Math.Min(ws, area.Width);
            int hs = DockPercent>0 ? ((int)(area.Height * DockPercent)) : window.Height;
            hs = Math.Min(hs, area.Height);

            Rectangle oldwindow = window;
            Rectangle areaout = area;

            if (docktype == DockingType.Fill)
            {
                window = area;
                areaout = new Rectangle(0, 0, 0, 0);
            }
            else if (docktype == DockingType.Center)
            {
                int xcentre = (area.Left + area.Right) / 2;
                int ycentre = (area.Top + area.Bottom) / 2;
                Width = Math.Min(area.Width, Width);
                Height = Math.Min(area.Height, Height);
                window = new Rectangle(xcentre - Width / 2, ycentre - Height / 2, Width, Height);       // centre in area, bounded by area, no change in area in
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
                if ( levelbmp != null && oldwindow.Size != window.Size) // if window size changed
                {
                    levelbmp.Dispose();
                    levelbmp = new Bitmap(Width, Height);       // occurs for controls directly under form
                }
            }

            return areaout;
        }

        public void PerformSizeChildren(GLBaseControl c)
        {
            c.InAutosize = true;            // this flag stops reentrancy due to size changes
            c.PerformSize();
            c.InAutosize = false;
        }

        public virtual void PerformSize()        // see if you want to resize
        {
        }

        public virtual void DebugWhoWantsRedraw()
        {
            if (NeedRedraw)
                System.Diagnostics.Debug.WriteLine("Debug Redraw Flag "+ Name);

            foreach (var c in children)
                c.DebugWhoWantsRedraw();
        }

        public virtual bool Redraw(Bitmap usebmp, Rectangle area, Graphics gr, bool forceredraw )
        {
            if (levelbmp != null)                               // bitmap on this level, use it for the children
            {
                usebmp = levelbmp;
                area = new Rectangle(0, 0, Width, Height);      // restate area in terms of bitmap
                gr = Graphics.FromImage(usebmp);        // get graphics for it
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            }

            bool redrawn = false;

            if (NeedRedraw || forceredraw)          // if we need a redraw, or we are forced to draw by a parent redrawing above us.
            {
                System.Diagnostics.Debug.WriteLine("Redraw {0} to {1} nr {2} fr {3}", Name, area, NeedRedraw, forceredraw);

                if (BackColor != Color.Transparent)
                {
                    using (Brush b = new SolidBrush(BackColor))
                        gr.FillRectangle(b, area);
                }

                if (BorderColor != Color.Transparent)
                {
                    Rectangle rectarea = new Rectangle(area.Left + Margin.Left,
                                                    area.Top + Margin.Top,
                                                    area.Width - Margin.TotalWidth - 1,
                                                    area.Height - Margin.TotalHeight - 1);

                    using (var p = new Pen(BorderColor, BorderWidth))
                    {
                        gr.DrawRectangle(p, rectarea);
                    }
                }

                foreach (var c in children)
                {
                    Rectangle controlarea = new Rectangle(area.Left + c.Left,
                                                            area.Top + c.Top,
                                                            c.Width, c.Height);
                    // child, redraw using this bitmap, in this area of the bitmap
                    c.Redraw(usebmp, controlarea, gr, true);
                }

                Paint(usebmp, area, gr);

                NeedRedraw = false;
                redrawn = true;
            }
            else
            {                                                           // we don't require a redraw, but the children might
                foreach (var c in children)
                {
                    Rectangle controlarea = new Rectangle(area.Left + c.Left,
                                                            area.Top + c.Top,
                                                            c.Width, c.Height);
                    // child, redraw using this bitmap, in this area of the bitmap
                    redrawn |= c.Redraw(usebmp, controlarea, gr, false);
                }
            }

            if (levelbmp != null)                               // bitmap on this level, we made a GR, dispose
                gr.Dispose();

            return redrawn;
        }


        // overrides

        public virtual void Paint(Bitmap bmp, Rectangle area, Graphics gr)
        {
            System.Diagnostics.Debug.WriteLine("Paint {0} to {1}", Name, area);
        }

        public virtual void OnMouseLeave(MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("leave " + Name + " " + e.Location);
            MouseLeave?.Invoke(this,e);

            if (InvalidateOnEnterLeave)
                Invalidate();
        }
        public virtual void OnMouseEnter(MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("enter " + Name + " " + e.Location);
            MouseEnter?.Invoke(this,e);

            if (InvalidateOnEnterLeave)
                Invalidate();
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("up   " + Name + " " + e.Location + " " + e.Button);
            MouseUp?.Invoke(this,e);

            if (InvalidateOnMouseDownUp)
                Invalidate();
        }

        public virtual void OnMouseDown(MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("down " + Name + " " + e.Location +" " + e.Button);
            MouseDown?.Invoke(this,e);

            if (InvalidateOnMouseDownUp)
            {
                Invalidate();
            }
        }

        public virtual void OnMouseClick(MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("click " + Name + " " + e.Button + " " + e.Clicks + " " + e.Location );
            MouseClick?.Invoke(this,e);
        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Over " + Name + " " + e.Location);
            MouseMove?.Invoke(this, e);
        }

        public virtual void OnMouseWheel(MouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Over " + Name + " " + e.Location);
            MouseWheel?.Invoke(this, e);
        }

        public virtual void OnKeyDown(KeyEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Over " + Name + " " + e.Location);
            KeyDown?.Invoke(this, e);
        }

        public virtual void OnKeyUp(KeyEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Over " + Name + " " + e.Location);
            KeyUp?.Invoke(this, e);
        }
    }
}
