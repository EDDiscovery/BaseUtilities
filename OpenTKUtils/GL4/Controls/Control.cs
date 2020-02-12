/*
 * Copyright 2019-2020 Robbyxp1 @ github.com
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

// Rules - no winforms in Control land except for Keys

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

    public enum DockingType {   None, Fill, Center,
                                Left, LeftCenter, LeftTop, LeftBottom,              // order vital to layout test, keep
                                Right, RightCenter, RightTop, RightBottom,
                                Top, TopCenter, TopLeft, TopRight,
                                Bottom, BottomCentre, BottomLeft, BottomRight,
                              };

    [System.Diagnostics.DebuggerDisplay("Control {Name} {window}")]
    public abstract class GLBaseControl
    {
        #region Main UI
        public string Name { get; set; } = "?";

        // bounds of the window - include all margin/padding/borders/
        // co-ords are in offsets from 0,0 being the parent top left corner.

        public Rectangle Bounds { get { return window; } set { SetPos(value.Left, value.Top, value.Width, value.Height); } }
        public int Left { get { return window.Left; } set { SetPos(value, window.Top, window.Width, window.Height); } }
        public int Right { get { return window.Right; } set { SetPos(window.Left, window.Top, value - window.Left, window.Height); } }
        public int Top { get { return window.Top; } set { SetPos(window.Left, value, window.Width, window.Height); } }
        public int Bottom { get { return window.Bottom; } set { SetPos(window.Left, window.Top, window.Width, value - window.Top); } }
        public int Width { get { return window.Width; } set { SetPos(window.Left, window.Top, value, window.Height); } }
        public int Height { get { return window.Height; } set { SetPos(window.Left, window.Top, window.Width, value); } }
        public Point Location { get { return new Point(window.Left, window.Top); } set { SetLocation(value.X, value.Y); } }
        public Size Size { get { return new Size(window.Width, window.Height); } set { SetPos(window.Left, window.Top, value.Width, value.Height); } }

        // padding/margin and border control

        public GL4.Controls.Padding Padding { get { return padding; } set { if (padding != value) { padding = value; InvalidateLayout(); } } }
        public GL4.Controls.Margin Margin { get { return margin; } set { if (margin != value) { margin = value; InvalidateLayout(); } } }
        public void SetMarginBorderWidth(Margin m, int borderw, Color borderc, Padding p) { margin = m; padding = p; bordercolor = borderc; borderwidth = borderw; InvalidateLayout(); }
        public Color BorderColor { get { return bordercolor; } set { if (bordercolor != value) { bordercolor = value; Invalidate(); } } }
        public int BorderWidth { get { return borderwidth; } set { if (borderwidth != value) { borderwidth = value; InvalidateLayout(); } } }

        // this is the client area, inside the margin/padding/border

        public int ClientLeftMargin { get { return Margin.Left + Padding.Left + BorderWidth; } }
        public int ClientRightMargin { get { return Margin.Right + Padding.Right + BorderWidth; } }
        public int ClientTopMargin { get { return Margin.Top + Padding.Top + BorderWidth; } }
        public int ClientBottomMargin { get { return Margin.Bottom + Padding.Bottom + BorderWidth; } }
        public int ClientWidth { get { return Width - Margin.TotalWidth - Padding.TotalWidth - BorderWidth*2; } }
        public int ClientHeight { get { return Height - Margin.TotalHeight - Padding.TotalHeight - BorderWidth * 2; } }
        public Size ClientSize { get { return new Size(ClientWidth, ClientHeight); } }
        public Point ClientLocation { get { return new Point(ClientLeftMargin, ClientTopMargin); } }
        public Rectangle ClientRectangle { get { return new Rectangle(0,0,ClientWidth,ClientHeight); }  }

        // docking control

        public DockingType Dock { get { return docktype; } set { if (docktype != value) { docktype = value; InvalidateLayoutParent(); } } }
        public float DockPercent { get { return dockpercent; } set { if (value != dockpercent) { dockpercent = value; InvalidateLayoutParent(); } } }        // % in 0-1 terms used to dock on left,top,right,bottom.  0 means just use width/height
        public Margin DockingMargin { get { return dockingmargin; } set { if (dockingmargin != value) { dockingmargin = value; InvalidateLayout(); } } }

        // toggle controls
        public bool Enabled { get { return enabled; } set { if (enabled != value) { SetEnabled(value); Invalidate(); } } }
        public bool Visible { get { return visible; } set { if (visible != value) { visible = value; InvalidateLayoutParent(); } } }
        public virtual bool Focused { get { return focused; } }
        public virtual bool Focusable { get { return focusable; } set { focusable = value; } }
        public virtual void SetFocus() { FindDisplay()?.SetFocus(this); }

        // colour font

        public Font Font { get { return font ?? parent?.Font; } set { SetFont(value); InvalidateLayout(); } }
        public void SetDefaultFont() { font = new Font("Microsoft Sans Serif", 8.25f); }
        public Color BackColor { get { return backcolor; } set { if (backcolor != value) { backcolor = value; Invalidate(); } } }
        public int BackColorGradient { get { return backcolorgradient;} set { if ( backcolorgradient != value) { backcolorgradient = value;Invalidate(); } } }
        public Color BackColorGradientAlt { get { return backcolorgradientalt; } set { if (backcolorgradientalt != value) { backcolorgradientalt = value; Invalidate(); } } }

        // other

        public GLBaseControl Parent { get { return parent; } }
        public GLControlDisplay FindDisplay() { return this is GLControlDisplay ? this as GLControlDisplay : parent?.FindDisplay(); }
        public GLBaseControl FindControlUnderDisplay() { return Parent is GLControlDisplay ? this : parent?.FindControlUnderDisplay(); }
        //public GLForm FindForm() { return this is GLForm ? this as GLForm : parent?.FindForm(); }

        public bool AutoSize { get { return autosize; } set { if (autosize != value) { autosize = value; InvalidateLayoutParent(); } } }

        public int Row { get { return row; } set { row = value; InvalidateLayoutParent(); } }       // for table layouts
        public int Column { get { return column; } set { column = value; InvalidateLayoutParent(); } } // for table layouts

        public bool InvalidateOnEnterLeave { get; set; } = false;       // if set, invalidate on enter/leave to force a redraw
        public bool InvalidateOnMouseDownUp { get; set; } = false;      // if set, invalidate on mouse button down/up to force a redraw
        public bool InvalidateOnFocusChange { get; set; } = false;      // if set, invalidate on focus change

        public bool Hover { get; set; } = false;            // mouse is over control
        public GLMouseEventArgs.MouseButtons MouseButtonsDown { get; set; } // set if mouse buttons down over control

        public Bitmap LevelBitmap { get { return levelbmp; } }  // return level bitmap, null if not a bitmap control

        public Object Tag { get; set; }                         // control tag, user controlled

        // control lists

        public virtual List<GLBaseControl> ControlsIZ { get { return childreniz; } }      // read only, in inv zorder, so 0 = last layout first drawn
        public virtual List<GLBaseControl> ControlsOrderAdded { get { return childreniz; } }      // in order added
        public virtual List<GLBaseControl> ControlsZ { get { return childrenz; } }          // read only, in zorder, so 0 = first layout last painted

        // events

        public Action<Object, GLMouseEventArgs> MouseDown { get; set; } = null;  // location in client terms, NonClientArea set if on border with negative/too big x/y for clients
        public Action<Object, GLMouseEventArgs> MouseUp { get; set; } = null;
        public Action<Object, GLMouseEventArgs> MouseMove { get; set; } = null;
        public Action<Object, GLMouseEventArgs> MouseClick { get; set; } = null;
        public Action<Object, GLMouseEventArgs> MouseWheel { get; set; } = null;
        public Action<Object, GLMouseEventArgs> MouseEnter { get; set; } = null;  // location in terms of whole window
        public Action<Object, GLMouseEventArgs> MouseLeave { get; set; } = null;  // location in terms of whole window
        public Action<Object, GLKeyEventArgs> KeyDown { get; set; } = null;
        public Action<Object, GLKeyEventArgs> KeyUp { get; set; } = null;
        public Action<Object, GLKeyEventArgs> KeyPress { get; set; } = null;
        public Action<Object, bool, GLBaseControl> FocusChanged { get; set; } = null;
        public Action<Object> FontChanged { get; set; } = null;
        public Action<Object> Resize { get; set; } = null;
        public Action<Object> Moved { get; set; } = null;
        public Action<GLBaseControl,GLBaseControl> ControlAdd { get; set; } = null;
        public Action<GLBaseControl,GLBaseControl> ControlRemove { get; set; } = null;

        // default color schemes and sizes

        public static Action<GLBaseControl> Themer = null;                 // set this up, will be called during construction with the class for you to theme the colours/options

        static public Color DefaultBackColor = Color.White;
        static public Color DefaultForeColor = Color.Black;
        static public Color DefaultBorderColor = Color.Gray;

        public void Invalidate()
        {
            //System.Diagnostics.Debug.WriteLine("Invalidate " + Name);
            NeedRedraw = true;

            if ( BackColor == Color.Transparent )   // if we are transparent, we need the parent also to redraw to force it to redraw its background.
            {
                //System.Diagnostics.Debug.WriteLine("Invalidate " + Name + " is transparent, parent needs it too");
                Parent?.Invalidate();
            }

            var f = FindDisplay();                  // set the display to request render
            if (f != null)
                f.RequestRender = true;
        }

        public void InvalidateLayout()
        {
            Invalidate();
            PerformLayout();
        }

        public void InvalidateLayoutParent()
        {
            //System.Diagnostics.Debug.WriteLine("Invalidate Layout Parent " + Name);
            if (parent != null)
            {
                var f = FindDisplay();
                if (f != null)
                    f.RequestRender = true;
                //System.Diagnostics.Debug.WriteLine(".. Redraw and layout on " + Parent.Name);
                parent.NeedRedraw = true;
                parent.PerformLayout();
            }
        }

        public Point DisplayControlCoords(bool clienttopleft)       // return in display co-ord terms either the bounds top left or the client rectangle top left
        {
            Point p = Location;     // Left/Top of bounding box
            GLBaseControl b = this;
            while ( b.Parent != null )
            {       // we need to add on the parent left and clientleftmargin, top the same, to move the point up to the next level
                p = new Point(p.X + b.parent.Left + b.parent.ClientLeftMargin, p.Y + b.parent.Top + b.parent.ClientTopMargin);
                b = b.parent;
            }
            if (clienttopleft)
            {
                p.X += ClientLeftMargin;
                p.Y += ClientTopMargin;
            }
            return p;
        }

        public Point ScreenCoords(bool clienttopleft)           // return in windows screen co-ords the top left of the selected control
        {
            Point p = DisplayControlCoords(clienttopleft);
            GLControlDisplay d = FindDisplay();
            Rectangle sp = d.ClientScreenPos;
            return new Point(p.X + sp.Left, p.Y + sp.Top);
        }

        public Rectangle ChildArea()
        {
            int left = int.MaxValue, right = int.MinValue, top = int.MaxValue, bottom = int.MinValue;

            foreach (var c in childrenz)         
            {
                if (c.Left < left)
                    left = c.Left;
                if (c.Right > right)
                    right = c.Right;
                if (c.Top < top)
                    top = c.Top;
                if (c.Bottom > bottom)
                    bottom = c.Bottom;
            }

            return new Rectangle(left, top, right - left, bottom - top);
        }

        public void PerformLayout()     // override for other layouts
        {
            if (suspendLayoutSet)
            {
                needLayout = true;
                //System.Diagnostics.Debug.WriteLine("Suspended layout on " + Name);
            }
            else
            {
                PerformRecursiveSize(Parent?.ClientSize ?? ClientSize);         // we recusively size, from lowest child up
                PerformRecursiveLayout();       // and we layout, top down
            }
        }

        public void SuspendLayout()
        {
            suspendLayoutSet = true;
        }

        public void ResumeLayout()
        {
            //if ( suspendLayoutSet )   System.Diagnostics.Debug.WriteLine("Resume Layout on " + Name);

            suspendLayoutSet = false;
            if (needLayout)
            {
                //System.Diagnostics.Debug.WriteLine("Required layout " + Name);
                PerformLayout();
            }
        }

        public virtual void Add(GLBaseControl other)
        {
            other.parent = this;
            childrenz.Insert(0, other);   // in z order.  First is top of z
            childreniz.Add(other);       // in inv z order. Last is top of z

            if (this is GLControlDisplay) // if adding to a form, the child must have a bitmap
            {
                System.Diagnostics.Debug.Assert(other is GLVerticalScrollPanel == false, "GLScrollPanel must not be child of GLForm");
                other.levelbmp = new Bitmap(other.Width, other.Height);
            }

            OnControlAdd(this, other);
            Invalidate();           // we are invalidated
            PerformLayout();        // reperform layout
        }

        public virtual void Remove(GLBaseControl other)
        {
            if (childrenz.Contains(other))
            {
                OnControlRemove(this, other);

                FindDisplay()?.ControlRemoved(other);

                if (other.levelbmp != null)
                    other.levelbmp.Dispose();

                childrenz.Remove(other);
                childreniz.Remove(other);

                Invalidate();
                PerformLayout();        // reperform layout
            }
        }

        public virtual bool BringToFront()      // bring to the front, true if it was at the front
        {
            return Parent?.BringToFront(this) ?? true;
        }

        public virtual bool BringToFront(GLBaseControl child)   // bring child to front
        {
            if (childrenz.Contains(child) && childrenz[0] != child)
            {
                childreniz.Remove(child);
                childrenz.Remove(child);

                childrenz.Insert(0, child);   // in z order.  First is top of z
                childreniz.Add(child);       // in inv z order. Last is top of z

                Invalidate();
                return false;
            }
            else
                return true;
        }

        #endregion

        #region For Inheritors

        protected GLBaseControl(string name, Rectangle location, Color backcolor)
        {
            this.Name = name;

            if (location.Width == 0 || location.Height == 0)
            {
                location.Width = location.Height = 10;  // nominal
                AutoSize = true;
            }

            this.window = location;
            this.backcolor = backcolor;

            Themer?.Invoke(this);
        }

        static protected readonly Rectangle DefaultWindowRectangle = new Rectangle(0, 0, 10, 10);
        static protected readonly int MinimumResizeWidth = 10;
        static protected readonly int MinimumResizeHeight = 10;

        // these change without invalidation or layout - for constructors of inheritors or for Layout/SizeControl overrides

        protected GL4.Controls.Margin MarginNI { set { margin = value; } }
        protected GL4.Controls.Padding PaddingNI { set { padding = value; } }
        protected int BorderWidthNI { set { borderwidth = value; } }
        protected Color BorderColorNI { set { bordercolor = value; } }
        public bool VisibleNI { set { visible = value; } }

        public void SetLocationSizeNI( Point? location = null, Size? size = null, bool clipsize = false)      // use by inheritors only.  Does not invalidate/Layout.
        {
            Point oldloc = Location;
            Size oldsize = Size;

            if (clipsize)
            {
                size = new Size(Math.Min(Width, size.Value.Width), Math.Min(Height, size.Value.Height));
            }

            if (location.HasValue)
            {
                window.Location = location.Value;

                if (window.Location != oldloc)
                    OnMoved();
            }

            if ( size.HasValue )
            {
                window.Size = size.Value;

                if (oldsize != size.Value)
                    OnResize();
            }
            //System.Diagnostics.Debug.WriteLine("SetPosNI {0}", window);
        }

        protected void SetLevelBitmap(int width , int height)
        {
            levelbmp?.Dispose();
            levelbmp = null;
            if (width > 0 && height > 0)
                levelbmp = new Bitmap(width, height);
        }

        protected GLBaseControl FindControlOver(Point p)       // p = form co-coords, finds including margin/padding/border area, so inside bounds
        {
            //System.Diagnostics.Debug.WriteLine("Find " + Name + " "  + p + " in " + Bounds + " " + ClientLeftMargin + " " + ClientTopMargin);
            if (p.X < Left || p.X > Right || p.Y < Top || p.Y > Bottom)     
                return null;

            foreach (GLBaseControl c in childrenz)       // in Z order
            {
                if (c.Visible)      // must be visible to be found..
                {
                    var r = c.FindControlOver(new Point(p.X - Left - ClientLeftMargin, p.Y - Top - ClientTopMargin));   // find, converting co-ords into child co-ords
                    if (r != null)
                        return r;
                }
            }

            return this;
        }

        #endregion

        #region Overridables

        // first,perform recursive sizing. do children first, then do us
        // pass in the parent size of client rectangle to each size to give them a hint what they can autosize into

        protected virtual void PerformRecursiveSize(Size parentclientrect)   
        {
            int width = (Dock == DockingType.Top || Dock == DockingType.Bottom) ? (parentclientrect.Width-DockingMargin.TotalWidth) : ClientWidth;
            int height = (Dock == DockingType.Left || Dock == DockingType.Right) ? (parentclientrect.Height-DockingMargin.TotalHeight) : ClientHeight;
            Size estsize = new Size(width, height);

            //System.Diagnostics.Debug.WriteLine("Size " + Name + " Estsize " + estsize);
            foreach (var c in childrenz) // in Z order
            {
                if (c.Visible)      // invisible children don't layout
                {
                    c.PerformRecursiveSize(estsize);
                }
            }

            SizeControl(estsize);              // size ourselves after children sized
        }

        // override to auto size. Only use the NI functions to change size.  size is size of parent before layout occurs, but takes into account docking.

        protected virtual void SizeControl(Size parentclientrect)        
        {
            //System.Diagnostics.Debug.WriteLine("..Size " + Name + " area est is " + parentclientrect);
        }

        // second, layout after sizing, layout children.  We are layedout by parent, and lay out our children inside our client rectangle

        public virtual void PerformRecursiveLayout()     // go down the tree.  
        {
            //System.Diagnostics.Debug.WriteLine("Laying out " + Name);
            Rectangle area = ClientRectangle;

            foreach (var c in childrenz)     // in z order, top gets first go
            {
                if (c.Visible)      // invisible children don't layout
                {
                    c.Layout(ref area);
                    c.PerformRecursiveLayout();
                }
            }

            //System.Diagnostics.Debug.WriteLine("Finished Laying out " + Name);

            //if (suspendLayoutSet)  System.Diagnostics.Debug.WriteLine("Removing suspend on " + Name);

            suspendLayoutSet = false;   // we can't be suspended
            needLayout = false;     // we have layed out
        }

        // standard layout function, layout yourself inside the area, return area left.
        public virtual void Layout(ref Rectangle parentarea)     
        {
            //System.Diagnostics.Debug.WriteLine("Control " + Name + " " + window + " " + Dock);
            int dockedwidth = DockPercent > 0 ? ((int)(parentarea.Width * DockPercent)) : (window.Width);
            int dockedheight = DockPercent > 0 ? ((int)(parentarea.Height * DockPercent)) : (window.Height);
            int wl = Width;
            int hl = Height;

            Rectangle oldwindow = window;
            Rectangle areaout = parentarea;

            if (docktype == DockingType.Fill)
            {
                window = parentarea;
                areaout = new Rectangle(0, 0, 0, 0);
            }
            else if (docktype == DockingType.Center)
            {
                int xcentre = (parentarea.Left + parentarea.Right) / 2;
                int ycentre = (parentarea.Top + parentarea.Bottom) / 2;
                Width = Math.Min(parentarea.Width, Width);
                Height = Math.Min(parentarea.Height, Height);
                window = new Rectangle(xcentre - Width / 2, ycentre - Height / 2, Width, Height);       // centre in area, bounded by area, no change in area in
            }
            else if (docktype == DockingType.None)
            {
            }
            else if (docktype >= DockingType.Bottom)
            {
                if (docktype == DockingType.Bottom)
                    window = new Rectangle(parentarea.Left + dockingmargin.Left, parentarea.Bottom - dockedheight - dockingmargin.Bottom, parentarea.Width - dockingmargin.TotalWidth, dockedheight);
                else if (docktype == DockingType.BottomCentre)
                    window = new Rectangle(parentarea.Left + parentarea.Width / 2 - wl / 2, parentarea.Bottom - dockedheight - dockingmargin.Bottom, wl, dockedheight);
                else if (docktype == DockingType.BottomLeft)
                    window = new Rectangle(parentarea.Left + dockingmargin.Left, parentarea.Bottom - dockedheight - dockingmargin.Bottom, wl, dockedheight);
                else // bottomright
                    window = new Rectangle(parentarea.Right - dockingmargin.Right - wl, parentarea.Bottom - dockedheight - dockingmargin.Bottom, wl, dockedheight);

                areaout = new Rectangle(parentarea.Left, parentarea.Top, parentarea.Width, parentarea.Height - dockedheight - dockingmargin.TotalWidth);
            }
            else if (docktype >= DockingType.Top)
            {
                if (docktype == DockingType.Top)
                    window = new Rectangle(parentarea.Left + dockingmargin.Left, parentarea.Top + dockingmargin.Top, parentarea.Width - dockingmargin.TotalWidth, dockedheight);
                else if (docktype == DockingType.TopCenter)
                    window = new Rectangle(parentarea.Left + parentarea.Width / 2 - wl / 2, parentarea.Top + dockingmargin.Top, wl, dockedheight);
                else if (docktype == DockingType.TopLeft)
                    window = new Rectangle(parentarea.Left + dockingmargin.Left, parentarea.Top + dockingmargin.Top, wl, dockedheight);
                else // topright
                    window = new Rectangle(parentarea.Right - dockingmargin.Right - wl, parentarea.Top + dockingmargin.Top, wl, dockedheight);

                areaout = new Rectangle(parentarea.Left, parentarea.Top + dockedheight + dockingmargin.TotalHeight, parentarea.Width, parentarea.Height - dockedheight - dockingmargin.TotalHeight);
            }
            else if (docktype >= DockingType.Right)
            {
                if (docktype == DockingType.Right)
                    window = new Rectangle(parentarea.Right - dockedwidth - dockingmargin.Right, parentarea.Top + dockingmargin.Top, dockedwidth, parentarea.Height - dockingmargin.TotalHeight);
                else if (docktype == DockingType.RightCenter)
                    window = new Rectangle(parentarea.Right - dockedwidth - dockingmargin.Right, parentarea.Top + parentarea.Height / 2 - hl / 2, dockedwidth, hl);
                else if (docktype == DockingType.RightTop)
                    window = new Rectangle(parentarea.Right - dockedwidth - dockingmargin.Right, parentarea.Top + dockingmargin.Top, dockedwidth, hl);
                else // rightbottom
                    window = new Rectangle(parentarea.Right - dockedwidth - dockingmargin.Right, parentarea.Bottom - dockingmargin.Bottom - hl, dockedwidth, hl);

                areaout = new Rectangle(parentarea.Left, parentarea.Top, parentarea.Width - window.Width - dockingmargin.TotalWidth, parentarea.Height);
            }
            else // must be left!
            {
                if (docktype == DockingType.Left)
                    window = new Rectangle(parentarea.Left + dockingmargin.Left, parentarea.Top + dockingmargin.Top, dockedwidth, parentarea.Height - dockingmargin.TotalHeight);
                else if (docktype == DockingType.LeftCenter)
                    window = new Rectangle(parentarea.Left + dockingmargin.Left, parentarea.Top + parentarea.Height / 2 - hl / 2, dockedwidth, hl);
                else if (docktype == DockingType.LeftTop)
                    window = new Rectangle(parentarea.Left + dockingmargin.Left, parentarea.Top + dockingmargin.Top, dockedwidth, hl);
                else  // leftbottom
                    window = new Rectangle(parentarea.Left + dockingmargin.Left, parentarea.Bottom - dockingmargin.Bottom - hl, dockedwidth, hl);

                areaout = new Rectangle(parentarea.Left + dockedwidth + dockingmargin.TotalWidth, parentarea.Top, parentarea.Width - dockedwidth - dockingmargin.TotalWidth, parentarea.Height);
            }

            System.Diagnostics.Debug.WriteLine("{0} dock {1} win {2} Area in {3} Area out {4}", Name, Dock, window, parentarea, areaout);

            CheckBitmapAfterLayout();       // check bitmap, virtual as inheritors may need to override this, make sure bitmap is the same width/height as ours
                                            // needs to be done in layout as ControlDisplay::PerformRecursiveLayout sets the textures up to match.

            parentarea = areaout;
        }

        // Override if required if you run a bitmap. Standard actions is to replace it if width/height is different.

        public virtual void CheckBitmapAfterLayout()
        {
            if (levelbmp != null && ( levelbmp.Width != Width || levelbmp.Height != Height ))
            {
                System.Diagnostics.Debug.WriteLine("Remake bitmap for " + Name);
                levelbmp.Dispose();
                levelbmp = new Bitmap(Width, Height);       // occurs for controls directly under form
            }
        }

        // redraw, into usebmp
        // bounds = area that our control occupies on the bitmap, in bitmap co-ords. This may be outside of the clip area below if the child is outside of the client area of its parent control
        // cliparea = area that we can draw into, in bitmap co-ords, so we don't exceed the bounds of any parent clip areas above us. clipareas are continually narrowed
        // gr = graphics to draw into
        // we must be visible to be called. Children may not be visible

        public virtual bool Redraw(Bitmap usebmp, Rectangle bounds, Rectangle cliparea, Graphics gr, bool forceredraw)
        {
            Graphics parentgr = null;                           // if we changed level bmp, we need to give the control the opportunity
            Rectangle parentarea = bounds;                      // to paint thru its level bmp to the parent bmp

            if (levelbmp != null)                               // bitmap on this level, use it for itself and its children
            {
                if ( usebmp != null )                           // must have a bitmap to paint thru to
                    parentgr = gr;                              // allow parent paint thru

                usebmp = levelbmp;

                cliparea = bounds = new Rectangle(0, 0, usebmp.Width, usebmp.Height);      // restate area in terms of bitmap, this is the bounds and the clip area

                gr = Graphics.FromImage(usebmp);        // get graphics for it
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            }

            bool redrawn = false;

            if (NeedRedraw || forceredraw)          // if we need a redraw, or we are forced to draw by a parent redrawing above us.
            {
                System.Diagnostics.Debug.WriteLine("redraw {0}->{1} Bounds {2} clip {3} client {4} ({5},{6},{7},{8}) nr {9} fr {10}", Parent?.Name, Name, bounds, cliparea, 
                                            ClientRectangle, ClientLeftMargin, ClientTopMargin, ClientRightMargin, ClientBottomMargin, NeedRedraw, forceredraw);

                gr.SetClip(cliparea);   // set graphics to the clip area so we can draw the background/border

                DrawBack(bounds, gr, BackColor, BackColorGradientAlt, BackColorGradient);
                DrawBorder(bounds, gr, BorderColor, BorderWidth);

                forceredraw = true;             // all children, force redraw
                NeedRedraw = false;             // we have been redrawn

                redrawn = true;                 // and signal up we have been redrawn
            }

            // client area, in terms of last bitmap
            Rectangle clientarea = new Rectangle(bounds.Left + ClientLeftMargin, bounds.Top + ClientTopMargin, ClientWidth, ClientHeight);

            foreach( var c in childreniz)       // in inverse Z order, last is top Z
            {
                if (c.Visible)
                {
                    Rectangle childbounds = new Rectangle(clientarea.Left + c.Left,     // not bounded by clip area, in bitmap coords
                                                          clientarea.Top + c.Top,
                                                          c.Width,
                                                          c.Height);

                    // clip area is progressively narrowed as we go down the children
                    // its the minimum of the previous clip area
                    // the child bounds
                    // and the client rectangle
 
                    int cleft = Math.Max(childbounds.Left, cliparea.Left);          // clipped to child left or cliparea left
                    cleft = Math.Max(cleft, bounds.Left + this.ClientLeftMargin);
                    int ctop = Math.Max(childbounds.Top, cliparea.Top);             // clipped to child top or cliparea top
                    ctop = Math.Max(ctop, bounds.Top + this.ClientTopMargin);
                    int cright = Math.Min(childbounds.Left + c.Width, cliparea.Right);  // clipped to child left+width or the cliparea right
                    cright = Math.Min(cright, bounds.Right - this.ClientRightMargin);     // additionally clipped to our bounds right less its client margin
                    int cbot = Math.Min(childbounds.Top + c.Height, cliparea.Bottom);   // clipped to child bottom or cliparea bottom
                    cbot = Math.Min(cbot, bounds.Bottom - this.ClientBottomMargin);       // additionally clipped to bounds bottom less its client margin

                    Rectangle childcliparea = new Rectangle(cleft, ctop, cright - cleft, cbot - ctop);  // clip area to pass down in bitmap coords

                    redrawn |= c.Redraw(usebmp, childbounds, childcliparea, gr, forceredraw);
                }
            }

            if ( forceredraw)       // will be set if NeedRedrawn or forceredrawn
            {
                gr.SetClip(cliparea);   // set graphics to the clip area

                Paint(clientarea, gr); // Paint, nominally in the client area, but you can access the whole of the cliparea which includes the margins

                if (parentgr != null)      // give us a chance of parent paint thru
                {
                    parentgr.SetClip(parentarea);       // must set the clip area again to address the parent area
                    PaintParent(parentarea, parentgr);
                }
            }

            if (levelbmp != null)        // bitmap on this level, we made a GR, dispose
                gr.Dispose();

            return redrawn;
        }

        // draw border area, override to draw something different
        protected virtual void DrawBorder(Rectangle bounds, Graphics gr, Color bc, float bw)
        {
            if (bw > 0)
            {
                Rectangle rectarea = new Rectangle(bounds.Left + Margin.Left,
                                                bounds.Top + Margin.Top,
                                                bounds.Width - Margin.TotalWidth - 1,
                                                bounds.Height - Margin.TotalHeight - 1);

                using (var p = new Pen(bc, bw))
                {
                    gr.DrawRectangle(p, rectarea);
                }
            }
        }

        // draw back area - override to paint something different
        protected virtual void DrawBack(Rectangle bounds, Graphics gr, Color bc, Color bcgradientalt, int bcgradient)
        {
            if ( levelbmp != null)                  // if we own a bitmap, reset back to transparent, erasing anything that we drew before
                gr.Clear(Color.Transparent);        // tbd check

            if (bc != Color.Transparent)            // and draw what the back colour is
            {
                if ( levelbmp == null )             // if we are a normal control, we need to start from the pixels inside us being transparent
                    gr.Clear(Color.Transparent);    // erasing anything that we drew before, because if we have half alpha in the colour, it will build up

                if (bcgradient != int.MinValue)
                {
                    //System.Diagnostics.Debug.WriteLine("Background " + Name +  " " + bounds + " " + bc + " -> " + bcgradientalt );
                    using (var b = new System.Drawing.Drawing2D.LinearGradientBrush(bounds, bc, bcgradientalt, bcgradient))
                        gr.FillRectangle(b, bounds);       // linear grad brushes do not respect smoothing mode, btw
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("Background " + Name + " " + bounds + " " + backcolor);
                    using (Brush b = new SolidBrush(bc))     // always fill, so we get back to start
                        gr.FillRectangle(b, bounds);
                }
            }
        }

        
        protected virtual void Paint(Rectangle area, Graphics gr)      // normal override, you can overdraw border if required.
        {
            //System.Diagnostics.Debug.WriteLine("Paint {0} to {1}", Name, area);
        }

        protected virtual void PaintParent(Rectangle parentarea, Graphics parentgr) // only called if you've defined a bitmap yourself, 
        {                                                                        // gives you a chance to paint to the parent bitmap
           // System.Diagnostics.Debug.WriteLine("Paint Into parent {0} to {1}", Name, parentarea);
        }

        #endregion

        #region UI Overrides

        public virtual void OnMouseLeave(GLMouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("leave " + Name + " " + e.Location);
            MouseLeave?.Invoke(this, e);

            if (InvalidateOnEnterLeave)
                Invalidate();
        }

        public virtual void OnMouseEnter(GLMouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("enter " + Name + " " + e.Location);
            MouseEnter?.Invoke(this, e);

            if (InvalidateOnEnterLeave)
                Invalidate();
        }

        public virtual void OnMouseUp(GLMouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("up   " + Name + " " + e.Location + " " + e.Button);
            MouseUp?.Invoke(this, e);

            if (InvalidateOnMouseDownUp)
                Invalidate();
        }

        public virtual void OnMouseDown(GLMouseEventArgs e)
        {
           // System.Diagnostics.Debug.WriteLine("down " + Name + " " + e.Location +" " + e.Button);
            MouseDown?.Invoke(this, e);

            if (InvalidateOnMouseDownUp)
                Invalidate();
        }

        public virtual void OnMouseClick(GLMouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("click " + Name + " " + e.Button + " " + e.Clicks + " " + e.Location);
            MouseClick?.Invoke(this, e);
        }

        public virtual void OnMouseMove(GLMouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Over " + Name + " " + e.Location);
            MouseMove?.Invoke(this, e);
        }

        public virtual void OnMouseWheel(GLMouseEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Over " + Name + " " + e.Location);
            MouseWheel?.Invoke(this, e);
        }

        public delegate void KeyFunc(GLKeyEventArgs e);
        public void CallKeyFunction(KeyFunc f, GLKeyEventArgs e)
        {
            f.Invoke(e);
        }

        public virtual void OnKeyDown(GLKeyEventArgs e)
        {
            KeyDown?.Invoke(this, e);
        }

        public virtual void OnKeyUp(GLKeyEventArgs e)
        {
            KeyUp?.Invoke(this, e);
        }

        public virtual void OnKeyPress(GLKeyEventArgs e)
        {
            KeyPress?.Invoke(this, e);
        }

        public virtual void OnFocusChanged(bool focused, GLBaseControl fromto)      // false, fromto = where it going to
        {
            this.focused = focused;
            if (InvalidateOnFocusChange)
                Invalidate();
            FocusChanged?.Invoke(this, focused, fromto);
        }

        public virtual void OnFontChanged()
        {
            FontChanged?.Invoke(this);
        }

        public virtual void OnResize()
        {
            Resize?.Invoke(this);
        }

        public virtual void OnMoved()
        {
            Moved?.Invoke(this);
        }

        public virtual void OnControlAdd(GLBaseControl parent, GLBaseControl child)
        {
            ControlAdd?.Invoke(parent, child);
        }

        public virtual void OnControlRemove(GLBaseControl parent, GLBaseControl child)
        {
            ControlRemove?.Invoke(parent, child);
        }

        #endregion


        #region Implementation

        // set location - allowing override of invalidate behaviour

        private void SetLocation(int left, int top)
        {
            Rectangle w = new Rectangle(left, top, Width, Height);
            if (w != window)
            {
                window = w;

                OnMoved();

                if ( (Parent?.ChildLocationChanged(this) ?? false) == false)     // give a class a chance to move windows in a different manner than causing a bit repaint/invalidate
                {
                    NeedRedraw = true;      // we need a redraw
                    parent?.Invalidate();   // parent is invalidated as well, and the whole form needs reendering
                    parent?.PerformLayout();     // go up one and perform layout on all its children, since we are part of it.
                }
            }
        }

        // normally a location changed (left,top) means a invalidate of parent and re-layout. But for top level windows under GLDisplayControl
        // we don't need to lay them out as they are top level GL objects and we just need to move the texture co-ords
        // this bit does that - allows the top level parent to not have to invalidate if it returns true
        protected virtual bool ChildLocationChanged(GLBaseControl child)
        {
            return false;
        }

        // Set Position, causing an invalidation layout at parent level

        private void SetPos(int left, int top, int width, int height) // change window rectangle, with layout
        {
            Rectangle w = new Rectangle(left, top, width, height);

            if (w != window)        // if changed
            {
                bool resized = w.Size != window.Size;
                bool moved = w.Location != window.Location;
                window = w;

                if (moved)
                    OnMoved();

                if (resized)
                    OnResize();

                NeedRedraw = true;      // we need a redraw
                //System.Diagnostics.Debug.WriteLine("setpos need redraw on " + Name);
                parent?.Invalidate();   // parent is invalidated as well, and the whole form needs reendering

                parent?.PerformLayout();     // go up one and perform layout on all its children, since we are part of it.
            }
        }

        private void SetEnabled(bool v)
        {
            enabled = v;
            foreach (var c in childrenz)
                SetEnabled(v);
        }

        private void SetFont(Font f)
        {
            font = f;
            PropergateFontChanged(this);
        }

        private void PropergateFontChanged(GLBaseControl p)
        {
            p.OnFontChanged();
            foreach (var c in p.childrenz)
            {
                if (c.Font == null)
                    PropergateFontChanged(c);
            }
        }

        protected bool NeedRedraw { get; set; } = true;         // we need to redraw, therefore all children also redraw

        private Bitmap levelbmp;       // set if the level has a new bitmap.  Controls under Form always does. Other ones may if they scroll
        private Font font = null;
        private Rectangle window;       // total area owned, in parent co-ords
        private bool needLayout { get; set; } = false;        // need a layout after suspend layout was called
        private bool suspendLayoutSet { get; set; } = false;        // suspend layout is on
        private bool enabled { get; set; } = true;
        private bool visible { get; set; } = true;
        private DockingType docktype { get; set; } = DockingType.None;
        private float dockpercent { get; set; } = 0;
        private Color backcolor { get; set; } = Color.Transparent;
        private Color backcolorgradientalt { get; set; } = Color.Black;
        private int backcolorgradient { get; set; } = int.MinValue;           // in degrees
        private Color bordercolor { get; set; } = Color.Transparent;         // Margin - border - padding is common to all controls. Area left is control area to draw in
        private int borderwidth { get; set; } = 0;
        private GL4.Controls.Padding padding { get; set; }
        private GL4.Controls.Margin margin { get; set; }
        private GL4.Controls.Margin dockingmargin { get; set; }
        private bool autosize { get; set; }
        private int column { get; set; } = 0;     // for table layouts
        private int row { get; set; } = 0;        // for table layouts
        private bool focused { get; set; } = false;
        private bool focusable { get; set; } = false;

        private GLBaseControl parent { get; set; } = null;       // its parent, null if top of top

        private List<GLBaseControl> childrenz = new List<GLBaseControl>();
        private List<GLBaseControl> childreniz = new List<GLBaseControl>();


        #endregion


    }
}
