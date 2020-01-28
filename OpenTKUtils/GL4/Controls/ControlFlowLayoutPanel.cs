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
        public GLFlowLayoutPanel(string name, Rectangle location, Color back) : base(name, location, back)
        {
        }

        public GLFlowLayoutPanel() : this("TLP?",DefaultWindowRectangle,DefaultBackColor)
        {
        }

        public enum ControlFlowDirection { Right };

        public ControlFlowDirection FlowDirection { get { return flowDirection; } set { flowDirection = value; InvalidateLayout(); } }
        public GL4.Controls.Padding FlowPadding { get { return flowPadding; } set { flowPadding = value; InvalidateLayout(); } }

        private GL4.Controls.Padding flowPadding { get; set; } = new Padding(1);
        private ControlFlowDirection flowDirection = ControlFlowDirection.Right;

        // Sizing has been recursively done for all children
        // now we are laying out from top down

        public override void PerformRecursiveLayout()
        {
            Rectangle flowpos = new Rectangle(ClientRectangle.Location,new Size(0,0));      // in terms of our client area

            foreach (GLBaseControl c in children)
            {
                System.Diagnostics.Debug.WriteLine("flow layout " + c.Name + " " + flowpos);

                if (flowpos.X + FlowPadding.Left + c.Width + flowPadding.Right >= ClientRectangle.Right)    // if beyond client right, more down
                {
                    flowpos = new Rectangle(ClientRectangle.X, flowpos.Bottom, 0, 0);
                }

                Point pos = new Point(flowpos.X + FlowPadding.Left, flowpos.Y + flowPadding.Top);

                c.SetLocationSizeNI(pos);

                int right = pos.X + c.Width + flowPadding.Right;
                int bot = Math.Max(flowpos.Bottom, pos.Y + c.Height + flowPadding.Bottom);

                flowpos = new Rectangle(right, flowpos.Top, 0, bot);

                c.PerformRecursiveLayout();
            }
        }
    }
}

