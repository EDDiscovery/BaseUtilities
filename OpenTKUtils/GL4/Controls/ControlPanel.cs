using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLPanel : GLBaseControl
    {
        public GLPanel(string name, Rectangle location, Color back) : base(name, location, back)
        {
        }

        public GLPanel() : this("P?", DefaultWindowRectangle,DefaultBackColor)
        {
        }

        public GLPanel(string name, DockingType type, float dockpercent, Color back) : base(name,DefaultWindowRectangle,back)
        {
            Dock = type;
            DockPercent = dockpercent;
        }
    }
}


