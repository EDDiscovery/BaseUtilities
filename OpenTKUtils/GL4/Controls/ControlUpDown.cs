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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLUpDownControl: GLButtonBase
    {
        public Action<GLBaseControl, GLMouseEventArgs> ValueChanged { get; set; } = null;           // Delta holds the direction

        public float MouseSelectedColorScaling { get { return mouseSelectedColorScaling; } set { mouseSelectedColorScaling = value; Invalidate(); } } 

        public GLUpDownControl(string name, Rectangle location, Color back) : base(name, location, back)
        {
            Focusable = true;
            repeattimer.Tick += RepeatClick;
        }

        public GLUpDownControl() : this("UD?", DefaultWindowRectangle, DefaultBackColor)
        {
        }

        public override void PerformRecursiveLayout()
        {
            base.PerformRecursiveLayout();
            int halfway = ClientRectangle.Height / 2 - 1;
            upperbuttonarea = new Rectangle(1, 0, ClientRectangle.Width - 2, halfway);
            lowerbuttonarea = new Rectangle(1, halfway + 2, ClientRectangle.Width - 2, halfway);
        }

        // called after the background of the panel has been drawn - so it will be clear to write.

        protected override void Paint(Rectangle area, Graphics gr)
        {
            Color pcup = (Enabled) ? ((mousedown == MouseOver.MouseOverUp) ? MouseDownBackColor : ((mouseover == MouseOver.MouseOverUp) ? MouseOverBackColor : this.BackColor)) : this.BackColor.Multiply(0.5F);
            Color pcdown = (Enabled) ? ((mousedown == MouseOver.MouseOverDown) ? MouseDownBackColor : ((mouseover == MouseOver.MouseOverDown) ? MouseOverBackColor : this.BackColor)) : this.BackColor.Multiply(0.5F);

            Rectangle drawupper = new Rectangle(area.Left + upperbuttonarea.Left, area.Top + upperbuttonarea.Top, upperbuttonarea.Width, upperbuttonarea.Height);  // seems to make it paint better
            Rectangle drawlower = new Rectangle(area.Left + lowerbuttonarea.Left, area.Top + lowerbuttonarea.Top, lowerbuttonarea.Width, lowerbuttonarea.Height);  // seems to make it paint better
            Rectangle sareaupper = drawupper;
            sareaupper.Height++;
            Rectangle sarealower = drawlower;
            sarealower.Height++;

            using (Brush b = new LinearGradientBrush(sareaupper, pcup, pcup.Multiply(BackColorScaling), 90))
                gr.FillRectangle(b, drawupper);

            using (Brush b = new LinearGradientBrush(sarealower, pcdown, pcdown.Multiply(BackColorScaling), 270))
                gr.FillRectangle(b, drawlower);

            if (Enabled)
            {
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (Pen p = new Pen(mousedown == MouseOver.MouseOverUp ? ForeColor.Multiply(MouseSelectedColorScaling) : ForeColor))
                {
                    int hoffset = drawupper.Width / 3;
                    int voffset = drawupper.Height / 3;

                    Point arrowpt1u = new Point(drawupper.X + hoffset, drawupper.Y + drawupper.Height - voffset);
                    Point arrowpt2u = new Point(drawupper.X + drawupper.Width / 2, drawupper.Y + voffset);
                    Point arrowpt3u = new Point(drawupper.X + drawupper.Width - hoffset, arrowpt1u.Y);
                    gr.DrawLine(p, arrowpt1u, arrowpt2u);            // the arrow!
                    gr.DrawLine(p, arrowpt2u, arrowpt3u);
                }

                using (Pen p = new Pen(mousedown == MouseOver.MouseOverDown ? ForeColor.Multiply(MouseSelectedColorScaling) : ForeColor))
                {
                    int hoffset = drawlower.Width / 3;
                    int voffset = drawlower.Height / 3;

                    Point arrowpt1d = new Point(drawlower.X + hoffset, drawlower.Y + voffset);
                    Point arrowpt2d = new Point(drawlower.X + drawlower.Width / 2, drawlower.Y + drawlower.Height - voffset);
                    Point arrowpt3d = new Point(drawlower.X + drawlower.Width - hoffset, arrowpt1d.Y);

                    gr.DrawLine(p, arrowpt1d, arrowpt2d);            // the arrow!
                    gr.DrawLine(p, arrowpt2d, arrowpt3d);

                }
            }

            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

        }

        public override void OnMouseMove(GLMouseEventArgs eventargs)
        {
            base.OnMouseMove(eventargs);

            if (!eventargs.Handled)
            {
                if (upperbuttonarea.Contains(eventargs.Location))
                {
                    if (mouseover != MouseOver.MouseOverUp)
                    {
                        mouseover = MouseOver.MouseOverUp;
                        Invalidate();
                    }
                }
                else if (lowerbuttonarea.Contains(eventargs.Location))
                {
                    if (mouseover != MouseOver.MouseOverDown)
                    {
                        mouseover = MouseOver.MouseOverDown;
                        Invalidate();
                    }
                }
                else if (mouseover != MouseOver.MouseOverNone)
                {
                    mouseover = MouseOver.MouseOverNone;
                    Invalidate();
                }
            }
        }

        public override void OnMouseLeave(GLMouseEventArgs e)
        {
            base.OnMouseLeave(e);
            mouseover = MouseOver.MouseOverNone;
            mousedown = MouseOver.MouseOverNone;
            Invalidate();
        }

        public override void OnMouseDown(GLMouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);

            if (!mevent.Handled)
            {
                if (upperbuttonarea.Contains(mevent.Location))
                {
                    mousedown = MouseOver.MouseOverUp;
                    Invalidate();
                    mevent.Delta = 1;
                    OnValueChanged(mevent);
                    StartRepeatClick(mevent);
                }
                else if (lowerbuttonarea.Contains(mevent.Location))
                {
                    mousedown = MouseOver.MouseOverDown;
                    Invalidate();
                    mevent.Delta = -1;
                    OnValueChanged(mevent);
                    StartRepeatClick(mevent);
                }
            }
        }

        public override void OnMouseUp(GLMouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            mousedown = MouseOver.MouseOverNone;
            repeattimer.Stop();
            Invalidate();
        }

        protected virtual void OnValueChanged(GLMouseEventArgs e)
        {
            ValueChanged?.Invoke(this,e);
        }

        private void StartRepeatClick(GLMouseEventArgs e)
        {
            if (!repeattimer.Running)
            {
                savedmevent = e;
                repeattimer.Start(50, true);
            }
        }

        private void RepeatClick(Timers.Timer t, long timeout)
        {
            OnValueChanged(savedmevent);
        }

        enum MouseOver { MouseOverUp, MouseOverDown, MouseOverNone };
        private MouseOver mouseover = MouseOver.MouseOverNone;
        private MouseOver mousedown = MouseOver.MouseOverNone;
        private Rectangle upperbuttonarea;
        private Rectangle lowerbuttonarea;
        private float mouseSelectedColorScaling { get; set; } = 1.5F;
        private OpenTKUtils.Timers.Timer repeattimer = new Timers.Timer();
        private GLMouseEventArgs savedmevent;


    }
}

