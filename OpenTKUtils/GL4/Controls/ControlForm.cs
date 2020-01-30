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
        public GLForm(string name, Rectangle location, Color back) : base(name, location, back)
        {
            BorderWidthNI = 1;
            BorderColorNI = DefaultBorderColor;
            MarginNI = new Margin(2);
            PaddingNI = new Padding(1);
        }

        public GLForm() : this("F?", DefaultWindowRectangle, DefaultBackColor)
        {
        }

        private GLMouseEventArgs.AreaType captured = GLMouseEventArgs.AreaType.Client;  // meaning none
        private Point capturelocation;
        private Rectangle originalwindow;
        private bool cursorindicatingmovement = false;

        public override void OnMouseMove(GLMouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Handled == false)
            {
                if (captured != GLMouseEventArgs.AreaType.Client)
                {
                    Point capturedelta = new Point(e.Location.X - capturelocation.X, e.Location.Y - capturelocation.Y);
                    System.Diagnostics.Debug.WriteLine("***************************");
                    System.Diagnostics.Debug.WriteLine("Form " + captured + " " + e.Location + " " + capturelocation + " " + capturedelta);

                    if (captured == GLMouseEventArgs.AreaType.Left)
                    {
                        int left = originalwindow.Left + capturedelta.X;
                        int width = originalwindow.Right - left;
                        if (width > MinimumResizeWidth)
                            Bounds = new Rectangle(left, originalwindow.Top, width, originalwindow.Height);
                    }
                    else if (captured == GLMouseEventArgs.AreaType.Right)
                    {
                        int right = originalwindow.Right + capturedelta.X;
                        int width = right - originalwindow.Left;
                        if (width > MinimumResizeWidth)
                            Bounds = new Rectangle(originalwindow.Left, originalwindow.Top, width, originalwindow.Height);
                    }
                    else if (captured == GLMouseEventArgs.AreaType.Top)
                    {
                        Location = new Point(originalwindow.Left + capturedelta.X, originalwindow.Top + capturedelta.Y);
                    }
                    else if (captured == GLMouseEventArgs.AreaType.Bottom)
                    {
                        int bottom = originalwindow.Bottom + capturedelta.Y;
                        int height = bottom - originalwindow.Top;
                        if ( height > MinimumResizeHeight)
                            Bounds = new Rectangle(originalwindow.Left, originalwindow.Top, originalwindow.Width, height);
                    }
                }
                else
                {
                    if (e.Area == GLMouseEventArgs.AreaType.Left || e.Area == GLMouseEventArgs.AreaType.Right)
                    {
                        FindDisplay()?.SetCursor(GLCursorType.EW);
                        cursorindicatingmovement = true;
                    }
                    else if (e.Area == GLMouseEventArgs.AreaType.Top)
                    {
                        FindDisplay()?.SetCursor(GLCursorType.Move);
                        cursorindicatingmovement = true;
                    }
                    else if (e.Area == GLMouseEventArgs.AreaType.Bottom)
                    {
                        FindDisplay()?.SetCursor(GLCursorType.NS);
                        cursorindicatingmovement = true;
                    }
                    else if ( cursorindicatingmovement )
                    {
                        FindDisplay()?.SetCursor(GLCursorType.Normal);
                        cursorindicatingmovement = false;
                    }
                }
            }
        }

        public override void OnMouseDown(GLMouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!e.Handled && e.Area != GLMouseEventArgs.AreaType.Client)
            {
                capturelocation = e.Location;
                originalwindow = Bounds;
                captured = e.Area;
                System.Diagnostics.Debug.WriteLine("Capture");
            }
        }

        public override void OnMouseUp(GLMouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (captured != GLMouseEventArgs.AreaType.Client)
            {
                captured = GLMouseEventArgs.AreaType.Client;
                FindDisplay()?.SetCursor(GLCursorType.Normal);
            }
        }

        public override void OnMouseLeave(GLMouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if ( cursorindicatingmovement )
            {
                FindDisplay()?.SetCursor(GLCursorType.Normal);
                cursorindicatingmovement = false;
            }
            System.Diagnostics.Debug.WriteLine("Leave Form");

        }

    }
}


