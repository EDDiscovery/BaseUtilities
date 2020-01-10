using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils.GL4.Controls
{
    public class GLListBox : GLBaseControl
    {
        public GLListBox()
        {
            Focusable = true;
        }

        public GLListBox(string name, Rectangle pos, string[] texts, Color backcolor)
        {
            Focusable = true;
            Name = name;
            Position = pos;
            BackColor = backcolor;
        }

        public Color ForeColor { get { return foreColor; } set { foreColor = value; Invalidate(); } }       // of text

        public float DisabledScaling
        {
            get { return disabledScaling; }
            set
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    return;
                else if (disabledScaling != value)
                {
                    disabledScaling = value;
                    Invalidate();
                }
            }
        }

        public Action<GLBaseControl,int> SelectionChanged { get; set; } = null;     // not fired by programatically changing Text
        public Action<GLBaseControl,int> ReturnPressed { get; set; } = null;     // not fired by programatically changing Text

        private float disabledScaling = 0.5F;
        private Color foreColor { get; set; } = Color.Black;
        //private int startpos = 0;
        //private int endpos = 0;

        public override void Paint(Rectangle area, Graphics gr)
        {
        }

        public override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if ( !e.Handled)
            {
            }
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled)
            {
                //System.Diagnostics.Debug.WriteLine("KDown " + Name + " " + e.KeyCode);

                if (e.KeyCode == System.Windows.Forms.Keys.Up)
                {
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.Down)
                {
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.Delete)
                {
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.Home)
                {
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.End)
                {
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.Return || e.KeyCode == System.Windows.Forms.Keys.Enter)
                {
                    ReturnPressed?.Invoke(this,0);
                }
            }
        }

    }
}
