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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLFlowLayoutPanel : GLPanel
    {
        public GLFlowLayoutPanel(string name, Rectangle location) : base(name, location)
        {
        }

        public GLFlowLayoutPanel(string name, DockingType type, float dockpercent) : base(name, DefaultWindowRectangle)
        {
            Dock = type;
            DockPercent = dockpercent;

        }

        public GLFlowLayoutPanel(string name, Size sizep, DockingType type, float dockpercentage) : base(name, DefaultWindowRectangle)
        {
            Dock = type;
            DockPercent = dockpercentage;
            SetLocationSizeNI(size: sizep);
        }

        public GLFlowLayoutPanel() : this("TLP?",DefaultWindowRectangle)
        {
        }

        public enum ControlFlowDirection { Right };

        public ControlFlowDirection FlowDirection { get { return flowDirection; } set { flowDirection = value; InvalidateLayout(); } }
        public GL4.Controls.Padding FlowPadding { get { return flowPadding; } set { flowPadding = value; InvalidateLayout(); } }

        private GL4.Controls.Padding flowPadding { get; set; } = new Padding(1);
        private ControlFlowDirection flowDirection = ControlFlowDirection.Right;

        // Sizing has been recursively done for all children, now size us

        protected override void SizeControl(Size parentsize)
        {
            base.SizeControl(parentsize);
    
            if (AutoSize)       // width stays the same, height changes, width based on what parent says we can have (either our width, or docked width)
            {
                int maxh = Flow(new Size(parentsize.Width,0), (c, p) => { });
                SetLocationSizeNI(size: new Size(Width, maxh + ClientBottomMargin + flowPadding.Bottom));
            }
        }

        // now we are laying out from top down

        public override void PerformRecursiveLayout()
        {
            //System.Diagnostics.Debug.WriteLine("Flow Laying out " + Name + " In client size " + ClientSize);

            Flow(ClientSize, (c, p) => 
            {
                c.SetLocationSizeNI(location:p);
                c.PerformRecursiveLayout();
            });
        }

        private int Flow(Size area, Action<GLBaseControl, Point> action)
        {
            Point flowpos = ClientLocation;
            int maxh = 0;
            foreach (GLBaseControl c in ControlsZ)
            {
                //System.Diagnostics.Debug.WriteLine("flow layout " + c.Name + " " + flowpos + " h " + maxh);

                if (flowpos.X + c.Width + flowPadding.TotalWidth >= area.Width)    // if beyond client right, more down
                {
                    flowpos = new Point(ClientLeftMargin, maxh);
                }

                Point pos = new Point(flowpos.X + FlowPadding.Left, flowpos.Y + flowPadding.Top);

                //System.Diagnostics.Debug.WriteLine("Control " + c.Name + " to " + pos);
                action(c, pos);

                flowpos.X += c.Width + flowPadding.TotalWidth;
                maxh = Math.Max(maxh, flowpos.Y + c.Height + FlowPadding.TotalHeight);
            }

            return maxh;
        }
    }
}

