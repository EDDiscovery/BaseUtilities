using CSCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AudioTest
{
    class AudioWaveform : Control
    {
        IWaveSource source;
        long nosamples;


        public void Draw(IWaveSource source, int nosamples )
        {
            this.source = source;
            this.nosamples = nosamples;

            Invalidate();
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            //Debug.WriteLine($"{nameof(DrawnPanel)}.{nameof(OnPaint)} ({this.Name ?? "unnamed"}): Enabled {Enabled}, State {_DrawState}");

            Color cFore = Color.Black;
            using (var p1 = new Pen(cFore, 1.0F))
            {
                e.Graphics.DrawRectangle(p1, new Rectangle(0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1));
            }

            int mid = ClientRectangle.Height / 2;
            int height = mid;

            Color cg = Color.Blue;
            using (var p1 = new Pen(cg, 1.0F))
            {
                e.Graphics.DrawLine(p1,new Point(0,mid), new Point(ClientRectangle.Width,mid));
            }

            if (source != null)
            {
                //source.Position = 0;

                Point p = new Point(1, mid);
                double scaling = (double)ClientRectangle.Width / nosamples;

                Color cp = Color.Red;

                using (var p1 = new Pen(cp, 1.0F))
                {
                    long l = nosamples;
                    byte[] buf = new byte[l*2];
                    int amount = source.Read(buf, 0, (int)l*2);

                    for (int i = 0; i < amount; i += 2)
                    {
                        int sample = i / 2;
                        short v1 = BitConverter.ToInt16(buf, i);
                        Point np = new Point((int)(sample * scaling), (int)(mid - (double)v1 / 32768.0 * height));
                        if (np.X != p.X)
                        {
                            e.Graphics.DrawLine(p1, p, np);
                            p = np;
                        }

                    }
                }
            }

            base.OnPaint(e);
        }


    }
}
