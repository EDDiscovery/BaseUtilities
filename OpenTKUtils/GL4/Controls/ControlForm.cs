using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLForm : GLBaseControl
    {
         public GLForm( OpenTK.GLControl gcp)
        {
            gc = gcp;
            SetPos(0, 0, gcp.Width, gcp.Height,false);
        }

        public void Render()
        {

            foreach( var c in children )
            {
                if ( c.NeedRedraw )
                {
                    c.Redraw(null,new Rectangle(0,0,0,0));
                }

            }

            // set up shader,
            // render each one.. shader needs vertex of each box in co-ords, it converts them to gl co-oords.  tex co-ords are made up
        }


        private OpenTK.GLControl gc;

    }
}
