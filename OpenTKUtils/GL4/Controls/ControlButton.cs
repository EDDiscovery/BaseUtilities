using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public abstract class GLButtonBase : GLBaseControl
    {
        public GLButtonBase()
        {
            InvalidateOnEnterLeave = true;
        }

        public string Text { get { return text; } set { text = value; SetRedraw(); } }
        public Image Image { get { return image; } set { image = value; SetRedraw(); } }

        string text;
        Image image;
    }

    public class GLButton : GLButtonBase
    { 
        public override void Paint(Bitmap bmp, Rectangle area, Graphics gr)
        {
            area.Inflate(new Size(-3, -3));
            System.Diagnostics.Debug.WriteLine("Button Paint {0} to {1}", Name, area);
            using (Brush b = new SolidBrush( Hover ? Color.Yellow :Color.Red))
                gr.FillRectangle(b, area);

        }
    }
}
