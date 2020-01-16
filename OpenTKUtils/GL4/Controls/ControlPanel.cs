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
        public GLPanel()
        {
        }

        public GLPanel(string name, Rectangle location, Color back)
        {
            Name = name;
            Bounds = location;
            BackColor = back;
        }

        public GLPanel(string name, DockingType type, float dockpercent, Color back)
        {
            Name = name;
            Bounds = new Rectangle(0, 0, 10, 10);
            Dock = type;
            DockPercent = dockpercent;
            BackColor = back;
        }
    }
}


