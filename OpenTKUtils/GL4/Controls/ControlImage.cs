using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLImage : GLBaseControl
    {
        Bitmap image; // fix up

        public GLImage(Bitmap bmpp )
        {
            image = bmpp;
            Size = new Size(bmpp.Width, bmpp.Height);
        }

        public override void Paint(Bitmap bmp, Rectangle area)
        {

        }

    }
}
