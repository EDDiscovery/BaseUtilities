using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public struct ScrollEventArgs
    {
        public int NewValue { get; set; }
        public int OldValue { get; }
        public ScrollEventArgs(int oldv, int newv) { NewValue = newv; OldValue = oldv; }
    }

    public class GLScrollBar : GLBaseControl
    {
        public Action<GLBaseControl, ScrollEventArgs> Scroll { get; set; } = null;

        public int Value { get { return thumbvalue; } set { SetValues(value, maximum, minimum, largechange, smallchange); } }
        public int ValueLimited { get { return thumbvalue; } set { SetValues(value, maximum, minimum, largechange, smallchange, true); } }
        public int Maximum { get { return maximum; } set { SetValues(thumbvalue, value, minimum, largechange, smallchange); } }
        public int Minimum { get { return minimum; } set { SetValues(thumbvalue, maximum, value, largechange, smallchange); } }
        public int LargeChange { get { return largechange; } set { SetValues(thumbvalue, maximum, minimum, value, smallchange); } }
        public int SmallChange { get { return smallchange; } set { SetValues(thumbvalue, maximum, minimum, largechange, value); } }
        public void SetValueMaximum(int v, int m) { SetValues(v, m, minimum, largechange, smallchange); }
        public void SetValueMaximumLargeChange(int v, int m, int lc) { SetValues(v, m, minimum, lc, smallchange); }
        public void SetValueMaximumMinimum(int v, int max, int min) { SetValues(v, max, min, largechange, smallchange); }
        public bool HideScrollBar { get; set; } = false;                   // hide if no scroll needed
        public bool IsScrollBarOn { get { return thumbenable; } }           // is it on?

        public Color ArrowColor { get { return arrowcolor; } set { arrowcolor = value; Invalidate(); } }       // of text
        public Color SliderColor { get { return slidercolor; } set { slidercolor = value; Invalidate(); } }

        public Color ArrowButtonColor { get { return arrowButtonColor; } set { arrowButtonColor = value; Invalidate(); } }
        public Color ArrowBorderColor { get { return arrowBorderColor; } set { arrowBorderColor = value; Invalidate(); } }
        public float ArrowUpDrawAngle { get { return arrowUpDrawAngle; } set { arrowUpDrawAngle = value; Invalidate(); } }
        public float ArrowDownDrawAngle { get { return arrowDownDrawAngle; } set { arrowDownDrawAngle = value; Invalidate(); } }
        public float ArrowColorScaling { get { return arrowColorScaling; } set { arrowColorScaling = value; Invalidate(); } }

        public Color MouseOverButtonColor { get { return mouseOverButtonColor; } set { mouseOverButtonColor = value; Invalidate(); } }
        public Color MousePressedButtonColor { get { return mousePressedButtonColor; } set { mousePressedButtonColor = value; Invalidate(); } }
        public Color ThumbButtonColor { get { return thumbButtonColor; } set { thumbButtonColor = value; Invalidate(); } }
        public Color ThumbBorderColor { get { return thumbBorderColor; } set { thumbBorderColor = value; Invalidate(); } }
        public float ThumbColorScaling { get { return thumbColorScaling; } set { thumbColorScaling = value; Invalidate(); } }
        public float ThumbDrawAngle { get { return thumbDrawAngle; } set { thumbDrawAngle = value; Invalidate(); } }

        public GLScrollBar()
        {
        }

        public GLScrollBar(string name, Rectangle pos, int min, int max)
        {
            Name = name;
            Bounds = pos;
            Value = Minimum = min;
            Maximum = max;
        }

        public override void Paint(Rectangle area, Graphics gr)
        {
            using (Brush br = new SolidBrush(this.SliderColor))
                gr.FillRectangle(br, new Rectangle(area.Left + sliderarea.Left, area.Top + sliderarea.Top, sliderarea.Width, sliderarea.Height));

            DrawButton(gr, new Rectangle(area.Left + upbuttonarea.Left, area.Top + upbuttonarea.Top,upbuttonarea.Width,upbuttonarea.Height), MouseOver.MouseOverUp);
            DrawButton(gr, new Rectangle(area.Left + downbuttonarea.Left, area.Top + downbuttonarea.Top, downbuttonarea.Width, downbuttonarea.Height), MouseOver.MouseOverDown);
            DrawButton(gr, new Rectangle(area.Left + thumbbuttonarea.Left, area.Top + thumbbuttonarea.Top, thumbbuttonarea.Width, thumbbuttonarea.Height), MouseOver.MouseOverThumb);
        }

        private void DrawButton(Graphics g, Rectangle rect, MouseOver but)
        {
            if (rect.Height < 4 || rect.Width < 4)
                return;

            bool isthumb = (but == MouseOver.MouseOverThumb);
            Color c1, c2;
            float angle;

            if (isthumb)
            {
                if (!thumbenable)
                    return;

                c1 = (mousepressed == but) ? MousePressedButtonColor : ((mouseover == but) ? MouseOverButtonColor : ThumbButtonColor);
                c2 = c1.Multiply(ThumbColorScaling);
                angle = ThumbDrawAngle;
            }
            else
            {
                c1 = (mousepressed == but) ? MousePressedButtonColor : ((mouseover == but) ? MouseOverButtonColor : ArrowButtonColor);
                c2 = c1.Multiply(ArrowColorScaling);
                angle = (but == MouseOver.MouseOverUp) ? ArrowUpDrawAngle : ArrowDownDrawAngle;
            }

            using (Brush bbck = new System.Drawing.Drawing2D.LinearGradientBrush(rect, c1, c2, angle))
                g.FillRectangle(bbck, rect);

            if (Enabled && thumbenable && !isthumb)
            {
                int hoffset = rect.Width / 3;
                int voffset = rect.Height / 3;
                Point arrowpt1 = new Point(rect.X + hoffset, rect.Y + voffset);
                Point arrowpt2 = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height - voffset);
                Point arrowpt3 = new Point(rect.X + rect.Width - hoffset, arrowpt1.Y);

                Point arrowpt1c = new Point(arrowpt1.X, arrowpt2.Y);
                Point arrowpt2c = new Point(arrowpt2.X, arrowpt1.Y);
                Point arrowpt3c = new Point(arrowpt3.X, arrowpt2.Y);

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (Pen p2 = new Pen(ArrowColor))
                {
                    if (but == MouseOver.MouseOverDown)
                    {
                        g.DrawLine(p2, arrowpt1, arrowpt2);            // the arrow!
                        g.DrawLine(p2, arrowpt2, arrowpt3);
                    }
                    else
                    {
                        g.DrawLine(p2, arrowpt1c, arrowpt2c);            // the arrow!
                        g.DrawLine(p2, arrowpt2c, arrowpt3c);
                    }
                }

            }

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

            if (but == mouseover || isthumb)
            {
                using (Pen p = new Pen(isthumb ? ThumbBorderColor : ArrowBorderColor))
                {
                    Rectangle border = rect;
                    border.Width--; border.Height--;
                    g.DrawRectangle(p, border);
                }
            }
        }

        public override void OnMouseMove(GLMouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!Enabled || !thumbenable)
                return;

            if (thumbmove)                        // if moving thumb, we calculate where we are in value
            {
                int voffset = e.Location.Y - (sliderarea.Y + thumbmovecaptureoffset);
                //Console.WriteLine("Voffset " + voffset);
                int sliderrangepx = sliderarea.Height - thumbbuttonarea.Height;      // range of values to represent Min-Max.
                voffset = Math.Min(Math.Max(voffset, 0), sliderrangepx);        // bound within slider range
                float percent = (float)voffset / (float)sliderrangepx;          // % in
                int newthumbvalue = minimum + (int)((float)(UserMaximum - minimum) * percent);
                //Console.WriteLine("Slider px" + voffset + " to value " + newthumbvalue);

                if (newthumbvalue != thumbvalue)        // and if changed, apply it.
                {
                    thumbvalue = newthumbvalue;
                    OnScroll(new ScrollEventArgs(thumbvalue, newthumbvalue));
                    CalculateThumb();
                    Invalidate();
                }
            }
            else if (upbuttonarea.Contains(e.Location))
            {
                if (mouseover != MouseOver.MouseOverUp)
                {
                    mouseover = MouseOver.MouseOverUp;
                    Invalidate();
                }
            }
            else if (downbuttonarea.Contains(e.Location))
            {
                if (mouseover != MouseOver.MouseOverDown)
                {
                    mouseover = MouseOver.MouseOverDown;
                    Invalidate();
                }
            }
            else if (thumbbuttonarea.Contains(e.Location))
            {
                if (mouseover != MouseOver.MouseOverThumb)
                {
                    mouseover = MouseOver.MouseOverThumb;
                    Invalidate();
                }
            }
            else if (mouseover != MouseOver.MouseOverNone)
            {
                mouseover = MouseOver.MouseOverNone;
                Invalidate();
            }
        }

        public override void OnMouseDown(GLMouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!Enabled || !thumbenable)
                return;

            if (upbuttonarea.Contains(e.Location))
            {
                mousepressed = MouseOver.MouseOverUp;
                Invalidate();
                //StartRepeatClick(e);
                MoveThumb(-smallchange);
            }
            else if (downbuttonarea.Contains(e.Location))
            {
                mousepressed = MouseOver.MouseOverDown;
                Invalidate();
                //StartRepeatClick(e);
                MoveThumb(smallchange);
            }
            else if (thumbbuttonarea.Contains(e.Location))
            {
                mousepressed = MouseOver.MouseOverThumb;
                Invalidate();
                thumbmove = true;                           // and mouseover should be on as well
                thumbmovecaptureoffset = e.Location.Y - thumbbuttonarea.Y;      // pixels down the thumb when captured..
                //Console.WriteLine("Thumb captured at " + thumbmovecaptureoffset);
            }
            else if (sliderarea.Contains(e.Location))      // slider, but not thumb..
                MoveThumb((e.Location.Y < thumbbuttonarea.Y) ? -largechange : largechange);

        }

        public override void OnMouseUp(GLMouseEventArgs e)
        {
            if (mousepressed != MouseOver.MouseOverNone)
            {
                mousepressed = MouseOver.MouseOverNone;
                Invalidate();
            }

            if (thumbmove)
            {
                thumbmove = false;
                Invalidate();
            }

            //repeatclick.Stop();

            base.OnMouseUp(e);
        }


        public override void OnMouseLeave(GLMouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!thumbmove && mouseover != MouseOver.MouseOverNone)
            {
                mouseover = MouseOver.MouseOverNone;
                Invalidate();
            }
        }


       public override Rectangle Layout(Rectangle area)
        {
            Rectangle areab = base.Layout(area);

            sliderarea = ClientRectangle;

            int scrollheight = sliderarea.Width;
            if (scrollheight * 2 > ClientRectangle.Height / 3)  // don't take up too much of the slider with the buttons
                scrollheight = ClientRectangle.Height / 6;

            upbuttonarea = sliderarea;
            upbuttonarea.Height = scrollheight;
            downbuttonarea = sliderarea;
            downbuttonarea.Y = sliderarea.Bottom - scrollheight;
            downbuttonarea.Height = scrollheight;
            sliderarea.Y += scrollheight;
            sliderarea.Height -= 2 * scrollheight;

            CalculateThumb();

            return areab;
        }

        private void CalculateThumb()
        {
            int userrange = maximum - minimum + 1;           // number of positions..

            if (largechange < userrange)                   // largerange is less than number of individual positions
            {
                float fthumbheight = ((float)largechange / (float)userrange) * sliderarea.Height;    // less than H
                int thumbheight = (int)fthumbheight;
                if (thumbheight < sliderarea.Width)             // too small, adjust
                    thumbheight = sliderarea.Width;

                int sliderrangev = UserMaximum - minimum;       // Usermaximum will be > minimum, due to above < test.
                int lthumb = Math.Min(thumbvalue, UserMaximum);         // values beyond User maximum screened out

                float fposition = (float)(lthumb - minimum) / (float)sliderrangev;

                int sliderrangepx = sliderarea.Height - thumbheight;      // range of values to represent Min-Max.
                int thumboffsetpx = (int)((float)sliderrangepx * fposition);
                thumboffsetpx = Math.Min(thumboffsetpx, sliderrangepx);     // LIMIT, because we can go over slider range if value=maximum

                thumbbuttonarea = new Rectangle(sliderarea.X, sliderarea.Y + thumboffsetpx, sliderarea.Width, thumbheight);
                thumbenable = true;
            }
            else
            {
                thumbenable = false;                        // else disable the thumb and scroll bar
                thumbmove = false;
                mouseover = MouseOver.MouseOverNone;
                mousepressed = MouseOver.MouseOverNone;
            }
        }

        private void MoveThumb(int vchange)
        {
            int oldvalue = thumbvalue;

            if (vchange < 0 && thumbvalue > minimum)
            {
                thumbvalue += vchange;
                thumbvalue = Math.Max(thumbvalue, minimum);
                OnScroll(new ScrollEventArgs( oldvalue, Value));
                CalculateThumb();
                Invalidate();
            }
            else if (vchange > 0 && thumbvalue < UserMaximum)
            {
                thumbvalue += vchange;
                thumbvalue = Math.Min(thumbvalue, UserMaximum);
                OnScroll(new ScrollEventArgs( oldvalue, Value));
                CalculateThumb();
                Invalidate();
            }

            //Console.WriteLine("Slider is " + thumbvalue + " from " + minimum + " to " + maximum);
        }


        private void SetValues(int v, int max, int min, int lc, int sc, bool limittousermax = false)   // this allows it to be set to maximum..
        {
            //System.Diagnostics.Debug.WriteLine("Set Scroll " + v + " min " + min + " max " + max + " lc "+ lc + " sc "+ sc + " Usermax "+ UserMaximum);
            smallchange = sc;                                   // has no effect on display of control
            bool iv = false;

            if (max != maximum || min != minimum || lc != largechange) // these do..
            {           // only invalidate if actually changed something
                maximum = max;
                minimum = min;
                largechange = lc;
                iv = true;
            }

            int newthumbvalue = Math.Min(Math.Max(v, minimum), maximum);

            if (limittousermax)
                newthumbvalue = Math.Min(newthumbvalue, UserMaximum);

            if (newthumbvalue != thumbvalue)        // if changed..
            {
                thumbvalue = newthumbvalue;
                iv = true;
            }

            if (iv)
            {
                CalculateThumb();
                Invalidate();
            }
        }

        private int UserMaximum { get { return Math.Max(maximum - largechange + 1, minimum); } }    // make sure it does not go below minimum whatever largechange is set to.

        protected virtual void OnScroll(ScrollEventArgs se)
        {
            Scroll?.Invoke(this, se);
        }


        private Color arrowcolor { get; set; } = Color.Black;
        private Color slidercolor { get; set; } = Color.DarkGray;

        private Color arrowButtonColor { get; set; } = Color.LightGray;
        private Color arrowBorderColor { get; set; } = Color.LightBlue;
        private float arrowUpDrawAngle { get; set; } = 90F;
        private float arrowDownDrawAngle { get; set; } = 270F;
        private float arrowColorScaling { get; set; } = 0.5F;

        private Color mouseOverButtonColor { get; set; } = Color.Green;
        private Color mousePressedButtonColor { get; set; } = Color.Red;
        private Color thumbButtonColor { get; set; } = Color.DarkBlue;
        private Color thumbBorderColor { get; set; } = Color.Yellow;
        private float thumbColorScaling { get; set; } = 0.5F;
        private float thumbDrawAngle { get; set; } = 0F;

        private Rectangle sliderarea;
        private Rectangle upbuttonarea;
        private Rectangle downbuttonarea;
        private Rectangle thumbbuttonarea;

        private int maximum = 100;
        private int minimum = 0;
        private int largechange = 10;
        private int smallchange = 1;
        private int thumbvalue = 0;
        private bool thumbenable = true;
        private bool thumbmove = false;

        enum MouseOver { MouseOverNone, MouseOverUp, MouseOverDown, MouseOverThumb };
        private MouseOver mouseover = MouseOver.MouseOverNone;
        private MouseOver mousepressed = MouseOver.MouseOverNone;
        private int thumbmovecaptureoffset = 0;     // px down the thumb when captured..

    }
}
