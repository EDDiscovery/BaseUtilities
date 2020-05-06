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

using System.Drawing;

namespace OpenTKUtils.GL4.Controls
{
    public class GLToolTip : GLForeDisplayBase
    {
        public int AutomaticDelay { get; set; } = 500;
        public int AutoPopDelay { get; set; } = 5000;
        public StringFormat StringFormat = null;
        public Point AutoPlacementOffset = new Point(10, 0);

        public GLToolTip(string name, Color? backcolour = null) : base(name, DefaultWindowRectangle)
        {
            BackColor = backcolour.HasValue ? backcolour.Value : DefaultControlBackColor;
            VisibleNI = false;
            PaddingNI = new Padding(3);
            timer.Tick += TimeOut;
        }

        public GLToolTip() : this("TB?", null)
        {
        }

        protected override void Paint(Rectangle area, Graphics gr)
        {
            using (Brush br = new SolidBrush(ForeColor))
            {
                if (StringFormat != null)
                    gr.DrawString(tiptext, Font, br, area,StringFormat);
                else
                    gr.DrawString(tiptext, Font, br, area);
            }
        }


        public override void OnControlAdd(GLBaseControl parent, GLBaseControl child)
        {
            base.OnControlAdd(parent, child);

            var p = parent as GLControlDisplay;     // if attached to control display, its an automatic tool tip
            if (p != null)
            {
                p.GlobalMouseMove += MouseMoved;
            }
        }

        public override void OnControlRemove(GLBaseControl parent, GLBaseControl child)
        {
            var p = parent as GLControlDisplay;     // if attached to control display, its an automatic tool tip
            if (p != null)
            {
                p.GlobalMouseMove -= MouseMoved;
            }

            base.OnControlRemove(parent, child);
        }

        public void Show(Point pos, string text)
        {
            if (Visible == false)
            {
                var size = BitMapHelpers.MeasureStringInBitmap(text, Font, StringFormat);
                Location = new Point(pos.X+AutoPlacementOffset.X, pos.Y + AutoPlacementOffset.Y);
                ClientSize = new Size((int)size.Width + 1, (int)size.Height + 1);
                TopMost = true;
                tiptext = text;
                Visible = true;
            }
        }

        public void Hide()
        {
            Visible = false;
        }

        private void MouseMoved(GLMouseEventArgs e)
        {
            GLControlDisplay disp = FindDisplay();
            GLBaseControl ctrl = disp.FindControlOver(e.Location);

            if (mouseover != ctrl)        // moved into new control or out of it
            {
                Hide();

                if (ctrl == null)       // out
                {
                    timer.Stop();
                }
                else if (mouseover != null )    // into
                {
                    if (ctrl.ToolTipText.HasChars())
                    {
                        System.Diagnostics.Debug.WriteLine("Tooltip Found " + ctrl.Name + " " + e.Location);
                        timer.Start(AutomaticDelay);     // start timer
                        showloc = entryloc = e.Location;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No Tooltip Found " + ctrl.Name + " " + e.Location);
                        timer.Stop();
                    }
                }

                mouseover = ctrl;       // set control mouse is over
            }
            else
            {       // in same control
                if (timer.Running)
                {
                    int delta2 = (e.Location.X - entryloc.X) * (e.Location.X - entryloc.X) + (e.Location.Y + entryloc.Y) * (e.Location.Y + entryloc.Y);

                    if (delta2 > 16)
                    {
                        entryloc = e.Location;
                        timer.Start(AutomaticDelay);        // moved within control, restart
                        System.Diagnostics.Debug.WriteLine("Restart " + mouseover.Name);
                    }

                    showloc = e.Location;
                }
            }
        }

        private void TimeOut(Timers.Timer t, long timeout)
        {
            if (!Visible && mouseover != null )
            {
                System.Diagnostics.Debug.WriteLine("Show " + mouseover.Name + " " + showloc);
                Show(showloc, mouseover.ToolTipText);
            }
        }

        private OpenTKUtils.Timers.Timer timer = new Timers.Timer();
        private Point entryloc;
        private Point showloc;
        private GLBaseControl mouseover = null;
        private string tiptext;

    }
}
